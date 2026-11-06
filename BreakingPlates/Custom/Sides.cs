using System.Collections.Generic;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using UnityEngine;

namespace BreakingPlates.Custom
{
    // Beilagen: passen wie Schraderbräu zu jedem Hauptgericht.

    // ---------- Birthday Bacon ----------
    // Nutzt den Speck aus IngredientLib: Schwein aus dem Kühlschrank -> schneiden -> braten -> Bacon.
    public class BaconDish : CandyCardBase
    {
        private static Item ILib<T>() where T : CustomGameDataObject => (Item)GDOUtils.GetCustomGameDataObject<T>().GameDataObject;
        public override string UniqueNameID => "Birthday Bacon Side";
        public override DishType Type => DishType.Side;
        public override int Difficulty => 2;
        public override HashSet<Item> MinimumIngredients => new HashSet<Item> { ILib<IngredientLib.Ingredient.Items.Pork>() };
        public override HashSet<Process> RequiredProcesses => new HashSet<Process> { G.P(ProcessReferences.Chop), G.P(ProcessReferences.Cook) };
        public override List<Dish.MenuItem> ResultingMenuItems => new List<Dish.MenuItem> { Menu(ILib<IngredientLib.Ingredient.Items.Bacon>(), 1f, MenuPhase.Side) };
        private GameObject _icon;
        public override GameObject IconPrefab => _icon ??= Icon("Birthday Bacon - Icon", t =>
            PrefabUtil.CloneCustomItem(GDOUtils.GetCustomGameDataObject<IngredientLib.Ingredient.Items.Bacon>(), t, "Bacon", Vector3.zero, 1.4f));
        protected override string CardName => "Birthday Bacon";
        protected override string CardDescription => "Adds Bacon as a side";
        protected override string Quote => "Fifty-two.";
        protected override string RecipeText => "Take pork from the fridge, chop it and fry it into bacon. Serve it with the order.";
    }
}
