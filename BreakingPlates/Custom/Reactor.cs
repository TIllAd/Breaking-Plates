using System.Collections.Generic;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using UnityEngine;

namespace BreakingPlates.Custom
{
    // Reactor: kocht UND kühlt automatisch (Kolben rein -> Kristalle raus). Eigenes Gerät, kein Upgrade,
    // eigener Prozess, damit er nur Kolben verarbeitet. Zum Vergleich gibt es gerade zwei Optiken:
    // A = umgebauter Ofen (Superlab), B = Glas-Reaktor auf Arbeitsfläche (Glass Reactor).
    public class ReactProcess : CustomProcess
    {
        public override string UniqueNameID => "React";
        public override GameDataObject BasicEnablingAppliance => GDOUtils.GetCustomGameDataObject<Superlab>().GameDataObject;
        public override bool CanObfuscateProgress => true;
        public override List<(Locale, ProcessInfo)> InfoList => new List<(Locale, ProcessInfo)>
        {
            (Locale.English, new ProcessInfo { Name = "Cook", Icon = "<sprite name=\"cook\">" })
        };

        public static Process P => (Process)GDOUtils.GetCustomGameDataObject<ReactProcess>().GameDataObject;
        public const float CookTime = 5f;
        public const float CoolTime = 5f;
    }

    // Zweiter Schritt im Reactor: Abkühlen (eigenes Symbol, Eiskristall).
    public class ReactCoolProcess : CustomProcess
    {
        public override string UniqueNameID => "React Cool";
        public override GameDataObject BasicEnablingAppliance => GDOUtils.GetCustomGameDataObject<Superlab>().GameDataObject;
        public override bool CanObfuscateProgress => true;
        public override List<(Locale, ProcessInfo)> InfoList => new List<(Locale, ProcessInfo)>
        {
            (Locale.English, new ProcessInfo { Name = "Cool", Icon = "<sprite name=\"snow\">" })
        };
        public static Process P => (Process)GDOUtils.GetCustomGameDataObject<ReactCoolProcess>().GameDataObject;
    }

    public abstract class ReactorBase : CustomAppliance
    {
        protected abstract Appliance Template { get; }
        protected abstract string Title { get; }
        protected abstract string Desc { get; }
        protected abstract void Build(GameObject go);

        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= Make();
        private GameObject Make()
        {
            var go = Object.Instantiate(Template.Prefab, PrefabUtil.Holder, false);
            go.name = "BreakingPlates " + Title;
            Build(go);
            return go;
        }

        public override List<Appliance.ApplianceProcesses> Processes => new List<Appliance.ApplianceProcesses>
        {
            // Kocht wie ein Ofen alles (Spiel-Prozess "Cook", schneller als der Herd) und kühlt zusätzlich unsere Kolben.
            new Appliance.ApplianceProcesses { Process = (Process)GDOUtils.GetExistingGDO(ProcessReferences.Cook), IsAutomatic = true, Speed = 1.6f },
            new Appliance.ApplianceProcesses { Process = ReactCoolProcess.P, IsAutomatic = true, Speed = 1f },
            // Friert auch Spiel-Sachen ein wie der Gefrierschrank (z.B. Homemade Ice Cream). Fajita-"Abkühlen" ist im Spiel ein Fehler-Vorgang, das bleibt draußen.
            new Appliance.ApplianceProcesses { Process = (Process)GDOUtils.GetExistingGDO(ProcessReferences.Freeze), IsAutomatic = true, Speed = 1f }
        };
        public override List<IApplianceProperty> Properties => Template.Properties;
        public override bool IsPurchasable => true;
        public override PriceTier PriceTier => PriceTier.Expensive;
        public override RarityTier RarityTier => RarityTier.Rare;
        // Nur "Kochen": mit "Technik" passte es auf zu viele Shop-Plätze und kam sehr oft.
        public override ShoppingTags ShoppingTags => ShoppingTags.Cooking;
        // Nur im Shop, wenn Kolben im Spiel sind (also Blue Rock Candy auf der Karte steht).
        public override List<Item> RequiresIngredientForShop => new List<Item> { G.I<LabFlask>() };
#pragma warning disable CS0618
        public override List<(Locale, ApplianceInfo)> InfoList => new List<(Locale, ApplianceInfo)>
        {
            (Locale.English, LocalisationUtils.CreateApplianceInfo(Title, Desc,
                new List<Appliance.Section> { new Appliance.Section { Title = "Automatic", Description = "Turns a flask of sugar and water straight into crystals" } },
                new List<string>()))
        };
#pragma warning restore CS0618

        protected static GameObject Part(string name, Transform parent, Mesh mesh, Material m, Vector3 pos, Vector3? rot = null)
        {
            var go = ProcMesh.Create(name, parent, mesh, m, pos);
            if (rot.HasValue) go.transform.localRotation = Quaternion.Euler(rot.Value);
            return go;
        }

        protected static GameObject Tuned(Transform parent, string name, string key, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            void Apply(GameObject t)
            {
                t.transform.localPosition = Tuning.V3(key + ".pos", pos);
                t.transform.localScale = Vector3.one * Tuning.F(key + ".scale", 1f);
            }
            Apply(go);
            Tuning.Register(go, key, Apply);
            return go;
        }
    }

