using KitchenData;
using KitchenLib.References;
using UnityEngine;

namespace BreakingPlates.Custom
{
    // Pilot: neue Deko-Ideen (06.11.). Alles aus einfachen Formen, wie die bisherige Deko.
    internal static class Shapes
    {
        public static readonly Vector2[] SphereProfile = MakeSphere(10);
        private static Vector2[] MakeSphere(int n)
        {
            var p = new Vector2[n + 1];
            for (int i = 0; i <= n; i++)
            {
                float a = -Mathf.PI / 2f + Mathf.PI * i / n;
                p[i] = new Vector2(Mathf.Cos(a) * 0.5f, 0.5f + Mathf.Sin(a) * 0.5f);
            }
            p[0].x = 0f; p[n].x = 0f;
            return p;
        }
        // Kugel/Ellipsoid mit Durchmesser size, Mittelpunkt bei pos.
        public static GameObject Ball(string name, Transform parent, Vector3 size, Material m, Vector3 pos)
        {
            var go = ProcMesh.Create(name, parent, ProcMesh.Lathe(SphereProfile, 14), m, pos - new Vector3(0f, size.y / 2f, 0f));
            go.transform.localScale = size;
            return go;
        }
        public static readonly Vector2[] BellProfile =
        {
            new Vector2(0f, 0f), new Vector2(0.5f, 0f), new Vector2(0.45f, 0.2f), new Vector2(0.38f, 0.55f), new Vector2(0.25f, 0.9f), new Vector2(0f, 1f)
        };
        public static readonly Vector2[] PotProfile =
        {
            new Vector2(0f, 0f), new Vector2(0.38f, 0f), new Vector2(0.5f, 0.85f), new Vector2(0.55f, 0.88f), new Vector2(0.55f, 1f), new Vector2(0.44f, 1f), new Vector2(0.44f, 0.9f), new Vector2(0f, 0.9f)
        };
    }

    // --- Charming ---

    // Rosa Teddybär aus dem Pool: ein Auge fehlt, ein paar angekokelte Stellen.
    public class PinkTeddy : DecoBase
    {
        public override string UniqueNameID => "Pink Teddy";
        protected override DecorationType Theme => DecorationType.Charming;
        protected override int[] VanillaIDs => new[] { ApplianceReferences.CosyBarrel };
        protected override string Title => "Pink Teddy Bear";
        protected override string Desc => "Found it in the pool. Don't ask.";
        protected override Vector3 DefaultRot => new Vector3(0f, -20f, 8f);

        protected override void Build(Transform root)
        {
            var pink = Materials.Solid("Teddy Pink", new Color(0.95f, 0.55f, 0.72f));
            var light = Materials.Solid("Teddy Light", new Color(0.99f, 0.8f, 0.86f));
            var burnt = Materials.Solid("Teddy Burnt", new Color(0.18f, 0.13f, 0.12f));
            Shapes.Ball("Body", root, new Vector3(0.32f, 0.36f, 0.28f), pink, new Vector3(0f, 0.2f, 0f));
            Shapes.Ball("Belly", root, new Vector3(0.2f, 0.24f, 0.1f), light, new Vector3(0f, 0.19f, -0.1f));
            Shapes.Ball("Head", root, new Vector3(0.26f, 0.24f, 0.24f), pink, new Vector3(0f, 0.48f, -0.02f));
            Shapes.Ball("Snout", root, new Vector3(0.1f, 0.08f, 0.08f), light, new Vector3(0f, 0.45f, -0.13f));
            Shapes.Ball("Nose", root, new Vector3(0.035f, 0.025f, 0.025f), Materials.Black, new Vector3(0f, 0.47f, -0.17f));
            Shapes.Ball("Ear L", root, new Vector3(0.08f, 0.08f, 0.05f), pink, new Vector3(-0.1f, 0.6f, 0f));
            Shapes.Ball("Ear R", root, new Vector3(0.08f, 0.08f, 0.05f), pink, new Vector3(0.1f, 0.6f, 0f));
            // Ein Auge – das andere fehlt (nur eine helle Naht).
            Shapes.Ball("Eye", root, new Vector3(0.035f, 0.035f, 0.02f), Materials.Black, new Vector3(-0.055f, 0.51f, -0.11f));
            Box(root, "Missing Eye", new Vector3(0.04f, 0.008f, 0.01f), new Vector3(0.055f, 0.51f, -0.115f), light);
            foreach (var s in new[] { -1f, 1f })
            {
                Shapes.Ball("Arm", root, new Vector3(0.09f, 0.18f, 0.09f), pink, new Vector3(0.17f * s, 0.25f, -0.03f));
                Shapes.Ball("Leg", root, new Vector3(0.11f, 0.1f, 0.16f), pink, new Vector3(0.1f * s, 0.05f, -0.08f));
            }
            // Angekokelt
            Shapes.Ball("Burn 1", root, new Vector3(0.09f, 0.06f, 0.02f), burnt, new Vector3(0.09f, 0.26f, -0.12f));
            Shapes.Ball("Burn 2", root, new Vector3(0.06f, 0.05f, 0.02f), burnt, new Vector3(-0.07f, 0.12f, -0.12f));
            Shapes.Ball("Burn 3", root, new Vector3(0.05f, 0.04f, 0.03f), burnt, new Vector3(0.1f, 0.58f, -0.02f));
        }
    }

