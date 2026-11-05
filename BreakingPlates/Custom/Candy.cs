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
    // Ablauf: Kolben + Zucker + Wasser -> kochen -> blauer Sirup -> abkühlen (Freezer schneller) -> Kristalle
    // -> mit der Hand zerschlagen (Kneten) -> Portionen. Lässt man die Kristalle noch länger abkühlen, werden sie rein (99,1 %).
    // Varianten: Superlab (großer Kolben, 10 Portionen), Stevia (Sugar-Free), Stiel (Lollis).

    internal static class G
    {
        // Stevia-Kette: grüner Zweig ragt aus dem Kolbenhals, damit man sie nicht mit Zucker verwechselt.
        public static void SteviaMark(GameObject flask)
        {
            var sprig = SteviaHeap(flask.transform, "Stevia Mark", new Vector3(0.02f, 0.3f, 0f));
            sprig.transform.localScale = Vector3.one * 0.9f;
        }
        public static Item I<C>() where C : CustomGameDataObject => (Item)GDOUtils.GetCustomGameDataObject<C>().GameDataObject;
        public static Process P(int id) => (Process)GDOUtils.GetExistingGDO(id);
    }

    // ---------- Basisklassen ----------

    public abstract class RawBatchBase : CustomItemGroup
    {
        protected abstract Item Container { get; }
        protected abstract Item Sweetener { get; }
        protected abstract Item CookResult { get; }
        protected virtual float CookTime => 8f;
        protected virtual float FlaskScale => 1f;
        protected virtual bool IsStevia => false;

        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override Item DisposesTo => Container;

        public override List<ItemGroup.ItemSet> Sets => new List<ItemGroup.ItemSet>
        {
            new ItemGroup.ItemSet { Min = 2, Max = 2, Items = new List<Item> { Sweetener, Ref(ItemReferences.Water) } },
            new ItemGroup.ItemSet { Min = 1, Max = 1, IsMandatory = true, Items = new List<Item> { Container } },
        };

        public override List<Item.ItemProcess> Processes => new List<Item.ItemProcess>
        {
            new Item.ItemProcess { Process = G.P(ProcessReferences.Cook), Duration = CookTime, Result = CookResult }
        };

        private GameObject _prefab, _sweet, _water;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot(UniqueNameID);
                    var flask = Flask(_prefab.transform, "Flask", Vector3.zero, FlaskScale);
                    _water = FlaskLiquid(flask.transform, "Water", Materials.Water);
                    // Zuckerberg ragt deutlich über das Wasser, damit man "nur Wasser" und "Zucker + Wasser" unterscheiden kann.
                    var heapPos = new Vector3(0f, 0.08f, 0f);
                    _sweet = IsStevia ? SteviaHeap(flask.transform, "Sweetener", heapPos)
                                      : SugarHeap(flask.transform, "Sweetener", heapPos);
                    _sweet.transform.localScale = new Vector3(0.85f, 1.4f, 0.85f);
                }
                return _prefab;
            }
        }

        // Farbenblind-Beschriftung: zeigt, was schon im Kolben ist.
        public override List<ItemGroupView.ColourBlindLabel> Labels => new List<ItemGroupView.ColourBlindLabel>
        {
            new ItemGroupView.ColourBlindLabel { Item = Sweetener, Text = IsStevia ? "Stv" : "Su" },
            new ItemGroupView.ColourBlindLabel { Item = Ref(ItemReferences.Water), Text = "W" },
        };

        public override void OnRegister(ItemGroup gdo)
        {
            Prefab.GetComponent<ItemGroupView>().ComponentGroups = new List<ItemGroupView.ComponentGroup>
            {
                new ItemGroupView.ComponentGroup { Item = Sweetener, GameObject = _sweet },
                new ItemGroupView.ComponentGroup { Item = Ref(ItemReferences.Water), GameObject = _water },
            };
        }
    }

    // Blauer Sirup: kühlt vom Herd genommen von selbst ab (wie die Fajita-Pfanne), im Freezer schneller.
    public abstract class SyrupBase : CustomItem
    {
        protected virtual bool MarkStevia => false;
        protected virtual Material SyrupMaterial => Materials.BlueSyrup;
        protected abstract Item Container { get; }
        protected abstract Item Crystallised { get; }
        protected virtual float CoolTime => 12f;
        protected virtual float FreezeTime => 4f;
        protected virtual float FlaskScale => 1f;

        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override Item DisposesTo => Container;
        public override string ColourBlindTag => "Sy";

        private Item.ItemProcess CoolOff => new Item.ItemProcess { Process = G.P(ProcessReferences.CoolOff), Duration = CoolTime, Result = Crystallised };
        private Item.ItemProcess Freeze => new Item.ItemProcess { Process = G.P(ProcessReferences.Freeze), Duration = FreezeTime, Result = Crystallised };
        public override Item.ItemProcess AutomaticItemProcess => CoolOff;
        private Item.ItemProcess React => new Item.ItemProcess { Process = ReactCoolProcess.P, Duration = ReactProcess.CoolTime, Result = Crystallised };
        public override List<Item.ItemProcess> Processes => new List<Item.ItemProcess> { CoolOff, Freeze, React };

        private GameObject _prefab;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot(UniqueNameID);
                    var flask = Flask(_prefab.transform, "Flask", Vector3.zero, FlaskScale, SyrupMaterial);
                    if (MarkStevia) G.SteviaMark(flask);
                }
                return _prefab;
            }
        }
    }

    // Auskristallisiert: zerschlagen (Kneten). Optional: im Crystal Purifier -> reine Kristalle.
    public abstract class CrystalBase : CustomItem
    {
        protected virtual bool MarkStevia => false;
        protected abstract Item Container { get; }
        protected abstract Item Smashed { get; }
        protected virtual Item OverCooled => null;
        protected virtual float FlaskScale => 1f;
        protected virtual bool Pure => false;

        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override Item DisposesTo => Container;
        public override string ColourBlindTag => Pure ? "99" : "Cr";

        private Item.ItemProcess Knead => new Item.ItemProcess { Process = G.P(ProcessReferences.Knead), Duration = 2.5f, Result = Smashed };
        public override List<Item.ItemProcess> Processes => new List<Item.ItemProcess> { Knead };

        private GameObject _prefab;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot(UniqueNameID);
                    var flask = Flask(_prefab.transform, "Flask", Vector3.zero, FlaskScale, Materials.BlueSyrup);
                    var crust = FlaskCrystals(flask.transform, "Crust", 0.06f, 8);
                    if (Pure) Materials.Apply(crust, Materials.PureCrystal);
                    if (MarkStevia) G.SteviaMark(flask);
                }
                return _prefab;
            }
        }
    }

    // Zerschlagen: N Portionen, die Kristalle sinken beim Entnehmen ab.
    public abstract class ShardsBase : CustomItem
    {
        protected virtual bool MarkStevia => false;
        protected abstract Item Container { get; }
        protected abstract Item Portion { get; }
        protected virtual int Portions => 5;
        protected virtual float FlaskScale => 1f;
        protected virtual bool Pure => false;

        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override Item SplitSubItem => Portion;
        public override int SplitCount => Portions;
        public override List<Item> SplitDepletedItems => new List<Item> { Container };
        public override Item DisposesTo => Container;
        public override string ColourBlindTag => Pure ? "99" : "Sh";
        // Karte "Mass Production": der volle Kolben geht an eine ganze Gruppe, danach bleibt der leere Kolben stehen.
        protected virtual bool WholeBatch => true;
        public override int MaxOrderSharers => WholeBatch ? 4 : 0;
        public override Item DirtiesTo => WholeBatch ? Container : null;
        // Ganzer Kolben = 5 Portionen für eine Gruppe: fester Preis statt "Small" (3).
        public override ItemValue ItemValue => WholeBatch ? ItemValue.ExtraLarge : ItemValue.Small;
        public override int RewardOverride => WholeBatch ? 20 : -1;

        private GameObject _prefab;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    var full = new Vector3(0f, 0.06f, 0f);
                    var empty = new Vector3(0f, -0.06f, 0f);
                    _prefab = PrefabUtil.CreateRoot(UniqueNameID);
                    var flask = Flask(_prefab.transform, "Flask", Vector3.zero, FlaskScale);
                    var pile = FlaskCrystals(flask.transform, "Shards", full.y, 8);
                    if (Pure) Materials.Apply(pile, Materials.PureCrystal);
                    if (MarkStevia) G.SteviaMark(flask);
                    var view = _prefab.AddComponent<PositionSplittableView>();
                    ReflectionUtils.GetField<PositionSplittableView>("FullPosition").SetValue(view, full);
                    ReflectionUtils.GetField<PositionSplittableView>("EmptyPosition").SetValue(view, empty);
                    ReflectionUtils.GetField<PositionSplittableView>("Objects").SetValue(view, new List<GameObject> { pile });
                }
                return _prefab;
            }
        }
    }

    // ---------- Standard (Kolben + Zucker) ----------

    public class SugarPot : RawBatchBase
    {
        public override string UniqueNameID => "Sugar Pot";
        protected override Item Container => G.I<LabFlask>();
        protected override Item Sweetener => Ref(ItemReferences.Sugar);
        protected override Item CookResult => G.I<SyrupPot>();
    }

    public class SyrupPot : SyrupBase
    {
        public override string UniqueNameID => "Blue Syrup Pot";
        protected override Item Container => G.I<LabFlask>();
        protected override Item Crystallised => G.I<CrystalPot>();
    }

    public class CrystalPot : CrystalBase
    {
        public override string UniqueNameID => "Crystal Pot";
        protected override Item Container => G.I<LabFlask>();
        protected override Item Smashed => G.I<ShardsPot>();
    }

    public class PureCrystalPot : CrystalBase
    {
        public override string UniqueNameID => "Crystal Pot - Pure";
        protected override Item Container => G.I<LabFlask>();
        protected override Item Smashed => G.I<PureShardsPot>();
        protected override bool Pure => true;
    }

    public class ShardsPot : ShardsBase
    {
        public override string UniqueNameID => "Crystal Shards Pot";
        protected override Item Container => G.I<LabFlask>();
        protected override Item Portion => G.I<BlueRockCandy>();
    }

    public class PureShardsPot : ShardsBase
    {
        public override string UniqueNameID => "Crystal Shards Pot - Pure";
        protected override Item Container => G.I<LabFlask>();
        protected override Item Portion => G.I<PureBlueRockCandy>();
        protected override bool Pure => true;
    }

    // ---------- 99,1 %: Umkristallisieren ----------
    // Zerschlagene Kristalle + Wasser zurück in den Kolben -> nochmal kochen -> abkühlen -> reine Kristalle.

    public class RecrystallisePot : CustomItemGroup
    {
        public override string UniqueNameID => "Recrystallise Pot";
        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override Item DisposesTo => G.I<LabFlask>();
        public override string ColourBlindTag => "Re";

        public override List<ItemGroup.ItemSet> Sets => new List<ItemGroup.ItemSet>
        {
            new ItemGroup.ItemSet { Min = 1, Max = 1, IsMandatory = true, Items = new List<Item> { G.I<ShardsPot>() } },
            new ItemGroup.ItemSet { Min = 1, Max = 1, Items = new List<Item> { Ref(ItemReferences.Water) } },
        };

        public override List<Item.ItemProcess> Processes => new List<Item.ItemProcess>
        {
            new Item.ItemProcess { Process = G.P(ProcessReferences.Cook), Duration = 8f, Result = G.I<PureSyrupPot>() }
        };

        public override List<ItemGroupView.ColourBlindLabel> Labels => new List<ItemGroupView.ColourBlindLabel>
        {
            new ItemGroupView.ColourBlindLabel { Item = Ref(ItemReferences.Water), Text = "W" },
        };

        private GameObject _prefab, _water;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot(UniqueNameID);
                    var flask = Flask(_prefab.transform, "Flask", Vector3.zero);
                    FlaskCrystals(flask.transform, "Shards", 0.06f, 8);
                    _water = FlaskLiquid(flask.transform, "Water", Materials.Water);
                }
                return _prefab;
            }
        }

        public override void OnRegister(ItemGroup gdo)
        {
            Prefab.GetComponent<ItemGroupView>().ComponentGroups = new List<ItemGroupView.ComponentGroup>
            {
                new ItemGroupView.ComponentGroup { Item = Ref(ItemReferences.Water), GameObject = _water },
            };
        }
    }

    // Klarer, heller Sirup: wird beim Abkühlen zu reinen Kristallen.
    public class PureSyrupPot : SyrupBase
    {
        public override string UniqueNameID => "Blue Syrup Pot - Pure";
        protected override Item Container => G.I<LabFlask>();
        protected override Item Crystallised => G.I<PureCrystalPot>();
        protected override Material SyrupMaterial => Materials.Solid("Pure Syrup", new Color(0.6f, 0.85f, 1f));
        public override string ColourBlindTag => "99";
    }

    // Das Produkt.
    public class BlueRockCandy : CustomItem
    {
        public override string UniqueNameID => "Blue Rock Candy";
        public override ItemStorage ItemStorageFlags => ItemStorage.StackableFood;
        public override ItemValue ItemValue => ItemValue.Medium;

        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= Make();
        private static GameObject Make()
        {
            var p = PrefabUtil.CreateRoot("Blue Rock Candy");
            CrystalPile(p.transform, "Candy", new Vector3(0f, 0.05f, 0f), 1f, 5);
            return p;
        }
    }

    // 99,1 %: reine Kristalle. Ohne Karte kann man sie zur Not wieder klein schneiden (= normale Portion).
    public class PureBlueRockCandy : CustomItem
    {
        public override string UniqueNameID => "Blue Rock Candy - Pure";
        public override ItemStorage ItemStorageFlags => ItemStorage.StackableFood;
        public override ItemValue ItemValue => ItemValue.Large;
        public override string ColourBlindTag => "99";
        public override List<Item.ItemProcess> Processes => new List<Item.ItemProcess>
        {
            new Item.ItemProcess { Process = G.P(ProcessReferences.Chop), Duration = 1f, Result = G.I<BlueRockCandy>() }
        };

        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= Make();
        private static GameObject Make()
        {
            var p = PrefabUtil.CreateRoot("Pure Blue Rock Candy");
            var pile = CrystalPile(p.transform, "Candy", new Vector3(0f, 0.05f, 0f), 1f, 3);
            Materials.Apply(pile, Materials.PureCrystal);
            PureCrystal(p.transform, "Crystal", new Vector3(0f, 0.12f, 0f), 0.8f);
            return p;
        }
    }

    // ---------- Superlab (großer Kolben, 10 Portionen) ----------

    public class BigSugarPot : RawBatchBase
    {
        public override string UniqueNameID => "Sugar Pot - Big";
        protected override Item Container => G.I<BigLabFlask>();
        protected override Item Sweetener => Ref(ItemReferences.Sugar);
        protected override Item CookResult => G.I<BigSyrupPot>();
        protected override float CookTime => 14f;
        protected override float FlaskScale => BigFlaskScale;
    }

    public class BigSyrupPot : SyrupBase
    {
        public override string UniqueNameID => "Blue Syrup Pot - Big";
        protected override Item Container => G.I<BigLabFlask>();
        protected override Item Crystallised => G.I<BigCrystalPot>();
        protected override float CoolTime => 18f;
        protected override float FreezeTime => 6f;
        protected override float FlaskScale => BigFlaskScale;
        public override string ColourBlindTag => "XL";
    }

    public class BigCrystalPot : CrystalBase
    {
        public override string UniqueNameID => "Crystal Pot - Big";
        protected override Item Container => G.I<BigLabFlask>();
        protected override Item Smashed => G.I<BigShardsPot>();
        protected override float FlaskScale => BigFlaskScale;
    }

    public class BigShardsPot : ShardsBase
    {
        public override string UniqueNameID => "Crystal Shards Pot - Big";
        protected override Item Container => G.I<BigLabFlask>();
        protected override Item Portion => G.I<BlueRockCandy>();
        protected override int Portions => 10;
        protected override float FlaskScale => BigFlaskScale;
    }

    // ---------- Sugar-Free (Stevia) ----------

    public class SteviaPot : RawBatchBase
    {
        public override string UniqueNameID => "Stevia Pot";
        protected override Item Container => G.I<LabFlask>();
        protected override Item Sweetener => G.I<Stevia>();
        protected override Item CookResult => G.I<SteviaSyrupPot>();
        protected override bool IsStevia => true;
    }

    public class SteviaSyrupPot : SyrupBase
    {
        protected override bool MarkStevia => true;
        public override string UniqueNameID => "Blue Syrup Pot - Stevia";
        protected override Item Container => G.I<LabFlask>();
        protected override Item Crystallised => G.I<SteviaCrystalPot>();
        public override string ColourBlindTag => "SF";
    }

    public class SteviaCrystalPot : CrystalBase
    {
        protected override bool MarkStevia => true;
        public override string UniqueNameID => "Crystal Pot - Stevia";
        protected override Item Container => G.I<LabFlask>();
        protected override Item Smashed => G.I<SteviaShardsPot>();
        public override string ColourBlindTag => "SF";
    }

    public class SteviaShardsPot : ShardsBase
    {
        protected override bool MarkStevia => true;
        public override string UniqueNameID => "Crystal Shards Pot - Stevia";
        protected override Item Container => G.I<LabFlask>();
        protected override Item Portion => G.I<SugarFreeCandy>();
        public override string ColourBlindTag => "SF";
    }

    public class SugarFreeCandy : CustomItem
    {
        public override string UniqueNameID => "Blue Rock Candy - Sugar-Free";
        public override ItemStorage ItemStorageFlags => ItemStorage.StackableFood;
        public override ItemValue ItemValue => ItemValue.Medium;
        public override string ColourBlindTag => "SF";
        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= Make();
        private static GameObject Make()
        {
            var p = PrefabUtil.CreateRoot("Sugar-Free Rock Candy");
            CrystalPile(p.transform, "Candy", new Vector3(0f, 0.05f, 0f), 1f, 5);
            var sprig = SteviaHeap(p.transform, "Stevia Mark", new Vector3(0.12f, 0.02f, 0f));
            sprig.transform.localRotation = Quaternion.Euler(0f, 0f, -60f);
            return p;
        }
    }

    // ---------- Am Stiel (Lollis) ----------

    // Sirup + Stiel: Stiel muss rein, bevor der Sirup fest wird.
    public class StickSyrupPot : CustomItemGroup
    {
        public override string UniqueNameID => "Blue Syrup Pot - Sticks";
        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override Item DisposesTo => G.I<LabFlask>();
        public override string ColourBlindTag => "St";

        public override List<ItemGroup.ItemSet> Sets => new List<ItemGroup.ItemSet>
        {
            new ItemGroup.ItemSet { Min = 1, Max = 1, IsMandatory = true, Items = new List<Item> { G.I<SyrupPot>() } },
            new ItemGroup.ItemSet { Min = 1, Max = 1, Items = new List<Item> { G.I<CandyStick>() } },
        };

        private static Item.ItemProcess CoolOff => new Item.ItemProcess { Process = G.P(ProcessReferences.CoolOff), Duration = 12f, Result = G.I<LolliPot>() };
        private static Item.ItemProcess Freeze => new Item.ItemProcess { Process = G.P(ProcessReferences.Freeze), Duration = 4f, Result = G.I<LolliPot>() };
        public override Item.ItemProcess AutomaticItemProcess => CoolOff;
        private static Item.ItemProcess React => new Item.ItemProcess { Process = ReactCoolProcess.P, Duration = ReactProcess.CoolTime, Result = G.I<LolliPot>() };
        public override List<Item.ItemProcess> Processes => new List<Item.ItemProcess> { CoolOff, Freeze, React };

        private GameObject _prefab, _sticks;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot(UniqueNameID);
                    var flask = Flask(_prefab.transform, "Flask", Vector3.zero, 1f, Materials.BlueSyrup);
                    _sticks = FlaskSticks(flask.transform, "Sticks");
                }
                return _prefab;
            }
        }

        public override void OnRegister(ItemGroup gdo)
        {
            Prefab.GetComponent<ItemGroupView>().ComponentGroups = new List<ItemGroupView.ComponentGroup>
            {
                new ItemGroupView.ComponentGroup { Item = G.I<CandyStick>(), GameObject = _sticks },
            };
        }
    }

    // Fertige Lollis im Kolben: direkt 5 Stück entnehmen (kein Kneten).
    public class LolliPot : ShardsBase
    {
        public override string UniqueNameID => "Crystal Lolli Pot";
        protected override bool WholeBatch => false;
        protected override Item Container => G.I<LabFlask>();
        protected override Item Portion => G.I<RockCandyLolli>();
        public override string ColourBlindTag => "St";
        private GameObject _sticks;
        public override GameObject Prefab
        {
            get
            {
                var p = base.Prefab;
                if (_sticks == null) _sticks = FlaskSticks(p.transform.Find("Flask"), "Sticks");
                return p;
            }
        }
    }

    public class RockCandyLolli : CustomItem
    {
        public override string UniqueNameID => "Blue Rock Candy Lolli";
        public override ItemStorage ItemStorageFlags => ItemStorage.StackableFood;
        public override ItemValue ItemValue => ItemValue.Medium;
        public override string ColourBlindTag => "St";
        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= Make();
        private static GameObject Make()
        {
            var p = PrefabUtil.CreateRoot("Rock Candy Lolli");
            var l = Lolli(p.transform, "Lolli", new Vector3(-0.15f, 0.04f, 0f));
            l.transform.localRotation = Quaternion.Euler(0f, 0f, -80f);
            return p;
        }
    }
}
