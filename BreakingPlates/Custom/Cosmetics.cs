using System.Collections.Generic;
using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using UnityEngine;

namespace BreakingPlates.Custom
{
    // Gelber Schutzanzug: Astronauten-Outfit aus dem Spiel umgefärbt (blassgelb, blaue Handschuhe), Helm weg. Gasmaske ist ein eigener Hut.
    public class HazmatSuit : CustomPlayerCosmetic
    {
        private const int AstronautChef = 20631900; // einteiliger Anzug ohne Gurte/Schnallen
        public override string UniqueNameID => "Hazmat Suit";
        public override CosmeticType CosmeticType => CosmeticType.Outfit;

        private GameObject _visual;
        public override GameObject Visual => _visual ??= Make();

        private static GameObject Make()
        {
            var src = ((PlayerCosmetic)GDOUtils.GetExistingGDO(AstronautChef)).Visual;
            var go = Object.Instantiate(src, PrefabUtil.Holder, false);
            go.name = "BreakingPlates Hazmat Suit";
            var outfit = go.GetComponent<PlayerOutfitComponent>();
            foreach (var r in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var n = r.gameObject.name;
                if (n.Contains("Helmet") || n.Contains("Tanks") || n.Contains("Pack"))
                {
                    // Ganz entfernen: das Spiel blendet "Hüte" (der Helm) sonst wieder ein.
                    if (outfit != null) { outfit.Renderers.Remove(r); outfit.Hats?.Remove(r); }
                    Object.DestroyImmediate(r.gameObject);
                    continue;
                }
                if (n.Contains("Suit")) SetAll(r, Materials.Hazmat);
                else if (n.Contains("Gloves")) SetAll(r, Materials.GloveBlue);
                else if (n.Contains("Boots")) SetAll(r, Materials.DarkGrey);
            }
            return go;
        }

        private static void SetAll(Renderer r, Material m)
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++) mats[i] = m;
            r.sharedMaterials = mats;
        }

        // Kapuze: sitzt am Kopf-Knochen; Lage per F8 einstellbar (suit.hood.*). Die Gasmaske ist ein eigener Hut.
        private static void BuildMask(Transform head)
        {
            // Gelbe Kapuze um den Kopf (statt Taucherhelm).
            var hood = new GameObject("Hood");
            hood.transform.SetParent(head, false);
            void ApplyHood(GameObject t)
            {
                t.transform.localPosition = Tuning.V3("suit.hood.pos", new Vector3(0f, 0.02f, 0f));
                t.transform.localScale = Tuning.V3("suit.hood.scale", new Vector3(1f, 1f, 1f));
            }
            ApplyHood(hood);
            Tuning.Register(hood, "suit.hood", ApplyHood);
            ProcMesh.Create("Dome", hood.transform, ProcMesh.Lathe(new[]
            {
                new Vector2(0.23f, 0.12f), new Vector2(0.22f, 0.22f),
                new Vector2(0.17f, 0.31f), new Vector2(0.09f, 0.36f), new Vector2(0f, 0.37f)
            }, 20, doubleSided: true), Materials.Hazmat, Vector3.zero);
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
            ProcMesh.Create("Face", m.transform, ProcMesh.Box(new Vector3(0.16f, 0.11f, 0.06f)), Materials.DarkGrey, Vector3.zero);
            ProcMesh.Create("Valve", m.transform, ProcMesh.Box(new Vector3(0.06f, 0.05f, 0.04f)), Materials.Black, new Vector3(0f, -0.02f, 0.04f));
            foreach (var x in new[] { -1f, 1f })
            {
                var f = ProcMesh.Create("Filter", m.transform, ProcMesh.Cylinder(0.05f, 0.045f, 14), Materials.FilterPink, new Vector3(x * 0.1f, 0f, 0.01f));
                f.transform.localRotation = Quaternion.Euler(0f, 0f, x * 90f) * Quaternion.Euler(0f, 0f, 0f);
            }
            ProcMesh.Create("Strap", m.transform, ProcMesh.Box(new Vector3(0.36f, 0.025f, 0.02f)), Materials.Black, new Vector3(0f, 0.05f, -0.12f));
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
