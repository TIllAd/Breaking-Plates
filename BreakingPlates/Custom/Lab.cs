using Kitchen;
using System.Collections.Generic;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using UnityEngine;
using static BreakingPlates.Custom.Visuals;

namespace BreakingPlates.Custom
{
    // Einfache Quellen (unbegrenzt) für Laborbedarf. Optik: Spielgerät als Basis + eigenes Teil obendrauf.
    public abstract class ProviderBase : CustomAppliance
    {
        protected abstract Item Provided { get; }
        protected abstract string Title { get; }
        protected abstract string Desc { get; }
        protected virtual int BaseApplianceID => ApplianceReferences.LeftoversBagStation;
        // Kleines Modell des Inhalts oben auf der Quelle (null = keins).
        protected virtual void Decorate(Transform top) { }
        protected virtual bool UseDecoration => false;
        // Für Anpassungen an der Spiel-Optik (z.B. Farben der Vorlage).
        protected virtual void Customize(GameObject go) { }
        // false = feste Optik (Decorate) statt der einzelnen Items auf den vier Plätzen.
        protected virtual bool ShowItemsInSlots => true;

        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= MakePrefab();

        private GameObject MakePrefab()
        {
            var src = (GDOUtils.GetExistingGDO(BaseApplianceID) as Appliance)?.Prefab
                ?? ((Appliance)GDOUtils.GetExistingGDO(ApplianceReferences.LeftoversBagStation)).Prefab;
            var go = Object.Instantiate(src, PrefabUtil.Holder, false);
            go.name = "BreakingPlates " + Title;
            // "Snapshot" (große Spiel-Tüte vorne oben) leeren: stand bei Kolben sichtbar über.
            // Das Item-Prefab gibt es zu diesem Zeitpunkt nur über das eigene Custom-Objekt (Spiel-Daten noch im Aufbau).
            var itemPrefab = (GDOUtils.GetCustomGameDataObject(Provided.ID) as CustomItem)?.Prefab ?? Provided.Prefab;
            if (!ShowItemsInSlots)
            {
                // Feste Optik statt einzelner Items: Plätze ausblenden, Anzeige-Logik stilllegen.
                var view = go.GetComponent<LimitedItemSourceView>();
                if (view != null)
                {
                    ReflectionUtils.GetField<LimitedItemSourceView>("FixedType").SetValue(view, true);
                    ReflectionUtils.GetField<LimitedItemSourceView>("Items").SetValue(view, new List<GameObject>());
                }
                foreach (Transform slot in go.transform)
                    if (slot.name.StartsWith("Takeaway Bag")) slot.gameObject.SetActive(false);
                var fixedTop = new GameObject("BP Display");
                fixedTop.transform.SetParent(go.transform, false);
                fixedTop.transform.localPosition = new Vector3(0f, 0.5f, 0.15f);
                Decorate(fixedTop.transform);
            }
            // Ablageplätze (im Spiel "Takeaway Bag") ebenfalls mit dem eigenen Item füllen, damit auch Vorschaubilder stimmen.
            foreach (Transform slot in go.transform)
            {
                if (!ShowItemsInSlots || !slot.name.StartsWith("Takeaway Bag") || itemPrefab == null) continue;
                for (int i = slot.childCount - 1; i >= 0; i--) Object.DestroyImmediate(slot.GetChild(i).gameObject);
                PrefabUtil.CloneVisual(itemPrefab, slot, "Item", Vector3.zero);
            }
            var snap = go.transform.Find("Snapshot");
            if (snap != null)
            {
                for (int i = snap.childCount - 1; i >= 0; i--) Object.DestroyImmediate(snap.GetChild(i).gameObject);
            }
            if (UseDecoration)
            {
                var top = new GameObject("BP Top");
                top.transform.SetParent(go.transform, false);
                top.transform.localPosition = new Vector3(0f, 0.95f, 0f);
                Decorate(top.transform);
            }
            Customize(go);
            return go;
        }