    // A: Ofen aus dem Spiel, Edelstahl statt Rot, gelber Warnstreifen, schwarze Regler, Display, Gefahren-Raute an der Seite.
    public class Superlab : ReactorBase
    {
        public override string UniqueNameID => "Superlab";
        protected override Appliance Template => (Appliance)GDOUtils.GetExistingGDO(ApplianceReferences.Oven);
        protected override string Title => "Superlab";
        protected override string Desc => "Cooks and cools in one go. Industrial chemistry.";

        protected override void Build(GameObject go)
        {
            var steel = Materials.Solid("Reactor Steel", Tuning.C("color.reactor", new Color(0.72f, 0.74f, 0.76f)));
            foreach (var r in go.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mats = r.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null) continue;
                    if (mats[i].name.Contains("Deep Red")) { mats[i] = steel; changed = true; }
                    else if (r.gameObject.name == "Circle") { mats[i] = Materials.Black; changed = true; }
                }
                if (changed) r.sharedMaterials = mats;
            }

            var knobs = go.transform.Find("Oven/Circle");
            if (knobs != null)
            {
                var baseKnobs = knobs.localPosition;
                void ApplyKnobs(GameObject t) => t.transform.localPosition = baseKnobs + Tuning.V3("superlab.knobs.offset", new Vector3(0.14f, 0f, 0f));
                ApplyKnobs(knobs.gameObject);
                Tuning.Register(knobs.gameObject, "superlab.knobs", ApplyKnobs);
            }

            // Display rechts neben den Reglern (außen), orange Warnlampe oben.
            var panel = Tuned(go.transform, "BP Panel", "superlab.panel", new Vector3(0.3f, 0.3f, -0.44f));
            Part("Display", panel.transform, ProcMesh.Box(new Vector3(0.17f, 0.08f, 0.012f)), Materials.Black, Vector3.zero);
            DecoBase.MakeText(panel.transform, "Timer", "00:10", new Vector3(0f, 0f, -0.007f), 0.38f, new Color(0.35f, 1f, 0.45f));

            var lamp = Tuned(go.transform, "BP Lamp", "superlab.lamp", new Vector3(0.3f, 0.92f, 0.3f));
            Part("Lamp Base", lamp.transform, ProcMesh.Cylinder(0.04f, 0.02f, 12), Materials.DarkGrey, Vector3.zero);
            var bulb = Part("Lamp", lamp.transform, ProcMesh.Cylinder(0.03f, 0.06f, 12), Materials.Solid("Reactor Lamp Off", new Color(0.45f, 0.3f, 0.15f)), new Vector3(0f, 0.02f, 0f));
            var status = go.AddComponent<ReactorStatus>();
            status.Bulb = bulb.GetComponent<MeshRenderer>();
            status.On = Materials.Solid("Reactor Lamp", new Color(1f, 0.6f, 0.1f));
            status.Off = Materials.Solid("Reactor Lamp Off", new Color(0.45f, 0.3f, 0.15f));
            status.Done = Materials.Solid("Reactor Lamp Done", new Color(0.3f, 0.95f, 0.35f));

