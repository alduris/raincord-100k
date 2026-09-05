using System;
using System.Collections.Generic;
using System.IO;

namespace Raincord100k
{
    public static class CreditsRegistry
    {
        private static Dictionary<string, string> creditsMap = null;

        private static void LoadRegistry()
        {
            if (creditsMap != null) return;

            creditsMap = new Dictionary<string, string>();
            string path = AssetManager.ResolveFilePath("100K_credits_map.txt");
            foreach (var line in File.ReadAllLines(path))
            {
                var split = line.Trim().Split([" : "], 2, StringSplitOptions.None);
                if (split.Length == 2)
                {
                    creditsMap.Add(split[0], split[1]);
                }
            }
        }

        public static string GetActualName(string identifier)
        {
            LoadRegistry();
            return creditsMap.TryGetValue(identifier, out string name) ? name : identifier;
        }
    }
}
