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

        public static Material PureCrystal => Flat("Pure Crystal", Tuning.C("color.pure", new Color(0.55f, 0.88f, 1f)), 1f);
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
            Set("Pure Crystal", "color.pure", new Color(0.55f, 0.88f, 1f));
            Set("Zip Red", "color.zip.chili", new Color(0.85f, 0.15f, 0.15f));
            Set("Zip Blue", "color.zip", new Color(0.2f, 0.45f, 0.9f));
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
