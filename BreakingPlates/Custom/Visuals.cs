using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using UnityEngine;

namespace BreakingPlates.Custom
{
    // Wiederverwendbare Bausteine für Töpfe und Kristalle.
    public static class Visuals
    {
        public static Item Ref(int id) => (Item)GDOUtils.GetExistingGDO(id);

        public static GameObject Pot(Transform parent) =>
            PrefabUtil.CloneVisual(Ref(ItemReferences.Pot).Prefab.transform.Find("Pot").gameObject, parent, "Pot", Vector3.zero);

        // Flüssigkeit im Topf (Wasser-Mesh aus dem Spiel, schmaler als im Original, damit es nicht unten raussteht).
        public static GameObject Liquid(Transform parent, string name, float height, Material material = null)
        {
            var go = PrefabUtil.CloneVisual(Ref(ItemReferences.Pot).Prefab.transform.Find("Water").gameObject, parent, name, Vector3.zero);
            go.transform.localPosition = new Vector3(0f, 0.1f + height / 2f, 0f);
            go.transform.localScale = new Vector3(0.52f, height / 2f, 0.52f);
            if (material != null) Materials.Apply(go, material);
            return go;
        }

        // Ein blauer Kristall: Eiswürfel-Mesh aus IngredientLib mit eigenem Material.
        public static GameObject Crystal(Transform parent, string name, Vector3 pos, float scale, float yRot = 0f)
        {
            var ice = (CustomItem<Item>)GDOUtils.GetCustomGameDataObject<IngredientLib.Ingredient.Items.Ice>();
            var go = PrefabUtil.CloneVisual(ice.Prefab.transform.Find("ice").gameObject, parent, name, pos, scale);
            go.transform.localRotation = Quaternion.Euler(Random.Range(-20f, 20f), yRot, Random.Range(-20f, 20f));
            Materials.Apply(go, Materials.BlueCrystal);
            return go;
        }

        // Zuckerberg: viele kleine weiße Würfel, in der Mitte höher aufgehäuft.
        public static GameObject SugarHeap(Transform parent, string name, Vector3 pos)
        {
            var heap = new GameObject(name);
            heap.transform.SetParent(parent, false);
            heap.transform.localPosition = pos;
            var rnd = new System.Random(42);
            for (int layer = 0; layer < 3; layer++)
            {
                int n = new[] { 10, 6, 3 }[layer];
                float radius = new[] { 0.15f, 0.09f, 0.04f }[layer];
                for (int i = 0; i < n; i++)
                {
                    float a = (i / (float)n) * Mathf.PI * 2f + layer;
                    float r = radius * (0.6f + 0.4f * (float)rnd.NextDouble());
                    var c = Crystal(heap.transform, $"Sugar {layer}-{i}", new Vector3(Mathf.Cos(a) * r, layer * 0.035f, Mathf.Sin(a) * r), 0.28f, (float)rnd.NextDouble() * 360f);
                    Materials.Apply(c, Materials.Sugar);
                }
            }
            return heap;
        }

        // EXPERIMENT Kolben: Kugel aus zwei Rührschüssel-Hälften, Hals aus dem Wasser-Zylinder,
        // Glas-Material vom Forschungs-Kolben, unten blaue Flüssigkeit.
        public static GameObject Flask(Transform parent, string name, Vector3 pos, float scale = 1f)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = pos;
            root.transform.localScale = Vector3.one * scale;

            var glass = Ref(ItemReferences.ResearchFlask).Prefab.transform.Find("ResearchFlask/Cube.001").GetComponent<MeshRenderer>().sharedMaterial;
            var bowl = Ref(ItemReferences.MixingBowlEmpty).Prefab.transform.Find("MixingBowl").gameObject;
            var water = Ref(ItemReferences.Pot).Prefab.transform.Find("Water").gameObject;

            // Bauch: untere Hälfte + umgedrehte obere Hälfte
            var lower = PrefabUtil.CloneVisual(bowl, root.transform, "Bulb Lower", new Vector3(0f, 0.16f, 0f), 0.75f);
            var upper = PrefabUtil.CloneVisual(bowl, root.transform, "Bulb Upper", new Vector3(0f, 0.16f, 0f), 0.75f);
            upper.transform.localRotation = Quaternion.Euler(180f, 0f, 0f) * upper.transform.localRotation;
            Materials.Apply(lower, glass);
            Materials.Apply(upper, glass);

