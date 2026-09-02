namespace Raincord100k.SpawnSpots
{
    internal static class SpawnSpotHooks
    {
        internal static void Apply()
        {
            On.SaveState.GetStoryDenPosition += SaveState_GetStoryDenPosition;
            On.SaveState.GetFinalFallbackShelter += SaveState_GetFinalFallbackShelter;
        }

        private static string SaveState_GetStoryDenPosition(On.SaveState.orig_GetStoryDenPosition orig, SlugcatStats.Name slugcat, out bool isVanilla)
        {
            if (slugcat == Constants.Slugcat)
            {
                isVanilla = false;
                return SpawnSpotRegistry.GetSpawnSpot();
            }
            return orig(slugcat, out isVanilla);
        }

        private static string SaveState_GetFinalFallbackShelter(On.SaveState.orig_GetFinalFallbackShelter orig, SlugcatStats.Name saveStateNumber)
        {
            if (saveStateNumber == Constants.Slugcat)
            {
                return "100K_SU_alduris_1";
            }
            return orig(saveStateNumber);
        }
    }
}
