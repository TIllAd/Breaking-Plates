using System.Collections.Generic;
using System.Linq;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using UnityEngine;
using static BreakingPlates.Custom.Visuals;

namespace BreakingPlates.Custom
{
    // Gemeinsame Einstellungen für die Karten zu Blue Rock Candy.
    public abstract class CandyCardBase : CustomDish
    {
        public override CardType CardType => CardType.Default;
        public override UnlockGroup UnlockGroup => UnlockGroup.Dish;
        public override DishCustomerChange CustomerMultiplier => DishCustomerChange.SmallDecrease;
        public override List<Unlock> HardcodedRequirements => new List<Unlock>
        {
            (Dish)GDOUtils.GetCustomGameDataObject<BlueRockCandyDish>().GameDataObject
        };

        protected abstract string CardName { get; }
        protected abstract string CardDescription { get; }
        protected abstract string Quote { get; }
        protected abstract string RecipeText { get; }

        public override Dictionary<Locale, string> Recipe => new Dictionary<Locale, string> { { Locale.English, RecipeText } };
        public override List<(Locale, UnlockInfo)> InfoList => new List<(Locale, UnlockInfo)>
        {
            (Locale.English, new UnlockInfo { Name = CardName, Description = CardDescription, FlavourText = Quote })
        };

        protected static Dish.MenuItem Menu(Item item, float weight, MenuPhase phase = MenuPhase.Main) =>
            new Dish.MenuItem { Item = item, Phase = phase, Weight = weight, DynamicMenuType = DynamicMenuType.Static };

        protected static GameObject Icon(string name, System.Action<Transform> build)
        {
            var root = PrefabUtil.CreateRoot(name);
            build(root.transform);
            return root;
        }
    }

    public class BlueRockCandyDish : CustomDish
    {
        public override string UniqueNameID => "Blue Rock Candy Dish";
        public override DishType Type => DishType.Base;
        public override CardType CardType => CardType.Default;
        public override UnlockGroup UnlockGroup => UnlockGroup.Dish;
        public override int Difficulty => 3;
        public override bool IsAvailableAsLobbyOption => true;
        public override bool IsMainThatDoesNotNeedPlates => true;
        public override DishCustomerChange CustomerMultiplier => DishCustomerChange.SmallDecrease;

        public override List<string> StartingNameSet => new List<string>
        {
            "Say My Name", "Breaking Plates", "Blue Sky Sweets", "The One Who Cooks",
            "Better Call Sugar", "Los Pollitos Hermanos", "99.1% Sugar", "Tread Lightly",
            "Gray Matter Candy Co.", "Yeah, Science!", "Sweet Chemistry", "The Crystal Ship", "Secret Lab", "Underground Kitchen", "Underground Lab", "Just a Laundry",
            "Respect The Chemistry", "Cook Responsibly", "Blue Magic", "Sugar Rush Labs"
        };

        public override HashSet<Item> MinimumIngredients => new HashSet<Item>
        {
            Ref(ItemReferences.Sugar), Ref(ItemReferences.Water), G.I<LabFlask>(), G.I<EmptyBag>()
        };
        public override HashSet<Process> RequiredProcesses => new HashSet<Process>
        {
            G.P(ProcessReferences.Cook), G.P(ProcessReferences.Knead)
        };

        public override List<Dish.MenuItem> ResultingMenuItems => new List<Dish.MenuItem>
        {
            new Dish.MenuItem { Item = G.I<CandyBag>(), Phase = MenuPhase.Main, Weight = 1, DynamicMenuType = DynamicMenuType.Static }
        };

        private GameObject _icon, _display;
        public override GameObject IconPrefab => _icon ??= MakeCandyDisplay("Blue Rock Candy - Icon");
        public override GameObject DisplayPrefab => _display ??= MakeFlaskDisplay("Blue Rock Candy - Display");

        private static GameObject MakeFlaskDisplay(string name)
        {
            var root = PrefabUtil.CreateRoot(name);
            CrystalPile(root.transform, "Candy", new Vector3(0.18f, 0.05f, 0f), 1f, 5);
            Flask(root.transform, "Flask", new Vector3(-0.15f, 0f, 0f), 1f, Materials.BlueSyrup);
            return root;
        }

