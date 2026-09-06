using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using Raincord100k.Damoonlord.Peanut;
using Raincord100k.Hooks;
using Raincord100k.Pearls;
using Raincord100k.SpawnSpots;

namespace Raincord100k
{
    [BepInDependency("rwmodding.coreorg.pom", BepInDependency.DependencyFlags.HardDependency)] // POM
    [BepInDependency("io.github.dual.fisobs", BepInDependency.DependencyFlags.HardDependency)] // Fisobs
    [BepInDependency("com.rainworldgame.garrakx.crs.mod", BepInDependency.DependencyFlags.HardDependency)] // CRS
    [BepInPlugin(MOD_ID, "Raincord 100k Gallery Region", "1.0")]
    public class Plugin : BaseUnityPlugin
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
                _ = Constants.PearlSpot; // init enums

                DevToolsHooks.Enable();
                MenuHooks.Apply();
                PeanutMeta.EnableHooks();
                PearlHooks.Apply();
                PlacedObjectRegistration.ApplyHooks();
                PlayerHooks.Apply();
                ProcessHooks.Apply();
                SpawnSpotHooks.Apply();
                TokenCacheHooks.Apply();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
        
        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            //Constants.RegisterCredits();
            ShaderLoader.LoadShaders();
            PomManager.RegisterPlacedObjects();
            MachineConnector.SetRegisteredOI(MOD_ID, new OptionsMenu());
            
            Futile.atlasManager.LoadAtlas("assets" + Path.DirectorySeparatorChar + "Peanut_Sprites");
        }

        public static string FixRoomName(string roomName)
        {
            string[] split = roomName.Split('_');
            split[0] = split[0].ToUpperInvariant();
            split[1] = split[1].ToUpperInvariant();
            split[2] = split[2].ToLowerInvariant();
            return string.Join("_", split);
        }
    }
}
