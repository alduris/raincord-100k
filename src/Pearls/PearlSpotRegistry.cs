using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using DataPearlType = DataPearl.AbstractDataPearl.DataPearlType;

namespace Raincord100k.Pearls
{
    public static class PearlSpotRegistry
    {
        private const string Identifier = "100K_";

        public static bool Is100kPearl(this DataPearlType type) => type.value.StartsWith(Identifier);
        public static bool Is100kPearl(this DataPearl pearl) => pearl.AbstractPearl.dataPearlType.Is100kPearl();

        public static IEnumerable<DataPearlType> AllPearlIDs 
            => DataPearl.AbstractDataPearl.DataPearlType.values.entries
                .Where(x => x.StartsWith(Identifier))
                .Select(x => new DataPearlType(x, false));

        public static List<Spot> PotentialPearlSpots { get; } = [];

        private static int _cachedPearlSpotMapSeed = int.MinValue;
        private static readonly Dictionary<string, Dictionary<int, DataPearlType>> _cachedPearlSpotMap = [];
        private static void MaybeRegeneratePearlSpotMap(int seed)
        {
            if (seed != _cachedPearlSpotMapSeed)
            {
                _cachedPearlSpotMapSeed = seed;

                Random.State oldState = Random.state;
                Random.InitState(seed);

                List<DataPearlType> pearlTypes = [.. AllPearlIDs];
                List<Spot> potentialSpots = [.. PotentialPearlSpots];

                foreach (var pearlType in pearlTypes)
                {
                    if (potentialSpots.Count == 0) break;

                    int pickedIndex = Random.Range(0, potentialSpots.Count);
                    Spot spot = potentialSpots[pickedIndex];
                    potentialSpots.RemoveAt(pickedIndex);

                    string key = spot.Room.ToLowerInvariant();
                    if (!_cachedPearlSpotMap.TryGetValue(key, out var pois))
                    {
                        pois = [];
                        _cachedPearlSpotMap.Add(key, pois);
                    }
                    pois.Add(spot.POIndex, pearlType);
                }

                Random.state = oldState;
            }
        }
        public static bool TryGetPearl(int seed, string room, int placedObjectIndex, out DataPearlType pearlType)
        {
            pearlType = null;
            MaybeRegeneratePearlSpotMap(seed);
            return _cachedPearlSpotMap.TryGetValue(room.ToLowerInvariant(), out var pois) && pois.TryGetValue(placedObjectIndex, out pearlType);
        }

        public record struct Spot(string Room, int POIndex)
        {
            public static Spot? TryFromString(string str)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    string[] split = str.Split([':'], 2);
                    if (split.Length == 2 && int.TryParse(split[1], NumberStyles.Any, CultureInfo.InvariantCulture, out int poIndex))
                    {
                        return new Spot(split[0], poIndex);
                    }
                }
                return null;
            }

            public override readonly string ToString()
            {
                return $"{Room}:{POIndex}";
            }
        }
    }
}
