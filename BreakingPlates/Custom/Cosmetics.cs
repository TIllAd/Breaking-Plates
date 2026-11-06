using System.Linq;
using System.Collections.Generic;
using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using UnityEngine;

namespace BreakingPlates.Custom
{
    // Gelber Schutzanzug: Taucheranzug aus dem Spiel als glatter Overall (alles gelb, leicht glänzend),
    // Stiefel als gelbe Überzieher, hellblaue Handschuhe, eigene Kapuze mit großem dunklem Visier.
    public class HazmatSuit : CustomPlayerCosmetic
    {
        public override string UniqueNameID => "Hazmat Suit";
        private const int DiverChef = -950136670;
        public override CosmeticType CosmeticType => CosmeticType.Outfit;

        private GameObject _visual;
        public override GameObject Visual => _visual ??= Make();

        private GameObject Make()
        {
            var src = (GDOUtils.GetExistingGDO(DiverChef) as PlayerCosmetic)?.Visual;
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
                SetAll(r, suit);
            }

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
            if (outfit != null) outfit.AttachToHeadBone = head;
            go.AddComponent<HeadFollower>();
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
            (Locale.English, new CosmeticInfo { Name = "Hazmat Suit", Description = "Cook responsibly." })
        };
    }

    // Gasmaske als eigener Hut: Vollmaske mit großen Augengläsern, rotem Filter und Schlauch.
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

            // Vollmaske: dunkle Gesichtsplatte, zwei große runde Augengläser, roter Filter vorne,
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