            // Warnschild an der Seite (oberer Teil, nicht im Knick): Flamme + Eiskristall = heizt und kühlt.
            var sign = Tuned(go.transform, "BP Sign", "superlab.sign", new Vector3(0.415f, 0.62f, 0f));
            Part("Border", sign.transform, ProcMesh.Box(new Vector3(0.01f, 0.18f, 0.34f)), Materials.Black, Vector3.zero);
            Part("Plate", sign.transform, ProcMesh.Box(new Vector3(0.012f, 0.15f, 0.31f)), Materials.Hazmat, Vector3.zero);
            // Flamme
            var flame = new GameObject("Flame");
            flame.transform.SetParent(sign.transform, false);
            flame.transform.localPosition = new Vector3(0.012f, -0.055f, -0.075f);
            flame.transform.localScale = new Vector3(0.02f, 0.11f, 0.11f);
            var flameProfile = new[] { new Vector2(0f, 0f), new Vector2(0.35f, 0.08f), new Vector2(0.45f, 0.3f), new Vector2(0.32f, 0.62f), new Vector2(0.12f, 0.85f), new Vector2(0f, 1f) };
            ProcMesh.Create("Outer", flame.transform, ProcMesh.Lathe(flameProfile, 16), Materials.ZipRed, Vector3.zero);
            var inner = ProcMesh.Create("Inner", flame.transform, ProcMesh.Lathe(flameProfile, 16), Materials.Solid("Flame Inner", new Color(1f, 0.7f, 0.15f)), new Vector3(0.3f, 0.02f, 0f));
            inner.transform.localScale = new Vector3(1f, 0.6f, 0.6f);
            // Eiskristall
            var snow = new GameObject("Snowflake");
            snow.transform.SetParent(sign.transform, false);
            snow.transform.localPosition = new Vector3(0.012f, 0f, 0.075f);
            var ice = Materials.Solid("Snow Icon", new Color(0.25f, 0.55f, 0.95f));
            for (int i = 0; i < 3; i++)
                Part("Arm " + i, snow.transform, ProcMesh.Box(new Vector3(0.008f, 0.11f, 0.016f)), ice, Vector3.zero, new Vector3(i * 60f, 0f, 0f));
        }
    }

    // B: Arbeitsfläche aus dem Spiel mit Glaskammer; der Kolben steht sichtbar darin. Rote Heizspirale unten, blaue Kühlspirale oben.
    public class GlassReactor : ReactorBase
    {
        public override string UniqueNameID => "Glass Reactor";
        protected override Appliance Template => (Appliance)GDOUtils.GetExistingGDO(ApplianceReferences.Countertop);
        protected override string Title => "Glass Reactor";
        protected override string Desc => "Watch it boil, watch it freeze. Science.";

        private static Mesh Ring(float r, float thick = 0.018f) => ProcMesh.Lathe(new[]
        {
            new Vector2(r - thick, 0f), new Vector2(r + thick, 0f), new Vector2(r + thick, thick * 2f), new Vector2(r - thick, thick * 2f), new Vector2(r - thick, 0f)
        }, 24);

        protected override void Build(GameObject go)
        {
            var root = Tuned(go.transform, "BP Glass Reactor", "glassreactor", new Vector3(0f, 0.5f, 0f));
            Part("Base Plate", root.transform, ProcMesh.Cylinder(0.36f, 0.04f, 24), Materials.DarkGrey, Vector3.zero);
            Part("Chamber", root.transform, ProcMesh.Mug(0.33f, 0.56f, 0.01f), Materials.Glass, new Vector3(0f, 0.04f, 0f));
            var heat = Materials.Solid("Heater Coil", new Color(0.95f, 0.35f, 0.12f));
            var cool = Materials.Solid("Cooler Coil", new Color(0.3f, 0.6f, 0.95f));
            Part("Heat 1", root.transform, Ring(0.3f), heat, new Vector3(0f, 0.07f, 0f));
            Part("Heat 2", root.transform, Ring(0.3f), heat, new Vector3(0f, 0.12f, 0f));
            Part("Cool 1", root.transform, Ring(0.3f), cool, new Vector3(0f, 0.46f, 0f));
            Part("Cool 2", root.transform, Ring(0.3f), cool, new Vector3(0f, 0.51f, 0f));
            Part("Lid", root.transform, ProcMesh.Cylinder(0.35f, 0.035f, 24), Materials.DarkGrey, new Vector3(0f, 0.6f, 0f));
            Part("Vent", root.transform, ProcMesh.Cylinder(0.05f, 0.08f, 12), Materials.Black, new Vector3(0.18f, 0.635f, 0.1f));
            var panel = new GameObject("Panel");
            panel.transform.SetParent(root.transform, false);
            panel.transform.localPosition = new Vector3(0f, -0.12f, -0.47f);
            Part("Display", panel.transform, ProcMesh.Box(new Vector3(0.2f, 0.08f, 0.02f)), Materials.Black, Vector3.zero);
            DecoBase.MakeText(panel.transform, "Timer", "00:10", new Vector3(0f, 0f, -0.012f), 0.42f, new Color(0.35f, 1f, 0.45f));
        }
    }

    // Lampe leuchtet und der Timer zählt runter, solange der Reactor arbeitet.
    // Liest die Werte, die das Spiel ohnehin für die Ofen-Animation setzt.
    public class ReactorStatus : MonoBehaviour
    {
        public MeshRenderer Bulb;
        public Material On, Off, Done;
        private Animator _anim;
        private TMPro.TextMeshPro _text;
        private static readonly int ProcessHash = Animator.StringToHash("Process");
        private static readonly int ProgressHash = Animator.StringToHash("Progress");
        private int _lastShown = -2;

        private void Update()
        {
            if (_anim == null) _anim = GetComponent<Animator>();
            if (_text == null) _text = GetComponentInChildren<TMPro.TextMeshPro>(true);
            if (_anim == null) return;
            bool active = _anim.GetInteger(ProcessHash) != 0;
            _done = !active && HasItem();
            if (Bulb != null) Bulb.sharedMaterial = active ? On : _done ? Done : Off;
            int secs = active ? Mathf.CeilToInt((1f - Mathf.Clamp01(_anim.GetFloat(ProgressHash))) * ReactProcess.CookTime) : -1;
            if (_done) secs = -3;
            if (_text != null && secs != _lastShown)
            {
                _text.text = secs == -3 ? "DONE" : secs < 0 ? "--:--" : "00:" + secs.ToString("00");
                _lastShown = secs;
            }
        }

        // Fertig = nichts läuft mehr, aber etwas liegt drin (grüne Lampe, "DONE"). Klappe bleibt wie beim Ofen.
        private bool _done;
        private Transform _hold;
        private bool HasItem()
        {
            if (_hold == null) _hold = transform.Find("HoldPoint");
            if (_hold == null) return false;
            for (int i = 0; i < _hold.childCount; i++) if (_hold.GetChild(i).gameObject.activeInHierarchy) return true;
            return false;
        }
    }
}
