using BepInEx;
﻿using BepInEx.Logging;
using Fisobs.Core;
using Raincord100k.Damoonlord;
using Raincord100k.Damoonlord.Peanut;
using Raincord100k.Hooks;
using System.IO;

namespace Raincord100k
{
    [BepInPlugin(MOD_ID, "Raincord 100k Gallery Region", "1.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "raincord_100k";

        public new static ManualLogSource Logger { get; private set; } = null!;

        // Add hooks
        public void OnEnable()
        {
            Logger = base.Logger;
        
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            MenuHooks.Apply();

            DamoonRooms.EnableHooks();
            PeanutMeta.EnableHooks();
        }

        public void OnDisable()
        {
            MenuHooks.Unapply();

            PeanutMeta.Disable();
        }
        
        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            ShaderLoader.LoadShaders();
            PomManager.RegisterPlacedObjects();
            
            Futile.atlasManager.LoadAtlas("assets" + Path.DirectorySeparatorChar + "Peanut_Sprites");
        }
    }
}
