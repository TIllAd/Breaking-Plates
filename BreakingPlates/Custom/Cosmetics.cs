using System.Collections.Generic;
using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using UnityEngine;

namespace BreakingPlates.Custom
{
    // Gelber Schutzanzug: Taucher-Outfit aus dem Spiel umgefärbt, Helm/Flaschen weg, Atemschutzmaske am Kopf.
    public class HazmatSuit : CustomPlayerCosmetic
    {
        private const int DiverChef = -950136670;
        public override string UniqueNameID => "Hazmat Suit";
        public override CosmeticType CosmeticType => CosmeticType.Outfit;

        private GameObject _visual;
        public override GameObject Visual => _visual ??= Make();

        private static GameObject Make()
        {
            var src = ((PlayerCosmetic)GDOUtils.GetExistingGDO(DiverChef)).Visual;
            var go = Object.Instantiate(src, PrefabUtil.Holder, false);
            go.name = "BreakingPlates Hazmat Suit";
            foreach (var r in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var n = r.gameObject.name;
                if (n.Contains("Helmet") || n.Contains("Tanks")) { r.gameObject.SetActive(false); continue; }
                if (n.Contains("Suit")) SetAll(r, Materials.Hazmat);
                else if (n.Contains("Gloves") || n.Contains("Boots")) SetAll(r, Materials.Black);
            }
            var outfit = go.GetComponent<PlayerOutfitComponent>();
            var mask = new GameObject("BP Mask");
            mask.transform.SetParent(go.transform, false);
            BuildMask(mask.transform);
            if (outfit != null) outfit.AttachToHeadBone = mask;
            return go;
        }

        private static void SetAll(Renderer r, Material m)
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++) mats[i] = m;
            r.sharedMaterials = mats;
        }

        // Atemschutzmaske: sitzt am Kopf-Knochen; Lage per F8 einstellbar (suit.mask.*).
        private static void BuildMask(Transform head)
        {
            var m = new GameObject("Mask");
            m.transform.SetParent(head, false);
            void Apply(GameObject t)
            {
                t.transform.localPosition = Tuning.V3("suit.mask.pos", new Vector3(0f, 0.12f, 0.16f));
                t.transform.localRotation = Quaternion.Euler(Tuning.V3("suit.mask.rot", Vector3.zero));
                t.transform.localScale = Vector3.one * Tuning.F("suit.mask.scale", 1f);
            }
            Apply(m);
            Tuning.Register(m, "suit.mask", Apply);
            ProcMesh.Create("Face", m.transform, ProcMesh.Box(new Vector3(0.16f, 0.09f, 0.06f)), Materials.Black, Vector3.zero);
            foreach (var x in new[] { -0.09f, 0.09f })
            {
                var f = ProcMesh.Create("Filter", m.transform, ProcMesh.Cylinder(0.035f, 0.04f, 12), Materials.DarkGrey, new Vector3(x, -0.02f, 0.02f));
                f.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            ProcMesh.Create("Goggles", m.transform, ProcMesh.Box(new Vector3(0.2f, 0.05f, 0.03f)), Materials.Glass, new Vector3(0f, 0.09f, -0.01f));
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
            var brim = ProcMesh.Create("Brim", model.transform, ProcMesh.Cylinder(0.24f, 0.015f, 24), Materials.Black, Vector3.zero);
            var crown = ProcMesh.Create("Crown", model.transform, ProcMesh.Lathe(new[]
            {
                new Vector2(0f, 0f), new Vector2(0.15f, 0f), new Vector2(0.15f, 0.1f), new Vector2(0.14f, 0.115f),
                new Vector2(0.1f, 0.115f), new Vector2(0.1f, 0.105f), new Vector2(0f, 0.105f)
            }, 24), Materials.Black, new Vector3(0f, 0.012f, 0f));
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
}
