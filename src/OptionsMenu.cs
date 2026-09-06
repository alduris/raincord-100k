using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Menu.Remix.MixedUI;
using Raincord100k.Pearls;
using Raincord100k.Remix;
using RWCustom;
using UnityEngine;
using PearlType = DataPearl.AbstractDataPearl.DataPearlType;

namespace Raincord100k
{
    public class OptionsMenu : OptionInterface
    {
        private OpScrollBox pearlScrollBox, textScrollBox;

        public override void Initialize()
        {
            base.Initialize();

            var tab = new OpTab(this, Translate("Collection"));
            Tabs = [tab];

            tab.AddItems(new OpLabel(new Vector2(0f, 570f), new Vector2(600f, 30f), Translate("COLLECTION"), FLabelAlignment.Center, true));
            pearlScrollBox = new OpScrollBox(new Vector2(0f, 200f), new Vector2(600f, 360f), 0f, false, true, true);
            textScrollBox = new OpScrollBox(new Vector2(0f, 0f), new Vector2(600f, 190f), 190f, false, true, true);
            tab.AddItems(pearlScrollBox, textScrollBox);
            textScrollBox.AddItems(new OpLabel(10f, textScrollBox.size.y - 30f, Translate("Pearl text will appear here")));
            FillPearlScrollbox();
        }

