using System.Collections.Generic;
using HarmonyLib;
using Kitchen;
using KitchenLib.Utils;

namespace BreakingPlates.Custom
{
    // 99,1 % ist nur besser, nie schlechter: Wer normale Ware bestellt hat, nimmt auch reine.
    // (Tütchen: gleiche Zutaten bis auf rein statt normal, z.B. Chili P bleibt Pflicht. Ganzer Kolben: rein statt normal.)
    [HarmonyPatch(typeof(OrderingHelpers), nameof(OrderingHelpers.IsRequestSatisfied),
        new[] { typeof(CItem), typeof(CItem), typeof(int), typeof(CSplittableItem) },
        new[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal })]
    public static class PureSatisfiesNormal
    {
        private static int Id<T>() where T : KitchenLib.Customs.CustomGameDataObject
            => GDOUtils.GetCustomGameDataObject<T>()?.GameDataObject?.ID ?? 0;

        public static void Postfix(CItem requested, CItem candidate, ref bool __result)
        {
            if (__result) return;
            try
            {
                if (requested.ID == Id<ShardsPot>() && candidate.ID == Id<PureShardsPot>()) { __result = true; return; }
                if (requested.ID != Id<CandyBag>() || candidate.ID != Id<PureCandyBag>()) return;

                int normal = Id<BlueRockCandy>(), pure = Id<PureBlueRockCandy>();
                var want = new List<int>();
                for (int i = 0; i < requested.Items.Count; i++) want.Add(requested.Items[i]);
                for (int i = 0; i < candidate.Items.Count; i++)
                {
                    int c = candidate.Items[i];
                    if (c == pure) c = normal;
                    if (!want.Remove(c)) return;
                }
                if (want.Count == 0) __result = true;
            }
            catch { }
        }
    }
}

namespace BreakingPlates.Custom
{
    // Reine Ware zuerst an den Gast, der rein bestellt hat: Hat am selben Tisch noch jemand eine offene
    // 99,1 %-Bestellung, die genau passt, nimmt der Gast mit der normalen Bestellung die reine Ware nicht.
    [HarmonyPatch(typeof(OrderingHelpers), nameof(OrderingHelpers.IsRequestSatisfied),
        new[] { typeof(EntityContext), typeof(Unity.Entities.Entity), typeof(Unity.Entities.Entity), typeof(int) },
        new[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out })]
    public static class PureGoesToPureFirst
    {
        private static int Id<T>() where T : KitchenLib.Customs.CustomGameDataObject
            => GDOUtils.GetCustomGameDataObject<T>()?.GameDataObject?.ID ?? 0;

        public static void Postfix(EntityContext ctx, Unity.Entities.Entity request, Unity.Entities.Entity candidate, ref bool __result)
        {
            if (!__result) return;
            try
            {
                var cand = ctx.GetOrDefault<CItem>(candidate);
                var req = ctx.GetOrDefault<CItem>(request);
                bool pureCand = cand.ID == Id<PureShardsPot>() || cand.ID == Id<PureCandyBag>();
                if (!pureCand || req.ID == cand.ID) return; // nur wenn reine Ware gerade eine normale Bestellung erfüllen würde

                var group = ctx.GetOrDefault<CRequestItemOf>(request).Group;
                var em = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;
                if (group == default || !em.HasComponent<CWaitingForItem>(group)) return;
                var split = ctx.GetOrDefault<CSplittableItem>(candidate);
                foreach (var w in ctx.GetBuffer<CWaitingForItem>(group))
                {
                    if (w.Satisfied || w.Item == request || w.ItemID != cand.ID) continue;
                    // Passt die reine Bestellung exakt? (gleiches Item, gleiche Zutaten)
                    if (OrderingHelpers.IsRequestSatisfied(ctx.GetOrDefault<CItem>(w.Item), cand, out _, split))
                    {
                        __result = false;
                        return;
                    }
                }
            }
            catch { }
        }
    }
}
