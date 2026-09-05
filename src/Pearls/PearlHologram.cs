using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using HUD;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using PearlType = DataPearl.AbstractDataPearl.DataPearlType;

namespace Raincord100k.Pearls
{
    public class PearlHologram : HudPart, Conversation.IOwnAConversation
    {
        public enum Reader
        {
            MoonPreCollapse,
            MoonPostCollapse,
            MoonFuture,
            Pebbles
        }

        public static SlugcatStats.Name ReaderToSlugcat(Reader? reader)
        {
            return reader switch
            {
                Reader.MoonPreCollapse => MoreSlugcatsEnums.SlugcatStatsName.Spear,
                Reader.MoonPostCollapse => null,
                Reader.MoonFuture => MoreSlugcatsEnums.SlugcatStatsName.Saint,
                Reader.Pebbles => MoreSlugcatsEnums.SlugcatStatsName.Artificer,
                _ => null
            };
        }

        public enum Mode
        {
            InitSelecting,
            Selecting,
            CancelSelection,
            InitReading,
            Reading,
            DoneReading
        }

        public readonly int triggeringPlayer;
        public readonly Room room;
        public readonly Vector2 inRoomPos;
        public readonly PearlType pearlType;
        public readonly List<Reader> pearlReaders;

        public Conversation currentConversation;
        public Reader? currentReader;
        public bool hasLetGoOfDirection = false;

        private readonly FContainer container;
        private readonly List<Selection> selections;
        public Mode mode;
        private float transition;
        private float lastTransition;

        public bool Controllable => mode is Mode.InitSelecting or Mode.Selecting;

        private Player hudOwner => (hud.owner as Player);
        private RainWorldGame game => hudOwner.abstractCreature.world.game;

        public PearlHologram(HUD.HUD hud, int triggeringPlayer, PearlType pearlType) : base(hud)
        {
            room = hudOwner.room;
            inRoomPos = hudOwner.mainBodyChunk.pos;
            this.triggeringPlayer = triggeringPlayer;
            this.pearlType = pearlType;

            pearlReaders = [Reader.MoonPostCollapse]; // submissions were required to have this
            (string, Reader)[] checks =
            [
                // anything else is additional dialogue for the same pearl
                ("Artificer", Reader.Pebbles),
                ("Spear", Reader.MoonPreCollapse),
                ("Saint", Reader.MoonFuture),
            ];
            foreach (var (toCheck, reader) in checks)
            {
                if (DoesVersionExist(toCheck))
                {
                    pearlReaders.Add(reader);
                }
            }

            IntVector2[][] useDirections = 
            [
                [new IntVector2(0, 1)],
                [new IntVector2(-1, 0), new IntVector2(1, 0)],
                [new IntVector2(0, 1), new IntVector2(1, 0), new IntVector2(-1, 0)],
                [new IntVector2(0, 1), new IntVector2(1, 0), new IntVector2(0, -1), new IntVector2(-1, 0)],
            ];
            container = new FContainer();
            hud.fContainers[0].AddChild(container);
            selections = new List<Selection>();
            for (int i = 0; i < pearlReaders.Count; i++)
            {
                selections.Add(new Selection(this, container, useDirections[pearlReaders.Count - 1][i], pearlReaders[i]));
            }
        }

        private bool DoesVersionExist(string check)
        {
            string fileName = pearlType.ToString() + "-" + check + ".txt";
            string lang = AssetManager.ResolveFilePath(Path.Combine(game.rainWorld.inGameTranslator.SpecificTextFolderDirectory(game.rainWorld.inGameTranslator.currentLanguage), fileName));
            string eng = AssetManager.ResolveFilePath(Path.Combine(game.rainWorld.inGameTranslator.SpecificTextFolderDirectory(InGameTranslator.LanguageID.English), fileName));

            return File.Exists(lang) || File.Exists(eng);
        }