        // 0 = unbegrenzt. Sonst: so viele liegen bereit, zurücklegen erlaubt, jeden Tag wieder aufgefüllt.
        protected virtual int Limit => 0;
        public override List<IApplianceProperty> Properties => new List<IApplianceProperty>
        {
            Limit > 0
                ? KitchenPropertiesUtils.GetCItemProvider(Provided.ID, Limit, Limit, false, false, false, false, false, false, true)
                : KitchenPropertiesUtils.GetUnlimitedCItemProvider(Provided.ID)
        };
        public override bool IsPurchasable => true;
        public override bool SellOnlyAsDuplicate => true;
        public override PriceTier PriceTier => PriceTier.Cheap;
        public override ShoppingTags ShoppingTags => ShoppingTags.Cooking | ShoppingTags.Misc;
#pragma warning disable CS0618
        public override List<(Locale, ApplianceInfo)> InfoList => new List<(Locale, ApplianceInfo)>
        {
            (Locale.English, LocalisationUtils.CreateApplianceInfo(Title, Desc, new List<Appliance.Section>(), new List<string>()))
        };
#pragma warning restore CS0618
    }

    // --- Tütchen ---
    public class BagProvider : ProviderBase
    {
        public override string UniqueNameID => "Candy Bag Provider";
        protected override Item Provided => (Item)GDOUtils.GetCustomGameDataObject<EmptyBag>().GameDataObject;
        protected override string Title => "Candy Bags";
        protected override string Desc => "Provides small bags. Strictly for candy.";
    }

    // --- Erlenmeyerkolben (ersetzt den Topf) ---
    public class LabFlask : CustomItem
    {
        public override string UniqueNameID => "Lab Flask";
        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override bool IsIndisposable => true;
        public override Appliance DedicatedProvider => (Appliance)GDOUtils.GetCustomGameDataObject<FlaskProvider>().GameDataObject;
        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= MakeFlask("Lab Flask", 1f);

        internal static GameObject MakeFlask(string name, float scale)
        {
            var root = PrefabUtil.CreateRoot(name);
            Flask(root.transform, "Flask", Vector3.zero, scale);
            return root;
        }
    }

    public class FlaskProvider : ProviderBase
    {
        public override string UniqueNameID => "Lab Flask Provider";
        protected override Item Provided => (Item)GDOUtils.GetCustomGameDataObject<LabFlask>().GameDataObject;
        protected override string Title => "Lab Flasks";
        protected override string Desc => "Think of it as a pot. A very scientific pot.";
        protected override int Limit => 4;
        protected override void Decorate(Transform top)
        {
            Flask(top, "Flask A", new Vector3(-0.15f, 0f, 0f), 0.7f);
            Flask(top, "Flask B", new Vector3(0.15f, 0f, 0f), 0.7f);
        }
    }

    // --- Großer Kolben (Superlab) ---
    public class BigLabFlask : CustomItem
    {
        public override string UniqueNameID => "Lab Flask - Big";
        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override bool IsIndisposable => true;
        public override string ColourBlindTag => "XL";
        public override Appliance DedicatedProvider => (Appliance)GDOUtils.GetCustomGameDataObject<BigFlaskProvider>().GameDataObject;
        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= LabFlask.MakeFlask("Big Lab Flask", BigFlaskScale);
    }

    public class BigFlaskProvider : ProviderBase
    {
        public override string UniqueNameID => "Big Lab Flask Provider";
        protected override Item Provided => (Item)GDOUtils.GetCustomGameDataObject<BigLabFlask>().GameDataObject;
        protected override string Title => "Superlab Flasks";
        protected override string Desc => "Bigger batches, longer cooks. Only 2.";
        protected override int Limit => 2;
        protected override void Decorate(Transform top) => Flask(top, "Flask", Vector3.zero, 0.9f);
    }

