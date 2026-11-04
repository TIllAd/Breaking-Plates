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
                    Pot(_prefab.transform);
                    _water = Liquid(_prefab.transform, "Water", 0.12f);
                    // Zuckerberg ragt aus dem Wasser heraus.
                    _sugar = SugarHeap(_prefab.transform, "Sugar", new Vector3(0f, 0.17f, 0f));
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

        public override Item.ItemProcess AutomaticItemProcess => CoolOff;
        public override List<Item.ItemProcess> Processes => new List<Item.ItemProcess> { CoolOff };

        private GameObject _prefab;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot("Blue Syrup Pot");
                    Pot(_prefab.transform);
                    Liquid(_prefab.transform, "Syrup", 0.14f, Materials.BlueSyrup);
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
                    Pot(_prefab.transform);
                    // Kristallkruste: Platte als Grund, darauf dicht gepackte, schräg herausragende Kristalle (wie eine Geode),
                    // damit der Topf nicht mehr wie Wasser aussieht.
                    Liquid(_prefab.transform, "Crystal Slab", 0.12f, Materials.BlueSyrup);
                    var crust = new GameObject("Crust");
                    crust.transform.SetParent(_prefab.transform, false);
                    for (int ring = 0; ring < 2; ring++)
                    {
                        int n = ring == 0 ? 4 : 9;
                        float r = ring == 0 ? 0.07f : 0.17f;
                        for (int i = 0; i < n; i++)
                        {
                            float a = (i / (float)n + ring * 0.13f) * Mathf.PI * 2f;
                            var c = Crystal(crust.transform, $"Crystal {ring}-{i}", new Vector3(Mathf.Cos(a) * r, 0.24f, Mathf.Sin(a) * r), 0.45f + 0.08f * (i % 3), a * Mathf.Rad2Deg);
                            // nach außen gekippt, damit es "gewachsen" aussieht
                            c.transform.localRotation = Quaternion.Euler(ring == 0 ? 10f : 32f, -a * Mathf.Rad2Deg + 90f, 0f) * Quaternion.Euler(0f, 0f, 45f);
                        }
                    }
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

        private static readonly Vector3 Full = new Vector3(0f, 0.22f, 0f);
        private static readonly Vector3 Empty = new Vector3(0f, 0.02f, 0f);

        private GameObject _prefab;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot("Crystal Shards Pot");
                    Pot(_prefab.transform);
                    var pile = CrystalPile(_prefab.transform, "Shards", Full, 1.3f, 8);
                    var view = _prefab.AddComponent<PositionSplittableView>();
                    ReflectionUtils.GetField<PositionSplittableView>("FullPosition").SetValue(view, Full);
                    ReflectionUtils.GetField<PositionSplittableView>("EmptyPosition").SetValue(view, Empty);
                    ReflectionUtils.GetField<PositionSplittableView>("Objects").SetValue(view, new List<GameObject> { pile });
                }
                return _prefab;
            }
        }
    }

    // 5) Das Produkt. Wird ohne Teller serviert.
    public class BlueRockCandy : CustomItem
    {
        public override string UniqueNameID => "Blue Rock Candy";
        public override ItemStorage ItemStorageFlags => ItemStorage.StackableFood;
        public override ItemValue ItemValue => ItemValue.Medium;

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
}
