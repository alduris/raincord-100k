using System.Collections.Generic;
using UnityEngine;

namespace Raincord100k.SpawnSpots
{
    public static class SpawnSpotRegistry
    {
        public static Dictionary<SpawnSpotData.SpawnRegion, List<string>> PotentialSpawnSpots { get; } = [];
        public static SpawnSpotData.SpawnRegion SelectedRegion { get; set; } = SpawnSpotData.SpawnRegion.SU;

        public static string GetSpawnSpot()
        {
            if (PotentialSpawnSpots.TryGetValue(SelectedRegion, out List<string> potentialRooms) && potentialRooms.Count > 0)
            {
                return potentialRooms[Random.Range(0, potentialRooms.Count)];
            }
            return "100K_SU_alduris_1";
        }
    }
}
