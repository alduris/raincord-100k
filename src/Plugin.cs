using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using Raincord100k.Damoonlord.Peanut;
using Raincord100k.Hooks;
using Raincord100k.Pearls;

namespace Raincord100k
{
    [BepInPlugin(MOD_ID, "Raincord 100k Gallery Region", "1.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "raincord_100k";

        public new static ManualLogSource Logger { get; private set; } = null!;

        // Add hooks
        public void OnEnable()
        {
            Logger = base.Logger;
        
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

            try
            {
                _ = Constants.PearlSpot100k; // init enums

                DevToolsHooks.Enable();
                MenuHooks.Apply();
                PeanutMeta.EnableHooks();
                PearlSpotHooks.Apply();
                PlacedObjectRegistration.ApplyHooks();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
        
        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            Constants.RegisterCredits();
            ShaderLoader.LoadShaders();
            PomManager.RegisterPlacedObjects();
            
            Futile.atlasManager.LoadAtlas("assets" + Path.DirectorySeparatorChar + "Peanut_Sprites");
        }
    }
}
