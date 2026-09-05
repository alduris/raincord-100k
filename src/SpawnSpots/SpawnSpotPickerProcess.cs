using Menu;
using UnityEngine;
using static Raincord100k.SpawnSpots.SpawnSpotData.SpawnRegion;

namespace Raincord100k.SpawnSpots
{
    public class SpawnSpotPickerProcess : Menu.Menu
    {
        private static readonly SpawnSpotData.SpawnRegion[,] RegionGrid = {
            { null, WPTA, null, null, LC,   RM,   WAUA, WRSA },
            { WARC, WARX, SI,   CC,   UW,   SS,   WXXR, WORA },
            { null, null, VS,   HI,   SH,   CL,   null, null },
            { null, OE,   LF,   SU,   GW,   SL,   WRFX, WSKX },
            { null, HR,   SB,   DS,   LM,   DM,   WVWA, WRRA },
            };

        private SpawnSpotButton[,] regionButtonGrid;

        public SpawnSpotPickerProcess(ProcessManager manager) : base(manager, Constants.SpawnSpotProcess)
        {
            const float buttonSpacing = 120f;

            var page = new Page(this, null, "main", 0);
            pages.Add(page);
            selectedObject = null;

            // Set music
            if (manager.musicPlayer.song != null)
            {
                manager.musicPlayer.FadeOutAllSongs(25f);
            }

            // Explanation labels
            var titleLabel = new MenuLabel(this, page, Translate("SELECT STARTING REGION"), new Vector2(0f, manager.rainWorld.screenSize.y - 60f), new Vector2(manager.rainWorld.screenSize.x, 30f), true);
            titleLabel.label.shader = manager.rainWorld.Shaders["MenuText"];
            page.subObjects.Add(titleLabel);

            // Generate buttons
            regionButtonGrid = new SpawnSpotButton[RegionGrid.GetLength(0), RegionGrid.GetLength(1)];
            for (int i = 0; i < RegionGrid.GetLength(0); i++)
            {
                for (int j = 0; j < RegionGrid.GetLength(1); j++)
                {
                    if (RegionGrid[i, j] == null)
                    {
                        regionButtonGrid[i, j] = null;
                        continue;
                    }

                    var button = new SpawnSpotButton(this, page, manager.rainWorld.screenSize / 2 + buttonSpacing * new Vector2(j - (RegionGrid.GetLength(1) - 1) / 2f, -i + (RegionGrid.GetLength(0) - 1) / 2f), RegionGrid[i, j]);
                    page.subObjects.Add(button);
                    regionButtonGrid[i, j] = button;
                }
            }

            // Link buttons
            for (int i = 0; i < regionButtonGrid.GetLength(0); i++)
            {
                for (int j = 0; j < regionButtonGrid.GetLength(1); j++)
                {
                    if (regionButtonGrid[i, j] == null) continue;

                    // Vertical
                    int k = i;
                    while (true)
                    {
                        k++;
                        k %= regionButtonGrid.GetLength(0);
                        if (regionButtonGrid[k, j] != null)
                        {
                            MutualVerticalButtonBind(regionButtonGrid[k, j], regionButtonGrid[i, j]);
                            break;
                        }
                    }
                    
                    // Horizontal
                    k = j;
                    while (true)
                    {
                        k++;
                        k %= regionButtonGrid.GetLength(1);
                        if (regionButtonGrid[i, k] != null)
                        {
                            MutualHorizontalButtonBind(regionButtonGrid[i, j], regionButtonGrid[i, k]);
                            break;
                        }
                    }
                }
            }
        }

        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(sender, message);
            if (sender is SpawnSpotButton spawnSpotButton)
            {
                SpawnSpotRegistry.SelectedRegion = spawnSpotButton.spawnRegion;
                manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
                PlaySound(SoundID.MENU_Continue_From_Sleep_Death_Screen);
            }
        }

        public override void Update()
        {
            base.Update();
            if (manager.musicPlayer.song == null)
            {
                manager.musicPlayer.MenuRequestsSong("RW_81 - Breathing Hyometer", 1f, 1f);
            }
        }
    }
}
