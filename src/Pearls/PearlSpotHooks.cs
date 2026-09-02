namespace Raincord100k.Pearls
{
    internal static class PearlSpotHooks
    {
        internal static void Apply()
        {
            // Scavenger ignorance
            On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool;
        }

        private static int ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
        {
            if (obj is DataPearl pearl && pearl.Is100kPearl()) return 0;
            return orig(self, obj, weaponFiltered);
        }
    }
}
