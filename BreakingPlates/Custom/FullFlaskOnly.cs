using System.Collections.Generic;
using HarmonyLib;
using Kitchen;
using KitchenLib.Utils;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;

namespace BreakingPlates.Custom
{
    // Ein angebrochener Kolben ist kein ganzer Kolben mehr:
    // 1) "Mass Production": nur ein voller Kolben erfüllt die Bestellung einer Gruppe.
    // 2) Umkristallisieren: Wasser lässt sich nur in einen vollen Kolben geben (sonst würden aus 4 Portionen 5 reine).
    internal static class FullFlask
    {
        private static HashSet<int> _ids;
        public static HashSet<int> Ids
        {
            get
            {
                if (_ids == null || _ids.Count == 0)
                {
                    _ids = new HashSet<int>();
                    Add<ShardsPot>(); Add<PureShardsPot>(); Add<SteviaShardsPot>(); Add<LolliPot>();
                }
                return _ids;
            }
        }
        private static void Add<T>() where T : KitchenLib.Customs.CustomGameDataObject
        {
            int id = GDOUtils.GetCustomGameDataObject<T>()?.GameDataObject?.ID ?? 0;
            if (id != 0) _ids.Add(id);
        }
        public static bool IsPartial(CSplittableItem s) => s.TotalCount > 0 && s.RemainingCount < s.TotalCount;
    }

    [HarmonyPatch(typeof(OrderingHelpers), nameof(OrderingHelpers.IsRequestSatisfied),
        new[] { typeof(CItem), typeof(CItem), typeof(int), typeof(CSplittableItem) },
        new[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal })]
    public static class FullFlaskForOrders
    {
        // Läuft nach PureSatisfiesNormal, damit auch "rein statt normal" geprüft wird.
        [HarmonyPriority(HarmonyLib.Priority.Low)]
        public static void Postfix(CItem candidate, CSplittableItem candidate_split, ref bool __result)
        {
            try
            {
                if (__result && FullFlask.Ids.Contains(candidate.ID) && FullFlask.IsPartial(candidate_split))
                    __result = false;
            }
            catch { }
        }
    }

    // Angebrochene Kolben: nichts mehr hineinmischen (Portionen entnehmen geht weiter).
    public class PartialFlaskNoMerge : GenericSystemBase, IModSystem
    {
        private EntityQuery _query;

        protected override void Initialise()
        {
            base.Initialise();
            _query = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(CItem), typeof(CSplittableItem) },
                None = new ComponentType[] { typeof(CPreventItemMerge) }
            });
        }

        protected override void OnUpdate()
        {
            using var entities = _query.ToEntityArray(Allocator.Temp);
            using var items = _query.ToComponentDataArray<CItem>(Allocator.Temp);
            using var splits = _query.ToComponentDataArray<CSplittableItem>(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                if (!FullFlask.Ids.Contains(items[i].ID) || !FullFlask.IsPartial(splits[i])) continue;
                EntityManager.AddComponentData(entities[i], new CPreventItemMerge { Condition = MergeCondition.NoMerge });
            }
        }
    }
}
