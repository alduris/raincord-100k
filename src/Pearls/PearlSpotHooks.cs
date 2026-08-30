using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MoreSlugcats;
using RWCustom;

namespace Raincord100k.Pearls
{
    internal static class PearlSpotHooks
    {
        internal static void Apply()
        {
            // Token cache
            On.RainWorld.ClearTokenCacheInMemory += RainWorld_ClearTokenCacheInMemory;
            On.RainWorld.ReadTokenCache += RainWorld_ReadTokenCache;
            On.RainWorld.BuildTokenCache += RainWorld_BuildTokenCache;

            // Scavenger ignorance
            On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool;
        }

        private static void RainWorld_ClearTokenCacheInMemory(On.RainWorld.orig_ClearTokenCacheInMemory orig, RainWorld self)
        {
            orig(self);
            PearlSpotRegistry.PotentialPearlSpots.Clear();
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

                string path = Path.Combine(basePath, "100kpearlspots.txt");

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
                List<PearlSpotRegistry.Spot> collectedSpots = [];
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
                                if (splitEntry[0] == Constants.PearlSpot100k.value)
                                {
                                    collectedSpots.Add(new PearlSpotRegistry.Spot(roomName.ToLowerInvariant(), poIndex));
                                }
                            }

                            break;
                        }
                    }
                }

                // Write
                lock (PearlSpotRegistry.PotentialPearlSpots)
                {
                    PearlSpotRegistry.PotentialPearlSpots.AddRange(collectedSpots);
                    File.WriteAllLines(path, [.. collectedSpots.Select(x => x.ToString())]);
                }
            }
        }

        private static void RainWorld_ReadTokenCache(On.RainWorld.orig_ReadTokenCache orig, RainWorld self)
        {
            orig(self);
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
                UnityEngine.Debug.LogError("100K PEARL SPOT MAP FAILED TO PARSE");
                UnityEngine.Debug.LogException(e);
                Plugin.Logger.LogFatal(e);
            }
        }

        private static int ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
        {
            if (obj is DataPearl pearl && pearl.Is100kPearl()) return 0;
            return orig(self, obj, weaponFiltered);
        }
    }
}
