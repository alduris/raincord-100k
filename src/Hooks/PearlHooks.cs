using Raincord100k.Pearls;
using UnityEngine;

namespace Raincord100k.Hooks
{
    internal static class PearlHooks
    {
        internal static void Apply()
        {
            On.DataPearl.Update += DataPearl_Update;
            On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool;
        }

        private static void DataPearl_Update(On.DataPearl.orig_Update orig, DataPearl self, bool eu)
        {
            orig(self, eu);
            if (self.room?.game != null && self.room.game.IsStorySession && self.room.game.GetStorySession.saveStateNumber == Constants.Slugcat && self.Is100kPearl() && !SaveData.HasBeenRead(self.AbstractPearl.dataPearlType) && Random.value < 1f / 400f)
            {
                self.room.AddObject(new PearlPing(self.firstChunk.pos));
                self.room.PlaySound(SoundID.Moon_Wake_Up_Swarmer_Ping, self.firstChunk, false, Random.Range(0.6f, 0.8f), Random.Range(0.7f, 1.3f));
            }
        }

        private static int ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
        {
            if (obj is DataPearl pearl && pearl.Is100kPearl() && !SaveData.HasBeenRead(pearl.AbstractPearl.dataPearlType)) return 0;
            return orig(self, obj, weaponFiltered);
        }
    }
}
