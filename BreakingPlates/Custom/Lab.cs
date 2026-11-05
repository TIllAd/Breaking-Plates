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

        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= MakePrefab();

        private GameObject MakePrefab()
        {
            var src = (GDOUtils.GetExistingGDO(BaseApplianceID) as Appliance)?.Prefab
                ?? ((Appliance)GDOUtils.GetExistingGDO(ApplianceReferences.LeftoversBagStation)).Prefab;
            var go = Object.Instantiate(src, PrefabUtil.Holder, false);
            go.name = "BreakingPlates " + Title;
            var top = new GameObject("BP Top");
            top.transform.SetParent(go.transform, false);
            void Place(GameObject t) => t.transform.localPosition = Tuning.V3("provider.top", new Vector3(0f, 0.95f, 0f));
            Place(top);
            Tuning.Register(top, "provider.top", Place);
            Decorate(top.transform);
            return go;
        }

        public override List<IApplianceProperty> Properties => new List<IApplianceProperty>
        {
            KitchenPropertiesUtils.GetUnlimitedCItemProvider(Provided.ID)
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
        protected override string Desc => "Borosilicate. Very professional.";
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
        protected override string Desc => "Bigger batches, longer cooks.";
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
        protected override void Decorate(Transform top)
        {
            for (int i = 0; i < 5; i++)
            {
                var s = Stick(top, "Stick " + i, new Vector3(-0.2f, 0.02f, -0.1f + i * 0.05f));
                s.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
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
        protected override int BaseApplianceID => ApplianceReferences.SourceSugar;
        protected override void Decorate(Transform top) => SteviaHeap(top, "Stevia", Vector3.zero);
    }

    // --- Schraderbräu (Beilage) ---
    public class Schraderbraeu : CustomItem
    {
        public override string UniqueNameID => "Schraderbraeu";
        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override ItemValue ItemValue => ItemValue.Small;
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
        protected override void Decorate(Transform top)
        {
            BeerMug(top, "Mug A", new Vector3(-0.13f, 0f, 0f), 0.7f);
            BeerMug(top, "Mug B", new Vector3(0.13f, 0f, 0f), 0.7f);
        }
    }
}