    // --- Holzstiele (Kristall-Lollis) ---
    public class CandyStick : CustomItem
    {
        public override string UniqueNameID => "Candy Stick";
        public override ItemStorage ItemStorageFlags => ItemStorage.StackableFood;
        public override Appliance DedicatedProvider => (Appliance)GDOUtils.GetCustomGameDataObject<StickProvider>().GameDataObject;
        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= MakeStick();
        private static GameObject MakeStick()
        {
            var root = PrefabUtil.CreateRoot("Candy Stick");
            var s = Stick(root.transform, "Stick", new Vector3(-0.2f, 0.03f, 0f));
            s.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
            return root;
        }
    }

    public class StickProvider : ProviderBase
    {
        public override string UniqueNameID => "Candy Stick Provider";
        protected override Item Provided => (Item)GDOUtils.GetCustomGameDataObject<CandyStick>().GameDataObject;
        protected override string Title => "Candy Sticks";
        protected override string Desc => "Put one in the syrup before it sets.";
        protected override bool ShowItemsInSlots => false;
        // Glas mit aufrecht stehenden, leicht aufgefächerten Stielen.
        protected override void Decorate(Transform top)
        {
            ProcMesh.Create("Jar", top, ProcMesh.Mug(0.11f, 0.2f, 0.01f), Materials.Glass, Vector3.zero);
            for (int i = 0; i < 9; i++)
            {
                float a = i * 40f * Mathf.Deg2Rad, r = 0.02f + 0.05f * (i % 3) / 2f;
                var st = Stick(top, "Stick " + i, new Vector3(Mathf.Cos(a) * r, 0.01f, Mathf.Sin(a) * r), 0.36f);
                st.transform.localRotation = Quaternion.Euler(Mathf.Sin(a) * 9f, 0f, -Mathf.Cos(a) * 9f);
            }
        }
    }

    // --- Stevia (Sugar-Free) ---
    public class Stevia : CustomItem
    {
        public override string UniqueNameID => "Stevia";
        public override ItemStorage ItemStorageFlags => ItemStorage.StackableFood;
        public override string ColourBlindTag => "Stv";
        public override Appliance DedicatedProvider => (Appliance)GDOUtils.GetCustomGameDataObject<SteviaProvider>().GameDataObject;
        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= MakeStevia();
        private static GameObject MakeStevia()
        {
            var root = PrefabUtil.CreateRoot("Stevia");
            SteviaHeap(root.transform, "Stevia", new Vector3(0f, 0.03f, 0f));
            return root;
        }
    }

