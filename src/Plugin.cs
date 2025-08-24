using BepInEx;
using Raincord100k.Hooks;

namespace Raincord100k
{
    [BepInPlugin(MOD_ID, "Raincord 100k Gallery Region", "1.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "raincord_100k";


        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            MenuHooks.Apply();

            On.MoreSlugcats.OEsphere.AddToContainer += OESphereFix;
            On.ShelterDoor.ShelterEntranceOverrides += ShelterEntrance;
            On.HUD.HUD.InitSinglePlayerHud += ShelterHUDInit;
            On.HUD.HUD.Update += ShelterHUDHide;
        }

        public void OnDisable()
        {
            MenuHooks.Unapply();
        }
        
        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
        }
    }
}
