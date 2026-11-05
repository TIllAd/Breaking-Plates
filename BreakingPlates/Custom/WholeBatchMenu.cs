using System.Collections.Generic;
using Kitchen;
using KitchenData;
using KitchenLib.Utils;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;

namespace BreakingPlates.Custom
{
    // "Mass Production": Mit der Karte bestellen Gruppen den ganzen Kolben – und zwar in jeder Variante,
    // deren Karte man auch hat (99,1 % -> reiner Kolben, Sugar-Free -> Stevia-Kolben).
    // Das Spiel kennt keine "Karte A + Karte B"-Bestellungen, daher legt dieses System die Bestellung selbst an.
    public class WholeBatchMenuSystem : RestaurantSystem, IModSystem
    {
        private EntityQuery _menu;

        protected override void Initialise()
        {
            base.Initialise();
            _menu = GetEntityQuery(typeof(CMenuItem));
        }

        protected override void OnUpdate()
        {
            using var items = _menu.ToComponentDataArray<CMenuItem>(Allocator.Temp);
            if (items.Length == 0) return;

            int batch = Id<PlatterDish>();
            var dishes = new HashSet<int>();
            var present = new HashSet<int>();
            float batchWeight = 0f;
            foreach (var m in items)
            {
                dishes.Add(m.SourceDish);
                present.Add(m.Item);
                if (m.SourceDish == batch) batchWeight = m.Weight;
            }
            if (!dishes.Contains(batch)) return;

            Add(dishes, present, Id<PureCandyDish>(), ItemId<PureShardsPot>(), batchWeight);
            Add(dishes, present, Id<SugarFreeDish>(), ItemId<SteviaShardsPot>(), batchWeight);
        }

        private void Add(HashSet<int> dishes, HashSet<int> present, int variantDish, int flask, float weight)
        {
            if (variantDish == 0 || flask == 0 || !dishes.Contains(variantDish) || present.Contains(flask)) return;
            var e = EntityManager.CreateEntity(typeof(CMenuItem), typeof(CAvailableIngredient));
            EntityManager.AddComponentData(e, new CMenuItem
            {
                Item = flask, Weight = weight > 0 ? weight : 0.5f, Phase = MenuPhase.Main, SourceDish = variantDish
            });
            EntityManager.AddComponent<CMenuItemMain>(e);
            present.Add(flask);
            Mod.LogInfo("Mass Production: ganze Kolben jetzt auch als Variante bestellbar (" + flask + ")");
        }

        private static int Id<T>() where T : KitchenLib.Customs.CustomGameDataObject
            => GDOUtils.GetCustomGameDataObject<T>()?.GameDataObject?.ID ?? 0;
        private static int ItemId<T>() where T : KitchenLib.Customs.CustomGameDataObject => Id<T>();
    }
}
