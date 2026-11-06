using System.Linq;
using System.Collections.Generic;
using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using UnityEngine;

namespace BreakingPlates.Custom
{
    // Gelber Schutzanzug (Runde 3): Taucheranzug aus dem Spiel als glatter Overall (alles gelb, glänzend),
    // Stiefel als gelbe Überzieher, hellblaue Handschuhe; dazu der runde Astronauten-Helm als gelbe Kapuze mit
    // großem dunklem Sichtfenster (wie beim Vollschutzanzug). Variante B: ohne Helm, nur Sichtfenster am Kopf.
    // Gewählt: eigene Kapuze mit großem Sichtfenster (Variante B). Die Helm-Variante (Astronauten-Helm) bleibt nur im Code.
    public class HazmatSuit : HazmatSuitBase
    {
        public override string UniqueNameID => "Hazmat Suit";
        protected override bool Helmet => false;
    }

    public abstract class HazmatSuitBase : CustomPlayerCosmetic
    {
        private const int DiverChef = -950136670;
        private const int AstronautChef = 20631900;
        protected abstract bool Helmet { get; }
        public override CosmeticType CosmeticType => CosmeticType.Outfit;

        private GameObject _visual;
        public override GameObject Visual => _visual ??= Make();

        private GameObject Make()
        {
            var src = (GDOUtils.GetExistingGDO(DiverChef) as PlayerCosmetic)?.Visual
                ?? (GDOUtils.GetExistingGDO(AstronautChef) as PlayerCosmetic)?.Visual;
            var go = Object.Instantiate(src, PrefabUtil.Holder, false);
            go.name = "BreakingPlates " + UniqueNameID;
            var outfit = go.GetComponent<PlayerOutfitComponent>();
            var suit = Materials.Flat("Hazmat Shell", Tuning.C("color.hazmat.suit", new Color(0.98f, 0.84f, 0.12f)), Tuning.F("hazmat.shine", 0.7f));
            var seam = Materials.Flat("Hazmat Seam", Tuning.C("color.hazmat.seam", new Color(0.85f, 0.7f, 0.08f)), 0.6f);
            var gloves = Materials.Flat("Hazmat Gloves", Tuning.C("color.hazmat.gloves", new Color(0.45f, 0.78f, 0.95f)), 0.6f);
            var glass = Materials.Flat("Hazmat Visor Glass", new Color(0.16f, 0.22f, 0.3f), 1f);
            // Keine dunklen Flecken (Overlay des Shaders aus) – glatter, beschichteter Look.
            foreach (var m in new[] { suit, seam, gloves, glass }) { m.SetFloat("_OverlayMax", 0f); m.SetFloat("_OverlayMin", 0f); m.SetFloat("_Flatness", Tuning.F("hazmat.flatness", 0.5f)); }

            foreach (var r in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (r == null) continue;
                var n = r.name;
                if (n == "Body") continue;
                if (n.Contains("Gloves")) SetAll(r, gloves);
                else if (n.Contains("Boots")) SetAll(r, suit);
                else if (n.Contains("Suit"))
                {
                    var mats = r.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++) mats[i] = suit; // auch Gürtel/Besätze gelb: ein glatter Overall
                    r.sharedMaterials = mats;
                }
                else Hide(outfit, r); // Taucherhelm, Tanks
            }