    // Maiglöckchen im Terrakotta-Topf: hängende Glöckchen, damit es nicht wie die Stevia-Pflanze aussieht.
    public class LilyOfTheValley : DecoBase
    {
        public override string UniqueNameID => "Lily Of The Valley";
        protected override DecorationType Theme => DecorationType.Charming;
        protected override int[] VanillaIDs => new[] { ApplianceReferences.CosyBarrel };
        protected override string Title => "Lily of the Valley";
        protected override string Desc => "Pretty. Keep away from children.";

        protected override void Build(Transform root)
        {
            var terracotta = Materials.Solid("Terracotta", new Color(0.78f, 0.42f, 0.25f));
            var soil = Materials.Solid("Soil", new Color(0.3f, 0.2f, 0.12f));
            var stem = Materials.Solid("Lily Stem", new Color(0.35f, 0.6f, 0.25f));
            var leaf = Materials.Solid("Lily Leaf", new Color(0.2f, 0.5f, 0.22f));
            var white = Materials.Solid("Lily Bell", new Color(0.98f, 0.98f, 0.95f));
            var pot = ProcMesh.Create("Pot", root, ProcMesh.Lathe(Shapes.PotProfile, 20), terracotta, Vector3.zero);
            pot.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
            ProcMesh.Create("Soil", root, ProcMesh.Cylinder(0.17f, 0.01f, 16), soil, new Vector3(0f, 0.27f, 0f));
            // Zwei breite Blätter
            foreach (var (ang, tilt) in new[] { (30f, -12f), (200f, 14f) })
            {
                var l = Shapes.Ball("Leaf", root, new Vector3(0.12f, 0.42f, 0.03f), leaf, new Vector3(0f, 0.48f, 0f));
                l.transform.localRotation = Quaternion.Euler(tilt, ang, 0f);
            }
            // Drei gebogene Stiele mit je 5 hängenden Glöckchen
            float[] dirs = { 0f, 120f, 240f };
            for (int s = 0; s < dirs.Length; s++)
            {
                var arc = new GameObject("Stem " + s);
                arc.transform.SetParent(root, false);
                arc.transform.localPosition = new Vector3(0f, 0.27f, 0f);
                arc.transform.localRotation = Quaternion.Euler(0f, dirs[s] + 15f, 0f);
                const int segs = 8;
                for (int i = 0; i < segs; i++)
                {
                    float t = i / (float)(segs - 1);
                    // Bogen: erst hoch, dann nach außen überhängend
                    var p = new Vector3(0f, 0.38f * Mathf.Sin(t * Mathf.PI * 0.62f) + 0.02f, 0.2f * t * t);
                    ProcMesh.Create("Seg", arc.transform, ProcMesh.Box(new Vector3(0.012f, 0.06f, 0.012f)), stem, p)
                        .transform.localRotation = Quaternion.Euler(70f * t, 0f, 0f);
                    if (i >= 3)
                    {
                        ProcMesh.Create("Pedicel", arc.transform, ProcMesh.Box(new Vector3(0.005f, 0.035f, 0.005f)), stem, p + new Vector3(0f, -0.02f, 0f));
                        var bell = ProcMesh.Create("Bell", arc.transform, ProcMesh.Lathe(Shapes.BellProfile, 12), white, p + new Vector3(0f, -0.075f, 0f));
                        bell.transform.localScale = Vector3.one * 0.07f;
                    }
                }
            }
        }
    }

