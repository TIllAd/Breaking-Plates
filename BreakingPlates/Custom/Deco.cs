using System.Collections.Generic;
using System.Linq;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using TMPro;
using UnityEngine;

namespace BreakingPlates.Custom
{
    // Deko im Breaking-Bad-Stil. Verhalten (Wand/Boden, Deko-Werte) wird von einem Spiel-Deko-Objekt übernommen,
    // dessen Optik ausgeblendet und durch eigene Teile ersetzt.
    public abstract class DecoBase : CustomAppliance
    {
        // Erste vorhandene Vorlage aus der Liste (manche Deko-Objekte gibt es nicht in jeder Spielversion).
        protected abstract int[] VanillaIDs { get; }
        protected abstract string Title { get; }
        protected abstract string Desc { get; }
        protected abstract void Build(Transform root);
        private Appliance _vanilla;
        protected Appliance Vanilla
        {
            get
            {
                if (_vanilla != null) return _vanilla;
                foreach (var id in VanillaIDs)
                    if (GDOUtils.GetExistingGDO(id) is Appliance a && a.Prefab != null) return _vanilla = a;
                Mod.LogWarning(UniqueNameID + ": keine Deko-Vorlage gefunden, nehme das Fass.");
                return _vanilla = (Appliance)GDOUtils.GetExistingGDO(ApplianceReferences.CosyBarrel);
            }
        }

        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= MakePrefab();
        private GameObject MakePrefab()
        {
            var go = Object.Instantiate(Vanilla.Prefab, PrefabUtil.Holder, false);
            go.name = "BreakingPlates " + Title;
            foreach (var r in go.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
            var root = new GameObject("BP Deco");
            root.transform.SetParent(go.transform, false);
            string key = "deco." + UniqueNameID.ToLower().Replace(" ", "");
            void Apply(GameObject t)
            {
                t.transform.localPosition = Tuning.V3(key + ".pos", DefaultPos);
                t.transform.localRotation = Quaternion.Euler(Tuning.V3(key + ".rot", DefaultRot));
                t.transform.localScale = Vector3.one * Tuning.F(key + ".scale", 1f);
            }
            Apply(root);
            Tuning.Register(root, key, Apply);
            Build(root.transform);
            return go;
        }
        protected virtual Vector3 DefaultPos => Vector3.zero;
        protected virtual Vector3 DefaultRot => Vector3.zero;

        // Ohne "unbeweglich", damit man sie verschieben und per Stuff on Demand (F3) holen kann.
        // Eigenes Thema statt dem der Vorlage (Shop zeigt Deko nur beim passenden Thema, sie zahlt darauf ein).
        protected abstract DecorationType Theme { get; }
        public override List<IApplianceProperty> Properties
        {
            get
            {
                var list = (Vanilla.Properties ?? new List<IApplianceProperty>())
                    .Where(p => p != null && !(p is Kitchen.CImmovable) && !(p is Kitchen.CGivesDecoration)).ToList();
                list.Add(KitchenPropertiesUtils.GetCGivesDecoration(
                    Theme == DecorationType.Affordable ? 1 : 0, Theme == DecorationType.Charming ? 1 : 0,
                    Theme == DecorationType.Exclusive ? 1 : 0, Theme == DecorationType.Formal ? 1 : 0, 0));
                return list;
            }
        }
        public override OccupancyLayer Layer => Vanilla.Layer;
        public override bool IsNonInteractive => Vanilla.IsNonInteractive;
        public override bool IsPurchasable => true;
        public override ShoppingTags ShoppingTags => Vanilla.ShoppingTags;
        public override RarityTier RarityTier => Vanilla.RarityTier;
        public override PriceTier PriceTier => Vanilla.PriceTier;
        public override DecorationType ThemeRequired => Theme;
        // Fester Platz im Shop, solange man sie noch nicht hat (sobald das passende Thema gewählt ist).
        public override bool StapleWhenMissing => true;
#pragma warning disable CS0618
        public override List<(Locale, ApplianceInfo)> InfoList => new List<(Locale, ApplianceInfo)>
        {
            (Locale.English, LocalisationUtils.CreateApplianceInfo(Title, Desc, new List<Appliance.Section>(), new List<string>()))
        };
#pragma warning restore CS0618

        protected static GameObject Box(Transform parent, string name, Vector3 size, Vector3 pos, Material m)
            => ProcMesh.Create(name, parent, ProcMesh.Box(size), m, pos);

        private static TMP_FontAsset _font;
        // TextMeshPro braucht ein aktives Objekt zum Initialisieren: erst außerhalb aufbauen, dann einhängen.
        // Klappt das nicht, gibt es einfach keinen Text (kein Ladefehler).
        public static GameObject MakeText(Transform parent, string name, string text, Vector3 pos, float size, Color color)
            => Text(parent, name, text, pos, size, color);
        protected static GameObject Text(Transform parent, string name, string text, Vector3 pos, float size, Color color)
        {
            var go = new GameObject(name);
            try
            {
                var tmp = go.AddComponent<TextMeshPro>();
                if (_font == null) _font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault();
                if (_font != null) tmp.font = _font;
                tmp.text = text;
                tmp.fontSize = size;
                tmp.color = color;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.enableWordWrapping = false;
                tmp.rectTransform.sizeDelta = new Vector2(2f, 0.5f);
                tmp.ForceMeshUpdate();
            }
            catch (System.Exception e) { Mod.LogWarning("Text '" + text + "' fehlgeschlagen: " + e.Message); }
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            return go;
        }
    }