        public override void Update()
        {
            base.Update();

            lastTransition = transition;
            switch (mode)
            {
                case Mode.InitSelecting:
                    if (transition == 1f)
                    {
                        mode = Mode.Selecting;
                        break;
                    }
                    transition = Mathf.Clamp01(transition + 1f / 20f);
                    break;
                case Mode.InitReading:
                    if (transition == 1f)
                    {
                        mode = Mode.Reading;
                        break;
                    }
                    transition = Mathf.Clamp01(transition + 1f / 20f);
                    break;
                case Mode.CancelSelection or Mode.DoneReading:
                    if (transition == 1f)
                    {
                        slatedForDeletion = true;
                        break;
                    }
                    transition = Mathf.Clamp01(transition + 1f / 20f);
                    break;
                case Mode.Selecting:
                    transition = 0f;
                    break;
                case Mode.Reading:
                    transition = 0f;
                    if (currentConversation == null)
                    {
                        if (room.game.cameras[0].hud.dialogBox == null)
                        {
                            room.game.cameras[0].hud.InitDialogBox();
                        }
                        currentConversation = new PearlReading(this, room.game.cameras[0].hud.dialogBox);
                    }
                    else
                    {
                        currentConversation.Update();
                        if (currentConversation.slatedForDeletion)
                        {
                            mode = Mode.DoneReading;
                        }
                    }
                    break;
            }

            foreach (var selection in selections)
            {
                selection.Update();
            }
        }

        public void UpdateHoldDirection(IntVector2 dir)
        {
            if (mode != Mode.Selecting || (dir.x == 0 && dir.y == 0)) return;
            currentReader = null;
            foreach (var selection in selections)
            {
                bool selected = selection.dir == dir;
                selection.hovered = selected;
                if (selected)
                {
                    currentReader = selection.reader;
                }
            }
        }

        public void TriggerSelection()
        {
            if (mode != Mode.Selecting) return;
            bool wasHovering = false;
            foreach (var selection in selections)
            {
                if (selection.hovered)
                {
                    wasHovering = true;
                    selection.selected = true;
                }
            }
            mode = wasHovering ? Mode.InitReading : Mode.CancelSelection;
        }

        public string Translate(string s)
        {
            return game.rainWorld.inGameTranslator.Translate(s);
        }

        public string ReplaceParts(string s)
        {
            bool pebbles = currentReader == Reader.Pebbles;
            string lowerName = pebbles ? PebblesNameForPlayer(false) : MoonNameForPlayer(false);
            string upperName = pebbles ? PebblesNameForPlayer(true) : MoonNameForPlayer(true);

            s = s.Replace("<PLAYERNAME>", lowerName);
            s = s.Replace("<CAPPLAYERNAME>", upperName);
            s = s.Replace("<PlayerName>", lowerName);
            s = s.Replace("<CapPlayerName>", upperName);

            return s;
        }

        public void SpecialEvent(string eventName)
        {
        }

        private string PebblesNameForPlayer(bool capitalized)
        {
            string text = Translate("creature");
            string text2 = Translate("little");
            if (capitalized && InGameTranslator.LanguageID.UsesCapitals(game.rainWorld.inGameTranslator.currentLanguage))
            {
                text2 = char.ToUpper(text2.ToCharArray()[0]).ToString() + text2.Substring(1);
            }
            if (game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Thai)
            {
                return text + text2;
            }
            return text2 + " " + text;
        }

        private string MoonNameForPlayer(bool capitalized)
        {
            string text = "creature";
            if (Random.value > 0.3f)
            {
                text = "friend";
            }
            if (game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Portuguese && (text == "friend" || text == "creature"))
            {
                string text2 = Translate(text);
                if (capitalized && InGameTranslator.LanguageID.UsesCapitals(game.rainWorld.inGameTranslator.currentLanguage))
                {
                    text2 = char.ToUpper(text2[0]).ToString() + text2.Substring(1);
                }
                return text2;
            }
            string text3 = Translate(text);
            string text4 = Translate("little");
            if (capitalized && InGameTranslator.LanguageID.UsesCapitals(game.rainWorld.inGameTranslator.currentLanguage))
            {
                text4 = char.ToUpper(text4[0]).ToString() + text4.Substring(1);
            }
            if (game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Thai)
            {
                return text3 + text4;
            }
            return text4 + " " + text3;
        }

        private static string SpriteForReader(Reader reader)
        {
            return (reader == Reader.Pebbles ? "GuidancePebbles" : "GuidanceMoon");
        }