    // --- Exclusive ---

    // "They're minerals, Marie!" – Vitrine mit Hanks Steinen (bewusst nicht lila).
    public class HankMinerals : DecoBase
    {
        public override string UniqueNameID => "Hank Minerals";
        protected override DecorationType Theme => DecorationType.Exclusive;
        protected override int[] VanillaIDs => new[] { ApplianceReferences.CosyBarrel };
        protected override string Title => "Mineral Collection";
        protected override string Desc => "They're minerals, Marie!";

        protected override void Build(Transform root)
        {
            var wood = Materials.Solid("Cabinet Wood", new Color(0.42f, 0.27f, 0.16f));
            var velvet = Materials.Solid("Velvet", new Color(0.12f, 0.2f, 0.35f));
            Box(root, "Stand", new Vector3(0.6f, 0.5f, 0.4f), new Vector3(0f, 0.25f, 0f), wood);
            Box(root, "Velvet", new Vector3(0.56f, 0.02f, 0.36f), new Vector3(0f, 0.51f, 0f), velvet);
            // Steine
            var pyrite = Materials.Flat("Pyrite", new Color(0.85f, 0.72f, 0.3f), 0.9f);
            var geodeOut = Materials.Solid("Geode Rock", new Color(0.5f, 0.47f, 0.43f));
            var geodeIn = Materials.Flat("Geode Inside", new Color(0.55f, 0.85f, 0.85f), 0.9f);
            var quartz = Materials.Flat("Quartz", new Color(0.95f, 0.95f, 0.97f), 0.9f);
            var malachite = Materials.Solid("Malachite", new Color(0.1f, 0.55f, 0.35f));
            for (int i = 0; i < 4; i++)
            {
                var c = ProcMesh.Create("Pyrite", root, ProcMesh.Box(Vector3.one * (0.05f - i * 0.006f)), pyrite, new Vector3(-0.2f + i * 0.025f, 0.54f + (i % 2) * 0.02f, 0.03f - i * 0.02f));
                c.transform.localRotation = Quaternion.Euler(i * 17f, i * 33f, i * 9f);
            }
            var g = Shapes.Ball("Geode", root, new Vector3(0.14f, 0.1f, 0.14f), geodeOut, new Vector3(-0.02f, 0.56f, 0f));
            var gi = Shapes.Ball("Geode Inside", root, new Vector3(0.11f, 0.02f, 0.11f), geodeIn, new Vector3(-0.02f, 0.6f, -0.005f));
            for (int i = 0; i < 3; i++)
                Visuals.Crystal(root, "Quartz " + i, new Vector3(0.13f + i * 0.035f, 0.53f, -0.02f + (i % 2) * 0.05f), 0.5f, i * 40f);
            foreach (Transform t in root) if (t.name.StartsWith("Quartz")) Materials.Apply(t.gameObject, quartz);
            Shapes.Ball("Malachite", root, new Vector3(0.08f, 0.05f, 0.07f), malachite, new Vector3(0.12f, 0.545f, 0.1f));
            // Glashaube
            Box(root, "Glass", new Vector3(0.58f, 0.24f, 0.38f), new Vector3(0f, 0.64f, 0f), Materials.Glass);
            // Kleines Messingschild vorne
            var brass = Materials.Flat("Brass", new Color(0.8f, 0.65f, 0.3f), 0.8f);
            Box(root, "Plaque", new Vector3(0.24f, 0.06f, 0.01f), new Vector3(0f, 0.4f, -0.205f), brass);
            Text(root, "Plaque Text", "MINERALS", new Vector3(0f, 0.4f, -0.212f), 0.32f, new Color(0.2f, 0.15f, 0.05f));
        }
    }

