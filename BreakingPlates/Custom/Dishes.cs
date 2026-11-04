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
            Ref(ItemReferences.Sugar), Ref(ItemReferences.Water), Ref(ItemReferences.Pot)
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
                Item = (Item)GDOUtils.GetCustomGameDataObject<BlueRockCandy>().GameDataObject,
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
            { Locale.English, "Cook sugar and water in a pot. Take the blue syrup off the heat and let it crystallise (a freezer speeds this up), then smash it by hand on a counter. Serve the candy as it is - no plate needed. Respect the chemistry." }
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
}
