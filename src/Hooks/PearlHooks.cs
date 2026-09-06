using System.Linq;
using Raincord100k.Pearls;
using UnityEngine;

namespace Raincord100k.Hooks
{
    internal static class PearlHooks
    {
        internal static void Apply()
        {
            On.DataPearl.Update += DataPearl_Update;
        }

        private static void DataPearl_Update(On.DataPearl.orig_Update orig, DataPearl self, bool eu)
        {
            orig(self, eu);
            if (self.room?.game != null && self.room.game.IsStorySession && self.room.game.GetStorySession.saveStateNumber == Constants.Slugcat && self.Is100kPearl() && !OptionsMenu.HasBeenRead(self.AbstractPearl.dataPearlType) && Random.value < 1f / 400f)
            {
                self.room.AddObject(new PearlPing(self.firstChunk.pos));
                self.room.PlaySound(SoundID.Moon_Wake_Up_Swarmer_Ping, self.firstChunk, false, Random.Range(0.6f, 0.8f), Random.Range(0.7f, 1.3f));
            }
        }
    }
}