        private static Color ColorForReader(Reader reader)
        {
            return reader switch
            {
                Reader.MoonPreCollapse => new Color(0.68f, 0.56f, 0.74f),
                Reader.MoonPostCollapse => new Color(1f, 0.8f, 0.3f),
                Reader.MoonFuture => new Color(0.29411766f, 0.45490196f, 0.5254902f), // new Color(0.66667f, 0.9451f, 0.33725f),
                Reader.Pebbles => new Color(0.44705883f, 0.9019608f, 0.76862746f),
                _ => throw new System.NotImplementedException(),
            };
        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);
            Vector2 camPos = Vector2.Lerp(game.cameras[0].lastPos, game.cameras[0].pos, timeStacker);
            foreach (var selection in selections)
            {
                if (game.cameras[0].room != room || ((mode == Mode.Reading || mode == Mode.DoneReading) && !selection.selected))
                {
                    selection.SetVisibility(false);
                }
                else
                {
                    selection.DrawSprites(timeStacker, camPos);
                }
            }
        }

        public override void ClearSprites()
        {
            base.ClearSprites();
            container.RemoveAllChildren(); // technically this isn't necessary but oh well
            container.RemoveFromContainer();
        }

        private class Selection
        {
            public bool hovered = false;
            public bool selected = false;

            public PearlHologram owner;
            public Reader reader;
            public IntVector2 dir;
            public SelectionBump bumpBehav;

            private Color origColor;
            private FSprite bgSprite;
            private FSprite iconSprite;
            private FSprite[] outlineSprites;
            private FSprite[] bumpSprites;

            public Selection(PearlHologram owner, FContainer container, IntVector2 dir, Reader reader)
            {
                this.owner = owner;
                this.reader = reader;
                this.dir = dir;
                origColor = ColorForReader(reader);
                bumpBehav = new SelectionBump(this);

                // Init sprites
                bgSprite = new FSprite("Futile_White")
                {
                    shader = Custom.rainWorld.Shaders["FlatLight"],
                    color = origColor,
                    alpha = 0f
                };
                iconSprite = new FSprite(SpriteForReader(reader))
                {
                    color = origColor,
                    alpha = 0f
                };
                container.AddChild(bgSprite);
                container.AddChild(iconSprite);

                outlineSprites = new FSprite[4];
                for (int i = 0; i < outlineSprites.Length; i++)
                {
                    outlineSprites[i] = new FSprite("pixel")
                    {
                        shader = Custom.rainWorld.Shaders["Hologram"],
                        color = origColor,
                        alpha = 0f,
                        anchorY = 0f
                    };
                    container.AddChild(outlineSprites[i]);
                }
                bumpSprites = new FSprite[4];
                for (int i = 0; i < bumpSprites.Length; i++)
                {
                    bumpSprites[i] = new FSprite("pixel")
                    {
                        shader = Custom.rainWorld.Shaders["Hologram"],
                        color = origColor,
                        alpha = 0f,
                        anchorY = 0f
                    };
                    container.AddChild(bumpSprites[i]);
                }
            }

            public void Update()
            {
                bumpBehav.Update();
            }

            private float Fade(float timeStacker)
            {
                float useTransition = Mathf.Lerp(owner.lastTransition, owner.transition, timeStacker);
                float fac = 1f;
                switch (owner.mode)
                {
                    case Mode.InitSelecting:
                        fac = Mathf.Sqrt(useTransition);
                        break;
                    case Mode.CancelSelection or Mode.DoneReading:
                        fac = 1f - useTransition;
                        break;
                    case Mode.InitReading when !selected:
                        fac = 1f - useTransition;
                        break;
                }
                return fac;
            }

            private Vector2 UsePos(float timeStacker)
            {
                Vector2 center = owner.inRoomPos;
                float offsetFac = Mathf.Lerp(0.5f, 1f, Fade(timeStacker));
                return center + dir.ToVector2() * 40f * offsetFac;
            }

            private Vector2 CornerPos(int i, bool bump, float timeStacker, Vector2 drawPos)
            {
                float offset = 20f;
                if (bump)
                {
                    offset += 8f * (bumpBehav.sizeBump + 0.5f * Mathf.Sin(bumpBehav.extraSizeBump * 3.1415927f));
                }
                offset *= Mathf.Lerp(0.75f, 1f, Fade(timeStacker));

                float f = Mathf.InverseLerp(0, outlineSprites.Length, i);
                float angle = f * 2 * Mathf.PI;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));

