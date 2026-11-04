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