        private static GameObject MakeCandyDisplay(string name)
        {
            var root = PrefabUtil.CreateRoot(name);
            CrystalPile(root.transform, "Candy", new Vector3(0f, 0.05f, 0f), 1.2f, 7);
            return root;
        }

        public override Dictionary<Locale, string> Recipe => new Dictionary<Locale, string>
        {
            { Locale.English, "Cook sugar and water in a lab flask. Take the blue syrup off the heat and let it crystallise (a freezer speeds this up), then smash it by hand on a counter. Put a portion in a candy bag and serve - no plate needed. Respect the chemistry." }
        };
        public override List<(Locale, UnlockInfo)> InfoList => new List<(Locale, UnlockInfo)>
        {
            (Locale.English, new UnlockInfo { Name = "Blue Rock Candy", Description = "Adds Blue Rock Candy as a main", FlavourText = "We need to cook." })
        };
    }

    // 99,1 %: Kristalle im Crystal Purifier reinigen -> reine Kristalle.
    public class PureCandyDish : CandyCardBase
    {
        public override string UniqueNameID => "Pure Blue Rock Candy Dish";
        public override DishType Type => DishType.Main;
        public override bool IsMainThatDoesNotNeedPlates => true;
        public override int Difficulty => 2;
        public override HashSet<Process> RequiredProcesses => new HashSet<Process>
        {
            G.P(ProcessReferences.Cook), G.P(ProcessReferences.Knead)
        };
        public override List<Dish.MenuItem> ResultingMenuItems => new List<Dish.MenuItem> { Menu(G.I<PureCandyBag>(), 0.75f) };
        private GameObject _icon;
        public override GameObject IconPrefab => _icon ??= Icon("Pure Candy - Icon", t => PureCrystal(t, "Crystal", new Vector3(0f, 0.12f, 0f), 1.3f));
        protected override string CardName => "99.1% Purity";
        protected override string CardDescription => "Some customers only accept the purest batch: recrystallise your candy for 99.1%.";
        protected override string Quote => "A higher purity means a greater yield.";
        protected override string RecipeText => "Recrystallise: add water to a flask of smashed candy, cook it again and let it cool. The crystals come out pure (99). Smash and bag as usual. Pure candy also works for normal orders.";
    }

    // Chili P: Jalapeño schneiden und ins Tütchen -> roter Verschluss.
    public class ChiliPDish : CandyCardBase
    {
        public override string UniqueNameID => "Chili P Dish";
        public override DishType Type => DishType.Extra;
        public override int Difficulty => 1;
        public override HashSet<Item> MinimumIngredients => new HashSet<Item> { G.I<IngredientLib.Ingredient.Items.Jalapeno>() };
        public override HashSet<Process> RequiredProcesses => new HashSet<Process> { G.P(ProcessReferences.Chop) };
        public override HashSet<Dish.IngredientUnlock> IngredientsUnlocks
        {
            get
            {
                var chili = FilledBagBase.ChiliP;
                return new HashSet<Dish.IngredientUnlock>
                {
                    new Dish.IngredientUnlock { MenuItem = (ItemGroup)G.I<CandyBag>(), Ingredient = chili },
                    new Dish.IngredientUnlock { MenuItem = (ItemGroup)G.I<PureCandyBag>(), Ingredient = chili },
                    new Dish.IngredientUnlock { MenuItem = (ItemGroup)G.I<SugarFreeBag>(), Ingredient = chili },
                };
            }
        }
        private GameObject _icon;
        public override GameObject IconPrefab => _icon ??= Icon("Chili P - Icon", t =>
            PrefabUtil.CloneCustomItem(GDOUtils.GetCustomGameDataObject<IngredientLib.Ingredient.Items.ChoppedJalapeno>(), t, "Chili", Vector3.zero, 1f));
        protected override string CardName => "Chili P";
        protected override string CardDescription => "Customers may want a signature touch of chili in their bag";
        protected override string Quote => "Chili P is my signature!";
        protected override string RecipeText => "Chop a jalapeño and add it to the candy bag.";
    }

