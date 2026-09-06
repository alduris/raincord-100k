using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menu.Remix.MixedUI;
using Raincord100k.Pearls;
using PearlType = DataPearl.AbstractDataPearl.DataPearlType;

namespace Raincord100k
{
    public class OptionsMenu : OptionInterface
    {
        private static OptionsMenu Instance;

        private static readonly Dictionary<PearlType, Configurable<bool>> internalReadMap = [];
        public static bool HasBeenRead(PearlType pearlType) => internalReadMap.TryGetValue(pearlType, out var tracker) && tracker.Value;
        public static void SetAsRead(PearlType pearlType)
        {
            if (internalReadMap.TryGetValue(pearlType, out var configurable) && !configurable.Value)
            {
                configurable.Value = true;
                Instance._SaveConfigFile();
            }
        }

        private static string SafeConfigName(string name) => string.Join("", name.Where(c => char.IsLetterOrDigit(c) || c == '_'));

        public OptionsMenu()
        {
            Instance = this;
            foreach (var pearl in PearlSpotRegistry.AllPearlIDs)
            {
                if (!internalReadMap.ContainsKey(pearl))
                {
                    internalReadMap.Add(pearl, config.Bind(SafeConfigName(pearl.value), false, new ConfigurableInfo("Tracks whether this pearl has been picked up")));
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            // crash deterrence until something gets put here
            var tab = new OpTab(this, "");
            Tabs = [tab];

            
            // todo: maybe put collections menu here?
        }
    }
}
