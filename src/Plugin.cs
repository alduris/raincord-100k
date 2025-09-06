using BepInEx;
using BepInEx.Logging;
using Raincord100k.Hooks;

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
            MenuHooks.Apply();
            CreditHooks.Apply();
        }
        
        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            Constants.RegisterCredits();

            ShaderLoader.LoadShaders();
            PomManager.RegisterPlacedObjects();
        }
    }
}
