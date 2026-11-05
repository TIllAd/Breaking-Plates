using System.Collections.Generic;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using UnityEngine;

namespace BreakingPlates.Custom
{
    // 99,1 %: Reine Kristalle gibt es nur im "Crystal Purifier" (kommt nur mit der Karte).
    // Ohne Karte kein Gerät -> keine reinen Kristalle, kein unerklärtes Weiterkühlen.
    public class PurifyProcess : CustomProcess
    {
        public override string UniqueNameID => "Purify";
        public override GameDataObject BasicEnablingAppliance => GDOUtils.GetCustomGameDataObject<CrystalPurifier>().GameDataObject;
        public override bool CanObfuscateProgress => true;
        public override List<(Locale, ProcessInfo)> InfoList => new List<(Locale, ProcessInfo)>
        {
            (Locale.English, new ProcessInfo { Name = "Purify", Icon = "<sprite name=\"snow\">" })
        };
    }

    // Optik: Gefrierschrank aus dem Spiel mit blauem Laborlicht und Warnstreifen.
    public class CrystalPurifier : CustomAppliance
    {
        public override string UniqueNameID => "Crystal Purifier";
        private static Appliance Freezer => (Appliance)GDOUtils.GetExistingGDO(ApplianceReferences.Freezer);

        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= Make();
        private static GameObject Make()
        {
            var go = Object.Instantiate(Freezer.Prefab, PrefabUtil.Holder, false);
            go.name = "BreakingPlates Crystal Purifier";
            var deco = new GameObject("BP Purifier");
            deco.transform.SetParent(go.transform, false);
            void Apply(GameObject t)
            {
                t.transform.localPosition = Tuning.V3("purifier.pos", new Vector3(0f, 1.05f, 0f));
                t.transform.localScale = Vector3.one * Tuning.F("purifier.scale", 1f);
            }
            Apply(deco);
            Tuning.Register(deco, "purifier", Apply);
            ProcMesh.Create("Light", deco.transform, ProcMesh.Cylinder(0.08f, 0.1f, 16), Materials.BlueCrystal, Vector3.zero);
            ProcMesh.Create("Base", deco.transform, ProcMesh.Cylinder(0.1f, 0.03f, 16), Materials.DarkGrey, new Vector3(0f, -0.01f, 0f));
            return go;
        }

        public override List<Appliance.ApplianceProcesses> Processes => new List<Appliance.ApplianceProcesses>
        {
            new Appliance.ApplianceProcesses
            {
                Process = (Process)GDOUtils.GetCustomGameDataObject<PurifyProcess>().GameDataObject,
                IsAutomatic = true, Speed = 1f
            }
        };
        public override List<IApplianceProperty> Properties => Freezer.Properties;
        public override bool IsPurchasable => true;
        public override PriceTier PriceTier => PriceTier.Medium;
        public override RarityTier RarityTier => RarityTier.Common;
        public override ShoppingTags ShoppingTags => ShoppingTags.Technology;
#pragma warning disable CS0618
        public override List<(Locale, ApplianceInfo)> InfoList => new List<(Locale, ApplianceInfo)>
        {
            (Locale.English, LocalisationUtils.CreateApplianceInfo("Crystal Purifier", "Turns set candy crystals into 99.1% pure ones.",
                new List<Appliance.Section> { new Appliance.Section { Title = "Purify", Description = "Place a flask of crystals inside" } }, new List<string>()))
        };
#pragma warning restore CS0618
    }
}