    // Geldstapel auf Palette, in Folie gewickelt.
    public class MoneyStack : DecoBase
    {
        public override string UniqueNameID => "Money Stack";
        protected override DecorationType Theme => DecorationType.Exclusive;
        protected override int[] VanillaIDs => new[] { ApplianceReferences.CosyBarrel };
        protected override string Title => "Money Stack";
        protected override string Desc => "We've got a storage unit for that.";

        protected override void Build(Transform root)
        {
            var wood = Materials.Solid("Pallet Wood", new Color(0.78f, 0.62f, 0.4f));
            var money = Materials.Solid("Money", new Color(0.55f, 0.72f, 0.5f));
            var band = Materials.Solid("Money Band", new Color(0.92f, 0.88f, 0.75f));
            for (int i = 0; i < 3; i++) Box(root, "Pallet Board", new Vector3(0.7f, 0.025f, 0.12f), new Vector3(0f, 0.08f, -0.2f + i * 0.2f), wood);
            for (int i = 0; i < 3; i++) Box(root, "Pallet Block", new Vector3(0.1f, 0.07f, 0.55f), new Vector3(-0.28f + i * 0.28f, 0.035f, 0f), wood);
            // Bündel: 4 x 3 Reihen, 5 Lagen
            const float bw = 0.15f, bd = 0.07f, bh = 0.045f;
            for (int layer = 0; layer < 5; layer++)
            for (int x = 0; x < 4; x++)
            for (int z = 0; z < 6; z++)
            {
                var pos = new Vector3(-0.24f + x * (bw + 0.01f), 0.115f + layer * bh, -0.21f + z * (bd + 0.012f));
                Box(root, "Bundle", new Vector3(bw, bh - 0.004f, bd), pos, money);
                Box(root, "Band", new Vector3(0.03f, bh - 0.002f, bd + 0.002f), pos, band);
            }
            // Folie drumherum
            Box(root, "Wrap", new Vector3(0.68f, 5 * bh + 0.02f, 0.5f), new Vector3(0f, 0.1f + 5 * bh / 2f + 0.01f, 0f), Materials.Glass);
            // Ein paar lose Bündel obendrauf
            for (int i = 0; i < 3; i++)
            {
                var b = Box(root, "Loose", new Vector3(bw, bh - 0.004f, bd), new Vector3(-0.1f + i * 0.09f, 0.36f + i * 0.02f, -0.05f + i * 0.04f), money);
                b.transform.localRotation = Quaternion.Euler(0f, 25f * i - 20f, 0f);
            }
        }
    }

    // --- Formal ---

    // Maries lila Ecke: lila Sessel + lila Vase mit lila Blumen.
    public class MariePurple : DecoBase
    {
        public override string UniqueNameID => "Marie Purple Chair";
        protected override DecorationType Theme => DecorationType.Formal;
        protected override int[] VanillaIDs => new[] { ApplianceReferences.CosyBarrel };
        protected override string Title => "Purple Armchair";
        protected override string Desc => "It's not purple. It's amethyst.";

        protected override void Build(Transform root)
        {
            var purple = Materials.Solid("Marie Purple", new Color(0.5f, 0.25f, 0.65f));
            var dark = Materials.Solid("Marie Purple Dark", new Color(0.35f, 0.15f, 0.48f));
            var lilac = Materials.Solid("Marie Lilac", new Color(0.75f, 0.55f, 0.9f));
            // Sessel
            Box(root, "Seat", new Vector3(0.48f, 0.14f, 0.42f), new Vector3(-0.08f, 0.24f, 0f), purple);
            Box(root, "Cushion", new Vector3(0.4f, 0.06f, 0.36f), new Vector3(-0.08f, 0.33f, -0.02f), lilac);
            Box(root, "Back", new Vector3(0.48f, 0.42f, 0.1f), new Vector3(-0.08f, 0.45f, 0.18f), purple);
            foreach (var s in new[] { -1f, 1f })
            {
                Box(root, "Armrest", new Vector3(0.08f, 0.2f, 0.42f), new Vector3(-0.08f + 0.24f * s, 0.34f, 0f), dark);
                Box(root, "Leg F", new Vector3(0.04f, 0.17f, 0.04f), new Vector3(-0.08f + 0.2f * s, 0.085f, -0.17f), Materials.Black);
                Box(root, "Leg B", new Vector3(0.04f, 0.17f, 0.04f), new Vector3(-0.08f + 0.2f * s, 0.085f, 0.17f), Materials.Black);
            }
            // Vase mit Blumen daneben
            var vase = ProcMesh.Create("Vase", root, ProcMesh.Lathe(Shapes.BellProfile, 16), dark, new Vector3(0.33f, 0f, -0.1f));
            vase.transform.localScale = new Vector3(0.14f, 0.3f, 0.14f);
            for (int i = 0; i < 4; i++)
            {
                float a = i * 90f * Mathf.Deg2Rad;
                var tip = new Vector3(0.33f + Mathf.Cos(a) * 0.05f, 0.42f + (i % 2) * 0.04f, -0.1f + Mathf.Sin(a) * 0.05f);
                Box(root, "Stem", new Vector3(0.01f, 0.14f, 0.01f), tip - new Vector3(0f, 0.08f, 0f), Materials.Green);
                Shapes.Ball("Flower", root, new Vector3(0.07f, 0.05f, 0.07f), i % 2 == 0 ? purple : lilac, tip);
            }
        }
    }

