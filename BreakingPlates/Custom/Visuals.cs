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

        // Erlenmeyerkolben (im Code erzeugt): Glas beidseitig sichtbar, unten blauer Sirup.
        public static GameObject Flask(Transform parent, string name, Vector3 pos, float scale = 1f)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = pos;
            root.transform.localScale = Vector3.one * scale;
            ProcMesh.Create("Glass", root.transform, ProcMesh.Lathe(ProcMesh.ErlenmeyerProfile, doubleSided: true), Materials.Glass, Vector3.zero);
            ProcMesh.Create("Liquid", root.transform, ProcMesh.Lathe(ProcMesh.ErlenmeyerLiquidProfile, capTop: true), Materials.BlueSyrup, Vector3.zero);
            return root;
        }

        // Folien-Tütchen (im Code erzeugt): durchsichtige, aufgeblähte Folie + roter Druckverschluss oben.
        // Live einstellbar (F8): bag.width, bag.height, bag.bulge, bag.zip.size
        public static GameObject Bag(Transform parent)
        {
            var root = new GameObject("Bag");
            root.transform.SetParent(parent, false);
            ProcMesh.Create("Foil", root.transform, ProcMesh.Pouch(), Materials.Plastic, Vector3.zero);
            MakeZip(root.transform, "Zip", Materials.ZipBlue);
            ApplyBag(root);
            Tuning.Register(root, "bag", ApplyBag);
            return root;
        }

        private const int ZipSegments = 8;

        private static GameObject MakeZip(Transform parent, string name, Material material)
        {
            var zip = new GameObject(name);
            zip.transform.SetParent(parent, false);
            for (int i = 0; i < ZipSegments; i++)
                ProcMesh.Create("Seg" + i, zip.transform, ProcMesh.Box(Vector3.one), material, Vector3.zero);
            return zip;
        }

        // Roter Verschluss für Chili-P-Tütchen: liegt minimal größer über dem blauen und wird nur bei Chili eingeblendet.
        public static GameObject ChiliZip(GameObject bag)
        {
            var zip = MakeZip(bag.transform, "ZipChili", Materials.ZipRed);
            ApplyBag(bag);
            return zip;
        }

        private static void ApplyBag(GameObject root)
        {
            float w = Tuning.F("bag.width", 0.42f), h = Tuning.F("bag.height", 0.45f), b = Tuning.F("bag.bulge", 0.08f);
            float tilt = Tuning.F("bag.tilt", 7f), skew = Tuning.F("bag.skew", 0.025f), wobble = Tuning.F("bag.wobble", 0.3f);
            float pinch = Tuning.F("bag.pinch", 0.22f), sag = Tuning.F("bag.sag", 0.02f), bow = Tuning.F("bag.bow", -0.01f);
            root.transform.localRotation = Quaternion.Euler(0f, Tuning.F("bag.turn", 12f), Tuning.F("bag.lean", 4f));

            var foil = root.transform.Find("Foil");
            if (foil != null) foil.GetComponent<MeshFilter>().sharedMesh = ProcMesh.Pouch(w, h, b, 14, tilt, skew, wobble, pinch, sag, bow);

            // Verschluss aus Segmenten, die dem Bogen der Oberkante folgen (blau; roter Chili-Verschluss etwas größer darüber).
            foreach (var zipName in new[] { "Zip", "ZipChili" })
            {
            var zip = root.transform.Find(zipName);
            if (zip == null) continue;
            float grow = zipName == "ZipChili" ? 1.12f : 1f;
            var zs = Tuning.V3("bag.zip.size", new Vector3(0.43f, 0.045f, 0.04f));
            float segW = w / ZipSegments;
            for (int i = 0; i < ZipSegments; i++)
            {
                var seg = zip.Find("Seg" + i);
                if (seg == null) continue;
                float x0 = -w / 2f + i * segW, x1 = x0 + segW, xm = (x0 + x1) / 2f;
                float y0 = ProcMesh.TopEdge(x0, w, h, tilt, bow), y1 = ProcMesh.TopEdge(x1, w, h, tilt, bow);
                float ym = (y0 + y1) / 2f - zs.y / 2f;
                float angle = Mathf.Atan2(y1 - y0, segW) * Mathf.Rad2Deg;
                seg.localPosition = new Vector3(xm + skew, ym, 0f);
                seg.localRotation = Quaternion.Euler(0f, 0f, angle);
                seg.localScale = new Vector3(segW * 1.08f * zs.x / 0.43f, zs.y * grow, zs.z * grow);
            }
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