    public class SteviaProvider : ProviderBase
    {
        public override string UniqueNameID => "Stevia Provider";
        protected override Item Provided => (Item)GDOUtils.GetCustomGameDataObject<Stevia>().GameDataObject;
        protected override string Title => "Stevia";
        protected override string Desc => "Sugar-free. Lydia insists.";
        // Topfpflanze aus dem Spiel als Quelle, aber eigene Farben + Schild, damit sie nicht wie die Deko-Pflanze aussieht.
        protected override int BaseApplianceID => ApplianceReferences.Plant;
        protected override void Customize(GameObject go)
        {
            var leaf = Materials.Solid("Stevia Plant", new Color(0.55f, 0.85f, 0.35f));
            foreach (var r in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = leaf;
                r.sharedMaterials = mats;
            }
            // Kleine weiße Blüten mit gelber Mitte (Stevia blüht weiß).
            var flowers = new GameObject("Stevia Flowers");
            flowers.transform.SetParent(go.transform, false);
            void ApplyF(GameObject t) => t.transform.localPosition = Tuning.V3("stevia.flowers.pos", Vector3.zero);
            ApplyF(flowers);
            Tuning.Register(flowers, "stevia.flowers", ApplyF);
            var white = Materials.Solid("Stevia Flower", new Color(0.98f, 0.98f, 0.95f));
            var yellow = Materials.Solid("Stevia Flower Centre", new Color(1f, 0.85f, 0.2f));
            Vector3[] spots =
            {
                new Vector3(0.16f, 0.6f, -0.14f), new Vector3(0.08f, 0.82f, -0.06f), new Vector3(-0.16f, 0.55f, 0.02f),
                new Vector3(-0.08f, 0.72f, 0.1f), new Vector3(0.18f, 0.5f, 0.14f), new Vector3(0.0f, 0.95f, 0.0f),
                new Vector3(-0.18f, 0.45f, -0.1f), new Vector3(0.22f, 0.68f, 0.02f),
            };
            for (int i = 0; i < spots.Length; i++)
            {
                var f = new GameObject("Flower " + i);
                f.transform.SetParent(flowers.transform, false);
                f.transform.localPosition = spots[i];
                f.transform.localRotation = Quaternion.Euler(i * 23f, i * 47f, 0f);
                for (int p = 0; p < 5; p++)
                {
                    float a = p * 72f * Mathf.Deg2Rad;
                    ProcMesh.Create("Petal", f.transform, ProcMesh.Box(new Vector3(0.045f, 0.012f, 0.045f)), white, new Vector3(Mathf.Cos(a) * 0.036f, 0f, Mathf.Sin(a) * 0.036f));
                }
                ProcMesh.Create("Centre", f.transform, ProcMesh.Box(new Vector3(0.032f, 0.018f, 0.032f)), yellow, Vector3.zero);
            }
            // Pflanzschild "Stevia"
            var tag = new GameObject("Stevia Tag");
            tag.transform.SetParent(go.transform, false);
            void Apply(GameObject t) => t.transform.localPosition = Tuning.V3("stevia.tag.pos", new Vector3(-0.12f, 0.42f, -0.14f));
            Apply(tag);
            Tuning.Register(tag, "stevia.tag", Apply);
            ProcMesh.Create("Stake", tag.transform, ProcMesh.Box(new Vector3(0.015f, 0.2f, 0.01f)), Materials.Wood, new Vector3(0f, -0.08f, 0f));
            ProcMesh.Create("Board", tag.transform, ProcMesh.Box(new Vector3(0.2f, 0.08f, 0.012f)), Materials.Paper, new Vector3(0f, 0.04f, 0f));
            DecoBase.MakeText(tag.transform, "Label", "STEVIA", new Vector3(0f, 0.04f, -0.008f), 0.4f, new Color(0.15f, 0.5f, 0.2f));
        }
    }

    // --- Schraderbräu (Beilage) ---
    public class Schraderbraeu : CustomItem
    {
        public override string UniqueNameID => "Schraderbraeu";
        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override ItemValue ItemValue => ItemValue.SideMedium;
        public override string ColourBlindTag => "Be";
        public override Appliance DedicatedProvider => (Appliance)GDOUtils.GetCustomGameDataObject<SchraderTap>().GameDataObject;
        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= MakeMug();
        private static GameObject MakeMug()
        {
            var root = PrefabUtil.CreateRoot("Schraderbraeu");
            BeerMug(root.transform, "Mug", Vector3.zero, 0.9f);
            return root;
        }
    }

    public class SchraderTap : ProviderBase
    {
        public override string UniqueNameID => "Schraderbraeu Tap";
        protected override Item Provided => (Item)GDOUtils.GetCustomGameDataObject<Schraderbraeu>().GameDataObject;
        protected override string Title => "Schraderbräu";
        protected override string Desc => "Home brewed by a DEA agent.";
        // Der (im Spiel ungenutzte) Bierzapfhahn als Optik, falls vorhanden.
        protected override int BaseApplianceID => ApplianceReferences.BeerTap;