    // Gales Laborbuch auf einem Lesepult.
    public class GaleNotebook : DecoBase
    {
        public override string UniqueNameID => "Gale Notebook";
        protected override DecorationType Theme => DecorationType.Formal;
        protected override int[] VanillaIDs => new[] { ApplianceReferences.CosyBarrel };
        protected override string Title => "Lab Notebook";
        protected override string Desc => "To W.W. My star, my perfect silence.";

        protected override void Build(Transform root)
        {
            var wood = Materials.Solid("Lectern Wood", new Color(0.45f, 0.3f, 0.18f));
            var cover = Materials.Solid("Notebook Cover", new Color(0.15f, 0.3f, 0.2f));
            Box(root, "Base", new Vector3(0.36f, 0.04f, 0.3f), new Vector3(0f, 0.02f, 0f), wood);
            Box(root, "Post", new Vector3(0.08f, 0.62f, 0.08f), new Vector3(0f, 0.33f, 0f), wood);
            var top = new GameObject("Top");
            top.transform.SetParent(root, false);
            top.transform.localPosition = new Vector3(0f, 0.68f, 0f);
            top.transform.localRotation = Quaternion.Euler(-25f, 0f, 0f);
            Box(top.transform, "Desk", new Vector3(0.46f, 0.03f, 0.34f), Vector3.zero, wood);
            Box(top.transform, "Lip", new Vector3(0.46f, 0.04f, 0.02f), new Vector3(0f, 0.025f, -0.16f), wood);
            Box(top.transform, "Cover", new Vector3(0.42f, 0.012f, 0.28f), new Vector3(0f, 0.022f, 0f), cover);
            foreach (var s in new[] { -1f, 1f })
            {
                var page = Box(top.transform, "Page", new Vector3(0.19f, 0.012f, 0.26f), new Vector3(0.1f * s, 0.033f, 0f), Materials.Paper);
                page.transform.localRotation = Quaternion.Euler(0f, 0f, -4f * s);
                // Formel-Zeilen
                for (int l = 0; l < 6; l++)
                    Box(top.transform, "Line", new Vector3(0.14f - (l % 3) * 0.02f, 0.002f, 0.008f), new Vector3(0.1f * s - (l % 3) * 0.01f, 0.041f, 0.09f - l * 0.035f), Materials.DarkGrey);
            }
            // Widmung "W.W." auf der linken Seite (als flacher Text nach oben)
            var ww = Text(top.transform, "WW", "W.W.", new Vector3(-0.1f, 0.042f, -0.09f), 0.35f, new Color(0.15f, 0.25f, 0.6f));
            ww.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }

    // --- Affordable (Wand) ---

    // "Fly": gerahmte Fliegenklatsche an der Wand, davor summt eine Fliege herum.
    public class TheFly : DecoBase
    {
        public override string UniqueNameID => "The Fly";
        protected override DecorationType Theme => DecorationType.Affordable;
        protected override int[] VanillaIDs => new[] { ApplianceReferences.FancyPainting, ApplianceReferences.Painting, ApplianceReferences.FriendsPoster, ApplianceReferences.BunnyPoster, ApplianceReferences.AffordableNeonSign1 };
        protected override string Title => "The Fly";
        protected override string Desc => "It's a contaminant.";
        protected override Vector3 DefaultPos => new Vector3(0f, 0.75f, 0.45f);