    // Periodensystem-Poster (Br und Ba grün hervorgehoben).
    public class PeriodicPoster : DecoBase
    {
        public override string UniqueNameID => "Periodic Table Poster";
        protected override DecorationType Theme => DecorationType.Formal;
        protected override int[] VanillaIDs => new[] { ApplianceReferences.FancyPainting, ApplianceReferences.Painting, ApplianceReferences.FriendsPoster, ApplianceReferences.BunnyPoster, ApplianceReferences.AffordableNeonSign1 };
        protected override string Title => "Periodic Table";
        protected override string Desc => "Br + Ba. Respect the chemistry.";
        protected override Vector3 DefaultPos => new Vector3(0f, 0.85f, 0.45f);

        protected override void Build(Transform root)
        {
            const float cell = 0.05f, gap = 0.006f;
            const int cols = 18, rows = 7;
            float w = cols * (cell + gap), h = rows * (cell + gap) + 0.12f;
            Box(root, "Paper", new Vector3(w + 0.06f, h + 0.06f, 0.01f), new Vector3(0f, 0f, 0.005f), Materials.Paper);
            var colors = new[]
            {
                Materials.Solid("PT Alkali", new Color(0.95f, 0.55f, 0.45f)), Materials.Solid("PT Earth", new Color(0.98f, 0.8f, 0.45f)),
                Materials.Solid("PT Transition", new Color(0.6f, 0.75f, 0.95f)), Materials.Solid("PT Other", new Color(0.75f, 0.9f, 0.7f)),
                Materials.Solid("PT Nonmetal", new Color(0.85f, 0.7f, 0.95f)), Materials.Solid("PT Noble", new Color(0.65f, 0.9f, 0.95f)),
            };
            for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                bool present = r == 0 ? (c == 0 || c == 17) : r < 3 ? (c < 2 || c >= 12) : true;
                if (!present) continue;
                bool highlight = (r == 3 && c == 16) || (r == 5 && c == 1); // Br, Ba
                var mat = highlight ? Materials.Green
                    : c == 17 ? colors[5] : c == 0 ? colors[0] : c == 1 ? colors[1] : c < 12 ? colors[2] : c < 15 ? colors[3] : colors[4];
                float x = -w / 2f + (c + 0.5f) * (cell + gap);
                float y = h / 2f - 0.12f - (r + 0.5f) * (cell + gap);
                Box(root, $"Cell {r}-{c}", new Vector3(cell, cell, 0.012f), new Vector3(x, y, 0f), mat);
                if (highlight) Text(root, highlight && r == 3 ? "Br" : "Ba", r == 3 ? "Br" : "Ba", new Vector3(x, y, -0.008f), 0.35f, Color.white);
            }
            Text(root, "Title", "PERIODIC TABLE", new Vector3(0f, h / 2f - 0.05f, -0.008f), 0.6f, new Color(0.15f, 0.15f, 0.15f));
        }
    }

