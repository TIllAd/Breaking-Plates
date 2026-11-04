using System.Collections.Generic;
using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using UnityEngine;
using static BreakingPlates.Custom.Visuals;

namespace BreakingPlates.Custom
{
    // Leeres Tütchen: kleine durchsichtige Tüte (Form der Reste-Tüte, Glas-Material).
    public class EmptyBag : CustomItem
    {
        public override string UniqueNameID => "Candy Bag - Empty";
        public override ItemStorage ItemStorageFlags => ItemStorage.StackableFood;
        public override Appliance DedicatedProvider => (Appliance)GDOUtils.GetCustomGameDataObject<BagProvider>().GameDataObject;

        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= MakeBag("Candy Bag - Empty");

        internal static GameObject MakeBag(string name)
        {
            var root = PrefabUtil.CreateRoot(name);
            Bag(root.transform);
            return root;
        }
    }

    // Quelle für Tütchen (Optik der Reste-Tüten-Station aus dem Spiel), unbegrenzt.
    public class BagProvider : CustomAppliance
    {
        public override string UniqueNameID => "Candy Bag Provider";
        public override GameObject Prefab => ((Appliance)GDOUtils.GetExistingGDO(ApplianceReferences.LeftoversBagStation)).Prefab;
        public override List<IApplianceProperty> Properties => new List<IApplianceProperty>
        {
            KitchenPropertiesUtils.GetUnlimitedCItemProvider(GDOUtils.GetCustomGameDataObject<EmptyBag>().GameDataObject.ID)
        };
        public override bool IsPurchasable => true;
        public override bool SellOnlyAsDuplicate => true;
        public override PriceTier PriceTier => PriceTier.Cheap;
        public override ShoppingTags ShoppingTags => ShoppingTags.Cooking | ShoppingTags.Misc;
        public override List<(Locale, ApplianceInfo)> InfoList => new List<(Locale, ApplianceInfo)>
        {
            (Locale.English, LocalisationUtils.CreateApplianceInfo("Candy Bags", "Provides small bags. Strictly for candy.", new List<Appliance.Section>(), new List<string>()))
        };
    }

    // Gefülltes Tütchen = das, was serviert wird.
    public abstract class FilledBagBase : CustomItemGroup
    {
        protected abstract Item Candy { get; }
        protected abstract string PrefabName { get; }
        protected abstract GameObject MakeContent(Transform parent);

        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override ItemValue ItemValue => ItemValue.Medium;

        public override List<ItemGroup.ItemSet> Sets => new List<ItemGroup.ItemSet>
        {
            new ItemGroup.ItemSet { Min = 1, Max = 1, IsMandatory = true,
                Items = new List<Item> { (Item)GDOUtils.GetCustomGameDataObject<EmptyBag>().GameDataObject } },
            new ItemGroup.ItemSet { Min = 1, Max = 1, Items = new List<Item> { Candy } },
        };

        private GameObject _prefab, _content;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot(PrefabName);
                    _content = MakeContent(_prefab.transform);
                    Bag(_prefab.transform);
                }
                return _prefab;
            }
        }

        public override void OnRegister(ItemGroup gdo)
        {
            Prefab.GetComponent<ItemGroupView>().ComponentGroups = new List<ItemGroupView.ComponentGroup>
            {
                new ItemGroupView.ComponentGroup { Item = Candy, GameObject = _content }
            };
        }
    }

    public class CandyBag : FilledBagBase
    {
        public override string UniqueNameID => "Candy Bag - Blue Rock Candy";
        protected override string PrefabName => "Candy Bag";
        protected override Item Candy => (Item)GDOUtils.GetCustomGameDataObject<BlueRockCandy>().GameDataObject;
        protected override GameObject MakeContent(Transform parent) => CrystalPile(parent, "Candy", new Vector3(0f, 0.06f, 0f), 0.75f, 5);
    }

    public class PureCandyBag : FilledBagBase
    {
        public override string UniqueNameID => "Candy Bag - Pure";
        protected override string PrefabName => "Pure Candy Bag";
        protected override Item Candy => (Item)GDOUtils.GetCustomGameDataObject<PureBlueRockCandy>().GameDataObject;
        protected override GameObject MakeContent(Transform parent) => PureCrystal(parent, "Pure Crystal", new Vector3(0f, 0.1f, 0f), 1f);
        public override List<ItemGroupView.ColourBlindLabel> Labels => new List<ItemGroupView.ColourBlindLabel>
        {
            new ItemGroupView.ColourBlindLabel { Item = Candy, Text = "99" }
        };
    }
}