    // Sugar-Free: Stevia statt Zucker, grüner Verschluss.
    public class SugarFreeDish : CandyCardBase
    {
        public override string UniqueNameID => "Sugar-Free Rock Candy Dish";
        public override DishType Type => DishType.Main;
        public override bool IsMainThatDoesNotNeedPlates => true;
        public override int Difficulty => 2;
        public override HashSet<Item> MinimumIngredients => new HashSet<Item> { G.I<Stevia>() };
        public override List<Dish.MenuItem> ResultingMenuItems => new List<Dish.MenuItem> { Menu(G.I<SugarFreeBag>(), 0.75f) };
        private GameObject _icon;
        public override GameObject IconPrefab => _icon ??= Icon("Sugar-Free - Icon", t => SteviaHeap(t, "Stevia", Vector3.zero));
        protected override string CardName => "Sugar-Free";
        protected override string CardDescription => "Some customers want their candy made with stevia (green zip)";
        protected override string Quote => "I'll need more stevia.";
        protected override string RecipeText => "Cook stevia and water instead of sugar. Everything else stays the same - the bag gets a green zip.";
    }

    // Am Stiel: Stiel in den Sirup, bevor er fest wird -> Kristall-Lollis.
    public class LolliDish : CandyCardBase
    {
        public override string UniqueNameID => "Rock Candy Lolli Dish";
        public override DishType Type => DishType.Main;
        public override bool IsMainThatDoesNotNeedPlates => true;
        public override int Difficulty => 2;
        public override HashSet<Item> MinimumIngredients => new HashSet<Item> { G.I<CandyStick>() };
        public override List<Dish.MenuItem> ResultingMenuItems => new List<Dish.MenuItem> { Menu(G.I<RockCandyLolli>(), 0.75f) };
        private GameObject _icon;
        public override GameObject IconPrefab => _icon ??= Icon("Lolli - Icon", t => Lolli(t, "Lolli", new Vector3(0f, -0.1f, 0f), 1.2f));
        protected override string CardName => "On a Stick";
        protected override string CardDescription => "Customers may order rock candy on a stick";
        protected override string Quote => "Yeah, science!";
        protected override string RecipeText => "Put a candy stick into the hot blue syrup before it crystallises. Once it has set, take the lollis straight out of the flask - no smashing, no bag.";
    }

    // Ganze Platte: der volle Kolben für eine ganze Gruppe.
    public class PlatterDish : CandyCardBase
    {
        public override string UniqueNameID => "Rock Candy Platter Dish";
        public override DishType Type => DishType.Main;
        public override int Difficulty => 2;
        public override bool IsMainThatDoesNotNeedPlates => true;
        public override List<Dish.MenuItem> ResultingMenuItems => new List<Dish.MenuItem> { Menu(G.I<ShardsPot>(), 0.5f) };
        private GameObject _icon;
        public override GameObject IconPrefab => _icon ??= Icon("Platter - Icon", t =>
        {
            Flask(t, "Flask", Vector3.zero, 1f);
            FlaskCrystals(t.Find("Flask"), "Shards", 0.06f, 8);
        });
        protected override string CardName => "Mass Production";
        protected override string CardDescription => "Groups may order a whole flask of smashed candy to share - in every variant you sell";
        protected override string Quote => "I'm in the empire business.";
        protected override string RecipeText => "Serve a full flask of smashed candy (all 5 portions) to the table. Works for every variant you have (99.1%, sugar-free). The group shares it and leaves the empty flask behind.";
    }

    // Großer Kolben, 10 Portionen, längere Kochzeit, mehr Bestellungen.
    public class BigFlaskDish : CandyCardBase
    {
        public override string UniqueNameID => "Big Flask Dish";
        public override DishType Type => DishType.Main;
        public override int Difficulty => 2;
        public override DishCustomerChange CustomerMultiplier => DishCustomerChange.SmallIncrease;
        public override HashSet<Item> MinimumIngredients => new HashSet<Item> { G.I<BigLabFlask>() };
        public override List<Dish.MenuItem> ResultingMenuItems => new List<Dish.MenuItem> { Menu(G.I<CandyBag>(), 0.5f) };
        private GameObject _icon;
        public override GameObject IconPrefab => _icon ??= Icon("Big Flask - Icon", t => Flask(t, "Flask", Vector3.zero, BigFlaskScale, Materials.BlueSyrup));
        protected override string CardName => "Big Flask";
        protected override string CardDescription => "Big flasks: 10 portions per batch, but they cook and cool longer. More customers.";
        protected override string Quote => "I'm in the empire business.";
        protected override string RecipeText => "Use the big lab flask with sugar and water. It takes longer to cook and cool, but gives 10 portions.";
    }

