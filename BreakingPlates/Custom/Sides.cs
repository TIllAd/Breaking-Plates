using System.Collections.Generic;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using UnityEngine;

namespace BreakingPlates.Custom
{
    // Pilot: neue Beilagen (06.11.). Wie Schraderbräu: passen zu jedem Hauptgericht.

    // ---------- Funions (Jesses Snack) ----------
    public class Funions : CustomItem
    {
        public override string UniqueNameID => "Funions";
        public override ItemStorage ItemStorageFlags => ItemStorage.StackableFood;
        public override ItemValue ItemValue => ItemValue.SideSmall;
        public override bool IsMergeableSide => true;
        public override string ColourBlindTag => "Fu";
        public override Appliance DedicatedProvider => (Appliance)GDOUtils.GetCustomGameDataObject<FunionsRack>().GameDataObject;
        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= Make();
        internal static GameObject Make(Transform parent = null)
        {
            var root = parent == null ? PrefabUtil.CreateRoot("Funions") : new GameObject("Funions");
            if (parent != null) root.transform.SetParent(parent, false);
            var bagMat = Materials.Solid("Funions Bag", new Color(0.98f, 0.82f, 0.15f));
            var stripe = Materials.Solid("Funions Green", new Color(0.3f, 0.65f, 0.2f));
            var ring = Materials.Solid("Onion Ring", new Color(0.93f, 0.78f, 0.45f));
            var bag = ProcMesh.Create("Bag", root.transform, ProcMesh.Pouch(0.26f, 0.3f, 0.07f), bagMat, Vector3.zero);
            bag.transform.localRotation = Quaternion.Euler(0f, 10f, 0f);
            var label = new GameObject("Label");
            label.transform.SetParent(bag.transform, false);
            label.transform.localPosition = new Vector3(0f, 0.13f, -0.072f);
            ProcMesh.Create("Stripe", label.transform, ProcMesh.Box(new Vector3(0.22f, 0.06f, 0.006f)), stripe, Vector3.zero);
            DecoBase.MakeText(label.transform, "Name", "FUNIONS", new Vector3(0f, 0f, -0.005f), 0.38f, Color.white);
            // Zwiebelringe gucken oben raus
            var ringMesh = ProcMesh.Lathe(new[] { new Vector2(0.018f, 0f), new Vector2(0.03f, 0f), new Vector2(0.03f, 0.008f), new Vector2(0.018f, 0.008f), new Vector2(0.018f, 0f) }, 14);
            for (int i = 0; i < 3; i++)
            {
                var r = ProcMesh.Create("Ring", root.transform, ringMesh, ring, new Vector3(-0.05f + i * 0.05f, 0.29f + (i % 2) * 0.01f, 0f));
                r.transform.localRotation = Quaternion.Euler(70f + i * 10f, i * 30f, 0f);
            }
            return root;
        }
    }

    public class FunionsRack : ProviderBase
    {
        public override string UniqueNameID => "Funions Rack";
        protected override Item Provided => G.I<Funions>();
        protected override string Title => "Funions Rack";
        protected override string Desc => "Breakfast of champions.";
    }

    public class FunionsDish : CandyCardBase
    {
        public override string UniqueNameID => "Funions Side";
        public override DishType Type => DishType.Side;
        public override int Difficulty => 1;
        public override HashSet<Item> MinimumIngredients => new HashSet<Item> { G.I<Funions>() };
        public override List<Dish.MenuItem> ResultingMenuItems => new List<Dish.MenuItem> { Menu(G.I<Funions>(), 1f, MenuPhase.Side) };
        private GameObject _icon;
        public override GameObject IconPrefab => _icon ??= Icon("Funions - Icon", t => Funions.Make(t));
        protected override string CardName => "Funions";
        protected override string CardDescription => "Adds Funions as a side";
        protected override string Quote => "Bitch, I'm eating Funions.";
        protected override string RecipeText => "Grab a bag of Funions from the rack and serve it with the order.";
    }

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
