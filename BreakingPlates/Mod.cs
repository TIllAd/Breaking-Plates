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
        public const string MOD_VERSION = "0.2.0";
        public const string MOD_AUTHOR = "Till";
        public const string MOD_GAMEVERSION = ">=1.1.4";

        public static KitchenLogger Logger;

        public Mod() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        private static HarmonyLib.Harmony _harmony;

        protected override void OnInitialise()
        {
            if (_harmony == null)
            {
                _harmony = new HarmonyLib.Harmony(MOD_GUID);
                try { _harmony.PatchAll(Assembly.GetExecutingAssembly()); }
                catch (System.Exception e) { LogError("Harmony-Patches fehlgeschlagen: " + e); }
            }
            Logger.LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
        }

        protected override void OnUpdate()
        {
#if DEBUG
            if (Input.GetKeyDown(KeyCode.F8)) Tuning.Reload();
#endif
        }

        protected override void OnPostActivate(KitchenMods.Mod mod)
        {
            Logger = InitLogger();
#if DEBUG
            Tuning.Load();
            Tuning.RegisterGlobal(Materials.RefreshColors);
#endif

            // Reihenfolge: Ergebnisse vor den Dingen, die auf sie verweisen.
            // Laborbedarf
            // AddGameDataObject<CrystalPurifier>(); // ersetzt durch Umkristallisieren
            AddGameDataObject<LabReactor>();
            // AddGameDataObject<GlassReactor>(); // Vergleichsvariante, Ofen-Reactor gewählt
            AddGameDataObject<ReactProcess>();
            AddGameDataObject<ReactCoolProcess>();
            AddGameDataObject<FlaskProvider>();
            AddGameDataObject<LabFlask>();
            // AddGameDataObject<BigFlaskProvider>(); // Superlab vorerst raus (verwirrte eher)
            // AddGameDataObject<BigLabFlask>(); // Superlab vorerst raus (verwirrte eher)
            AddGameDataObject<StickProvider>();
            AddGameDataObject<CandyStick>();
            AddGameDataObject<SteviaProvider>();
            AddGameDataObject<Stevia>();
            AddGameDataObject<SchraderTap>();
            AddGameDataObject<Schraderbraeu>();
            AddGameDataObject<BagProvider>();
            AddGameDataObject<EmptyBag>();

            // Produkte
            AddGameDataObject<BlueRockCandy>();
            AddGameDataObject<PureBlueRockCandy>();
            AddGameDataObject<SugarFreeCandy>();
            AddGameDataObject<RockCandyLolli>();
            AddGameDataObject<CandyBag>();
            AddGameDataObject<PureCandyBag>();
            AddGameDataObject<SugarFreeBag>();

            // Kochkette
            AddGameDataObject<ShardsPot>();
            AddGameDataObject<PureShardsPot>();
            AddGameDataObject<CrystalPot>();
            AddGameDataObject<PureCrystalPot>();
            AddGameDataObject<PureSyrupPot>();
            AddGameDataObject<RecrystallisePot>();
            AddGameDataObject<SyrupPot>();
            AddGameDataObject<SugarPot>();
            // AddGameDataObject<BigShardsPot>(); // Superlab vorerst raus (verwirrte eher)
            // AddGameDataObject<BigCrystalPot>(); // Superlab vorerst raus (verwirrte eher)
            // AddGameDataObject<BigSyrupPot>(); // Superlab vorerst raus (verwirrte eher)
            // AddGameDataObject<BigSugarPot>(); // Superlab vorerst raus (verwirrte eher)
            AddGameDataObject<SteviaShardsPot>();
            AddGameDataObject<SteviaCrystalPot>();
            AddGameDataObject<SteviaSyrupPot>();
            AddGameDataObject<SteviaPot>();
            AddGameDataObject<LolliPot>();
            AddGameDataObject<StickSyrupPot>();

            // Karten
            AddGameDataObject<BlueRockCandyDish>();
            AddGameDataObject<PureCandyDish>();
            AddGameDataObject<ChiliPDish>();
            AddGameDataObject<SugarFreeDish>();
            AddGameDataObject<LolliDish>();
            AddGameDataObject<PlatterDish>();
            // AddGameDataObject<SuperlabDish>(); // Superlab vorerst raus (verwirrte eher)
            AddGameDataObject<SchraderDish>();
            // AddGameDataObject<PizzaRoofDish>(); // Pizza vorerst raus (später evtl.)
            AddGameDataObject<BreakingEvenCard>();

            // Deko und Kostüm
            AddGameDataObject<PeriodicPoster>();
            AddGameDataObject<YellowBarrels>();
            AddGameDataObject<LosPollitosSign>();
            // AddGameDataObject<HazmatSuit>(); // Kostüme vorerst raus (macht Till später selbst)
            // AddGameDataObject<HeisenbergHat>(); // Kostüme vorerst raus (macht Till später selbst)
            // AddGameDataObject<GasMask>(); // Kostüme vorerst raus (macht Till später selbst)

            Events.BuildGameDataPreSetupEvent += (s, args) => PrefabUtil.FixMaterials();
            Events.BuildGameDataEvent += (s, args) =>
            {
                try { EasterEgg.Apply(args.gamedata); }
                catch (System.Exception e) { LogWarning("Easter Egg: " + e.Message); }
            };
#if DEBUG
            // Nach dem Aufbau aller Prefabs: Datei mit allen Schlüsseln anlegen, falls noch keine da ist.
            Events.BuildGameDataEvent += (s, args) => Tuning.WriteDefaultsIfMissing();
#endif
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
