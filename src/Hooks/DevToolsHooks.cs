using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevInterface;
using UnityEngine;

namespace Raincord100k.Hooks
{
    internal static class DevToolsHooks
    {
        internal static void Enable()
        {
            On.DevInterface.RoomSettingsPage.ctor += RoomSettingsPage_ctor;
            On.DevInterface.SoundPage.ctor += SoundPage_ctor;
        }

        private static void RoomSettingsPage_ctor(On.DevInterface.RoomSettingsPage.orig_ctor orig, RoomSettingsPage self, DevUI owner, string IDstring, DevUINode parentNode, string name)
        {
            orig(self, owner, IDstring, parentNode, name);
            ReplaceTemplateMenus(self);
        }

        private static void SoundPage_ctor(On.DevInterface.SoundPage.orig_ctor orig, SoundPage self, DevUI owner, string IDstring, DevUINode parentNode, string name)
        {
            orig(self, owner, IDstring, parentNode, name);
            ReplaceTemplateMenus(self);
        }

        private static void ReplaceTemplateMenus(DevUINode self)
        {
            if (self.owner.room.world.region != null && self.owner.room.world.region.name.Equals("100k", StringComparison.OrdinalIgnoreCase))
            {
                // Remove old template menus
                for (int i = self.subNodes.Count - 1; i >= 0; i--)
                {
                    DevUINode node = self.subNodes[i];
                    if (node is InheritFromTemplateMenu or SaveAsTemplateMenu)
                    {
                        node.ClearSprites();
                        self.subNodes.RemoveAt(i);
                    }
                }

                // Add in custom button
                self.subNodes.Add(new TemplatePicker100K(self.owner, "100K_TemplateSelectButton", self, new Vector2(1100f, 730f), 200f));
            }
        }

        private sealed class TemplatePicker100K : ButtonWithSelectPanel
        {
            private const string Prefix = "100k_settingstemplate_";

            public TemplatePicker100K(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width) : base(owner, IDstring, parentNode, pos, width, "Select template", MakeTemplateSelectPanel)
            {
                SetText();
            }

            public override void OnValueChange(string value)
            {
                RoomSettings.SetTemplate(value != "NONE" ? Prefix + value : value, owner.room.world.region);
                SetText();
                TopNode.Refresh();
            }

            private void SetText()
            {
                string parent = "NONE";
                if (RoomSettings.parent != null && !RoomSettings.parent.isAncestor)
                {
                    parent = RoomSettings.parent.name.Substring(Prefix.Length);
                }
                Text = $"Inherited template: {parent}";
            }

            private static SelectPanel MakeTemplateSelectPanel(ButtonWithSelectPanel button)
            {
                return new SelectPanel(button.owner, "100K_TemplateSelectPanel", button, new Vector2(10f, 10f) - button.absPos, new Vector2(305f, 420f), "Select template", [.. button.owner.room.world.region.roomSettingTemplateNames.Prepend("NONE")]);
            }
        }
    }
}
