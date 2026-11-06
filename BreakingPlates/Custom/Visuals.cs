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

        // Erlenmeyerkolben (im Code erzeugt): Glas beidseitig sichtbar. Optional mit Flüssigkeit unten.
        public static GameObject Flask(Transform parent, string name, Vector3 pos, float scale = 1f, Material liquid = null)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = pos;
            root.transform.localScale = Vector3.one * scale;
            ProcMesh.Create("Glass", root.transform, ProcMesh.Lathe(ProcMesh.ErlenmeyerProfile, doubleSided: true), Materials.Glass, Vector3.zero);
            if (liquid != null) FlaskLiquid(root.transform, "Liquid", liquid);
            return root;
        }

        public static GameObject FlaskLiquid(Transform parent, string name, Material material) =>
            ProcMesh.Create(name, parent, ProcMesh.Lathe(ProcMesh.ErlenmeyerLiquidProfile, capTop: true), material, Vector3.zero);

        // Kristalle passend in den Kolbenboden (Radius ~0,15).
        public static GameObject FlaskCrystals(Transform parent, string name, float y, int count = 7) =>
            CrystalPile(parent, name, new Vector3(0f, y, 0f), 0.7f, count);

        // Folien-Tütchen (im Code erzeugt): durchsichtige, aufgeblähte Folie + roter Druckverschluss oben.
        // Live einstellbar (F8): bag.width, bag.height, bag.bulge, bag.zip.size
        public static GameObject Bag(Transform parent, Material zipMaterial = null)
        {
            var root = new GameObject("Bag");
            root.transform.SetParent(parent, false);
            ProcMesh.Create("Foil", root.transform, ProcMesh.Pouch(), Materials.Plastic, Vector3.zero);
            MakeZip(root.transform, "Zip", zipMaterial ?? Materials.ZipBlue);
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

        // Aufkleber vorne auf dem Tütchen (Chili P = rote Chili, Sugar-Free = grünes Blatt). Verschluss bleibt immer blau.
        // Lage per F8: bag.sticker.chili / bag.sticker.stevia
        // Chili P: rote Chilischote, mit einer Klammer oben am Verschluss festgeklemmt (hängt vorne herunter).
        private static readonly Vector2[] ChiliProfile =
        {
            new Vector2(0f, 0f), new Vector2(0.012f, 0.012f), new Vector2(0.024f, 0.045f), new Vector2(0.03f, 0.09f),
            new Vector2(0.028f, 0.13f), new Vector2(0.02f, 0.16f), new Vector2(0f, 0.17f)
        };
        private static readonly Vector2[] LeafProfile =
        {
            new Vector2(0f, 0f), new Vector2(0.35f, 0.08f), new Vector2(0.5f, 0.3f), new Vector2(0.45f, 0.6f),
            new Vector2(0.25f, 0.85f), new Vector2(0f, 1f)
        };

        public static GameObject ChiliSticker(GameObject bag)
        {
            var st = Sticker(bag, "Chili Clip", "bag.sticker.chili", new Vector3(0.12f, 0.43f, -0.05f));
            // Klammer über dem Verschluss
            ProcMesh.Create("Clip", st.transform, ProcMesh.Box(new Vector3(0.05f, 0.06f, 0.1f)), Materials.DarkGrey, new Vector3(0f, 0.01f, 0.04f));
            // Schote hängt nach unten, Spitze unten
            var pod = ProcMesh.Create("Pod", st.transform, ProcMesh.Lathe(ChiliProfile, 12), Materials.ZipRed, new Vector3(0f, -0.02f, -0.01f));
            pod.transform.localRotation = Quaternion.Euler(0f, 0f, 168f);
            ProcMesh.Create("Stem", st.transform, ProcMesh.Cylinder(0.008f, 0.04f, 6), Materials.Green, new Vector3(0f, -0.03f, -0.01f));
            ProcMesh.Create("Cap", st.transform, ProcMesh.Cylinder(0.022f, 0.012f, 10), Materials.Green, new Vector3(0f, -0.035f, -0.01f));
            return st;
        }

        // Sugar-Free: Stevia-Zweig als Aufkleber (drei echte Blattformen mit Mittelrippe).
        public static GameObject SteviaSticker(GameObject bag)
        {
            var st = Sticker(bag, "Stevia Sticker", "bag.sticker.stevia", new Vector3(-0.02f, 0.17f, -0.1f));
            st.transform.localScale = Vector3.one * 1.7f;
            var leafMat = Materials.Solid("Stevia Sticker", new Color(0.3f, 0.75f, 0.3f));
            var leafMesh = ProcMesh.Lathe(LeafProfile, 16, doubleSided: true);
            float[] angles = { -35f, 0f, 35f };
            foreach (var ang in angles)
            {
                var holder = new GameObject("Leaf");
                holder.transform.SetParent(st.transform, false);
                holder.transform.localRotation = Quaternion.Euler(0f, 0f, ang);
                var l = ProcMesh.Create("Blade", holder.transform, leafMesh, leafMat, Vector3.zero);
                float len = ang == 0f ? 0.12f : 0.09f;
                l.transform.localScale = new Vector3(len * 0.45f, len, 0.012f);
                ProcMesh.Create("Rib", holder.transform, ProcMesh.Box(new Vector3(0.004f, len * 0.9f, 0.014f)), Materials.Green,
                    new Vector3(0f, len * 0.45f, -0.002f));
            }
            ProcMesh.Create("Stalk", st.transform, ProcMesh.Box(new Vector3(0.006f, 0.05f, 0.014f)), Materials.Green, new Vector3(0f, -0.025f, 0f));
            return st;
        }

        private static GameObject Sticker(GameObject bag, string name, string key, Vector3 def)
        {
            var st = new GameObject(name);
            st.transform.SetParent(bag.transform, false);
            void Apply(GameObject t) => t.transform.localPosition = Tuning.V3(key, def);
            Apply(st);
            Tuning.Register(st, key, Apply);
            return st;
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
            foreach (var zipName in new[] { "Zip" })
            {
            var zip = root.transform.Find(zipName);
            if (zip == null) continue;
            float grow = 1f;
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

        // Holzstiel (für Kristall-Lollis). Steht senkrecht, Unterkante bei y = 0.
        public static GameObject Stick(Transform parent, string name, Vector3 pos, float length = 0.42f)
            => ProcMesh.Create(name, parent, ProcMesh.Cylinder(0.012f, length, 8), Materials.Wood, pos);

        // Kristall-Lolli: Stiel mit Kristalltraube oben.
        public static GameObject Lolli(Transform parent, string name, Vector3 pos, float scale = 1f)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = pos;
            root.transform.localScale = Vector3.one * scale;
            Stick(root.transform, "Stick", Vector3.zero, 0.42f);
            var cluster = new GameObject("Crystals");
            cluster.transform.SetParent(root.transform, false);
            cluster.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            for (int i = 0; i < 6; i++)
            {
                float a = i * 60f * Mathf.Deg2Rad;
                Crystal(cluster.transform, "Crystal " + i, new Vector3(Mathf.Cos(a) * 0.03f, (i % 3) * 0.04f, Mathf.Sin(a) * 0.03f), 0.45f, i * 53f);
            }
            return root;
        }

        // Mehrere Stiele, die oben aus dem Kolben ragen.
        public static GameObject FlaskSticks(Transform parent, string name, int count = 3)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            for (int i = 0; i < count; i++)
            {
                float a = i * (360f / count) * Mathf.Deg2Rad;
                var st = Stick(root.transform, "Stick " + i, new Vector3(Mathf.Cos(a) * 0.012f, 0.03f, Mathf.Sin(a) * 0.012f), 0.48f);
                st.transform.localRotation = Quaternion.Euler(Mathf.Sin(a) * 4f, 0f, Mathf.Cos(a) * 4f);
            }
            return root;
        }

        // Stevia: kleiner Zweig mit grünen Blättern (Pflanze statt Pulver, damit man es nicht mit Zucker verwechselt).
        public static GameObject SteviaHeap(Transform parent, string name, Vector3 pos)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = pos;
            var stem = ProcMesh.Create("Stem", root.transform, ProcMesh.Cylinder(0.008f, 0.16f, 6), Materials.Green, Vector3.zero);
            var leaf = Materials.Solid("Stevia Leaf", new Color(0.35f, 0.75f, 0.3f));
            for (int i = 0; i < 6; i++)
            {
                float a = i * 137f;
                var l = ProcMesh.Create("Leaf " + i, root.transform, ProcMesh.Box(new Vector3(0.09f, 0.008f, 0.045f)), leaf,
                    new Vector3(0f, 0.04f + i * 0.022f, 0f));
                l.transform.localRotation = Quaternion.Euler(0f, a, 25f);
                l.transform.localPosition += l.transform.right * 0.045f;
            }
            return root;
        }

        // Bierkrug "Schraderbräu": Glaskrug, Bier, Schaumkrone, Henkel, Etikett.
        public static GameObject BeerMug(Transform parent, string name, Vector3 pos, float scale = 1f)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = pos;
            root.transform.localScale = Vector3.one * scale;
            ProcMesh.Create("Beer", root.transform, ProcMesh.Cylinder(0.085f, 0.24f, 16), Materials.Beer, new Vector3(0f, 0.01f, 0f));
            ProcMesh.Create("Foam", root.transform, ProcMesh.Cylinder(0.092f, 0.05f, 16), Materials.Foam, new Vector3(0f, 0.25f, 0f));
            ProcMesh.Create("Glass", root.transform, ProcMesh.Mug(0.095f, 0.27f), Materials.Glass, Vector3.zero);
            ProcMesh.Create("Handle Top", root.transform, ProcMesh.Box(new Vector3(0.07f, 0.025f, 0.03f)), Materials.Glass, new Vector3(0.12f, 0.21f, 0f));
            ProcMesh.Create("Handle Bottom", root.transform, ProcMesh.Box(new Vector3(0.07f, 0.025f, 0.03f)), Materials.Glass, new Vector3(0.12f, 0.06f, 0f));
            ProcMesh.Create("Handle Side", root.transform, ProcMesh.Box(new Vector3(0.025f, 0.17f, 0.03f)), Materials.Glass, new Vector3(0.155f, 0.135f, 0f));
            // Etikett: weißes Schild mit grünem Streifen.
            ProcMesh.Create("Label", root.transform, ProcMesh.Box(new Vector3(0.09f, 0.08f, 0.01f)), Materials.Paper, new Vector3(0f, 0.12f, -0.096f));
            ProcMesh.Create("Label Stripe", root.transform, ProcMesh.Box(new Vector3(0.092f, 0.02f, 0.012f)), Materials.Green, new Vector3(0f, 0.12f, -0.097f));
            return root;
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