                return drawPos + dir * offset;
            }

            public void DrawSprites(float timeStacker, Vector2 camPos)
            {
                SetVisibility(true);

                float fade = Fade(timeStacker);
                Vector2 drawPos = UsePos(timeStacker) - camPos;
                float colorFlash = Mathf.Max(Mathf.Lerp(bumpBehav.lastCol, bumpBehav.col, timeStacker), Mathf.Lerp(bumpBehav.lastFlash, bumpBehav.flash, timeStacker));
                Color useColor = Color.Lerp(origColor, Color.white, colorFlash);

                // bg glow
                bgSprite.SetPosition(drawPos);
                bgSprite.scale = 24f * Mathf.Lerp(0.5f, 1f, fade) / 8f;
                bgSprite.color = Color.Lerp(origColor, useColor, 0.5f);
                bgSprite.alpha = fade * 0.5f;

                // icon
                iconSprite.SetPosition(drawPos);
                iconSprite.color = useColor;
                iconSprite.alpha = fade;

                // outline
                for (int i = 0; i < outlineSprites.Length; i++)
                {
                    var sprite = outlineSprites[i];
                    Vector2 currPos = CornerPos(i, false, timeStacker, drawPos);
                    Vector2 nextPos = CornerPos((i + 1) % bumpSprites.Length, false, timeStacker, drawPos);
                    sprite.SetPosition(currPos);
                    sprite.rotation = Custom.AimFromOneVectorToAnother(currPos, nextPos);
                    sprite.scaleY = Vector2.Distance(currPos, nextPos);
                    sprite.color = useColor;
                    sprite.alpha = fade;
                }

                // bump outline
                float bumpAlpha = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(bumpBehav.lastSin, bumpBehav.sin, timeStacker) / 30f * 3.1415927f * 2f);
                bumpAlpha *= bumpBehav.sizeBump;
                for (int i = 0; i < bumpSprites.Length; i++)
                {
                    var sprite = bumpSprites[i];
                    Vector2 currPos = CornerPos(i, true, timeStacker, drawPos);
                    Vector2 nextPos = CornerPos((i + 1) % bumpSprites.Length, true, timeStacker, drawPos);
                    sprite.SetPosition(currPos);
                    sprite.rotation = Custom.AimFromOneVectorToAnother(currPos, nextPos);
                    sprite.scaleY = Vector2.Distance(currPos, nextPos);
                    sprite.color = Color.Lerp(origColor, Color.white, bumpBehav.sizeBump);
                    sprite.alpha = Mathf.Pow(bumpAlpha, 0.75f) * fade;
                }
            }

            public void SetVisibility(bool visibility)
            {
                bgSprite.isVisible = visibility;
                iconSprite.isVisible = visibility;
                foreach (var sprite in outlineSprites) sprite.isVisible = visibility;
                foreach (var sprite in bumpSprites) sprite.isVisible = visibility;
            }
        }

        private class SelectionBump
        {
            public SelectionBump(Selection owner)
            {
                this.owner = owner;
            }

            public void Update()
            {
                lastCol = col;
                lastFlash = flash;
                lastSin = sin;
                flash = Custom.LerpAndTick(flash, 0f, 0.03f, 0.16666667f);
                if (owner.hovered)
                {
                    if (!bump)
                    {
                        bump = true;
                    }
                    sizeBump = Custom.LerpAndTick(sizeBump, 1f, 0.1f, 0.1f);
                    sin += 1f;
                    if (!flashBool)
                    {
                        flashBool = true;
                        flash = 1f;
                    }
                    col = Mathf.Min(1f, col + 0.1f);
                }
                else
                {
                    bump = false;
                    flashBool = false;
                    sizeBump = Custom.LerpAndTick(sizeBump, 0f, 0.1f, 0.05f);
                    col = Mathf.Max(0f, col - 0.033333335f);
                }
                if (owner.selected)
                {
                    sizeBump = Custom.LerpAndTick(sizeBump, 1f, 0.1f, 0.1f);
                    sin = 7.5f;
                    bump = true;
                    if (flash < 0.75f)
                    {
                        flash = 0.75f;
                    }
                }
                lastExtraSizeBump = extraSizeBump;
                if (bump)
                {
                    extraSizeBump = Mathf.Min(1f, extraSizeBump + 0.1f);
                    return;
                }
                else
                {
                    extraSizeBump = 0f;
                }
            }

            public Selection owner;
            public float lastCol;
            public float col;
            public bool bump;
            public float sizeBump;
            public float extraSizeBump;
            public float lastExtraSizeBump;
            public float flash;
            public float lastFlash;
            public bool flashBool;
            public float sin;
            public float lastSin;
        }

        private class PearlReading : Conversation
        {
            private readonly PearlHologram owner;
            public PearlReading(PearlHologram owner, DialogBox dialogBox) : base(owner, Constants.PearlReading, dialogBox)
            {
                this.owner = owner;
                AddEvents();
            }

            public override void AddEvents()
            {
                base.AddEvents();

                // Credit
                string[] splitName = owner.pearlType.value.Split('_');
                string credit = CreditsRegistry.GetActualName(splitName[1]);
                events.Add(new TextEvent(this, 20, owner.Translate("[ Pearl text written by <NAME> ]").Replace("<NAME>", credit), 20));
                switch (owner.currentReader)
                {
                    case Reader.MoonPreCollapse:
                        events.Add(new TextEvent(this, 0, owner.Translate("[ Reading: Looks to the Moon (pre-collapse) ]"), 0));
                        break;
                    case Reader.MoonPostCollapse:
                        events.Add(new TextEvent(this, 0, owner.Translate("[ Reading: Looks to the Moon ]"), 0));
                        break;
                    case Reader.MoonFuture:
                        events.Add(new TextEvent(this, 0, owner.Translate("[ Reading: Looks to the Moon (future) ]"), 0));
                        break;
                    case Reader.Pebbles:
                        events.Add(new TextEvent(this, 0, owner.Translate("[ Reading: Five Pebbles ]"), 0));
                        break;
                    default:
                        events.Add(new TextEvent(this, 0, owner.Translate("[ Reading: Looks to the Moon ]"), 0));
                        break;
                }

                // Extra wait
                events.Add(new WaitEvent(this, 30));

                // Load file
                LoadEventsFromFile(this, owner.pearlType.value, ReaderToSlugcat(owner.currentReader));
            }

            public static void LoadEventsFromFile(Conversation self, string fileName, SlugcatStats.Name saveFile = null)
            {
                string path = SearchConvoFile(fileName, saveFile, out var languageID);
                if (!File.Exists(path)) return;

                string fileText = DecryptCustomText(path, languageID, fileName);

                string[] array = Regex.Split(fileText, "\r?\n");
                ParseConvoText(self, array);
            }

            public static string SearchConvoFile(string fileName, SlugcatStats.Name saveFile, out InGameTranslator.LanguageID languageID)
            {
                languageID = Custom.rainWorld.inGameTranslator.currentLanguage;

                string slugName = saveFile != null ? "-" + saveFile.value : "";
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

            public static void ParseConvoText(Conversation self, string[] array)
            {
                try
                {
                    for (int j = 1; j < array.Length; j++)
                    {
                        string[] array3 = LocalizationTranslator.ConsolidateLineInstructions(array[j]);
                        if (array3.Length == 3)
                        {
                            if (ModManager.MSC && !int.TryParse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture, out int num) && int.TryParse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture, out int num2))
                            {
                                self.events.Add(new TextEvent(self, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), array3[1], int.Parse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture)));
                            }
                            else
                            {
                                self.events.Add(new TextEvent(self, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), array3[2], int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                            }
                        }
                        else if (array3.Length == 2)
                        {
                            if (array3[0] == "SPECEVENT")
                            {
                                self.events.Add(new SpecialEvent(self, 0, array3[1]));
                            }
                        }
                        else if (array3.Length == 1 && array3[0].Length > 0)
                        {
                            self.events.Add(new TextEvent(self, 0, array3[0], 0));
                        }
                    }
                }
                catch
                {
                    self.events.Add(new TextEvent(self, 0, "TEXT ERROR", 100));
                }
            }
        }
    }
}
