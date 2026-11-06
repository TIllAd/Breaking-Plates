using System;
using System.Collections.Generic;
using KitchenData;
using KitchenLib.Customs;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BreakingPlates
{
    // Baut Prefabs zur Laufzeit aus vorhandenen Spielmodellen zusammen (kein Unity/Blender nötig).
    // Übernommen und verallgemeinert aus dem Chicken-&-Rice-Mod.
    public static class PrefabUtil
    {
        private static GameObject _holder;

        // Inaktiver Container: Prefabs darin werden nie selbst gerendert.
        public static Transform Holder
        {
            get
            {
                if (_holder == null)
                {
                    _holder = new GameObject("BreakingPlates Prefabs");
                    _holder.SetActive(false);
                    Object.DontDestroyOnLoad(_holder);
                }
                return _holder.transform;
            }
        }

        public static GameObject CreateRoot(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Holder, false);
            return go;
        }

        // Kopiert nur die Optik (Meshes/Materialien) eines Prefabs, ohne dessen Logik-Komponenten.
        public static GameObject CloneVisual(GameObject source, Transform parent, string name,
            Vector3 localPosition, float scale = 1f)
        {
            var clone = Object.Instantiate(source, parent, false);
            clone.name = name;
            foreach (var mb in clone.GetComponentsInChildren<MonoBehaviour>(true))
                Object.DestroyImmediate(mb);
            foreach (var col in clone.GetComponentsInChildren<Collider>(true))
                Object.DestroyImmediate(col);
            clone.transform.localPosition = localPosition;
            // Teilstücke (z.B. Topf, Wasser) behalten Drehung/Größe des Originals, ganze Item-Prefabs werden neutral platziert.
            bool isChildPart = source.transform.parent != null;
            clone.transform.localRotation = isChildPart ? source.transform.localRotation : Quaternion.identity;
            clone.transform.localScale = (isChildPart ? source.transform.localScale : Vector3.one) * scale;
            clone.SetActive(true);
            return clone;
        }

        // Fremde Mods (z.B. IngredientLib) setzen Materialien erst bei ihrer Registrierung.
        // Kopien von deren Prefabs werden hier nach dem Laden aller Mods nachgezogen.
        private static readonly List<(GameObject clone, CustomGameDataObject source)> MaterialFixups =
            new List<(GameObject, CustomGameDataObject)>();

        public static GameObject CloneCustomItem(CustomGameDataObject source, Transform parent, string name,
            Vector3 localPosition, float scale = 1f)
        {
            var go = CloneVisual(((CustomItem<Item>)source).Prefab, parent, name, localPosition, scale);
            MaterialFixups.Add((go, source));
            return go;
        }

        public static void FixMaterials()
        {
            foreach (var (clone, source) in MaterialFixups)
            {
                var item = source.GameDataObject as Item;
                if (item == null || item.Prefab == null || clone == null) continue;
                var src = item.Prefab.GetComponentsInChildren<MeshRenderer>(true);
                var dst = clone.GetComponentsInChildren<MeshRenderer>(true);
                for (int i = 0; i < Math.Min(src.Length, dst.Length); i++)
                    dst[i].sharedMaterials = src[i].sharedMaterials;
            }
        }

    }
}
