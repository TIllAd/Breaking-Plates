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
        // Vorlage: Ofen (großes Gerät mit Klappe, Inhalt beim Arbeiten verdeckt).
        private static Appliance Oven => (Appliance)GDOUtils.GetExistingGDO(ApplianceReferences.Oven);

        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= Make();
        private static GameObject Make()
        {
            var go = Object.Instantiate(Oven.Prefab, PrefabUtil.Holder, false);
            go.name = "BreakingPlates Lab Reactor";
            // Nur den roten Lack gelb färben; Schwarz, Metall und Glas bleiben. Drehregler schwarz.
            foreach (var r in go.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mats = r.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null) continue;
                    if (mats[i].name.Contains("Deep Red")) { mats[i] = Materials.Hazmat; changed = true; }
                    else if (r.gameObject.name == "Circle") { mats[i] = Materials.Black; changed = true; }
                }
                if (changed) r.sharedMaterials = mats;
            }
            // Display mit Timer + Warnlampe an der hohen Rückwand (Vorderseite = -z).
            var panel = new GameObject("BP Reactor");
            panel.transform.SetParent(go.transform, false);
            void Apply(GameObject t)
            {
                t.transform.localPosition = Tuning.V3("reactor.panel.pos", new Vector3(0f, 0.72f, 0.09f));
                t.transform.localScale = Vector3.one * Tuning.F("reactor.panel.scale", 1f);
            }
            Apply(panel);
            Tuning.Register(panel, "reactor.panel", Apply);
            ProcMesh.Create("Display", panel.transform, ProcMesh.Box(new Vector3(0.3f, 0.11f, 0.02f)), Materials.Black, Vector3.zero);
            DecoBase.MakeText(panel.transform, "Timer", "00:10", new Vector3(0f, 0f, -0.012f), 0.55f, new Color(0.35f, 1f, 0.45f));
            ProcMesh.Create("Lamp Base", panel.transform, ProcMesh.Cylinder(0.035f, 0.02f, 12), Materials.DarkGrey, new Vector3(0.28f, 0.1f, 0.05f));
            ProcMesh.Create("Lamp", panel.transform, ProcMesh.Cylinder(0.028f, 0.05f, 12), Materials.Solid("Reactor Lamp", new Color(1f, 0.55f, 0.1f)), new Vector3(0.28f, 0.12f, 0.05f));
            return go;
        }

        public override List<Appliance.ApplianceProcesses> Processes => new List<Appliance.ApplianceProcesses>
        {
            new Appliance.ApplianceProcesses { Process = ReactProcess.P, IsAutomatic = true, Speed = 1f }
        };
        public override List<IApplianceProperty> Properties => Oven.Properties;
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