            if (Helmet)
            {
                // Runden Astronauten-Helm aus dem anderen Outfit übernehmen (gleiches Skelett).
                var astro = (GDOUtils.GetExistingGDO(AstronautChef) as PlayerCosmetic)?.Visual;
                var helmSrc = astro?.GetComponentsInChildren<SkinnedMeshRenderer>(true).FirstOrDefault(r => r.name.Contains("Helmet"));
                var parent = go.GetComponentsInChildren<SkinnedMeshRenderer>(true).FirstOrDefault(r => r.name == "Body")?.transform.parent;
                if (helmSrc != null && parent != null)
                {
                    var helm = Object.Instantiate(helmSrc.gameObject, parent, false);
                    helm.name = "Hazmat Hood";
                    var hr = helm.GetComponent<SkinnedMeshRenderer>();
                    var mats = hr.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++) mats[i] = (mats[i] != null && mats[i].name.Contains("Red")) ? glass : suit;
                    hr.sharedMaterials = mats;
                    outfit?.Renderers.Add(hr);
                }
            }
            else
            {
                // Eigene Kapuze: gelbe Kugel um den Kopf, Kragen unten, großes dunkles Sichtfenster mit Rand vorne.
                var head = new GameObject("BP Hazmat Hood");
                head.transform.SetParent(go.transform, false);
                var hood = new GameObject("Hood");
                hood.transform.SetParent(head.transform, false);
                void Apply(GameObject t)
                {
                    t.transform.localPosition = Tuning.V3("hood.pos", new Vector3(0f, 0f, 0f));
                    t.transform.localScale = Vector3.one * Tuning.F("hood.scale", 1f);
                }
                Apply(hood);
                Tuning.Register(hood, "hood", Apply);
                Shapes.Ball("Hood", hood.transform, Tuning.V3("hood.size", new Vector3(0.58f, 0.6f, 0.56f)), suit, new Vector3(0f, 0.15f, 0f));
                Shapes.Ball("Collar", hood.transform, new Vector3(0.52f, 0.1f, 0.5f), seam, new Vector3(0f, -0.1f, 0f));
                var visor = new GameObject("Visor");
                visor.transform.SetParent(hood.transform, false);
                visor.transform.localPosition = Tuning.V3("visor.pos", new Vector3(0f, 0.15f, 0.25f));
                visor.transform.localRotation = Quaternion.Euler(-6f, 0f, 0f);
                Shapes.Ball("Rim", visor.transform, new Vector3(0.42f, 0.36f, 0.08f), seam, Vector3.zero);
                Shapes.Ball("Glass", visor.transform, new Vector3(0.36f, 0.3f, 0.08f), glass, new Vector3(0f, 0f, 0.012f));
                Shapes.Ball("Shine", visor.transform, new Vector3(0.08f, 0.04f, 0.02f), Materials.Flat("Hazmat Visor Shine", new Color(0.8f, 0.9f, 1f), 1f), new Vector3(-0.09f, 0.08f, 0.05f));
                if (outfit != null) outfit.AttachToHeadBone = head;
            }
            if (outfit != null) outfit.Hats = new List<Renderer>();
            return go;
        }

        private static void Hide(PlayerOutfitComponent outfit, SkinnedMeshRenderer r)
        {
            if (outfit != null) { outfit.Renderers.Remove(r); outfit.Hats?.Remove(r); }
            r.sharedMesh = null;
            r.enabled = false;
        }

        private static void SetAll(Renderer r, Material m)
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++) mats[i] = m;
            r.sharedMaterials = mats;
        }

        public override List<(Locale, CosmeticInfo)> InfoList => new List<(Locale, CosmeticInfo)>
        {
            (Locale.English, new CosmeticInfo { Name = "Hazmat Suit", Description = "Respect the chemistry." })
        };
    }

    // Heisenberg-Hut: schwarzer Pork-Pie-Hut, eigenes Modell am Kopf-Knochen.
    public class HeisenbergHat : CustomPlayerCosmetic
    {
        public override string UniqueNameID => "Heisenberg Hat";
        public override CosmeticType CosmeticType => CosmeticType.Hat;

        private GameObject _visual;
        public override GameObject Visual => _visual ??= Make();

        private static GameObject Make()
        {
            var go = PrefabUtil.CreateRoot("BreakingPlates Heisenberg Hat");
            var outfit = go.AddComponent<PlayerOutfitComponent>();
            var hat = new GameObject("BP Hat");
            hat.transform.SetParent(go.transform, false);
            var model = new GameObject("Hat");
            model.transform.SetParent(hat.transform, false);
            void Apply(GameObject t)
            {
                t.transform.localPosition = Tuning.V3("hat.pos", new Vector3(0f, 0.27f, 0f));
                t.transform.localRotation = Quaternion.Euler(Tuning.V3("hat.rot", Vector3.zero));
                t.transform.localScale = Vector3.one * Tuning.F("hat.scale", 1f);
            }
            Apply(model);
            Tuning.Register(model, "hat", Apply);
            var brim = ProcMesh.Create("Brim", model.transform, ProcMesh.Cylinder(0.21f, 0.012f, 24), Materials.Black, Vector3.zero);
            ProcMesh.Create("Crown", model.transform, ProcMesh.Cylinder(0.14f, 0.11f, 24), Materials.Black, new Vector3(0f, 0.012f, 0f));
            ProcMesh.Create("Band", model.transform, ProcMesh.Cylinder(0.153f, 0.025f, 24), Materials.DarkGrey, new Vector3(0f, 0.018f, 0f));
            outfit.AttachToHeadBone = hat;
            outfit.Renderers = new List<SkinnedMeshRenderer>();
            outfit.Hats = new List<Renderer>();
            return go;
        }

        public override List<(Locale, CosmeticInfo)> InfoList => new List<(Locale, CosmeticInfo)>
        {
            (Locale.English, new CosmeticInfo { Name = "Heisenberg", Description = "Say my name." })
        };
    }

    // Gasmaske als eigener Hut (passt zum Schutzanzug): dunkles Mundstück, zwei pinke Filter.
    public class GasMask : CustomPlayerCosmetic
    {
        public override string UniqueNameID => "Gas Mask";
        public override CosmeticType CosmeticType => CosmeticType.Hat;

        private GameObject _visual;
        public override GameObject Visual => _visual ??= Make();

        private static GameObject Make()
        {
            var go = PrefabUtil.CreateRoot("BreakingPlates Gas Mask");
            var outfit = go.AddComponent<PlayerOutfitComponent>();
            var head = new GameObject("BP Gas Mask");
            head.transform.SetParent(go.transform, false);
            var m = new GameObject("Mask");
            m.transform.SetParent(head.transform, false);
            void Apply(GameObject t)
            {
                t.transform.localPosition = Tuning.V3("mask.pos", new Vector3(0f, 0.03f, 0.21f));
                t.transform.localRotation = Quaternion.Euler(Tuning.V3("mask.rot", Vector3.zero));
                t.transform.localScale = Vector3.one * Tuning.F("mask.scale", 1f);
            }
            Apply(m);
            Tuning.Register(m, "mask", Apply);
            // Maske (Form B): dunkle Halbmaske mit einem großen roten Filter vorne.
            var maskMat = Materials.Solid("Gas Mask", new Color(0.22f, 0.22f, 0.24f));
            var filterRed = Materials.Solid("Gas Mask Filter", new Color(0.88f, 0.29f, 0.23f));
            var filterDark = Materials.Solid("Gas Mask Filter Inner", new Color(0.48f, 0.12f, 0.09f));
            Shapes.Ball("Face", m.transform, new Vector3(0.2f, 0.14f, 0.1f), maskMat, new Vector3(0f, -0.01f, 0f));
            var housing = ProcMesh.Create("Housing", m.transform, ProcMesh.Cylinder(0.035f, 0.03f, 14), Materials.Black, new Vector3(0f, -0.03f, 0.04f));
            housing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var filter = ProcMesh.Create("Filter", m.transform, ProcMesh.Cylinder(0.055f, 0.04f, 18), filterRed, new Vector3(0f, -0.03f, 0.065f));
            filter.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var inner = ProcMesh.Create("Filter Inner", m.transform, ProcMesh.Cylinder(0.028f, 0.01f, 14), filterDark, new Vector3(0f, -0.03f, 0.1f));
            inner.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            // Brille (aus A): schwarzer Rahmen mit zwei runden hellblauen Gläsern, Riemen um den Kopf.
            var g = new GameObject("Goggles");
            g.transform.SetParent(head.transform, false);
            void ApplyG(GameObject t)
            {
                t.transform.localPosition = Tuning.V3("goggles.pos", new Vector3(0f, 0.12f, 0.2f));
                t.transform.localRotation = Quaternion.Euler(Tuning.V3("goggles.rot", Vector3.zero));
                t.transform.localScale = Vector3.one * Tuning.F("goggles.scale", 1f);
            }
            ApplyG(g);
            Tuning.Register(g, "goggles", ApplyG);
            var lens = Materials.Solid("Goggle Lens", new Color(0.62f, 0.83f, 0.91f));
            ProcMesh.Create("Frame", g.transform, ProcMesh.Box(new Vector3(0.22f, 0.075f, 0.035f)), Materials.Black, Vector3.zero);
            foreach (var x in new[] { -1f, 1f })
            {
                var l = ProcMesh.Create("Lens", g.transform, ProcMesh.Cylinder(0.03f, 0.012f, 16), lens, new Vector3(x * 0.052f, 0f, 0.012f));
                l.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                ProcMesh.Create("Strap Side", g.transform, ProcMesh.Box(new Vector3(0.02f, 0.03f, 0.36f)), Materials.Black, new Vector3(x * 0.215f, 0f, -0.19f));
            }
            ProcMesh.Create("Strap Back", g.transform, ProcMesh.Box(new Vector3(0.45f, 0.03f, 0.02f)), Materials.Black, new Vector3(0f, 0f, -0.37f));
            outfit.AttachToHeadBone = head;
            outfit.Renderers = new List<SkinnedMeshRenderer>();
            outfit.Hats = new List<Renderer>();
            return go;
        }

        public override List<(Locale, CosmeticInfo)> InfoList => new List<(Locale, CosmeticInfo)>
        {
            (Locale.English, new CosmeticInfo { Name = "Gas Mask", Description = "Pairs well with a yellow suit." })
        };
    }
}
