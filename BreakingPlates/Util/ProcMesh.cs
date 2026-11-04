using System.Collections.Generic;
using UnityEngine;

namespace BreakingPlates
{
    // Formen, die es im Spiel nicht gibt, direkt im Code erzeugt (kein Blender nötig).
    public static class ProcMesh
    {
        private static readonly Dictionary<string, Mesh> Cache = new Dictionary<string, Mesh>();

        // Folien-Tütchen: flaches Kissen, an den Rändern verschweißt, in der Mitte aufgebläht.
        // Beidseitig (jedes Dreieck auch rückwärts), damit es aus jedem Winkel sichtbar ist.
        public static Mesh Pouch(float width = 0.34f, float height = 0.4f, float bulge = 0.09f, int res = 12)
        {
            string key = $"pouch{width:0.####}_{height:0.####}_{bulge:0.####}_{res}";
            if (Cache.TryGetValue(key, out var cached)) return cached;

            var verts = new List<Vector3>();
            var tris = new List<int>();
            for (int side = 0; side < 2; side++)
            {
                float dir = side == 0 ? -1f : 1f;
                int start = verts.Count;
                for (int y = 0; y <= res; y++)
                for (int x = 0; x <= res; x++)
                {
                    float u = x / (float)res, v = y / (float)res;
                    // Aufblähung: Null an den Rändern, max. in der Mitte. Die oberen 12 % bleiben ganz flach
                    // (verschweißter Streifen unter dem Druckverschluss).
                    float vv = Mathf.Clamp01(v / 0.88f);
                    float puff = Mathf.Sin(Mathf.PI * u) * Mathf.Pow(Mathf.Sin(Mathf.PI * vv), 0.6f) * (1f - 0.4f * vv);
                    verts.Add(new Vector3((u - 0.5f) * width, v * height, dir * bulge * puff));
                }
                for (int y = 0; y < res; y++)
                for (int x = 0; x < res; x++)
                {
                    int i = start + y * (res + 1) + x;
                    int a = i, b = i + 1, c = i + res + 1, d = i + res + 2;
                    // beide Wickelrichtungen -> doppelseitig
                    tris.AddRange(new[] { a, c, b, b, c, d });
                    tris.AddRange(new[] { a, b, c, b, d, c });
                }
            }
            var mesh = new Mesh { name = "BreakingPlates Pouch" };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            Cache[key] = mesh;
            return mesh;
        }

        // Einfacher Quader (für Druckverschluss-Streifen u.ä.).
        public static Mesh Box(Vector3 size)
        {
            string key = $"box{size}";
            if (Cache.TryGetValue(key, out var cached)) return cached;
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var mesh = Object.Instantiate(tmp.GetComponent<MeshFilter>().sharedMesh);
            Object.DestroyImmediate(tmp);
            var v = mesh.vertices;
            for (int i = 0; i < v.Length; i++) v[i] = Vector3.Scale(v[i], size);
            mesh.vertices = v;
            mesh.RecalculateBounds();
            mesh.name = "BreakingPlates Box";
            Cache[key] = mesh;
            return mesh;
        }

        public static GameObject Create(string name, Transform parent, Mesh mesh, Material material, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }
    }
}
