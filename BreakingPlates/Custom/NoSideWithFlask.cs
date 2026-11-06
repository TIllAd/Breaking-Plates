using System.Collections.Generic;
using HarmonyLib;
using Kitchen;
using KitchenData;
using KitchenLib.Utils;
using Unity.Entities;

namespace BreakingPlates.Custom
{
    // Wer einen ganzen Kolben (Mass Production) oder einen Lolli bestellt, bestellt keine Beilage dazu.
    // Ein Kolben wird vom ganzen Tisch geteilt und hat keinen Platz für Beilagen – das wäre nur verwirrend.
    [HarmonyPatch(typeof(AssignMenuRequests), nameof(AssignMenuRequests.OrderItem))]
    public static class NoSideWithFlask
    {
        private static readonly HashSet<(Entity, int)> FlaskOrders = new HashSet<(Entity, int)>();
        private static HashSet<int> _ids;
        private static HashSet<int> Ids
        {
            get
            {
                if (_ids == null || _ids.Count == 0)
                {
                    _ids = new HashSet<int>();
                    foreach (var id in new[] { Id<ShardsPot>(), Id<PureShardsPot>(), Id<SteviaShardsPot>(), Id<RockCandyLolli>() })
                        if (id != 0) _ids.Add(id);
                }
                return _ids;
            }
        }
        private static int Id<T>() where T : KitchenLib.Customs.CustomGameDataObject
            => GDOUtils.GetCustomGameDataObject<T>()?.GameDataObject?.ID ?? 0;

        public static bool Prefix(Item item_data, EntityContext ctx, Entity group, int member_index, MenuPhase phase)
        {
            try
            {
                if (item_data != null && phase != MenuPhase.Side && Ids.Contains(item_data.ID))
                {
                    if (FlaskOrders.Count > 512) FlaskOrders.Clear();
                    FlaskOrders.Add((group, member_index));
                    return true;
                }
                if (phase != MenuPhase.Side) return true;
                if (FlaskOrders.Contains((group, member_index))) return false;

                // Falls die Hauptbestellung schon früher angelegt wurde: in der Warteliste der Gruppe nachsehen.
                var em = World.DefaultGameObjectInjectionWorld?.EntityManager;
                if (em.HasValue && em.Value.HasComponent<CWaitingForItem>(group))
                {
                    foreach (var w in em.Value.GetBuffer<CWaitingForItem>(group))
                        if (w.MemberIndex == member_index && !w.IsSide && Ids.Contains(w.ItemID)) return false;
                }
            }
            catch { }
            return true;
        }
    }
}
