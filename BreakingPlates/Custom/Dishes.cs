using System.Collections.Generic;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using UnityEngine;
using static BreakingPlates.Custom.Visuals;

namespace BreakingPlates.Custom
{
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
            "Gray Matter Candy Co.", "Yeah, Science!", "Sweet Chemistry", "The Crystal Ship",
            "Respect The Chemistry", "Cook Responsibly", "Blue Magic", "Sugar Rush Labs"
        };

        public override HashSet<Item> MinimumIngredients => new HashSet<Item>
        {
            Ref(ItemReferences.Sugar), Ref(ItemReferences.Water), Ref(ItemReferences.Pot),
            (Item)GDOUtils.GetCustomGameDataObject<EmptyBag>().GameDataObject
        };
        public override HashSet<Process> RequiredProcesses => new HashSet<Process>
        {
            (Process)GDOUtils.GetExistingGDO(ProcessReferences.Cook),
            (Process)GDOUtils.GetExistingGDO(ProcessReferences.Knead)
        };

        public override List<Dish.MenuItem> ResultingMenuItems => new List<Dish.MenuItem>
        {
            new Dish.MenuItem
            {
                Item = (Item)GDOUtils.GetCustomGameDataObject<CandyBag>().GameDataObject,
                Phase = MenuPhase.Main, Weight = 1, DynamicMenuType = DynamicMenuType.Static
            }
        };

        private GameObject _icon, _display;
        public override GameObject IconPrefab => _icon ??= MakeCandyDisplay("Blue Rock Candy - Icon");
        // EXPERIMENT: Kolben neben den Kristallen, um ihn im Hauptquartier anzuschauen.
        public override GameObject DisplayPrefab => _display ??= MakeFlaskDisplay("Blue Rock Candy - Display");

        private static GameObject MakeFlaskDisplay(string name)
        {
            var root = PrefabUtil.CreateRoot(name);
            CrystalPile(root.transform, "Candy", new Vector3(0.18f, 0.05f, 0f), 1f, 5);
            Flask(root.transform, "Flask", new Vector3(-0.15f, 0f, 0f), 1f);
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
            { Locale.English, "Cook sugar and water in a pot. Take the blue syrup off the heat and let it crystallise (a freezer speeds this up), then smash it by hand on a counter. Put a portion in a candy bag and serve - no plate needed. Respect the chemistry." }
        };
        public override List<(Locale, UnlockInfo)> InfoList => new List<(Locale, UnlockInfo)>
        {
            (Locale.English, new UnlockInfo
            {
                Name = "Blue Rock Candy",
                Description = "Adds Blue Rock Candy as a main",
                FlavourText = "It's just sugar. Probably."
            })
        };
    }

    // Variante: 99 % Reinheit (wie dünnes/dickes Steak).
    public class PureCandyDish : CustomDish
    {
        public override string UniqueNameID => "Pure Blue Rock Candy Dish";
        public override DishType Type => DishType.Main;
        public override CardType CardType => CardType.Default;
        public override UnlockGroup UnlockGroup => UnlockGroup.Dish;
        public override int Difficulty => 2;
        public override DishCustomerChange CustomerMultiplier => DishCustomerChange.SmallDecrease;
        public override List<Unlock> HardcodedRequirements => new List<Unlock>
        {
            (Dish)GDOUtils.GetCustomGameDataObject<BlueRockCandyDish>().GameDataObject
        };
        public override HashSet<Process> RequiredProcesses => new HashSet<Process>
        {
            (Process)GDOUtils.GetExistingGDO(ProcessReferences.Chop)
        };
        public override List<Dish.MenuItem> ResultingMenuItems => new List<Dish.MenuItem>
        {
            new Dish.MenuItem
            {
                Item = (Item)GDOUtils.GetCustomGameDataObject<PureCandyBag>().GameDataObject,
                Phase = MenuPhase.Main, Weight = 0.75f, DynamicMenuType = DynamicMenuType.Static
            }
        };

        private GameObject _icon;
        public override GameObject IconPrefab => _icon ??= MakePureIcon();
        private static GameObject MakePureIcon()
        {
            var root = PrefabUtil.CreateRoot("Pure Candy - Icon");
            PureCrystal(root.transform, "Crystal", new Vector3(0f, 0.12f, 0f), 1.3f);
            return root;
        }

        public override Dictionary<Locale, string> Recipe => new Dictionary<Locale, string>
        {
            { Locale.English, "Chop a portion of candy to pick out the single purest crystal, then bag it." }
        };
        public override List<(Locale, UnlockInfo)> InfoList => new List<(Locale, UnlockInfo)>
        {
            (Locale.English, new UnlockInfo
            {
                Name = "99% Purity",
                Description = "Some customers only accept the purest crystal",
                FlavourText = "We are not making chili P."
            })
        };
    }

    // Extra: Chili P (Jalapeños schneiden und ins Tütchen).
    public class ChiliPDish : CustomDish
    {
        public override string UniqueNameID => "Chili P Dish";
        public override DishType Type => DishType.Extra;
        public override CardType CardType => CardType.Default;
        public override UnlockGroup UnlockGroup => UnlockGroup.Dish;
        public override int Difficulty => 1;
        public override DishCustomerChange CustomerMultiplier => DishCustomerChange.SmallDecrease;
        public override List<Unlock> HardcodedRequirements => new List<Unlock>
        {
            (Dish)GDOUtils.GetCustomGameDataObject<BlueRockCandyDish>().GameDataObject
        };
        public override HashSet<Item> MinimumIngredients => new HashSet<Item>
        {
            (Item)GDOUtils.GetCustomGameDataObject<IngredientLib.Ingredient.Items.Jalapeno>().GameDataObject
        };
        public override HashSet<Process> RequiredProcesses => new HashSet<Process>
        {
            (Process)GDOUtils.GetExistingGDO(ProcessReferences.Chop)
        };
        public override HashSet<Dish.IngredientUnlock> IngredientsUnlocks
        {
            get
            {
                var chili = FilledBagBase.ChiliP;
                return new HashSet<Dish.IngredientUnlock>
                {
                    new Dish.IngredientUnlock { MenuItem = (ItemGroup)GDOUtils.GetCustomGameDataObject<CandyBag>().GameDataObject, Ingredient = chili },
                    new Dish.IngredientUnlock { MenuItem = (ItemGroup)GDOUtils.GetCustomGameDataObject<PureCandyBag>().GameDataObject, Ingredient = chili },
                };
            }
        }

        private GameObject _icon;
        public override GameObject IconPrefab => _icon ??= MakeIcon();
        private static GameObject MakeIcon()
        {
            var root = PrefabUtil.CreateRoot("Chili P - Icon");
            PrefabUtil.CloneCustomItem(GDOUtils.GetCustomGameDataObject<IngredientLib.Ingredient.Items.ChoppedJalapeno>(), root.transform, "Chili", Vector3.zero, 1f);
            return root;
        }

        public override Dictionary<Locale, string> Recipe => new Dictionary<Locale, string>
        {
            { Locale.English, "Chop a jalapeño and add it to the candy bag." }
        };
        public override List<(Locale, UnlockInfo)> InfoList => new List<(Locale, UnlockInfo)>
        {
            (Locale.English, new UnlockInfo
            {
                Name = "Chili P",
                Description = "Customers may want a signature touch of chili in their bag",
                FlavourText = "It's my signature."
            })
        };
    }
}
