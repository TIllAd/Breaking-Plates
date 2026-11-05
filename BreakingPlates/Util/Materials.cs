using System.Collections.Generic;
using UnityEngine;

namespace BreakingPlates
{
    // Eigene Materialien im Stil des Spiels ("Simple Flat"-Shader).
    public static class Materials
    {
        private static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();

        public static Material Flat(string name, Color color, float shininess = 0f)
        {
            if (Cache.TryGetValue(name, out var m) && m != null) return m;
            m = new Material(Shader.Find("Simple Flat")) { name = "BreakingPlates - " + name };
            color.a = 0f;
            m.SetColor("_Color0", color);
            m.SetFloat("_Shininess", shininess);
            m.SetFloat("_OverlayScale", 10f);
            Cache[name] = m;
            return m;
        }

        // Das typische Blau.
        public static Material BlueSyrup => Flat("Blue Syrup", Tuning.C("color.syrup", new Color(0.18f, 0.52f, 0.95f)), 0.4f);
        public static Material BlueCrystal => Flat("Blue Crystal", Tuning.C("color.crystal", new Color(0.45f, 0.8f, 1f)), 0.9f);

        public static Material PureCrystal => Flat("Pure Crystal", Tuning.C("color.pure", new Color(0.9f, 0.98f, 1f)), 1f);
        public static Material Water => Flat("Water", Tuning.C("color.water", new Color(0.7f, 0.86f, 0.96f)), 0.6f);
        public static Material Sugar => Flat("Sugar", new Color(0.97f, 0.95f, 0.9f), 0.3f);

        // Milchig-durchsichtiges Plastik (Shader "Simple Transparent" wie Gläser im Spiel).
        public static Material Plastic
        {
            get
            {
                if (Cache.TryGetValue("Plastic", out var m) && m != null) return m;
                m = new Material(Shader.Find("Simple Transparent")) { name = "BreakingPlates - Plastic" };
                m.SetColor("_Color", Tuning.C("color.plastic", new Color(0.92f, 0.96f, 1f, 0.35f)));
                Cache["Plastic"] = m;
                return m;
            }
        }
        public static Material ZipRed => Flat("Zip Red", Tuning.C("color.zip.chili", new Color(0.85f, 0.15f, 0.15f)), 0.3f);
        public static Material ZipBlue => Flat("Zip Blue", Tuning.C("color.zip", new Color(0.2f, 0.45f, 0.9f)), 0.3f);
        public static Material Glass
        {
            get
            {
                if (Cache.TryGetValue("Glass", out var m) && m != null) return m;
                m = new Material(Shader.Find("Simple Transparent")) { name = "BreakingPlates - Glass" };
                m.SetColor("_Color", Tuning.C("color.glass", new Color(0.85f, 0.95f, 1f, 0.3f)));
                Cache["Glass"] = m;
                return m;
            }
        }
        public static Material ZipGreen => Flat("Zip Green", Tuning.C("color.zip.stevia", new Color(0.25f, 0.75f, 0.3f)), 0.3f);
        public static Material Stevia => Flat("Stevia", new Color(0.82f, 0.95f, 0.78f), 0.3f);
        public static Material Wood => Flat("Stick Wood", new Color(0.93f, 0.85f, 0.65f), 0.1f);
        public static Material Black => Flat("Black", new Color(0.08f, 0.08f, 0.09f), 0.4f);
        public static Material DarkGrey => Flat("Dark Grey", new Color(0.25f, 0.25f, 0.27f), 0.3f);
        public static Material Hazmat => Flat("Hazmat Yellow", Tuning.C("color.hazmat", new Color(0.96f, 0.86f, 0.38f)), 0.2f);
        public static Material GloveBlue => Flat("Glove Blue", new Color(0.2f, 0.4f, 0.85f), 0.4f);
        public static Material FilterPink => Flat("Filter Pink", new Color(0.92f, 0.45f, 0.6f), 0.2f);
        public static Material Beer => Flat("Beer", new Color(0.93f, 0.6f, 0.12f), 0.6f);
        public static Material Foam => Flat("Foam", new Color(0.99f, 0.97f, 0.9f), 0.1f);
        public static Material Red => Flat("Red", new Color(0.82f, 0.16f, 0.12f), 0.2f);
        public static Material Green => Flat("Green", new Color(0.18f, 0.55f, 0.22f), 0.2f);
        public static Material Paper => Flat("Paper", new Color(0.96f, 0.94f, 0.86f), 0f);
        public static Material Wall => Flat("Board", new Color(0.35f, 0.24f, 0.16f), 0.1f);
        public static Material PizzaYellow => Flat("Sign Yellow", new Color(0.99f, 0.78f, 0.18f), 0.2f);
        public static Material Solid(string name, Color c) => Flat(name, c, 0.2f);

        public static Material LabelWhite => Flat("Label White", new Color(0.97f, 0.97f, 0.95f));

        // Bei F8: Farben der vorhandenen Materialien aktualisieren.
        public static void RefreshColors()
        {
            void Set(string key, string tuneKey, Color def, string prop = "_Color0")
            {
                if (Cache.TryGetValue(key, out var m) && m != null) m.SetColor(prop, Tuning.C(tuneKey, def));
            }
            Set("Blue Syrup", "color.syrup", new Color(0.18f, 0.52f, 0.95f));
            Set("Blue Crystal", "color.crystal", new Color(0.45f, 0.8f, 1f));
            Set("Pure Crystal", "color.pure", new Color(0.9f, 0.98f, 1f));
            Set("Zip Red", "color.zip.chili", new Color(0.85f, 0.15f, 0.15f));
            Set("Zip Blue", "color.zip", new Color(0.2f, 0.45f, 0.9f));
            Set("Zip Green", "color.zip.stevia", new Color(0.25f, 0.75f, 0.3f));
            Set("Hazmat Yellow", "color.hazmat", new Color(0.96f, 0.86f, 0.38f));
            Set("Glass", "color.glass", new Color(0.85f, 0.95f, 1f, 0.3f), "_Color");
            Set("Plastic", "color.plastic", new Color(0.92f, 0.96f, 1f, 0.35f), "_Color");
        }

        public static void Apply(GameObject go, Material material)
        {
            foreach (var r in go.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = material;
                r.sharedMaterials = mats;
            }
        }
    }
}
