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
