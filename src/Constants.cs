using System.Linq;
using Menu;

namespace Raincord100k
{
    public static class Constants
    {
        public static SlugcatStats.Name Slugcat = new("Raincord100k", false);

        public static SlugcatStats.Timeline TimelinePast    = new("Raincord100k_Past",    false);
        public static SlugcatStats.Timeline TimelinePresent = new("Raincord100k_Present", false);
        public static SlugcatStats.Timeline TimelineFuture  = new("Raincord100k_Future",  false);

        public static EndCredits.Stage Credits_Logo;
        public static EndCredits.Stage Credits_Hosts;
        public static EndCredits.Stage Credits_Coders;
        public static EndCredits.Stage Credits_Rooms;
        public static EndCredits.Stage Credits_Music;
        public static EndCredits.Stage Credits_Decals;
        public static EndCredits.Stage Credits_Writers;
        public static EndCredits.Stage Credits_Thanks;
        public static EndCredits.Stage[] CreditsOrder;

        public static void RegisterCredits()
        {
            Credits_Logo    ??= new("100k_Logo",    true);
            Credits_Hosts   ??= new("100k_Hosts",   true);
            Credits_Coders  ??= new("100k_Coders",  true);
            Credits_Rooms   ??= new("100k_Rooms",   true);
            Credits_Music   ??= new("100k_Music",   true);
            Credits_Decals  ??= new("100k_Decals",  true);
            Credits_Writers ??= new("100k_Writers", true);
            Credits_Thanks  ??= new("100k_Thanks",  true);

            if (CreditsOrder == null)
            {
                CreditsOrder ??= [Credits_Logo, Credits_Hosts, Credits_Coders, Credits_Rooms, Credits_Music, Credits_Decals, Credits_Writers, Credits_Thanks];

                // Rearrange credit stages enum to place our enums at the front
                var values = EndCredits.Stage.values.entries;
                for (int i = values.Count - 1; i > CreditsOrder.Length; i--)
                {
                    values[i] = values[i - CreditsOrder.Length];
                }

                for (int i = 0; i < CreditsOrder.Length; i++)
                {
                    values[i + 1] = CreditsOrder[i].ToString(); // needs to leave [0] as it is
                }

                EndCredits.Stage.valuesVersion++;
            }
        }

        public static bool Is100KCreditsStage(EndCredits.Stage stage) => CreditsOrder.Contains(stage);
    }
}
