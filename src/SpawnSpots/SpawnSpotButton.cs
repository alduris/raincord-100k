using System.Collections.Generic;
using System.IO;
using Menu;
using RWCustom;
using UnityEngine;

namespace Raincord100k.SpawnSpots
{
    public class SpawnSpotButton : CircularMenuObject, SelectableMenuObject, ButtonMenuObject
    {
        public SpawnSpotData.SpawnRegion spawnRegion;
        public ButtonBehavior buttonBehav;
        private MenuMicrophone.MenuSoundLoop soundLoop;
        private bool hasSignalled;

        private float fillTime = 60f;
        private bool lastHeld, held;
        private float lastPulse, pulse;
        private float lastFilled, filled;
        private int buttonReleasedCounter;

        private FSprite backgroundSprite;
        private MenuLabel menuLabel;
        private FSprite[] circleSprites;

        public bool IsMouseOverMe => MouseOver;
        public bool CurrentlySelectableMouse => !buttonBehav.greyedOut && !hasSignalled;
        public bool CurrentlySelectableNonMouse => true;

        public ButtonBehavior GetButtonBehavior => buttonBehav;

        public SpawnSpotButton(Menu.Menu menu, MenuObject owner, Vector2 pos, SpawnSpotData.SpawnRegion region) : base(menu, owner, pos, 50f)
        {
            spawnRegion = region;
            buttonBehav = new ButtonBehavior(this);
            page.selectables.Add(this);

            // Background sprite
            string icon = "100k_" + region.value.ToLowerInvariant();
            if (!Futile.atlasManager.DoesContainElementWithName(icon))
            {
                Texture2D tex = new(0, 0);
                string path = AssetManager.ResolveFilePath("illustrations/" + icon + ".png");
                if (File.Exists(path))
                {
                    tex.LoadImage(File.ReadAllBytes(path));
                }
                else
                {
                    tex.LoadImage(File.ReadAllBytes(AssetManager.ResolveFilePath("illustrations/warp-unknown.png")));
                }
                tex.filterMode = FilterMode.Point;
                Futile.atlasManager.LoadAtlasFromTexture(icon, tex, false);
            }
            backgroundSprite = new FSprite(icon);
            backgroundSprite.SetAnchor(0.5f, 0.5f);
            Container.AddChild(backgroundSprite);

            // Circle sprites
            circleSprites = new FSprite[4];
            circleSprites[0] = new FSprite("Futile_White", true)
            {
                shader = menu.manager.rainWorld.Shaders["VectorCircleFadable"]
            };
            circleSprites[1] = new FSprite("Futile_White", true)
            {
                shader = menu.manager.rainWorld.Shaders["VectorCircle"]
            };
            circleSprites[2] = new FSprite("Futile_White", true)
            {
                shader = menu.manager.rainWorld.Shaders["HoldButtonCircle"]
            };
            circleSprites[3] = new FSprite("Futile_White", true)
            {
                shader = menu.manager.rainWorld.Shaders["VectorCircleFadable"]
            };
            for (int i = 0; i < circleSprites.Length; i++)
            {
                Container.AddChild(circleSprites[i]);
            }

            // Display text
            string displayText = region.RegionName;
            List<int> list = [];
            int lineChars = 0;
            for (int j = 0; j < displayText.Length; j++)
            {
                lineChars++;
                if (displayText[j] == ' ' && lineChars > 7) // low split
                {
                    lineChars = 0;
                    list.Add(j);
                }
            }
            int lengthOffset = 0;
            foreach (int breakAt in list)
            {
                int length = displayText.Length;
                int splitOffset = breakAt + lengthOffset;
                displayText = displayText.Substring(0, splitOffset) + "\n" + displayText.Substring(splitOffset + 1, displayText.Length - (splitOffset + 1));
                lengthOffset += displayText.Length - length;
            }
            menuLabel = new MenuLabel(menu, this, displayText, new Vector2(-50f, -15f), new Vector2(100f, 30f), false, null);
            subObjects.Add(menuLabel);
        }

