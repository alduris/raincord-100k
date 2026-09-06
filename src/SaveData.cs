using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raincord100k.Pearls;
using UnityEngine;
using PearlType = DataPearl.AbstractDataPearl.DataPearlType;

namespace Raincord100k
{
    public static class SaveData
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "100k_data.txt");
        private static bool hasReadFileYet = false;
        private static readonly HashSet<PearlType> readPearls = [];

        public static bool HasBeenRead(PearlType pearlType)
        {
            ReadSaveData();
            return readPearls.Contains(pearlType);
        }

        public static void SetAsRead(PearlType pearlType)
        {
            ReadSaveData();
            if (!readPearls.Contains(pearlType))
            {
                Plugin.Logger.LogInfo("Marked '" + pearlType.value + "' as read");
                readPearls.Add(pearlType);
                File.WriteAllLines(SavePath, [.. readPearls.Select(x => x.value)]);
            }
        }

        private static void ReadSaveData()
        {
            if (hasReadFileYet) return;
            hasReadFileYet = true;
            if (!File.Exists(SavePath)) return;
            foreach (var line in File.ReadLines(SavePath))
            {
                readPearls.Add(new PearlType(line.Trim(), false));
            }
        }
    }
}
