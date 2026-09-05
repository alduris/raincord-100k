using System.Runtime.CompilerServices;
using Raincord100k.Pearls;
using RWCustom;

namespace Raincord100k.Hooks
{
    internal static class PlayerHooks
    {
        private static readonly ConditionalWeakTable<RainWorldGame, PearlHologram> cwt = new();
        private static bool hasSeenPearlTutorial = false;

        internal static void Apply()
        {
            On.Player.checkInput += Player_checkInput;
            On.Player.Update += Player_Update;
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            if (self.room != null && cwt.TryGetValue(self.room.game, out var hologram) && hologram.slatedForDeletion)
            {
                cwt.Remove(self.room.game);
            }
            orig(self, eu);

            if (!hasSeenPearlTutorial && self.Consious && self.room != null && self.room.game.IsStorySession && self.room.game.GetStorySession.saveStateNumber == Constants.Slugcat)
            {
                //
                for (int i = 0; i < self.grasps.Length; i++)
                {
                    if (self.grasps[i]?.grabbed is DataPearl pearl && pearl.Is100kPearl())
                    {
                        hasSeenPearlTutorial = true;
                        self.room.AddObject(new PearlTutorial(self.room));
                    }
                }
            }
        }

        private static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
        {
            if (self.Consious && !self.isNPC && self.room != null && self.room.game.IsStorySession && self.room.game.cameras[0].hud?.owner == self && self.room.game.GetStorySession.saveStateNumber == Constants.Slugcat && self.bodyMode == Player.BodyModeIndex.Stand)
            {
                var hud = self.room.game.cameras[0].hud;
                for (int i = 0; i < self.grasps.Length; i++)
                {
                    if (self.grasps[i]?.grabbed is DataPearl pearl && pearl.Is100kPearl())
                    {
                        var currInput = RWInput.PlayerInput(self.playerState.playerNumber);
                        bool hadHologram;
                        if (!(hadHologram = cwt.TryGetValue(self.room.game, out var hologram)) || (hologram.triggeringPlayer == self.playerState.playerNumber && (hologram.Controllable || !hologram.hasLetGoOfDirection)))
                        {
                            if (currInput.spec && (!hadHologram || hologram.Controllable))
                            {
                                if (!hadHologram)
                                {
                                    hologram = new PearlHologram(hud, self.playerState.playerNumber, pearl.AbstractPearl.dataPearlType);
                                    hud.AddPart(hologram);
                                    cwt.Add(self.room.game, hologram);
                                }

                                IntVector2 holdDir = currInput.IntVec;
                                hologram.UpdateHoldDirection(holdDir);

                                for (int j = self.input.Length - 1; j > 0; j--)
                                {
                                    self.input[j] = self.input[j - 1];
                                }
                                currInput.spec = false;
                                currInput.x = 0;
                                currInput.y = 0;
                                self.input[0] = currInput;
                                return;
                            }
                            else if (hadHologram)
                            {
                                if (hologram.Controllable)
                                {
                                    hologram.TriggerSelection();
                                }
                                if (!hologram.hasLetGoOfDirection)
                                {
                                    if (currInput.x != 0 || currInput.y != 0)
                                    {
                                        for (int j = self.input.Length - 1; j > 0; j--)
                                        {
                                            self.input[j] = self.input[j - 1];
                                        }
                                        currInput.spec = false;
                                        currInput.x = 0;
                                        currInput.y = 0;
                                        self.input[0] = currInput;
                                        return;
                                    }
                                    else
                                    {
                                        hologram.hasLetGoOfDirection = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            orig(self);
        }
    }
}