    // Gelbe Fässer (nur Deko).
    public class YellowBarrels : DecoBase
    {
        public override string UniqueNameID => "Yellow Barrels";
        protected override DecorationType Theme => DecorationType.Affordable;
        protected override int[] VanillaIDs => new[] { ApplianceReferences.CosyBarrel };
        protected override string Title => "Yellow Barrels";
        protected override string Desc => "Why does a kitchen produce so much chemical waste?";

        protected override void Build(Transform root)
        {
            Barrel(root, new Vector3(-0.2f, 0f, 0.05f), 0f);
            Barrel(root, new Vector3(0.2f, 0f, -0.05f), 30f);

            // Läuft leicht aus: grün leuchtende Spur am rechten Fass und eine Pfütze davor, dazu ein schwaches grünes Licht.
            var goo = Materials.Solid("Toxic Goo", new Color(0.55f, 1f, 0.2f));
            ProcMesh.Create("Drip", root, ProcMesh.Box(new Vector3(0.05f, 0.3f, 0.01f)), goo, new Vector3(0.2f, 0.18f, -0.232f));
            var puddle = ProcMesh.Create("Puddle", root, ProcMesh.Cylinder(0.16f, 0.006f, 16), goo, new Vector3(0.18f, 0.002f, -0.33f));
            puddle.transform.localScale = new Vector3(1.3f, 1f, 0.7f);
            ProcMesh.Create("Puddle Drop", root, ProcMesh.Cylinder(0.05f, 0.006f, 10), goo, new Vector3(-0.02f, 0.002f, -0.4f));
            var glow = new GameObject("Glow");
            glow.transform.SetParent(root, false);
            glow.transform.localPosition = new Vector3(0.1f, 0.35f, -0.35f);
            var light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.5f, 1f, 0.25f);
            light.range = Tuning.F("deco.barrels.glow.range", 1.2f);
            light.intensity = Tuning.F("deco.barrels.glow.intensity", 1.5f);
        }

        private static void Barrel(Transform root, Vector3 pos, float rot)
        {
            var b = new GameObject("Barrel");
            b.transform.SetParent(root, false);
            b.transform.localPosition = pos;
            b.transform.localRotation = Quaternion.Euler(0f, rot, 0f);
            ProcMesh.Create("Body", b.transform, ProcMesh.Cylinder(0.18f, 0.6f, 20), Materials.Hazmat, Vector3.zero);
            foreach (var y in new[] { 0.15f, 0.45f })
                ProcMesh.Create("Ring", b.transform, ProcMesh.Cylinder(0.185f, 0.025f, 20), Materials.DarkGrey, new Vector3(0f, y, 0f));
            ProcMesh.Create("Lid", b.transform, ProcMesh.Cylinder(0.17f, 0.012f, 20), Materials.DarkGrey, new Vector3(0f, 0.6f, 0f));
            ProcMesh.Create("Cap", b.transform, ProcMesh.Cylinder(0.03f, 0.02f, 10), Materials.Black, new Vector3(0.08f, 0.61f, 0f));
        }
    }

    // Los-Pollitos-Schild (Wand).
    public class LosPollitosSign : DecoBase
    {
        public override string UniqueNameID => "Los Pollitos Sign";
        protected override DecorationType Theme => DecorationType.Affordable;
        protected override int[] VanillaIDs => new[] { ApplianceReferences.FancyPainting, ApplianceReferences.Painting, ApplianceReferences.FriendsPoster, ApplianceReferences.BunnyPoster, ApplianceReferences.AffordableNeonSign1 };
        protected override string Title => "Los Pollitos Hermanos";
        protected override string Desc => "Taste the family. Totally unrelated to any other business.";
        protected override Vector3 DefaultPos => new Vector3(0f, 0.5f, 0.45f);

        protected override void Build(Transform root)
        {
            Box(root, "Board", new Vector3(0.9f, 0.32f, 0.03f), new Vector3(0f, 0f, 0.015f), Materials.PizzaYellow);
            Box(root, "Border", new Vector3(0.94f, 0.36f, 0.02f), new Vector3(0f, 0f, 0.03f), Materials.Red);
            Text(root, "Name", "LOS POLLITOS", new Vector3(0f, 0.05f, -0.005f), 1.1f, new Color(0.75f, 0.1f, 0.08f));
            Text(root, "Sub", "HERMANOS", new Vector3(0f, -0.08f, -0.005f), 0.7f, new Color(0.75f, 0.1f, 0.08f));
        }
    }
}
