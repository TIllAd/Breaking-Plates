using System.Reflection;
using KitchenLib;
using KitchenLib.Event;
using KitchenLib.Logging;
using KitchenMods;
using UnityEngine;
using BreakingPlates.Custom;

namespace BreakingPlates
{
    public class Mod : BaseMod, IModSystem
    {
        public const string MOD_GUID = "TillAd.PlateUp.BreakingPlates";
        public const string MOD_NAME = "Breaking Plates";
        public const string MOD_VERSION = "0.1.0";
        public const string MOD_AUTHOR = "Till";
        public const string MOD_GAMEVERSION = ">=1.1.4";

        public static KitchenLogger Logger;

        public Mod() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void OnInitialise()
        {
            Logger.LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
        }

        protected override void OnUpdate() { }

        protected override void OnPostActivate(KitchenMods.Mod mod)
        {
            Logger = InitLogger();

            // Reihenfolge: Ergebnisse vor den Dingen, die auf sie verweisen.
            AddGameDataObject<BagProvider>();
            AddGameDataObject<EmptyBag>();
            AddGameDataObject<PureBlueRockCandy>();
            AddGameDataObject<BlueRockCandy>();
            AddGameDataObject<CandyBag>();
            AddGameDataObject<PureCandyBag>();
            AddGameDataObject<ShardsPot>();
            AddGameDataObject<CrystalPot>();
            AddGameDataObject<SyrupPot>();
            AddGameDataObject<SugarPot>();
            AddGameDataObject<BlueRockCandyDish>();
            AddGameDataObject<PureCandyDish>();
            AddGameDataObject<ChiliPDish>();

            Events.BuildGameDataPreSetupEvent += (s, args) => PrefabUtil.FixMaterials();
#if DEBUG
            // Nur in Debug-Builds: Spieldaten zum Nachschauen in eine Datei schreiben.
            Events.BuildGameDataEvent += (s, args) =>
            {
                if (!args.firstBuild) return;
                try { DataDump.Write(args.gamedata); }
                catch (System.Exception e) { LogError("Dump fehlgeschlagen: " + e); }
            };
#endif
        }

        public static void LogInfo(string msg) => Debug.Log("[BreakingPlates] " + msg);
        public static void LogWarning(string msg) => Debug.LogWarning("[BreakingPlates] " + msg);
        public static void LogError(string msg) => Debug.LogError("[BreakingPlates] " + msg);
    }
}