        protected override void Build(Transform root)
        {
            var frame = Materials.Solid("Fly Frame", new Color(0.2f, 0.15f, 0.1f));
            Box(root, "Frame", new Vector3(0.46f, 0.56f, 0.02f), new Vector3(0f, 0f, 0.01f), frame);
            Box(root, "Back", new Vector3(0.4f, 0.5f, 0.01f), new Vector3(0f, 0f, 0f), Materials.Paper);
            // Fliegenklatsche schräg im Rahmen
            var sw = new GameObject("Swatter");
            sw.transform.SetParent(root, false);
            sw.transform.localPosition = new Vector3(0f, 0f, -0.01f);
            sw.transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
            var swMat = Materials.Solid("Swatter", new Color(0.2f, 0.6f, 0.85f));
            Box(sw.transform, "Head", new Vector3(0.16f, 0.18f, 0.008f), new Vector3(0f, 0.11f, 0f), swMat);
            Box(sw.transform, "Handle", new Vector3(0.02f, 0.26f, 0.01f), new Vector3(0f, -0.11f, 0f), swMat);
            for (int i = 0; i < 4; i++) Box(sw.transform, "Grid", new Vector3(0.14f, 0.004f, 0.01f), new Vector3(0f, 0.04f + i * 0.045f, -0.002f), Materials.Black);
            Text(root, "Label", "THE FLY", new Vector3(0f, -0.23f, -0.012f), 0.4f, new Color(0.1f, 0.1f, 0.1f));

            // Lebendige Fliege davor
            var fly = new GameObject("Fly");
            fly.transform.SetParent(root, false);
            fly.transform.localPosition = new Vector3(0f, 0f, -0.3f);
            var body = new GameObject("Body");
            body.transform.SetParent(fly.transform, false);
            Shapes.Ball("Thorax", body.transform, new Vector3(0.04f, 0.035f, 0.05f), Materials.Black, Vector3.zero);
            Shapes.Ball("Abdomen", body.transform, new Vector3(0.035f, 0.03f, 0.05f), Materials.DarkGrey, new Vector3(0f, 0f, 0.04f));
            var eye = Materials.Solid("Fly Eye", new Color(0.7f, 0.1f, 0.08f));
            Shapes.Ball("Eye L", body.transform, new Vector3(0.02f, 0.02f, 0.02f), eye, new Vector3(-0.014f, 0.006f, -0.024f));
            Shapes.Ball("Eye R", body.transform, new Vector3(0.02f, 0.02f, 0.02f), eye, new Vector3(0.014f, 0.006f, -0.024f));
            var wingMat = Materials.Glass;
            var wl = Shapes.Ball("Wing L", body.transform, new Vector3(0.05f, 0.004f, 0.03f), wingMat, new Vector3(-0.03f, 0.02f, 0.015f));
            var wr = Shapes.Ball("Wing R", body.transform, new Vector3(0.05f, 0.004f, 0.03f), wingMat, new Vector3(0.03f, 0.02f, 0.015f));
            var buzz = fly.AddComponent<FlyBuzz>();
            buzz.Body = body.transform;
            buzz.WingL = wl.transform;
            buzz.WingR = wr.transform;
        }
    }

    // Fliegt eine unruhige Acht vor dem Rahmen, Flügel schlagen.
    public class FlyBuzz : MonoBehaviour
    {
        public Transform Body, WingL, WingR;
        private Vector3 _home;
        private float _seed;
        private void Start()
        {
            _home = transform.localPosition;
            _seed = Random.value * 10f;
        }
        private void Update()
        {
            if (Tuning.F("deco.fly.live", 1f) < 0.5f) return;
            float t = Time.time * Tuning.F("deco.fly.speed", 1.4f) + _seed;
            var p = new Vector3(Mathf.Sin(t) * 0.35f, Mathf.Sin(2f * t) * 0.18f + Mathf.Sin(t * 5.3f) * 0.02f, Mathf.Cos(t * 0.7f) * 0.12f);
            var next = new Vector3(Mathf.Sin(t + 0.05f) * 0.35f, Mathf.Sin(2f * (t + 0.05f)) * 0.18f, Mathf.Cos((t + 0.05f) * 0.7f) * 0.12f);
            transform.localPosition = _home + p;
            var dir = next - p;
            if (dir.sqrMagnitude > 0.000001f) transform.localRotation = Quaternion.LookRotation(-dir, Vector3.up);
            float flap = Mathf.Sin(Time.time * 60f) * 35f;
            if (WingL != null) WingL.localRotation = Quaternion.Euler(0f, 0f, flap);
            if (WingR != null) WingR.localRotation = Quaternion.Euler(0f, 0f, -flap);
        }
    }