        private void FillPearlScrollbox()
        {
            // Remove old
            foreach (UIelement element in pearlScrollBox.items)
            {
                element.Deactivate();
                element.tab._RemoveItem(element);
            }
            pearlScrollBox.items.Clear();
            pearlScrollBox.SetContentSize(0);

            // Add pearl buttons
            float x = 10f;
            float y = pearlScrollBox.size.y - 40f;
            foreach (var pearl in PearlSpotRegistry.AllPearlIDs.OrderBy(x => x.value, StringComparer.OrdinalIgnoreCase))
            {
                var readers = AvailablePearlReadersFor(pearl).ToList();

                if (SaveData.HasBeenRead(pearl))
                {
                    if (readers.Count == 1)
                    {
                        var button = new OpPearlButton(new Vector2(x, y), pearl, null);
                        button.OnClick += ReadPearl;
                        AdjustPointerPos(button);
                        pearlScrollBox.AddItems(button);
                    }
                    else
                    {
                        foreach (var reader in readers)
                        {
                            var button = new OpPearlButton(new Vector2(x, y), pearl, reader);
                            button.OnClick += ReadPearl;
                            AdjustPointerPos(button);
                            pearlScrollBox.AddItems(button);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < readers.Count; i++)
                    {
                        var button = new OpSimpleImageButton(new Vector2(x, y), new Vector2(30f, 30f), "Symbol_Unknown")
                        {
                            greyedOut = true
                        };
                        AdjustPointerPos(button);
                        pearlScrollBox.AddItems(button);
                    }
                }
            }

            pearlScrollBox.SetContentSize(pearlScrollBox.size.y - y + 10f);
            pearlScrollBox.ScrollToTop();

            void AdjustPointerPos(OpSimpleButton button)
            {
                x += button.size.x;
                if (x > pearlScrollBox.size.x - 40f) // 40f = 10 left padding + 10 right padding + 20 scroll bar padding
                {
                    x = 10f;
                    y -= 40f;
                    button.pos = new Vector2(x, y);
                    x += button.size.x;
                }
                x += 10f;
            }
        }

        private void ReadPearl(UIfocusable trigger)
        {
            if (trigger is OpPearlButton pearlButton)
            {
                // Remove old
                foreach (UIelement element in textScrollBox.items)
                {
                    element.Deactivate();
                    element.tab._RemoveItem(element);
                }
                textScrollBox.items.Clear();
                textScrollBox.SetContentSize(0);

                // Add pearl text to add
                var pearl = pearlButton.pearlType;
                var reader = pearlButton.reader;

                float y = textScrollBox.size.y;
                float width = textScrollBox.size.x - 40f;
                foreach (var line in ReadTextFor(pearl, reader))
                {
                    y -= 10f;
                    if (line == null) continue; // null acts as spacing

                    var label = new OpLabelLong(new Vector2(10f, y), new Vector2(width, 600f), line, true, FLabelAlignment.Left);
                    float height = label.GetDisplaySize().y;
                    label.size = new Vector2(width, height);
                    label.PosY -= height;
                    textScrollBox.AddItems(label);
                    y -= height;
                }

                textScrollBox.SetContentSize(textScrollBox.size.y - y + 10f);
                textScrollBox.ScrollToTop();
            }
        }

        private static IEnumerable<PearlHologram.Reader> AvailablePearlReadersFor(PearlType pearlType)
        {
            yield return PearlHologram.Reader.MoonPostCollapse;
            if (DoesVersionExist("Artificer")) yield return PearlHologram.Reader.Pebbles;
            if (DoesVersionExist("Spear")) yield return PearlHologram.Reader.MoonPreCollapse;
            if (DoesVersionExist("Saint")) yield return PearlHologram.Reader.MoonFuture;
            yield break;

            bool DoesVersionExist(string check)
            {
                string fileName = pearlType.ToString() + "-" + check + ".txt";
                string lang = AssetManager.ResolveFilePath(Path.Combine(Custom.rainWorld.inGameTranslator.SpecificTextFolderDirectory(Custom.rainWorld.inGameTranslator.currentLanguage), fileName));
                string eng = AssetManager.ResolveFilePath(Path.Combine(Custom.rainWorld.inGameTranslator.SpecificTextFolderDirectory(InGameTranslator.LanguageID.English), fileName));

                return File.Exists(lang) || File.Exists(eng);
            }
        }

        private static IEnumerable<string> ReadTextFor(PearlType pearlType, PearlHologram.Reader reader)
        {
            // Credit
            string[] splitName = pearlType.value.Split('_');
            string credit = CreditsRegistry.GetActualName(splitName[1]);
            string prefixText = Translate("[ Pearl text written by <NAME> ]").Replace("<NAME>", credit);

            // Reading
            switch (reader)
            {
                case PearlHologram.Reader.MoonPreCollapse:
                    prefixText += "\n" + Translate("[ Reading: Looks to the Moon (pre-collapse) ]");
                    break;
                case PearlHologram.Reader.MoonPostCollapse:
                    prefixText += "\n" + Translate("[ Reading: Looks to the Moon ]");
                    break;
                case PearlHologram.Reader.MoonFuture:
                    prefixText += "\n" + Translate("[ Reading: Looks to the Moon (future) ]");
                    break;
                case PearlHologram.Reader.Pebbles:
                    prefixText += "\n" + Translate("[ Reading: Five Pebbles ]");
                    break;
                default:
                    prefixText += "\n" + Translate("[ Reading: Looks to the Moon ]");
                    break;
            }
            yield return prefixText;

            yield return null;

            foreach (var line in LoadEventsFromFile(pearlType.value, PearlHologram.ReaderToSlugcat(reader)))
            {
                yield return ReplaceParts(line);
            }

            string ReplaceParts(string s)
            {
                if (s == null) return s;

                bool pebbles = reader == PearlHologram.Reader.Pebbles;
                string lowerName = pebbles ? PearlHologram.PebblesNameForPlayer(false) : PearlHologram.MoonNameForPlayer(false);
                string upperName = pebbles ? PearlHologram.PebblesNameForPlayer(true) : PearlHologram.MoonNameForPlayer(true);

                s = s.Replace("<LINE>", "\n");
                s = s.Replace("<PLAYERNAME>", lowerName);
                s = s.Replace("<CAPPLAYERNAME>", upperName);
                s = s.Replace("<PlayerName>", lowerName);
                s = s.Replace("<CapPlayerName>", upperName);
                s = s.Trim();
                s = Regex.Replace(s, " +", " "); // remove multi spaces
                s = Regex.Replace(s, "[\r\n]+", "\n"); // remove multi line breaks

                return s;
            }
        }

        private static IEnumerable<string> LoadEventsFromFile(string fileName, SlugcatStats.Name saveFile = null)
        {
            string path = SearchConvoFile(fileName, saveFile, out var languageID);
            if (!File.Exists(path)) yield break;

            string fileText = DecryptCustomText(path, languageID, fileName);

            string[] array = Regex.Split(fileText, "\r?\n");
            foreach (var line in ParseConvoText(array))
            {
                yield return line;
            }
        }

        public static string SearchConvoFile(string fileName, SlugcatStats.Name slugcat, out InGameTranslator.LanguageID languageID)
        {
            languageID = Custom.rainWorld.inGameTranslator.currentLanguage;

            string slugName = slugcat != null ? "-" + slugcat.value : "";
            InGameTranslator.LanguageID[] languageIDs = [languageID, InGameTranslator.LanguageID.English];

            foreach (var checkID in languageIDs)
            {
                languageID = checkID;
                string langDirectory = Custom.rainWorld.inGameTranslator.SpecificTextFolderDirectory(checkID);
                string path = Path.Combine(langDirectory, fileName + slugName + ".txt");
                path = AssetManager.ResolveFilePath(path);
                if (File.Exists(path)) return path;
            }

            return null;
        }

        public static string DecryptCustomText(string path, InGameTranslator.LanguageID languageID, string pearlName)
        {
            string fileText = InGameTranslator.EncryptDecryptFile(path, false, true);
            if (fileText == null)
            {
                return File.ReadAllText(path);
            }
            else
            {
                string[] array = Regex.Split(fileText, "\r\n");
                if (array.Length > 0 && array[0].Length > 0 && array[0].Substring(1).ToLower() == $"-{Path.GetFileNameWithoutExtension(path).ToLower()}")
                {
                    return fileText;
                }
                else
                {
                    return Custom.xorEncrypt(File.ReadAllText(path, Encoding.UTF8), 54 + pearlName.GetHashCode() + (int)languageID * 7);
                }
            }
        }

        public static IEnumerable<string> ParseConvoText(string[] array)
        {
            for (int i = 1; i < array.Length; i++)
            {
                string[] lineInstructions = LocalizationTranslator.ConsolidateLineInstructions(array[i]);
                if (lineInstructions.Length == 3)
                {
                    if (ModManager.MSC && !int.TryParse(lineInstructions[1], NumberStyles.Any, CultureInfo.InvariantCulture, out _) && int.TryParse(lineInstructions[2], NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    {
                        yield return lineInstructions[1];
                    }
                    else
                    {
                        yield return lineInstructions[2];
                    }
                }
                else if (lineInstructions.Length == 1 && lineInstructions[0].Length > 0)
                {
                    yield return lineInstructions[0];
                }
            }
        }
    }
}
