using System.Collections.Generic;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using UnityEngine;

namespace BreakingPlates.Custom
{
    // Lab Reactor: kocht UND kühlt automatisch (Kolben rein -> Kristalle raus). Eigenes Gerät, kein Upgrade,
    // und ein eigener Prozess, damit er nur Kolben verarbeitet und nicht jedes Essen kocht/einfriert.
    public class ReactProcess : CustomProcess
    {
        public override string UniqueNameID => "React";
        public override GameDataObject BasicEnablingAppliance => GDOUtils.GetCustomGameDataObject<LabReactor>().GameDataObject;
        public override bool CanObfuscateProgress => true;
        public override List<(Locale, ProcessInfo)> InfoList => new List<(Locale, ProcessInfo)>
        {
            (Locale.English, new ProcessInfo { Name = "React", Icon = "<sprite name=\"cook\">" })
        };

        public static Process P => (Process)GDOUtils.GetCustomGameDataObject<ReactProcess>().GameDataObject;
        public const float CookTime = 5f;
        public const float CoolTime = 5f;
    }

    public class LabReactor : CustomAppliance
    {
        public override string UniqueNameID => "Lab Reactor";
        private static Appliance Freezer => (Appliance)GDOUtils.GetExistingGDO(ApplianceReferences.Freezer);

        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= Make();
        private static GameObject Make()
        {
            var go = Object.Instantiate(Freezer.Prefab, PrefabUtil.Holder, false);
            go.name = "BreakingPlates Lab Reactor";
            // Freezer-Optik gelb einfärben (Industriemaschine) + Tank und Leitungen obendrauf.
            PrefabUtil.Tint(go, Tuning.C("color.reactor", new Color(0.95f, 0.8f, 0.2f)));
            var deco = new GameObject("BP Reactor");
            deco.transform.SetParent(go.transform, false);
            void Apply(GameObject t)
            {
                t.transform.localPosition = Tuning.V3("reactor.pos", new Vector3(0.25f, 1.0f, 0.25f));
                t.transform.localScale = Vector3.one * Tuning.F("reactor.scale", 1f);
            }
            Apply(deco);
            Tuning.Register(deco, "reactor", Apply);
            ProcMesh.Create("Tank", deco.transform, ProcMesh.Cylinder(0.1f, 0.3f, 16), Materials.DarkGrey, Vector3.zero);
            ProcMesh.Create("Tank Top", deco.transform, ProcMesh.Cylinder(0.11f, 0.03f, 16), Materials.Black, new Vector3(0f, 0.3f, 0f));
            var pipe = ProcMesh.Create("Pipe", deco.transform, ProcMesh.Cylinder(0.02f, 0.3f, 8), Materials.DarkGrey, new Vector3(-0.05f, 0.24f, 0f));
            pipe.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            ProcMesh.Create("Light", deco.transform, ProcMesh.Cylinder(0.035f, 0.05f, 10), Materials.ZipRed, new Vector3(0f, 0.33f, 0f));
            return go;
        }

        public override List<Appliance.ApplianceProcesses> Processes => new List<Appliance.ApplianceProcesses>
        {
            new Appliance.ApplianceProcesses { Process = ReactProcess.P, IsAutomatic = true, Speed = 1f }
        };
        public override List<IApplianceProperty> Properties => Freezer.Properties;
        public override bool IsPurchasable => true;
        public override PriceTier PriceTier => PriceTier.Expensive;
        public override RarityTier RarityTier => RarityTier.Rare;
        public override ShoppingTags ShoppingTags => ShoppingTags.Cooking | ShoppingTags.Technology;
        // Nur im Shop, wenn Kolben im Spiel sind (also Blue Rock Candy auf der Karte steht).
        public override List<Item> RequiresIngredientForShop => new List<Item> { G.I<LabFlask>() };
#pragma warning disable CS0618
        public override List<(Locale, ApplianceInfo)> InfoList => new List<(Locale, ApplianceInfo)>
        {
            (Locale.English, LocalisationUtils.CreateApplianceInfo("Lab Reactor", "Cooks and cools in one go. Industrial chemistry.",
                new List<Appliance.Section> { new Appliance.Section { Title = "Automatic", Description = "Turns a flask of sugar and water straight into crystals" } },
                new List<string>()))
        };
#pragma warning restore CS0618
    }
}
