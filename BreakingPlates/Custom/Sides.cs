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

    // ---------- Birthday Bacon (52) ----------
    internal static class BaconVisual
    {
        // Sieben-Segment-Ziffern aus Speckstreifen, flach liegend (Ziffer zeigt nach oben).
        private static readonly Dictionary<char, string> Segs = new Dictionary<char, string> { { '5', "afgcd" }, { '2', "abged" } };
        public static void Digits(Transform parent, string digits, Material meat, Material fat)
        {
            const float L = 0.13f, W = 0.016f, H = 0.01f, gap = 0.21f;
            float x0 = -(digits.Length - 1) * gap / 2f;
            for (int d = 0; d < digits.Length; d++)
            {
                float cx = x0 + d * gap;
                foreach (char s in Segs[digits[d]])
                {
                    Vector3 pos; bool vertical;
                    switch (s)
                    {
                        case 'a': pos = new Vector3(cx, 0f, L); vertical = false; break;
                        case 'g': pos = new Vector3(cx, 0f, 0f); vertical = false; break;
                        case 'd': pos = new Vector3(cx, 0f, -L); vertical = false; break;
                        case 'f': pos = new Vector3(cx - L / 2f, 0f, L / 2f); vertical = true; break;
                        case 'b': pos = new Vector3(cx + L / 2f, 0f, L / 2f); vertical = true; break;
                        case 'e': pos = new Vector3(cx - L / 2f, 0f, -L / 2f); vertical = true; break;
                        default: pos = new Vector3(cx + L / 2f, 0f, -L / 2f); vertical = true; break; // c
                    }
                    var strip = new GameObject("Strip " + s);
                    strip.transform.SetParent(parent, false);
                    strip.transform.localPosition = pos + new Vector3(0f, H / 2f, 0f);
                    // Ohne Drehung: senkrechte Streifen bekommen einfach vertauschte Maße.
                    var meatSize = vertical ? new Vector3(W, H, L * 0.86f) : new Vector3(L * 0.86f, H, W);
                    var fatSize = vertical ? new Vector3(W * 0.3f, H + 0.002f, L * 0.86f) : new Vector3(L * 0.86f, H + 0.002f, W * 0.3f);
                    var fatPos = vertical ? new Vector3(W * 0.25f, 0f, 0f) : new Vector3(0f, 0f, W * 0.25f);
                    ProcMesh.Create("Meat", strip.transform, ProcMesh.Box(meatSize), meat, Vector3.zero);
                    ProcMesh.Create("Fat", strip.transform, ProcMesh.Box(fatSize), fat, fatPos);
                }
            }
        }
    }

    public class RawBacon : CustomItem
    {
        public override string UniqueNameID => "Raw Bacon";
        public override ItemStorage ItemStorageFlags => ItemStorage.StackableFood;
        public override string ColourBlindTag => "Ba";
        public override Appliance DedicatedProvider => (Appliance)GDOUtils.GetCustomGameDataObject<BaconProvider>().GameDataObject;
        public override List<Item.ItemProcess> Processes => new List<Item.ItemProcess>
        {
            new Item.ItemProcess { Process = G.P(ProcessReferences.Cook), Duration = 4f, Result = G.I<BirthdayBacon>() }
        };
        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= Make();
        private static GameObject Make()
        {
            var root = PrefabUtil.CreateRoot("Raw Bacon");
            var meat = Materials.Solid("Raw Bacon", new Color(0.92f, 0.45f, 0.48f));
            var fat = Materials.Solid("Raw Bacon Fat", new Color(0.99f, 0.9f, 0.88f));
            for (int i = 0; i < 4; i++)
            {
                var s = new GameObject("Strip");
                s.transform.SetParent(root.transform, false);
                s.transform.localPosition = new Vector3(0f, 0.01f + i * 0.012f, -0.04f + i * 0.025f);
                s.transform.localRotation = Quaternion.Euler(0f, i * 6f - 9f, 0f);
                ProcMesh.Create("Meat", s.transform, ProcMesh.Box(new Vector3(0.26f, 0.01f, 0.06f)), meat, Vector3.zero);
                ProcMesh.Create("Fat", s.transform, ProcMesh.Box(new Vector3(0.26f, 0.012f, 0.018f)), fat, new Vector3(0f, 0f, 0.015f));
            }
            return root;
        }
    }

    public class BirthdayBacon : CustomItem
    {
        public override string UniqueNameID => "Birthday Bacon";
        public override ItemStorage ItemStorageFlags => ItemStorage.StackableFood;
        public override ItemValue ItemValue => ItemValue.SideMedium;
        public override bool IsMergeableSide => true;
        public override string ColourBlindTag => "52";
        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= Make();
        internal static GameObject Make(Transform parent = null)
        {
            var root = parent == null ? PrefabUtil.CreateRoot("Birthday Bacon") : new GameObject("Birthday Bacon");
            if (parent != null) root.transform.SetParent(parent, false);
            var meat = Materials.Solid("Cooked Bacon", new Color(0.62f, 0.25f, 0.15f));
            var fat = Materials.Solid("Cooked Bacon Fat", new Color(0.93f, 0.75f, 0.5f));
            // Kleiner Teller darunter, damit die "52" lesbar ist.
            var plate = ProcMesh.Create("Plate", root.transform, ProcMesh.Cylinder(0.24f, 0.012f, 24), Materials.LabelWhite, Vector3.zero);
            plate.transform.localScale = new Vector3(1.2f, 1f, 1f);
            var digits = new GameObject("52");
            digits.transform.SetParent(root.transform, false);
            digits.transform.localPosition = new Vector3(0f, 0.012f, 0f);
            BaconVisual.Digits(digits.transform, "52", meat, fat);
            return root;
        }
    }

    public class BaconProvider : ProviderBase
    {
        public override string UniqueNameID => "Bacon Provider";
        protected override Item Provided => G.I<RawBacon>();
        protected override string Title => "Bacon";
        protected override string Desc => "Fifty-two. Happy birthday.";
    }

    public class BaconDish : CandyCardBase
    {
        public override string UniqueNameID => "Birthday Bacon Side";
        public override DishType Type => DishType.Side;
        public override int Difficulty => 2;
        public override HashSet<Item> MinimumIngredients => new HashSet<Item> { G.I<RawBacon>() };
        public override HashSet<Process> RequiredProcesses => new HashSet<Process> { G.P(ProcessReferences.Cook) };
        public override List<Dish.MenuItem> ResultingMenuItems => new List<Dish.MenuItem> { Menu(G.I<BirthdayBacon>(), 1f, MenuPhase.Side) };
        private GameObject _icon;
        public override GameObject IconPrefab => _icon ??= Icon("Birthday Bacon - Icon", t => BirthdayBacon.Make(t));
        protected override string CardName => "Birthday Bacon";
        protected override string CardDescription => "Adds Birthday Bacon as a side";
        protected override string Quote => "Fifty-two.";
        protected override string RecipeText => "Cook the bacon on the hob. It comes out as a 52 – serve it with the order.";
    }
}
