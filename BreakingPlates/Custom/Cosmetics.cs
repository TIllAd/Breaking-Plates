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
    // Grund-Outfit-Vergleich (Runde 4): Taucheranzug gewann gegen Nightmare-Chef (Knopfleiste), Mumie (Bandagen) und Astronaut (Brust-Panel).

    public abstract class HazmatSuitBase : CustomPlayerCosmetic
    {
        private const int DiverChef = -950136670;
        private const int AstronautChef = 20631900;
        protected abstract bool Helmet { get; }
        protected virtual int BaseOutfit => DiverChef;
        public override CosmeticType CosmeticType => CosmeticType.Outfit;

        private GameObject _visual;
        public override GameObject Visual => _visual ??= Make();

        private GameObject Make()
        {
            var src = (GDOUtils.GetExistingGDO(BaseOutfit) as PlayerCosmetic)?.Visual
                ?? (GDOUtils.GetExistingGDO(DiverChef) as PlayerCosmetic)?.Visual
                ?? (GDOUtils.GetExistingGDO(AstronautChef) as PlayerCosmetic)?.Visual;
            var go = Object.Instantiate(src, PrefabUtil.Holder, false);
            go.name = "BreakingPlates " + UniqueNameID;
            var outfit = go.GetComponent<PlayerOutfitComponent>();
            var suit = Materials.Flat("Hazmat Shell", Tuning.C("color.hazmat.suit", new Color(0.98f, 0.84f, 0.12f)), Tuning.F("hazmat.shine", 0.6f));
            var seam = Materials.Flat("Hazmat Seam", Tuning.C("color.hazmat.seam", new Color(0.85f, 0.7f, 0.08f)), 0.6f);
            var gloves = Materials.Flat("Hazmat Gloves", Tuning.C("color.hazmat.gloves", new Color(0.45f, 0.78f, 0.95f)), 0.6f);
            var glass = Materials.Flat("Hazmat Visor Glass", new Color(0.2f, 0.27f, 0.36f), 1f);
            // Keine dunklen Flecken (Overlay des Shaders aus) – glatter, beschichteter Look.
            foreach (var m in new[] { suit, seam, gloves, glass }) { m.SetFloat("_OverlayMax", 0f); m.SetFloat("_OverlayMin", 0f); m.SetFloat("_Flatness", Tuning.F("hazmat.flatness", 0.5f)); }

            foreach (var r in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (r == null) continue;
                var n = r.name;
                if (n == "Body") continue;
                if (n.Contains("Gloves")) { SetAll(r, gloves); continue; }
                string[] extras = { "Helmet", "Hat", "Tanks", "Badge", "Buttons", "Strap", "Eyes", "Teeth", "Belt", "Apron", "Scarf", "Tie" };
                if (System.Array.Exists(extras, e => n.Contains(e))) { Hide(outfit, r); continue; }
                // Anzug, Hose, Stiefel: alles gelb, auch Besätze – ein glatter Overall.
                SetAll(r, suit); // Taucherhelm, Tanks
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
                Shapes.Ball("Hood", hood.transform, Tuning.V3("hood.size", new Vector3(0.56f, 0.62f, 0.54f)), suit, new Vector3(0f, 0.15f, 0f));
                // Verjüngung zum Hals: weniger Kugel, mehr weiche Kapuze.
                Shapes.Ball("Taper", hood.transform, new Vector3(0.46f, 0.22f, 0.44f), suit, new Vector3(0f, -0.01f, 0f));
                // Dichtung am Hals: flach und in Anzugfarbe, damit Kapuze und Anzug wie ein Stück wirken.
                Shapes.Ball("Seal", hood.transform, new Vector3(0.4f, 0.06f, 0.38f), suit, new Vector3(0f, -0.06f, 0f)); // schmaler als die Verjüngung: keine seitlichen Spitzen
                var visor = new GameObject("Visor");
                visor.transform.SetParent(hood.transform, false);
                // Lage live mit F8 einstellbar: visor.pos und visor.rot (x = Kippung, oben nach hinten positiv; y = Drehung; z = Neigung zur Seite).
                void ApplyVisor(GameObject t)
                {
                    t.transform.localPosition = Tuning.V3("visor.pos", new Vector3(0f, 0.15f, 0.25f));
                    t.transform.localRotation = Quaternion.Euler(Tuning.V3("visor.rot", new Vector3(5f, 5f, 20f)));
                }
                ApplyVisor(visor);
                Tuning.Register(visor, "visor", ApplyVisor);
                // Visier tief in die Kapuze gedrückt: große, flache Ellipsoide, die aus der Rundung herauswachsen statt wie ein Teller aufzuliegen.
                Shapes.Ball("Rim", visor.transform, new Vector3(0.39f, 0.33f, 0.2f), seam, new Vector3(0f, 0f, -0.058f)); // schmaler Rahmen, mehr Glas
                Shapes.Ball("Glass", visor.transform, new Vector3(0.36f, 0.3f, 0.2f), glass, new Vector3(0f, 0f, -0.042f));
                // Dezente, größere Reflexion statt weißem Punkt
                // Keine aufgesetzte Reflexion: das Glas glänzt selbst (Shininess), die flache Scheibe stand bei gekipptem Visier heraus.
                // Spielerfarbe: Anzug, Kapuze und Rand übernehmen die gewählte Farbe der Figur.
                var tint = go.AddComponent<HazmatTint>();
                tint.Outfit = outfit;
                tint.Main = new List<Renderer>();
                tint.Dark = new List<Renderer>();
                foreach (var rr in go.GetComponentsInChildren<Renderer>(true))
                {
                    var sm = rr.sharedMaterial;
                    if (sm == suit) tint.Main.Add(rr);
                    else if (sm == seam) tint.Dark.Add(rr);
                }
                if (outfit != null) outfit.AttachToHeadBone = head;
                go.AddComponent<HeadFollower>();
            }
            if (outfit != null) outfit.Hats = new List<Renderer>();
            return go;
        }

        // Warnschild wie im echten Leben: schwarzes Dreieck (Rand), gelbes Dreieck, schwarzes Kleeblatt.
        private static void WarningSign(Transform parent, float size)
        {
            var yellow = Materials.Flat("Hazard Sign Yellow", new Color(1f, 0.82f, 0.1f), 0.2f);
            yellow.SetFloat("_OverlayMax", 0f);
            ProcMesh.Create("Outline", parent, Triangle(size), Materials.Black, Vector3.zero);
            ProcMesh.Create("Fill", parent, Triangle(size * 0.8f), yellow, new Vector3(0f, -size * 0.02f, -0.004f));
            var t = new GameObject("Trefoil");
            t.transform.SetParent(parent, false);
            t.transform.localPosition = new Vector3(0f, -size * 0.08f, -0.008f);
            Trefoil(t, size * 0.3f, Materials.Black);
        }

        private static Mesh Triangle(float size)
        {
            float h = size * 0.866f;
            var verts = new List<Vector3> { new Vector3(-size / 2f, -h / 3f, 0f), new Vector3(size / 2f, -h / 3f, 0f), new Vector3(0f, 2f * h / 3f, 0f) };
            var mesh = new Mesh { name = "BreakingPlates Triangle" };
            mesh.SetVertices(verts); mesh.SetTriangles(new List<int> { 0, 2, 1, 0, 1, 2 }, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }

        // Strahlungs-Warnzeichen: Punkt in der Mitte + drei 60°-Segmente.
        private static void Trefoil(GameObject parentGo, float radius, Material m) => Trefoil(parentGo.transform, radius, m);
        private static void Trefoil(Transform parent, float radius, Material m)
        {
            ProcMesh.Create("Dot", parent, ProcMesh.Cylinder(radius * 0.17f, 0.006f, 16), m, Vector3.zero).transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            for (int i = 0; i < 3; i++)
                ProcMesh.Create("Blade " + i, parent, Sector(radius * 0.28f, radius, 90f + i * 120f, 60f), m, Vector3.zero);
        }

        private static Mesh Sector(float inner, float outer, float centerDeg, float spanDeg, int steps = 10)
        {
            var verts = new List<Vector3>(); var tris = new List<int>();
            for (int i = 0; i <= steps; i++)
            {
                float a = (centerDeg - spanDeg / 2f + spanDeg * i / steps) * Mathf.Deg2Rad;
                verts.Add(new Vector3(Mathf.Cos(a) * inner, Mathf.Sin(a) * inner, 0f));
                verts.Add(new Vector3(Mathf.Cos(a) * outer, Mathf.Sin(a) * outer, 0f));
            }
            for (int i = 0; i < steps; i++)
            {
                int b = i * 2;
                // beidseitig
                tris.AddRange(new[] { b, b + 1, b + 2, b + 1, b + 3, b + 2, b, b + 2, b + 1, b + 1, b + 2, b + 3 });
            }
            var mesh = new Mesh { name = "BreakingPlates Sector" };
            mesh.SetVertices(verts); mesh.SetTriangles(tris, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
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
                t.transform.localPosition = Tuning.V3("mask.pos", Vector3.zero);
                t.transform.localRotation = Quaternion.Euler(Tuning.V3("mask.rot", Vector3.zero));
                t.transform.localScale = Vector3.one * Tuning.F("mask.scale", 1f);
            }
            Apply(m);
            Tuning.Register(m, "mask", Apply);

            // Vollmaske: dunkle Gesichtsplatte, zwei große runde Augengläser, roter Filter vorne, Riemen um den Kopf,
            // dazu ein kurzer geriffelter Schlauch vom Filter nach unten.
            var maskMat = Materials.Flat("Gas Mask", new Color(0.2f, 0.2f, 0.22f), 0.4f);
            var rubber = Materials.Flat("Gas Mask Rubber", new Color(0.1f, 0.1f, 0.11f), 0.3f);
            var lens = Materials.Flat("Gas Mask Lens", new Color(0.55f, 0.75f, 0.85f), 1f);
            var filterRed = Materials.Solid("Gas Mask Filter", new Color(0.88f, 0.29f, 0.23f));
            var filterDark = Materials.Solid("Gas Mask Filter Inner", new Color(0.48f, 0.12f, 0.09f));
            foreach (var mat in new[] { maskMat, rubber, lens }) mat.SetFloat("_OverlayMax", 0f);

            Shapes.Ball("Face Plate", m.transform, new Vector3(0.36f, 0.34f, 0.16f), maskMat, new Vector3(0f, 0.07f, 0.2f));
            foreach (var x in new[] { -1f, 1f })
            {
                var eye = new GameObject("Eye");
                eye.transform.SetParent(m.transform, false);
                eye.transform.localPosition = new Vector3(x * 0.075f, 0.12f, 0.272f);
                eye.transform.localRotation = Quaternion.Euler(90f - 8f, x * 14f, 0f);
                ProcMesh.Create("Rim", eye.transform, ProcMesh.Cylinder(0.062f, 0.022f, 20), rubber, Vector3.zero);
                ProcMesh.Create("Lens", eye.transform, ProcMesh.Cylinder(0.05f, 0.026f, 20), lens, Vector3.zero);
            }
            var housing = ProcMesh.Create("Housing", m.transform, ProcMesh.Cylinder(0.04f, 0.04f, 14), rubber, new Vector3(0f, -0.01f, 0.27f));
            housing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var filter = ProcMesh.Create("Filter", m.transform, ProcMesh.Cylinder(0.06f, 0.045f, 18), filterRed, new Vector3(0f, -0.015f, 0.3f));
            filter.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var inner = ProcMesh.Create("Filter Inner", m.transform, ProcMesh.Cylinder(0.03f, 0.01f, 14), filterDark, new Vector3(0f, -0.015f, 0.345f));
            inner.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            // (Riemen um den Kopf entfernt – sahen wie ein Kasten um den Kopf aus.)
            // Schlauch: Ringe entlang eines Bogens vom Filter nach unten/seitlich
            if (Tuning.F("mask.hose", 1f) > 0.5f)
            {
                var hose = new GameObject("Hose");
                hose.transform.SetParent(m.transform, false);
                Vector3 p0 = new Vector3(0f, -0.06f, 0.3f), p1 = new Vector3(0.05f, -0.22f, 0.34f), p2 = new Vector3(0.14f, -0.32f, 0.2f);
                const int rings = 9;
                for (int i = 0; i <= rings; i++)
                {
                    float t = i / (float)rings;
                    var pos = (1 - t) * (1 - t) * p0 + 2 * (1 - t) * t * p1 + t * t * p2;
                    var tan = (2 * (1 - t) * (p1 - p0) + 2 * t * (p2 - p1)).normalized;
                    var ring = ProcMesh.Create("Ring", hose.transform, ProcMesh.Cylinder(0.026f, 0.018f, 12), i % 2 == 0 ? rubber : maskMat, pos);
                    ring.transform.localRotation = Quaternion.FromToRotation(Vector3.up, tan);
                }
            }
            outfit.AttachToHeadBone = head;
            outfit.Renderers = new List<SkinnedMeshRenderer>();
            outfit.Hats = new List<Renderer>();
            go.AddComponent<HeadFollower>();
            return go;
        }

        public override List<(Locale, CosmeticInfo)> InfoList => new List<(Locale, CosmeticInfo)>
        {
            (Locale.English, new CosmeticInfo { Name = "Gas Mask", Description = "Pairs well with a yellow suit." })
        };
    }

    // Färbt den Anzug in der Spielerfarbe (die das Spiel auf den Körper legt). Ohne Spieler (Vorschau) bleibt er gelb.
    public class HazmatTint : MonoBehaviour
    {
        public PlayerOutfitComponent Outfit;
        public List<Renderer> Main = new List<Renderer>(), Dark = new List<Renderer>();
        private static readonly int Color0 = Shader.PropertyToID("_Color0");
        private MaterialPropertyBlock _block;
        private Color _last = new Color(-1f, 0f, 0f, 0f);

        private void LateUpdate()
        {
            if (Tuning.F("hazmat.playercolour", 0f) < 0.5f) return;
            var src = Outfit != null ? Outfit.Source : null;
            if (src == null || src.sharedMaterial == null || !src.sharedMaterial.HasProperty(Color0)) return;
            var c = src.sharedMaterial.GetColor(Color0);
            if (c == _last) return;
            _last = c;
            _block ??= new MaterialPropertyBlock();
            var main = new Color(c.r, c.g, c.b, 0f);
            var dark = new Color(c.r * 0.82f, c.g * 0.82f, c.b * 0.82f, 0f);
            foreach (var r in Main) Set(r, main);
            foreach (var r in Dark) Set(r, dark);
        }

        private void Set(Renderer r, Color c)
        {
            if (r == null) return;
            r.GetPropertyBlock(_block);
            _block.SetColor(Color0, c);
            r.SetPropertyBlock(_block);
        }
    }

    // Hängt Kapuze/Maske NICHT als Kind an den Kopf-Knochen (das bringt den Animator der Spielfigur beim Szenenwechsel
    // zum Absturz: "Transform ... not found in HumanDescription"), sondern lässt sie dem Kopf jedes Bild folgen.
    [DefaultExecutionOrder(10000)] // nach den Animations-/IK-Skripten der Figur, damit nichts nachwackelt
    public class HeadFollower : MonoBehaviour
    {
        private PlayerOutfitComponent _outfit;
        private GameObject _obj;
        private Transform _head;

        private void Awake()
        {
            _outfit = GetComponent<PlayerOutfitComponent>();
            if (_outfit == null || _outfit.AttachToHeadBone == null) return;
            _obj = _outfit.AttachToHeadBone;
            _outfit.AttachToHeadBone = null; // das Spiel soll es nicht in die Knochen hängen
            _obj.transform.SetParent(null, false);
            DontDestroyOnLoad(_obj);
            _obj.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_obj == null) return;
            if (_head == null)
            {
                _head = FindHead();
                if (_head == null) return;
            }
            if (!_obj.activeSelf) _obj.SetActive(true);
            _obj.transform.SetPositionAndRotation(_head.position, _head.rotation);
            _obj.transform.localScale = _head.lossyScale;
        }

        // Kopf-Knochen der Spielfigur finden: das Spiel setzt "Source" nicht immer, aber es gibt den Renderern
        // des Outfits die Knochen der Figur. Für Hüte (ohne eigene Renderer) den Körper der Figur oberhalb suchen.
        private Transform FindHead()
        {
            if (_outfit != null && _outfit.Source != null) { var h = Head(_outfit.Source.bones); if (h != null) return h; }
            foreach (var r in GetComponentsInChildren<SkinnedMeshRenderer>(true)) { var h = Head(r.bones); if (h != null && !h.IsChildOf(transform)) return h; }
            for (var t = transform.parent; t != null; t = t.parent)
                foreach (var r in t.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    if (r.transform.IsChildOf(transform)) continue;
                    var h = Head(r.bones);
                    if (h != null) return h;
                }
            return null;
        }

        private static Transform Head(Transform[] bones)
        {
            if (bones == null) return null;
            foreach (var b in bones) if (b != null && b.name == "Head") return b;
            return null;
        }

        private void OnDisable() { if (_obj != null) _obj.SetActive(false); }
        private void OnDestroy() { if (_obj != null) Destroy(_obj); }
    }
}
