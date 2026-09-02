using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Raincord100k.Pearls;
using Raincord100k.SpawnSpots;
using RWCustom;

namespace Raincord100k.Hooks
{
    internal static class TokenCacheHooks
    {
        internal static void Apply()
        {
            // Token cache
            On.RainWorld.ClearTokenCacheInMemory += RainWorld_ClearTokenCacheInMemory;
            On.RainWorld.ReadTokenCache += RainWorld_ReadTokenCache;
            On.RainWorld.BuildTokenCache += RainWorld_BuildTokenCache;
        }

        private static void RainWorld_ClearTokenCacheInMemory(On.RainWorld.orig_ClearTokenCacheInMemory orig, RainWorld self)
        {
            orig(self);
            PearlSpotRegistry.PotentialPearlSpots.Clear();
            SpawnSpotRegistry.PotentialSpawnSpots.Clear();
        }

        private static void RainWorld_BuildTokenCache(On.RainWorld.orig_BuildTokenCache orig, RainWorld self, bool modded, string region)
        {
            orig(self, modded, region);
            if (region.Equals("100K", StringComparison.OrdinalIgnoreCase))
            {
                // Setup
                lock (PearlSpotRegistry.PotentialPearlSpots)
                {
                    PearlSpotRegistry.PotentialPearlSpots.Clear();
                }

                // Find path
                string basePath = Path.Combine(Custom.RootFolderDirectory(), "World", "IndexMaps");
                if (modded)
                {
                    basePath = Path.Combine(Custom.RootFolderDirectory(), "mergedmods", "World", "IndexMaps");
                }

                string pearlPath = Path.Combine(basePath, "100kpearlspots.txt");
                string spawnPath = Path.Combine(basePath, "100kspawnspots.txt");

                // Find room settings
                string[] allFiles = AssetManager.ListDirectory("World" + Path.DirectorySeparatorChar.ToString() + region + "-Rooms", false, false, false);
                List<string> settingsToLoad = [];
                for (int i = 0; i < allFiles.Length; i++)
                {
                    string fileName = Path.GetFileName(allFiles[i]);
                    if (fileName.Contains("_settings"))
                    {
                        settingsToLoad.Add(allFiles[i]);
                    }
                }

                // Read room settings
                List<PearlSpotRegistry.Spot> collectedPearlSpots = [];
                Dictionary<string, List<string>> collectedSpawnSpots = [];
                foreach (string fileName in settingsToLoad)
                {
                    string[] lines = File.ReadAllLines(fileName);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string[] splitLine = Custom.ValidateSpacedDelimiter(lines[i], ":").Split([": "], 2, StringSplitOptions.None);
                        if (splitLine.Length == 2 && splitLine[0] == "PlacedObjects")
                        {
                            string[] placedObjectEntries = Regex.Split(Custom.ValidateSpacedDelimiter(splitLine[1], ","), ", ");
                            string roomName = Path.GetFileName(fileName.Substring(0, fileName.ToLowerInvariant().IndexOf("_settings")));

                            for (int poIndex = 0; poIndex < placedObjectEntries.Length; poIndex++)
                            {
                                string placedObjectEntry = placedObjectEntries[poIndex];
                                string[] splitEntry = placedObjectEntry.Split(["><"], StringSplitOptions.None);
                                if (splitEntry[0] == Constants.PearlSpot.value)
                                {
                                    collectedPearlSpots.Add(new PearlSpotRegistry.Spot(roomName.ToLowerInvariant(), poIndex));
                                }
                                else if (splitEntry[0] == Constants.SpawnSpot.value)
                                {
                                    string data = splitEntry[3];
                                    if (new SpawnSpotData.SpawnRegion(data).Index > -1)
                                    {
                                        if (!collectedSpawnSpots.TryGetValue(data, out var rooms))
                                        {
                                            collectedSpawnSpots.Add(data, rooms = []);
                                        }
                                        rooms.Add(Plugin.FixRoomName(roomName));
                                    }
                                }
                            }

                            break;
                        }
                    }
                }

                // Write
                lock (PearlSpotRegistry.PotentialPearlSpots)
                {
                    PearlSpotRegistry.PotentialPearlSpots.AddRange(collectedPearlSpots);
                    File.WriteAllLines(pearlPath, [.. collectedPearlSpots.Select(x => x.ToString())]);
                }

                lock (SpawnSpotRegistry.PotentialSpawnSpots)
                {
                    foreach (var (spawnRegion, rooms) in collectedSpawnSpots)
                    {
                        SpawnSpotRegistry.PotentialSpawnSpots[new SpawnSpotData.SpawnRegion(spawnRegion)] = rooms;
                    }
                    File.WriteAllLines(spawnPath, [.. collectedSpawnSpots.Select(x => $"{x.Key}:{string.Join(",", x.Value)}")]);
                }
            }
        }

        private static void RainWorld_ReadTokenCache(On.RainWorld.orig_ReadTokenCache orig, RainWorld self)
        {
            orig(self);

            // Pearl spots
            try
            {
                string path = AssetManager.ResolveFilePath(Path.Combine("World", "indexmaps", "100kpearlspots.txt"));
                if (File.Exists(path))
                {
                    foreach (var line in File.ReadAllLines(path))
                    {
                        if (PearlSpotRegistry.Spot.TryFromString(line) is { } spot)
                        {
                            PearlSpotRegistry.PotentialPearlSpots.Add(spot);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("100K PEARL DATA FAILED TO PARSE");
                UnityEngine.Debug.LogException(e);
                Plugin.Logger.LogFatal(e);
            }

            // Spawn spots
            try
            {
                string path = AssetManager.ResolveFilePath(Path.Combine("World", "indexmaps", "100kspawnspots.txt"));
                if (File.Exists(path))
                {
                    foreach (var line in File.ReadAllLines(path))
                    {
                        string[] split = line.Split(':');
                        SpawnSpotData.SpawnRegion region = new(split[0]);
                        if (split.Length == 2 && region.Index > -1)
                        {
                            string[] rooms = split[1].Split(',');
                            SpawnSpotRegistry.PotentialSpawnSpots.Add(region, [.. rooms]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("100K SPAWN SPOT DATA FAILED TO PARSE");
                UnityEngine.Debug.LogException(e);
                Plugin.Logger.LogFatal(e);
            }
        }
    }
}
