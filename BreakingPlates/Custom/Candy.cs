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
    // Ablauf: Topf + Zucker + Wasser -> kochen -> blauer Sirup -> vom Herd nehmen, abkühlen lassen
    // -> Kristallplatte -> mit der Hand zerschlagen (Kneten) -> 5 Portionen Blue Rock Candy.

    // 1) Roher Ansatz im Topf.
    public class SugarPot : CustomItemGroup
    {
        public override string UniqueNameID => "Sugar Pot";
        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override Item DisposesTo => Ref(ItemReferences.Pot);

        public override List<ItemGroup.ItemSet> Sets => new List<ItemGroup.ItemSet>
        {
            new ItemGroup.ItemSet { Min = 2, Max = 2, Items = new List<Item> { Ref(ItemReferences.Sugar), Ref(ItemReferences.Water) } },
            new ItemGroup.ItemSet { Min = 1, Max = 1, IsMandatory = true, Items = new List<Item> { Ref(ItemReferences.Pot) } },
        };

        public override List<Item.ItemProcess> Processes => new List<Item.ItemProcess>
        {
            new Item.ItemProcess
            {
                Process = (Process)GDOUtils.GetExistingGDO(ProcessReferences.Cook),
                Duration = 8f,
                Result = (Item)GDOUtils.GetCustomGameDataObject<SyrupPot>().GameDataObject
            }
        };

        private GameObject _prefab, _sugar, _water;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot("Sugar Pot");
                    // Optik: Erlenmeyerkolben statt Topf (Spiel-Logik bleibt der Topf).
                    var flask = Flask(_prefab.transform, "Flask", Vector3.zero);
                    _water = FlaskLiquid(flask.transform, "Water", Materials.Water);
                    // Zuckerberg ragt aus dem Wasser heraus.
                    _sugar = SugarHeap(flask.transform, "Sugar", new Vector3(0f, 0.04f, 0f));
                    _sugar.transform.localScale = Vector3.one * 0.75f;
                }
                return _prefab;
            }
        }

        public override void OnRegister(ItemGroup gdo)
        {
            Prefab.GetComponent<ItemGroupView>().ComponentGroups = new List<ItemGroupView.ComponentGroup>
            {
                new ItemGroupView.ComponentGroup { Item = Ref(ItemReferences.Sugar), GameObject = _sugar },
                new ItemGroupView.ComponentGroup { Item = Ref(ItemReferences.Water), GameObject = _water },
            };
        }
    }

    // 2) Blauer Sirup. Kühlt vom Herd genommen von selbst ab (wie die Fajita-Pfanne), Kochen hält ihn flüssig.
    public class SyrupPot : CustomItem
    {
        public override string UniqueNameID => "Blue Syrup Pot";
        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override Item DisposesTo => Ref(ItemReferences.Pot);
        public override string ColourBlindTag => "Sy";

        private static Item.ItemProcess CoolOff => new Item.ItemProcess
        {
            Process = (Process)GDOUtils.GetExistingGDO(ProcessReferences.CoolOff),
            Duration = 12f,
            Result = (Item)GDOUtils.GetCustomGameDataObject<CrystalPot>().GameDataObject
        };

        // Abkürzung: im Gefrierschrank kristallisiert es deutlich schneller.
        private static Item.ItemProcess Freeze => new Item.ItemProcess
        {
            Process = (Process)GDOUtils.GetExistingGDO(ProcessReferences.Freeze),
            Duration = 4f,
            Result = (Item)GDOUtils.GetCustomGameDataObject<CrystalPot>().GameDataObject
        };

        public override Item.ItemProcess AutomaticItemProcess => CoolOff;
        public override List<Item.ItemProcess> Processes => new List<Item.ItemProcess> { CoolOff, Freeze };

        private GameObject _prefab;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot("Blue Syrup Pot");
                    Flask(_prefab.transform, "Flask", Vector3.zero, 1f, Materials.BlueSyrup);
                }
                return _prefab;
            }
        }
    }

    // 3) Auskristallisiert: muss noch zerschlagen werden (Kneten an der Arbeitsfläche).
    public class CrystalPot : CustomItem
    {
        public override string UniqueNameID => "Crystal Pot";
        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override Item DisposesTo => Ref(ItemReferences.Pot);
        public override string ColourBlindTag => "Cr";

        public override List<Item.ItemProcess> Processes => new List<Item.ItemProcess>
        {
            new Item.ItemProcess
            {
                Process = (Process)GDOUtils.GetExistingGDO(ProcessReferences.Knead),
                Duration = 2.5f,
                Result = (Item)GDOUtils.GetCustomGameDataObject<ShardsPot>().GameDataObject
            }
        };

        private GameObject _prefab;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot("Crystal Pot");
                    // Kolben mit auskristallisiertem Sirup: Kristalle füllen den Boden, ragen schräg heraus.
                    var flask = Flask(_prefab.transform, "Flask", Vector3.zero, 1f, Materials.BlueSyrup);
                    var crust = FlaskCrystals(flask.transform, "Crust", 0.06f, 8);
                }
                return _prefab;
            }
        }
    }

    // 4) Zerschlagen: 5 Portionen, die Kristalle sinken beim Entnehmen ab.
    public class ShardsPot : CustomItem
    {
        public override string UniqueNameID => "Crystal Shards Pot";
        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override Item SplitSubItem => (Item)GDOUtils.GetCustomGameDataObject<BlueRockCandy>().GameDataObject;
        public override int SplitCount => 5;
        public override List<Item> SplitDepletedItems => new List<Item> { Ref(ItemReferences.Pot) };
        public override Item DisposesTo => Ref(ItemReferences.Pot);
        public override string ColourBlindTag => "Sh";

        private static readonly Vector3 Full = new Vector3(0f, 0.06f, 0f);
        private static readonly Vector3 Empty = new Vector3(0f, -0.06f, 0f);

        private GameObject _prefab;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot("Crystal Shards Pot");
                    var flask = Flask(_prefab.transform, "Flask", Vector3.zero);
                    var pile = FlaskCrystals(flask.transform, "Shards", Full.y, 8);
                    var view = _prefab.AddComponent<PositionSplittableView>();
                    ReflectionUtils.GetField<PositionSplittableView>("FullPosition").SetValue(view, Full);
                    ReflectionUtils.GetField<PositionSplittableView>("EmptyPosition").SetValue(view, Empty);
                    ReflectionUtils.GetField<PositionSplittableView>("Objects").SetValue(view, new List<GameObject> { pile });
                }
                return _prefab;
            }
        }
    }

    // 5) Das Produkt. Kommt in ein Tütchen. Mit der Karte "99 %" kann man es noch sortieren (Schneiden).
    public class BlueRockCandy : CustomItem
    {
        public override string UniqueNameID => "Blue Rock Candy";
        public override ItemStorage ItemStorageFlags => ItemStorage.StackableFood;
        public override ItemValue ItemValue => ItemValue.Medium;

        public override List<Item.ItemProcess> Processes => new List<Item.ItemProcess>
        {
            new Item.ItemProcess
            {
                Process = (Process)GDOUtils.GetExistingGDO(ProcessReferences.Chop),
                Duration = 1.5f,
                Result = (Item)GDOUtils.GetCustomGameDataObject<PureBlueRockCandy>().GameDataObject
            }
        };

        private GameObject _prefab;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot("Blue Rock Candy");
                    CrystalPile(_prefab.transform, "Candy", new Vector3(0f, 0.05f, 0f), 1f, 5);
                }
                return _prefab;
            }
        }
    }

    // 6) 99 % Reinheit: nur der schönste Kristall bleibt übrig.
    public class PureBlueRockCandy : CustomItem
    {
        public override string UniqueNameID => "Blue Rock Candy - Pure";
        public override ItemStorage ItemStorageFlags => ItemStorage.StackableFood;
        public override ItemValue ItemValue => ItemValue.Large;
        public override string ColourBlindTag => "99";

        private GameObject _prefab;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot("Pure Blue Rock Candy");
                    PureCrystal(_prefab.transform, "Crystal", new Vector3(0f, 0.12f, 0f), 1f);
                }
                return _prefab;
            }
        }
    }
}