        // Etwas mehr Leben: Logo-Schild vorne, Abtropfschale, ein Krug und Hanks DEA-Cap in der Ecke (Easter Egg).
        protected override void Customize(GameObject go)
        {
            var root = new GameObject("BP Schrader");
            root.transform.SetParent(go.transform, false);
            void Apply(GameObject t) => t.transform.localPosition = Tuning.V3("schrader.pos", Vector3.zero);
            Apply(root);
            Tuning.Register(root, "schrader", Apply);

            var sign = new GameObject("Sign");
            sign.transform.SetParent(root.transform, false);
            sign.transform.localPosition = new Vector3(0f, 0.3f, -0.51f);
            ProcMesh.Create("Border", sign.transform, ProcMesh.Box(new Vector3(0.66f, 0.2f, 0.01f)), Materials.Solid("Schrader Gold", new Color(0.85f, 0.7f, 0.3f)), Vector3.zero);
            ProcMesh.Create("Plate", sign.transform, ProcMesh.Box(new Vector3(0.62f, 0.16f, 0.012f)), Materials.Solid("Schrader Green", new Color(0.12f, 0.35f, 0.18f)), Vector3.zero);
            DecoBase.MakeText(sign.transform, "Logo", "SCHRADERBRÄU", new Vector3(0f, 0f, -0.01f), 0.75f, new Color(0.98f, 0.92f, 0.7f));

            ProcMesh.Create("Drip Tray", root.transform, ProcMesh.Box(new Vector3(0.22f, 0.02f, 0.14f)), Materials.DarkGrey, new Vector3(0f, 0.56f, -0.3f));
            BeerMug(root.transform, "Mug", new Vector3(-0.3f, 0.55f, 0.25f), 0.55f);

            // Easter Egg: Hanks DEA-Cap liegt auf der Theke.
            var cap = new GameObject("DEA Cap");
            cap.transform.SetParent(root.transform, false);
            void ApplyCap(GameObject t)
            {
                t.transform.localPosition = Tuning.V3("schrader.cap.pos", new Vector3(0.28f, 0.55f, 0.2f));
                t.transform.localRotation = Quaternion.Euler(0f, Tuning.F("schrader.cap.yrot", -25f), 0f);
            }
            ApplyCap(cap);
            Tuning.Register(cap, "schrader.cap", ApplyCap);
            var navy = Materials.Solid("DEA Navy", new Color(0.08f, 0.11f, 0.22f));
            var dome = ProcMesh.Create("Crown", cap.transform, ProcMesh.Lathe(CapProfile, 20, capTop: true), navy, Vector3.zero);
            dome.transform.localScale = new Vector3(1f, 0.8f, 1f);
            var brim = ProcMesh.Create("Brim", cap.transform, ProcMesh.Cylinder(0.085f, 0.01f, 16), navy, new Vector3(0f, 0.002f, -0.1f));
            brim.transform.localScale = new Vector3(1f, 1f, 0.75f);
            ProcMesh.Create("Button", cap.transform, ProcMesh.Cylinder(0.012f, 0.008f, 8), navy, new Vector3(0f, 0.078f, 0f));
            var label = DecoBase.MakeText(cap.transform, "DEA", "DEA", new Vector3(0f, 0.042f, -0.098f), 0.5f, new Color(1f, 0.85f, 0.15f));
            label.transform.localRotation = Quaternion.Euler(35f, 0f, 0f);
        }
        // Kuppel der Cap (Viertelkreis, Radius 0.1).
        static readonly Vector2[] CapProfile =
        {
            new Vector2(0.1f, 0f), new Vector2(0.097f, 0.026f), new Vector2(0.087f, 0.05f), new Vector2(0.071f, 0.071f),
            new Vector2(0.05f, 0.087f), new Vector2(0.026f, 0.097f), new Vector2(0f, 0.1f)
        };
        protected override void Decorate(Transform top)
        {
            BeerMug(top, "Mug A", new Vector3(-0.13f, 0f, 0f), 0.7f);
            BeerMug(top, "Mug B", new Vector3(0.13f, 0f, 0f), 0.7f);
        }
    }
}