    // "Better Call Saul!"-Plakat: Variante mit Silhouette und Variante nur mit Schrift (zum Vergleichen).
    public abstract class SaulPosterBase : DecoBase
    {
        protected override DecorationType Theme => DecorationType.Affordable;
        protected override int[] VanillaIDs => new[] { ApplianceReferences.FancyPainting, ApplianceReferences.Painting, ApplianceReferences.FriendsPoster, ApplianceReferences.BunnyPoster, ApplianceReferences.AffordableNeonSign1 };
        protected override string Desc => "Did you know that you have rights?";
        protected override Vector3 DefaultPos => new Vector3(0f, 0.75f, 0.45f);
        protected abstract bool Silhouette { get; }

        protected override void Build(Transform root)
        {
            var bg = Materials.Solid("Saul Yellow", new Color(0.98f, 0.86f, 0.25f));
            var red = new Color(0.75f, 0.1f, 0.08f);
            Box(root, "Border", new Vector3(0.84f, 0.6f, 0.02f), new Vector3(0f, 0f, 0.01f), Materials.Red);
            Box(root, "Paper", new Vector3(0.78f, 0.54f, 0.012f), new Vector3(0f, 0f, 0f), bg);
            float textX = 0f;
            if (Silhouette)
            {
                textX = 0.12f;
                var dark = Materials.Solid("Saul Silhouette", new Color(0.12f, 0.12f, 0.16f));
                // Kopf, Schultern/Anzug, erhobener Zeigefinger
                Shapes.Ball("Head", root, new Vector3(0.12f, 0.14f, 0.01f), dark, new Vector3(-0.24f, 0.08f, -0.008f));
                Box(root, "Torso", new Vector3(0.26f, 0.2f, 0.008f), new Vector3(-0.24f, -0.15f, -0.008f), dark);
                Box(root, "Shoulders", new Vector3(0.22f, 0.06f, 0.008f), new Vector3(-0.24f, -0.03f, -0.008f), dark);
                var arm = Box(root, "Arm", new Vector3(0.05f, 0.18f, 0.008f), new Vector3(-0.1f, 0.0f, -0.008f), dark);
                arm.transform.localRotation = Quaternion.Euler(0f, 0f, -20f);
                Box(root, "Finger", new Vector3(0.018f, 0.07f, 0.008f), new Vector3(-0.065f, 0.12f, -0.008f), dark);
                // Krawatte als Akzent
                Box(root, "Tie", new Vector3(0.03f, 0.12f, 0.006f), new Vector3(-0.24f, -0.12f, -0.012f), Materials.Red);
            }
            float size = Silhouette ? 0.75f : 1f;
            Text(root, "Line 1", "BETTER CALL", new Vector3(textX, 0.11f, -0.01f), 0.9f * size, red);
            Text(root, "Line 2", "SAUL!", new Vector3(textX, -0.02f, -0.01f), 1.5f * size, red);
            Text(root, "Phone", "505-503-4455", new Vector3(textX, -0.18f, -0.01f), 0.55f * size, new Color(0.1f, 0.1f, 0.15f));
        }
    }
    public class SaulPoster : SaulPosterBase
    {
        public override string UniqueNameID => "Saul Poster";
        protected override string Title => "Better Call Saul!";
        protected override bool Silhouette => true;
    }
    public class SaulPosterText : SaulPosterBase
    {
        public override string UniqueNameID => "Saul Poster Text";
        protected override string Title => "Better Call Saul! (Text)";
        protected override bool Silhouette => false;
    }
}