        public override void Update()
        {
            base.Update();
            buttonBehav.Update();
            lastHeld = held;

            if (held)
            {
                soundLoop ??= menu.PlayLoop(SoundID.MENU_Security_Button_LOOP, 0f, 0f, 1f, false);
                soundLoop.loopVolume = Mathf.Lerp(soundLoop.loopVolume, 1f, 0.85f);
                soundLoop.loopPitch = Mathf.Lerp(0.3f, 1.5f, filled) - 0.15f * Mathf.Sin(pulse * 3.1415927f * 2f);
            }
            else if (!held && soundLoop != null)
            {
                soundLoop.loopVolume = Mathf.Max(0f, soundLoop.loopVolume - 0.125f);
                if (soundLoop.loopVolume <= 0f)
                {
                    soundLoop.Destroy();
                    soundLoop = null;
                }
            }

            if (buttonBehav.clicked)
            {
                lastPulse = pulse;
                pulse += filled / 20f;
            }
            else
            {
                pulse = 0f;
                lastPulse = 0f;
            }

            lastFilled = filled;
            held = Selected && !buttonBehav.greyedOut && !hasSignalled && menu.holdButton;

            if (held)
            {
                buttonBehav.sin = pulse;
                filled = Custom.LerpAndTick(filled, 1f, 0.007f, 1f / fillTime);
                if (filled >= 1f && !hasSignalled)
                {
                    Singal(this, spawnRegion.value);
                    hasSignalled = true;
                    menu.ResetSelection();
                }

                buttonReleasedCounter = 0;

                if (!lastHeld)
                {
                    menu.PlaySound(SoundID.MENU_Security_Button_Init);
                }
            }
            else
            {
                if (lastHeld && !hasSignalled)
                {
                    menu.PlaySound(SoundID.MENU_Security_Button_Release);
                }

                if (hasSignalled)
                {
                    buttonReleasedCounter++;
                    if (buttonReleasedCounter <= 30)
                    {
                        filled = 1f;
                    }
                    else
                    {
                        filled = Custom.LerpAndTick(filled, 0f, 0.04f, 0.025f);
                        if (filled < 0.5f)
                        {
                            hasSignalled = false;
                        }
                    }
                }
                else
                {
                    filled = Custom.LerpAndTick(filled, 0f, 0.04f, 0.025f);
                }
            }
        }

        public Color MyColor(float timeStacker)
        {
            if (buttonBehav.greyedOut)
            {
                return MenuColorEffect.rgbDarkGrey;
            }
            float f = Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker);
            return Color.Lerp(MenuColorEffect.rgbMediumGrey, MenuColorEffect.rgbWhite, f);
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            menuLabel.label.color = MyColor(timeStacker);
            float useRad = Mathf.Lerp(lastRad, rad, timeStacker);
            float bumpRad = useRad + 8f * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * 3.1415927f)) * (buttonBehav.clicked ? (0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(lastPulse, pulse, timeStacker) * 3.1415927f * 2f)) : 1f);
            float fillSpriteRad = bumpRad - 8f;
            Vector2 drawPos = DrawPos(timeStacker);
            bumpRad += 0.5f;

            backgroundSprite.SetPosition(drawPos);

            for (int i = 0; i < circleSprites.Length; i++)
            {
                circleSprites[i].x = drawPos.x;
                circleSprites[i].y = drawPos.y;
                circleSprites[i].scale = bumpRad / 8f;
            }
            circleSprites[0].color = new Color(0.019607844f, 0f, Mathf.Lerp(0.3f, 0.6f, buttonBehav.col));
            circleSprites[1].color = MyColor(timeStacker);
            circleSprites[1].alpha = 2f / bumpRad;
            circleSprites[2].scale = fillSpriteRad / 8f;
            circleSprites[2].alpha = Mathf.Lerp(lastFilled, filled, timeStacker);
            float fade = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(buttonBehav.lastSin, buttonBehav.sin, timeStacker) / 30f * 3.1415927f * 2f);
            fade *= buttonBehav.sizeBump;
            if (buttonBehav.greyedOut)
            {
                fade = 0f;
            }
            circleSprites[3].scale = (bumpRad - 8f * buttonBehav.sizeBump) / 8f;
            circleSprites[3].alpha = 2f / (bumpRad - 8f * buttonBehav.sizeBump);
            circleSprites[3].color = new Color(0f, 0f, fade);
        }

        public override void RemoveSprites()
        {
            base.RemoveSprites();
            backgroundSprite.RemoveFromContainer();
            for (int i = 0; i < circleSprites.Length; i++)
            {
                circleSprites[i].RemoveFromContainer();
            }
            soundLoop?.Destroy();
        }

        public virtual void Clicked()
        {
        }
    }
}
