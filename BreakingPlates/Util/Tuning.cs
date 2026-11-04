using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace BreakingPlates
{
    // Live-Tuning für Optik: Werte stehen in Mods/BreakingPlates/tuning.txt ("schlüssel = wert").
    // Im Spiel F8 drücken -> Datei neu laden und auf alle markierten Objekte anwenden (auch Prefabs).
    // In Release-Builds ohne Datei gelten einfach die Standardwerte.
    public static class Tuning
    {
        private static readonly Dictionary<string, float[]> Values = new Dictionary<string, float[]>();
        private static readonly Dictionary<string, string> Defaults = new Dictionary<string, string>();
        private static readonly Dictionary<string, Action<GameObject>> Appliers = new Dictionary<string, Action<GameObject>>();
        private static readonly List<Action> GlobalAppliers = new List<Action>();

        public static string FilePath => Path.Combine(Application.dataPath, "..", "Mods", "BreakingPlates", "tuning.txt");

        public static float F(string key, float def) => Get(key, new[] { def })[0];
        public static Vector3 V3(string key, Vector3 def)
        {
            var v = Get(key, new[] { def.x, def.y, def.z });
            return v.Length >= 3 ? new Vector3(v[0], v[1], v[2]) : def;
        }
        public static Color C(string key, Color def)
        {
            var v = Get(key, new[] { def.r, def.g, def.b, def.a });
            return v.Length >= 4 ? new Color(v[0], v[1], v[2], v[3]) : v.Length == 3 ? new Color(v[0], v[1], v[2], def.a) : def;
        }

        private static float[] Get(string key, float[] def)
        {
            if (!Defaults.ContainsKey(key))
                Defaults[key] = string.Join(", ", Array.ConvertAll(def, f => f.ToString("0.###", CultureInfo.InvariantCulture)));
            return Values.TryGetValue(key, out var v) ? v : def;
        }

        // Markiert ein Objekt, damit es bei F8 mit "apply" neu eingestellt wird.
        public static void Register(GameObject go, string tag, Action<GameObject> apply)
        {
            go.AddComponent<Tunable>().Tag = tag;
            Appliers[tag] = apply;
        }

        // Für Dinge ohne Objekt (z.B. Materialfarben).
        public static void RegisterGlobal(Action apply) => GlobalAppliers.Add(apply);

        public static void Load()
        {
            Values.Clear();
            try
            {
                if (!File.Exists(FilePath)) return;
                foreach (var raw in File.ReadAllLines(FilePath))
                {
                    var line = raw.Split('#')[0].Trim();
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    var key = line.Substring(0, eq).Trim();
                    var parts = line.Substring(eq + 1).Split(',');
                    var nums = new List<float>();
                    foreach (var p in parts)
                        if (float.TryParse(p.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var f)) nums.Add(f);
                    if (nums.Count > 0) Values[key] = nums.ToArray();
                }
            }
            catch (Exception e) { Mod.LogWarning("Tuning laden fehlgeschlagen: " + e.Message); }
        }

        // Schreibt alle bekannten Schlüssel mit aktuellen Werten (für den Start / zum Nachschauen).
        public static void WriteDefaultsIfMissing()
        {
            try
            {
                if (File.Exists(FilePath)) return;
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                var lines = new List<string> { "# Breaking Plates Live-Tuning. Werte aendern, im Spiel F8 druecken.", "" };
                var keys = new List<string>(Defaults.Keys); keys.Sort();
                foreach (var k in keys) lines.Add($"{k} = {Defaults[k]}");
                File.WriteAllLines(FilePath, lines);
            }
            catch (Exception e) { Mod.LogWarning("Tuning schreiben fehlgeschlagen: " + e.Message); }
        }

        public static void Reload()
        {
            Load();
            foreach (var g in GlobalAppliers) g();
            int n = 0;
            foreach (var t in Resources.FindObjectsOfTypeAll<Tunable>())
                if (t != null && Appliers.TryGetValue(t.Tag, out var a)) { a(t.gameObject); n++; }
            Mod.LogWarning($"Tuning neu geladen ({Values.Count} Werte, {n} Objekte).");
        }
    }

    public class Tunable : MonoBehaviour
    {
        public string Tag;
    }
}
