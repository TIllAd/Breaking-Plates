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
            new Vector2(0f, 0f), new Vector2(0.5f, 0f), new Vector2(0.47f, 0.15f), new Vector2(0.44f, 0.4f), new Vector2(0.41f, 0.65f), new Vector2(0.33f, 0.85f), new Vector2(0.18f, 0.97f), new Vector2(0f, 1f)
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
                const int segs = 12;
                for (int i = 0; i < segs; i++)
                {
                    float t = i / (float)(segs - 1);
                    // Bogen: erst hoch, dann nach außen überhängend
                    var p = new Vector3(0f, 0.38f * Mathf.Sin(t * Mathf.PI * 0.62f) + 0.02f, 0.2f * t * t);
                    ProcMesh.Create("Seg", arc.transform, ProcMesh.Box(new Vector3(0.009f, 0.045f, 0.009f)), stem, p)
                        .transform.localRotation = Quaternion.Euler(70f * t, 0f, 0f);
                    if (i >= 5 && i % 2 == 1 || i == segs - 1)
                    {
                        ProcMesh.Create("Pedicel", arc.transform, ProcMesh.Box(new Vector3(0.005f, 0.035f, 0.005f)), stem, p + new Vector3(0f, -0.02f, 0f));
                        var bell = ProcMesh.Create("Bell", arc.transform, ProcMesh.Lathe(Shapes.BellProfile, 12), white, p + new Vector3(0f, -0.075f, 0f));
                        bell.transform.localScale = Vector3.one * 0.07f;
                        // kleiner grüner Kelch oben auf dem Glöckchen
                        Shapes.Ball("Calyx", arc.transform, new Vector3(0.025f, 0.015f, 0.025f), stem, p + new Vector3(0f, -0.006f, 0f));
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
            // Steine: drei Stücke auf kleinen Sockeln, jedes mit eigener Struktur
            var pyrite = Materials.Flat("Pyrite", new Color(0.85f, 0.72f, 0.3f), 0.9f);
            var pyriteDark = Materials.Flat("Pyrite Dark", new Color(0.62f, 0.5f, 0.2f), 0.6f);
            var geodeOut = Materials.Solid("Geode Rock", new Color(0.55f, 0.5f, 0.45f));
            var geodeIn = Materials.Flat("Geode Inside", new Color(0.5f, 0.82f, 0.85f), 0.9f);
            var quartz = Materials.Flat("Quartz", new Color(0.95f, 0.95f, 0.97f), 0.9f);
            var rock = Materials.Solid("Matrix Rock", new Color(0.45f, 0.42f, 0.4f));
            var plinth = Materials.Solid("Plinth", new Color(0.2f, 0.2f, 0.22f));
            float[] xs = { -0.19f, 0f, 0.19f };
            foreach (var x in xs) Box(root, "Plinth", new Vector3(0.12f, 0.03f, 0.12f), new Vector3(x, 0.535f, 0.03f), plinth);
            // 1) Pyrit: ineinander gewachsene Würfel mit Streifen
            var py = new GameObject("Pyrite Cluster");
            py.transform.SetParent(root, false);
            py.transform.localPosition = new Vector3(xs[0], 0.55f, 0.03f);
            float[][] cubes = { new[] { 0f, 0.03f, 0f, 0.06f }, new[] { 0.03f, 0.022f, 0.02f, 0.045f }, new[] { -0.03f, 0.02f, -0.015f, 0.04f }, new[] { 0.005f, 0.065f, 0.01f, 0.035f }, new[] { -0.02f, 0.05f, 0.025f, 0.03f } };
            for (int i = 0; i < cubes.Length; i++)
            {
                var c = cubes[i];
                var cube = ProcMesh.Create("Cube", py.transform, ProcMesh.Box(Vector3.one * c[3]), pyrite, new Vector3(c[0], c[1], c[2]));
                cube.transform.localRotation = Quaternion.Euler(0f, i * 23f, i % 2 == 0 ? 0f : 8f);
                ProcMesh.Create("Striation", cube.transform, ProcMesh.Box(new Vector3(c[3] * 1.01f, c[3] * 0.12f, c[3] * 1.01f)), pyriteDark, Vector3.zero);
            }
            // 2) Aufgeschnittene Geode: Schale, helles Inneres, Kranz aus kleinen Kristallspitzen
            var ge = new GameObject("Geode");
            ge.transform.SetParent(root, false);
            ge.transform.localPosition = new Vector3(xs[1], 0.55f, 0.03f);
            Shapes.Ball("Shell", ge.transform, new Vector3(0.13f, 0.08f, 0.13f), geodeOut, new Vector3(0f, 0.04f, 0f));
            Shapes.Ball("Inside", ge.transform, new Vector3(0.105f, 0.012f, 0.105f), geodeIn, new Vector3(0f, 0.08f, 0f));
            for (int i = 0; i < 10; i++)
            {
                float a = i * 36f * Mathf.Deg2Rad;
                var tip = ProcMesh.Create("Tip", ge.transform, ProcMesh.Lathe(Shapes.BellProfile, 6), quartz, new Vector3(Mathf.Cos(a) * 0.035f, 0.075f, Mathf.Sin(a) * 0.035f));
                tip.transform.localScale = new Vector3(0.012f, 0.025f, 0.012f);
            }
            // 3) Quarz-Stufe: Gesteinsbrocken mit mehreren sechseckigen Spitzen in verschiedenen Größen
            var qz = new GameObject("Quartz Cluster");
            qz.transform.SetParent(root, false);
            qz.transform.localPosition = new Vector3(xs[2], 0.55f, 0.03f);
            Shapes.Ball("Matrix", qz.transform, new Vector3(0.11f, 0.04f, 0.09f), rock, new Vector3(0f, 0.02f, 0f));
            var prism = new[] { new Vector2(0f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.7f), new Vector2(0f, 1f) };
            float[][] spikes = { new[] { 0f, 0f, 0.1f, 0f, 0f }, new[] { 0.025f, 0.01f, 0.07f, 15f, -10f }, new[] { -0.025f, -0.01f, 0.08f, -18f, 8f }, new[] { 0.01f, -0.025f, 0.055f, 10f, 20f }, new[] { -0.015f, 0.025f, 0.05f, -12f, -18f } };
            foreach (var sp in spikes)
            {
                var t = ProcMesh.Create("Spike", qz.transform, ProcMesh.Lathe(prism, 6), quartz, new Vector3(sp[0], 0.03f, sp[1]));
                t.transform.localScale = new Vector3(0.025f, sp[2], 0.025f);
                t.transform.localRotation = Quaternion.Euler(sp[3], 0f, sp[4]);
            }
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
            var wood = Materials.Solid("Pallet Wood", new Color(0.72f, 0.55f, 0.35f));
            var note = Materials.Solid("Money Note", new Color(0.42f, 0.62f, 0.38f));
            var noteEdge = Materials.Solid("Money Edge", new Color(0.86f, 0.9f, 0.8f));
            var noteDark = Materials.Solid("Money Print", new Color(0.22f, 0.4f, 0.24f));
            var band = Materials.Solid("Money Band", new Color(0.95f, 0.85f, 0.45f));
            for (int i = 0; i < 3; i++) Box(root, "Pallet Board", new Vector3(0.66f, 0.025f, 0.12f), new Vector3(0f, 0.08f, -0.2f + i * 0.2f), wood);
            for (int i = 0; i < 3; i++) Box(root, "Pallet Block", new Vector3(0.1f, 0.07f, 0.52f), new Vector3(-0.26f + i * 0.26f, 0.035f, 0f), wood);
            // Unregelmäßiger Stapel: größere Bündel mit Papierkante, aufgedrucktem Mittelfeld und gelber Banderole.
            const float bw = 0.19f, bd = 0.09f, bh = 0.06f;
            int[] heights = { 4, 3, 4, 2, 3, 4, 3, 2, 3, 4, 2, 3 };
            int k = 0;
            for (int x = 0; x < 3; x++)
            for (int z = 0; z < 4; z++, k++)
            {
                int hgt = heights[k];
                for (int layer = 0; layer < hgt; layer++)
                {
                    float jitter = ((k * 7 + layer * 3) % 5 - 2) * 0.006f;
                    var pos = new Vector3(-0.2f + x * (bw + 0.012f) + jitter, 0.12f + layer * bh, -0.17f + z * (bd + 0.022f));
                    Bundle(root, pos, bw, bd, bh, note, noteEdge, noteDark, band);
                }
            }
            // Zwei lose Bündel obendrauf, leicht schräg
            Bundle(root, new Vector3(-0.05f, 0.12f + 4 * bh, 0.0f), bw, bd, bh, note, noteEdge, noteDark, band).transform.localRotation = Quaternion.Euler(0f, 25f, 0f);
            Bundle(root, new Vector3(0.12f, 0.12f + 4 * bh, -0.12f), bw, bd, bh, note, noteEdge, noteDark, band).transform.localRotation = Quaternion.Euler(0f, -15f, 0f);
        }

        private static GameObject Bundle(Transform root, Vector3 pos, float w, float d, float h, Material note, Material edge, Material print, Material band)
        {
            var b = new GameObject("Bundle");
            b.transform.SetParent(root, false);
            b.transform.localPosition = pos;
            ProcMesh.Create("Paper", b.transform, ProcMesh.Box(new Vector3(w, h - 0.006f, d)), edge, Vector3.zero);
            ProcMesh.Create("Top", b.transform, ProcMesh.Box(new Vector3(w - 0.008f, 0.004f, d - 0.008f)), note, new Vector3(0f, (h - 0.006f) / 2f, 0f));
            ProcMesh.Create("Print", b.transform, ProcMesh.Box(new Vector3(0.05f, 0.004f, 0.05f)), print, new Vector3(-0.045f, (h - 0.006f) / 2f + 0.002f, 0f));
            ProcMesh.Create("Band", b.transform, ProcMesh.Box(new Vector3(0.035f, h + 0.004f, d + 0.006f)), band, new Vector3(0.03f, 0f, 0f));
            return b;
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
            // Weicher, runder Sessel aus gepolsterten Formen
            var c = new Vector3(-0.08f, 0f, 0f);
            Shapes.Ball("Base", root, new Vector3(0.5f, 0.2f, 0.44f), purple, c + new Vector3(0f, 0.2f, 0f));
            Shapes.Ball("Cushion", root, new Vector3(0.38f, 0.1f, 0.34f), lilac, c + new Vector3(0f, 0.3f, -0.03f));
            Shapes.Ball("Back", root, new Vector3(0.5f, 0.5f, 0.16f), purple, c + new Vector3(0f, 0.46f, 0.18f));
            Shapes.Ball("Back Cushion", root, new Vector3(0.36f, 0.3f, 0.08f), lilac, c + new Vector3(0f, 0.46f, 0.11f));
            foreach (var sd in new[] { -1f, 1f })
            {
                Shapes.Ball("Arm", root, new Vector3(0.12f, 0.24f, 0.44f), dark, c + new Vector3(0.24f * sd, 0.3f, 0f));
                Shapes.Ball("Foot F", root, new Vector3(0.05f, 0.08f, 0.05f), Materials.Black, c + new Vector3(0.18f * sd, 0.04f, -0.15f));
                Shapes.Ball("Foot B", root, new Vector3(0.05f, 0.08f, 0.05f), Materials.Black, c + new Vector3(0.18f * sd, 0.04f, 0.15f));
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
        protected override Vector3 DefaultRot => new Vector3(0f, 0f, 0f);

        protected override void Build(Transform root)
        {
            // Kein Pult mehr: ein Bücherstapel am Boden, obenauf Gales aufgeschlagenes Laborheft.
            Color[] covers = { new Color(0.55f, 0.18f, 0.15f), new Color(0.2f, 0.3f, 0.5f), new Color(0.3f, 0.42f, 0.25f), new Color(0.5f, 0.4f, 0.25f) };
            float y = 0f;
            for (int i = 0; i < covers.Length; i++)
            {
                float h = 0.07f + (i % 2) * 0.02f;
                var book = new GameObject("Book " + i);
                book.transform.SetParent(root, false);
                book.transform.localPosition = new Vector3((i % 2 == 0 ? -0.02f : 0.03f), y + h / 2f, (i % 3 - 1) * 0.02f);
                book.transform.localRotation = Quaternion.Euler(0f, i * 11f - 15f, 0f);
                var size = new Vector3(0.42f - i * 0.03f, h, 0.3f - i * 0.015f);
                ProcMesh.Create("Cover", book.transform, ProcMesh.Box(size), Materials.Solid("Book " + i, covers[i]), Vector3.zero);
                ProcMesh.Create("Pages", book.transform, ProcMesh.Box(new Vector3(size.x - 0.02f, h - 0.016f, size.z + 0.002f)), Materials.Paper, new Vector3(0.012f, 0f, 0f));
                y += h;
            }
            // Aufgeschlagenes Heft obendrauf
            var nb = new GameObject("Notebook");
            nb.transform.SetParent(root, false);
            nb.transform.localPosition = new Vector3(0f, y, 0f);
            nb.transform.localRotation = Quaternion.Euler(0f, 8f, 0f);
            ProcMesh.Create("Cover", nb.transform, ProcMesh.Box(new Vector3(0.36f, 0.01f, 0.24f)), Materials.Solid("Notebook Cover", new Color(0.15f, 0.3f, 0.2f)), new Vector3(0f, 0.005f, 0f));
            foreach (var sd in new[] { -1f, 1f })
            {
                ProcMesh.Create("Page", nb.transform, ProcMesh.Box(new Vector3(0.165f, 0.012f, 0.22f)), Materials.Paper, new Vector3(0.087f * sd, 0.014f, 0f));
                for (int l = 0; l < 6; l++)
                    ProcMesh.Create("Line", nb.transform, ProcMesh.Box(new Vector3(0.12f - (l % 3) * 0.02f, 0.002f, 0.007f)), Materials.DarkGrey, new Vector3(0.087f * sd - (l % 3) * 0.01f, 0.021f, 0.08f - l * 0.03f));
            }
            var ww = Text(nb.transform, "WW", "To W.W.", new Vector3(-0.087f, 0.022f, -0.08f), 0.26f, new Color(0.15f, 0.25f, 0.6f));
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
        protected override Vector3 DefaultPos => new Vector3(0f, 0.75f, 0.36f);

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
        protected override Vector3 DefaultPos => new Vector3(0f, 0.75f, 0.36f);
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
        // Till: ohne Figur gefällt besser.
        protected override bool Silhouette => false;
    }
}