    // Schraderbräu als Beilage.
    public class SchraderDish : CandyCardBase
    {
        public override string UniqueNameID => "Schraderbraeu Side";
        public override DishType Type => DishType.Side;
        public override int Difficulty => 1;
        public override HashSet<Item> MinimumIngredients => new HashSet<Item> { G.I<Schraderbraeu>() };
        public override List<Dish.MenuItem> ResultingMenuItems => new List<Dish.MenuItem> { Menu(G.I<Schraderbraeu>(), 1f, MenuPhase.Side) };
        private GameObject _icon;
        public override GameObject IconPrefab => _icon ??= Icon("Schraderbraeu - Icon", t => BeerMug(t, "Mug", Vector3.zero, 1.2f));
        protected override string CardName => "Schraderbräu";
        protected override string CardDescription => "Adds Schraderbräu as a side";
        protected override string Quote => "Jesus, Marie!";
        protected override string RecipeText => "Pour a Schraderbräu from the tap and serve it with the order.";
    }

    // Pizza als Extra (mit dem normalen Pizza-Ablauf aus dem Spiel).
    public class PizzaRoofDish : CandyCardBase
    {
        public override string UniqueNameID => "Pizza On The Roof";
        public override DishType Type => DishType.Extra;
        public override int Difficulty => 2;
        private static Dish VanillaPizza => GDOUtils.GetExistingGDO(DishReferences.PizzaBase) as Dish;
        private static Item PizzaItem
        {
            get
            {
                var d = VanillaPizza;
                var mi = d?.UnlocksMenuItems?.FirstOrDefault(m => m.Item != null);
                return mi?.Item ?? Ref(ItemReferences.PizzaSlice);
            }
        }
        public override HashSet<Item> MinimumIngredients => VanillaPizza?.MinimumIngredients != null ? new HashSet<Item>(VanillaPizza.MinimumIngredients) : new HashSet<Item>();
        public override HashSet<Process> RequiredProcesses => VanillaPizza?.RequiredProcesses != null ? new HashSet<Process>(VanillaPizza.RequiredProcesses) : new HashSet<Process>();
        public override HashSet<Dish.IngredientUnlock> ExtraOrderUnlocks => new HashSet<Dish.IngredientUnlock>
        {
            new Dish.IngredientUnlock { MenuItem = (ItemGroup)G.I<CandyBag>(), Ingredient = PizzaItem }
        };
        private GameObject _icon;
        public override GameObject IconPrefab => _icon ??= Icon("Pizza - Icon", t => PrefabUtil.CloneVisual(Ref(ItemReferences.PizzaSlice).Prefab, t, "Pizza", Vector3.zero));
        protected override string CardName => "Pizza on the Roof";
        protected override string CardDescription => "Customers may order a pizza with their candy";
        protected override string Quote => "Stay out of my territory.";
        protected override string RecipeText => "Make a pizza the usual way and serve it alongside the candy bag.";
    }

    // Effekt-Karte "Breaking Even": weniger Kunden (~30 %), dafür 25 % weniger Geld am Tagesende.
    public class BreakingEvenCard : CustomUnlockCard
    {
        public override string UniqueNameID => "Breaking Even";
        public override CardType CardType => CardType.Default;
        public override UnlockGroup UnlockGroup => UnlockGroup.Generic;
        public override bool IsUnlockable => true;
        public override List<Unlock> HardcodedRequirements => new List<Unlock>
        {
            (Dish)GDOUtils.GetCustomGameDataObject<BlueRockCandyDish>().GameDataObject
        };
        // Kundenabnahme über den normalen Karten-Mechanismus (zeigt den pinken Kunden-Tag auf der Karte).
        public override DishCustomerChange CustomerMultiplier => DishCustomerChange.LargeDecrease;
        public override List<UnlockEffect> Effects => new List<UnlockEffect>
        {
            new StatusEffect { Status = RestaurantStatus.HeatLessMoney },
        };
        public override List<(Locale, UnlockInfo)> InfoList => new List<(Locale, UnlockInfo)>
        {
            (Locale.English, new UnlockInfo
            {
                Name = "Breaking Even",
                Description = "Fewer customers. At the end of each day, 25% of that day's earnings are taken away",
                FlavourText = "Better call Saul!"
            })
        };
    }
}
