using System.Collections.Generic;

namespace Raincord100k.SpawnSpots
{
    public class SpawnSpotData : PlacedObject.Data
    {
        private bool regionDefault = true;
        private SpawnRegion backingRegion = SpawnRegion.SU;
        public SpawnRegion Region 
        {
            get => backingRegion;
            set 
            {
                backingRegion = value;
                regionDefault = false;
            }
        }

        public SpawnSpotData(PlacedObject owner) : base(owner)
        {
        }

        public void SetIfDefault(SpawnRegion region)
        {
            Region = region;
        }

        public override void FromString(string s)
        {
            base.FromString(s);
            regionDefault = false;
            Region = new SpawnRegion(s);
            if (Region.Index == -1)
            {
                Region = SpawnRegion.SU;
                regionDefault = true;
            }
        }

        public override string ToString()
        {
            return Region.value;
        }

        public class SpawnRegion : ExtEnum<SpawnRegion>
        {
            private static readonly Dictionary<string, string> RegisteredNames = [];

            public string RegionName => RegisteredNames.TryGetValue(value, out var name) ? name : "???";

            public SpawnRegion(string value) : base(value, false) { }

            public SpawnRegion(string value, string name) : base(value, true)
            {
                if (!RegisteredNames.ContainsKey(value))
                {
                    RegisteredNames.Add(value, name);
                }
            }

            public static readonly SpawnRegion SU = new(nameof(SU), "Outskirts");
            public static readonly SpawnRegion HI = new(nameof(HI), "Industrial Complex");
            public static readonly SpawnRegion DS = new(nameof(DS), "Drainage System");
            public static readonly SpawnRegion GW = new(nameof(GW), "Garbage Wastes");
            public static readonly SpawnRegion SL = new(nameof(SL), "Shoreline");
            public static readonly SpawnRegion SH = new(nameof(SH), "Shaded Citadel");
            public static readonly SpawnRegion UW = new(nameof(UW), "The Exterior");
            public static readonly SpawnRegion SS = new(nameof(SS), "Five Pebbles");
            public static readonly SpawnRegion CC = new(nameof(CC), "Chimney Canopy");
            public static readonly SpawnRegion SI = new(nameof(SI), "Sky Islands");
            public static readonly SpawnRegion LF = new(nameof(LF), "Farm Arrays");
            public static readonly SpawnRegion SB = new(nameof(SB), "Subterranean");
            public static readonly SpawnRegion OE = new(nameof(OE), "Outer Expanse");
            public static readonly SpawnRegion VS = new(nameof(VS), "Pipeyard");
            public static readonly SpawnRegion MS = new(nameof(MS), "Submerged Superstructure");
            public static readonly SpawnRegion LC = new(nameof(LC), "Metropolis");
            public static readonly SpawnRegion RM = new(nameof(RM), "The Rot");
            public static readonly SpawnRegion CL = new(nameof(CL), "Silent Construct");
            public static readonly SpawnRegion LM = new(nameof(LM), "Waterfront Facility");
            public static readonly SpawnRegion DM = new(nameof(DM), "Looks to the Moon");
            public static readonly SpawnRegion UG = new(nameof(UG), "Undergrowth");
            public static readonly SpawnRegion WARX = new(nameof(WARX), "Aether Ridge");
            public static readonly SpawnRegion WARC = new(nameof(WARC), "Fetid Glen");
            public static readonly SpawnRegion WAUA = new(nameof(WAUA), "Ancient Urban");
            public static readonly SpawnRegion WBLA = new(nameof(WBLA), "Badlands");
            public static readonly SpawnRegion WXXR = new(nameof(WXXR), "Rotted Regions");
            public static readonly SpawnRegion WORA = new(nameof(WORA), "Outer Rim");
            public static readonly SpawnRegion WPTA = new(nameof(WPTA), "Signal Spires");
            public static readonly SpawnRegion WRFX = new(nameof(WRFX), "Coral Caves");
            public static readonly SpawnRegion WRRA = new(nameof(WRRA), "Rusted Wrecks");
            public static readonly SpawnRegion WRSA = new(nameof(WRSA), "Daemon");
            public static readonly SpawnRegion WSKX = new(nameof(WSKX), "Stormy Coast");
            public static readonly SpawnRegion WTDA = new(nameof(WTDA), "Torrid Desert");
            public static readonly SpawnRegion WTDB = new(nameof(WTDB), "Desolate Tract");
            public static readonly SpawnRegion WVWA = new(nameof(WVWA), "Verdant Waterways");
        }
    }
}
