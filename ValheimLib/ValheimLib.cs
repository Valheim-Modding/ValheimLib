using BepInEx;
using ValheimLib.ObjectDBLib;

namespace ValheimLib
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class ValheimLib : BaseUnityPlugin
    {
        public const string ModGuid = "ValheimModdingTeam." + ModName;
        public const string ModName = "ValheimLib";
        public const string ModVer = "0.0.1";

        internal static ValheimLib Instance { get; private set; }

        private void Awake()
        {
            Log.Init(Logger);

            Language.Init();
            ObjectDBHelper.Init();

            Instance = this;
        }
    }
}