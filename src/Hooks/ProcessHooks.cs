using System.Collections.Generic;
using System.Globalization;
using Music;
using Raincord100k.SpawnSpots;
using RWCustom;
using UnityEngine;

namespace Raincord100k.Hooks
{
    internal static class ProcessHooks
    {
        internal static void Apply()
        {
            On.ProcessManager.PostSwitchMainProcess += ProcessManager_PostSwitchMainProcess;
            On.Menu.SlugcatSelectMenu.StartGame += SlugcatSelectMenu_StartGame;
        }

        private static void ProcessManager_PostSwitchMainProcess(On.ProcessManager.orig_PostSwitchMainProcess orig, ProcessManager self, ProcessManager.ProcessID ID)
        {
            if (ID == Constants.SpawnSpotProcess)
            {
                self.currentMainLoop = new SpawnSpotPickerProcess(self);
            }
            else
            {
                orig(self, ID);
            }
        }

        private static void SlugcatSelectMenu_StartGame(On.Menu.SlugcatSelectMenu.orig_StartGame orig, Menu.SlugcatSelectMenu self, SlugcatStats.Name storyGameCharacter)
        {
            if (storyGameCharacter == Constants.Slugcat)
            {
                // Tell the game that we want to play our campaign
                self.manager.rainWorld.inGameSlugCat = storyGameCharacter;

                // Assign custom colors
                if (ModManager.MMF && self.manager.rainWorld.progression.miscProgressionData.colorsEnabled.ContainsKey(self.slugcatColorOrder[self.slugcatPageIndex].value) && self.manager.rainWorld.progression.miscProgressionData.colorsEnabled[self.slugcatColorOrder[self.slugcatPageIndex].value])
                {
                    var colors = new List<Color>();
                    for (int i = 0; i < self.manager.rainWorld.progression.miscProgressionData.colorChoices[self.slugcatColorOrder[self.slugcatPageIndex].value].Count; i++)
                    {
                        var hsl = new Vector3(1f, 1f, 1f);
                        if (self.manager.rainWorld.progression.miscProgressionData.colorChoices[self.slugcatColorOrder[self.slugcatPageIndex].value][i].Contains(","))
                        {
                            string[] array = self.manager.rainWorld.progression.miscProgressionData.colorChoices[self.slugcatColorOrder[self.slugcatPageIndex].value][i].Split([',']);
                            hsl = new Vector3(float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture));
                        }
                        colors.Add(Custom.HSL2RGB(hsl[0], hsl[1], hsl[2]));
                    }
                    PlayerGraphics.customColors = colors;
                }
                else
                {
                    PlayerGraphics.customColors = null;
                }

                // Tell the game that we want to play our campaign, but differently
                self.manager.arenaSitting = null;
                self.manager.rainWorld.progression.currentSaveState = null;
                self.manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = storyGameCharacter;

                // Jolly Coop
                if (ModManager.CoopAvailable)
                {
                    for (int j = 1; j < self.manager.rainWorld.options.JollyPlayerCount; j++)
                    {
                        self.manager.rainWorld.ActivatePlayer(j);
                    }
                    for (int k = self.manager.rainWorld.options.JollyPlayerCount; k < 4; k++)
                    {
                        self.manager.rainWorld.DeactivatePlayer(k);
                    }
                }

                // Switch processes
                if (!self.restartChecked && self.manager.rainWorld.progression.IsThereASavedGame(storyGameCharacter))
                {
                    self.ContinueStartedGame(storyGameCharacter);
                }
                else
                {
                    self.manager.rainWorld.progression.WipeSaveState(storyGameCharacter);
                    self.manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.New;
                    self.manager.RequestMainProcessSwitch(Constants.SpawnSpotProcess);
                    self.PlaySound(SoundID.MENU_Start_New_Game);
                }

                // Fade out music
                if (self.manager.musicPlayer != null && self.manager.musicPlayer.song != null && self.manager.musicPlayer.song is IntroRollMusic)
                {
                    self.manager.musicPlayer.song.FadeOut(20f);
                }
            }
            else
            {
                orig(self, storyGameCharacter);
            }
        }
    }
}