            // Flüssigkeit im unteren Teil
            var liquid = PrefabUtil.CloneVisual(bowl, root.transform, "Liquid", new Vector3(0f, 0.15f, 0f), 0.62f);
            Materials.Apply(liquid, Materials.BlueSyrup);

            // Hals
            var neck = PrefabUtil.CloneVisual(water, root.transform, "Neck", new Vector3(0f, 0.42f, 0f));
            neck.transform.localScale = new Vector3(0.11f, 0.13f, 0.11f);
            Materials.Apply(neck, glass);
            return root;
        }

        // Folien-Tütchen (im Code erzeugt): durchsichtige, aufgeblähte Folie + roter Druckverschluss oben.
        // Live einstellbar (F8): bag.width, bag.height, bag.bulge, bag.zip.size
        public static GameObject Bag(Transform parent)
        {
            var root = new GameObject("Bag");
            root.transform.SetParent(parent, false);
            ProcMesh.Create("Foil", root.transform, ProcMesh.Pouch(), Materials.Plastic, Vector3.zero);
            ProcMesh.Create("Zip", root.transform, ProcMesh.Box(Vector3.one), Materials.ZipRed, Vector3.zero);
            ApplyBag(root);
            Tuning.Register(root, "bag", ApplyBag);
            return root;
        }

        private static void ApplyBag(GameObject root)
        {
            float w = Tuning.F("bag.width", 0.34f), h = Tuning.F("bag.height", 0.38f), b = Tuning.F("bag.bulge", 0.09f);
            float tilt = Tuning.F("bag.tilt", 7f), skew = Tuning.F("bag.skew", 0.025f), wobble = Tuning.F("bag.wobble", 0.3f);
            float lean = Tuning.F("bag.lean", 4f);
            root.transform.localRotation = Quaternion.Euler(0f, Tuning.F("bag.turn", 12f), lean);

            var foil = root.transform.Find("Foil");
            var zip = root.transform.Find("Zip");
            if (foil != null) foil.GetComponent<MeshFilter>().sharedMesh = ProcMesh.Pouch(w, h, b, 12, tilt, skew, wobble);
            if (zip != null)
            {
                var zs = Tuning.V3("bag.zip.size", new Vector3(0.36f, 0.04f, 0.035f));
                zip.localScale = zs;
                zip.localRotation = Quaternion.Euler(0f, 0f, tilt);
                // Mitte der schrägen Oberkante (inkl. seitlichem Versatz)
                zip.localPosition = new Vector3(skew, h - zs.y / 2f, 0f);
            }
        }

        // Ein einzelner großer, besonders klarer Kristall (99 % Reinheit).
        public static GameObject PureCrystal(Transform parent, string name, Vector3 pos, float scale)
        {
            var go = Crystal(parent, name, pos, 1.1f * scale, 30f);
            go.transform.localRotation = Quaternion.Euler(0f, 30f, 45f);
            Materials.Apply(go, Materials.PureCrystal);
            return go;
        }

        // Ein Häufchen Kristalle (für Portion, Topf, Icon).
        public static GameObject CrystalPile(Transform parent, string name, Vector3 pos, float scale, int count = 5)
        {
            var pile = new GameObject(name);
            pile.transform.SetParent(parent, false);
            pile.transform.localPosition = pos;
            pile.transform.localScale = Vector3.one * scale;
            Vector3[] spots =
            {
                new Vector3(0f, 0f, 0f), new Vector3(0.09f, 0f, 0.05f), new Vector3(-0.08f, 0f, 0.06f),
                new Vector3(0.03f, 0.07f, -0.07f), new Vector3(-0.05f, 0.06f, -0.04f), new Vector3(0.07f, 0.06f, -0.02f),
                new Vector3(-0.1f, 0f, -0.08f), new Vector3(0.1f, 0f, -0.09f),
            };
            for (int i = 0; i < count && i < spots.Length; i++)
                Crystal(pile.transform, "Crystal " + i, spots[i], 0.55f + 0.1f * (i % 3), i * 37f);
            return pile;
        }
    }
}
