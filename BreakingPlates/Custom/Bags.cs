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

    // Gefülltes Tütchen = das, was serviert wird.
    public abstract class FilledBagBase : CustomItemGroup
    {
        protected abstract Item Candy { get; }
        protected abstract string PrefabName { get; }
        protected abstract GameObject MakeContent(Transform parent);
        protected virtual Material Zip => null;
        // Zusätzliche Kennzeichnung außen am Tütchen (z.B. Stevia-Blatt).
        protected virtual void Decorate(GameObject bag) { }

        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override ItemValue ItemValue => ItemValue.Medium;

        public override List<ItemGroup.ItemSet> Sets => new List<ItemGroup.ItemSet>
        {
            new ItemGroup.ItemSet { Min = 1, Max = 1, IsMandatory = true,
                Items = new List<Item> { (Item)GDOUtils.GetCustomGameDataObject<EmptyBag>().GameDataObject } },
            new ItemGroup.ItemSet { Min = 1, Max = 1, Items = new List<Item> { Candy } },
            // Karte "Chili P": geschnittene Jalapeños als Topping (nur mit Karte).
            new ItemGroup.ItemSet { Min = 0, Max = 1, RequiresUnlock = true, Items = new List<Item> { ChiliP } },
        };

        internal static Item ChiliP => (Item)GDOUtils.GetCustomGameDataObject<IngredientLib.Ingredient.Items.ChoppedJalapeno>().GameDataObject;

        public override List<ItemGroup.ItemReward> Rewards => new List<ItemGroup.ItemReward>
        {
            new ItemGroup.ItemReward { Item = ChiliP, RewardAmount = 1 }
        };

        public override List<ItemGroupView.ColourBlindLabel> Labels => new List<ItemGroupView.ColourBlindLabel>
        {
            new ItemGroupView.ColourBlindLabel { Item = ChiliP, Text = "P" }
        };

        private GameObject _prefab, _content, _chili;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot(PrefabName);
                    _content = MakeContent(_prefab.transform);
                    // Inhalt flach drücken, damit er in der Folie bleibt. Live einstellbar: bag.content.pos/scale
                    var baseScale = _content.transform.localScale;
                    var basePos = _content.transform.localPosition;
                    void ApplyContent(GameObject go)
                    {
                        go.transform.localScale = Vector3.Scale(baseScale, Tuning.V3("bag.content.scale", new Vector3(0.85f, 0.85f, 0.4f)));
                        go.transform.localPosition = basePos + Tuning.V3("bag.content.offset", Vector3.zero);
                    }
                    ApplyContent(_content);
                    Tuning.Register(_content, "bag.content." + PrefabName, ApplyContent);
                    var bag = Bag(_prefab.transform, Zip);
                    // Chili P erkennt man am roten Chili-Aufkleber.
                    _chili = ChiliSticker(bag);
                    Decorate(bag);

                }
                return _prefab;
            }
        }

        public override void OnRegister(ItemGroup gdo)
        {
            Prefab.GetComponent<ItemGroupView>().ComponentGroups = new List<ItemGroupView.ComponentGroup>
            {
                new ItemGroupView.ComponentGroup { Item = Candy, GameObject = _content },
                new ItemGroupView.ComponentGroup { Item = ChiliP, GameObject = _chili }
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
        public override ItemValue ItemValue => ItemValue.MediumLarge; // aufwendiger (Purifier) -> etwas mehr Geld
        protected override string PrefabName => "Pure Candy Bag";
        protected override Item Candy => (Item)GDOUtils.GetCustomGameDataObject<PureBlueRockCandy>().GameDataObject;
        protected override GameObject MakeContent(Transform parent) => PureCrystal(parent, "Pure Crystal", new Vector3(0f, 0.1f, 0f), 1f);
        public override List<ItemGroupView.ColourBlindLabel> Labels => new List<ItemGroupView.ColourBlindLabel>
        {
            new ItemGroupView.ColourBlindLabel { Item = Candy, Text = "99" },
            new ItemGroupView.ColourBlindLabel { Item = ChiliP, Text = "P" }
        };
    }

    // Sugar-Free: grüner Verschluss.
    public class SugarFreeBag : FilledBagBase
    {
        public override string UniqueNameID => "Candy Bag - Sugar-Free";
        protected override string PrefabName => "Sugar-Free Candy Bag";
        protected override Item Candy => (Item)GDOUtils.GetCustomGameDataObject<SugarFreeCandy>().GameDataObject;
        // Grünes Blatt als Aufkleber (gleiches Tütchen, gleicher blauer Verschluss).
        protected override void Decorate(GameObject bag) => SteviaSticker(bag);
        protected override GameObject MakeContent(Transform parent) => CrystalPile(parent, "Candy", new Vector3(0f, 0.06f, 0f), 0.75f, 5);
        public override List<ItemGroupView.ColourBlindLabel> Labels => new List<ItemGroupView.ColourBlindLabel>
        {
            new ItemGroupView.ColourBlindLabel { Item = Candy, Text = "SF" },
            new ItemGroupView.ColourBlindLabel { Item = ChiliP, Text = "P" }
        };
    }
}
