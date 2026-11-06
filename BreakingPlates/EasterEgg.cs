using System.Collections.Generic;
using System.Linq;
using KitchenData;
using KitchenLib.Utils;
using UnityEngine;

namespace BreakingPlates
{
    // Easter Egg (Los Pollos Hermanos): Hühnchen-Karten anderer Mods werden bei der Kartenauswahl bevorzugt –
    // alle Karten von "Chicken & Rice" und Chicken-Karten anderer Mods wie "Fry That!" (erkannt am Namen).
    // (Die Mods bleiben sonst völlig getrennt.)
    public static class EasterEgg
    {
        public const string ChickenModID = "TillAd.PlateUp.ChickenAndRice";
        public const float Chance = 0.35f;

        public static void Apply(GameData gameData)
        {
            var cards = new List<Unlock>();
            try
            {
                cards.AddRange(GDOUtils.GetCustomGameDataObjectsFromMod(ChickenModID)
                    .Select(c => c.GameDataObject).OfType<Unlock>());
            }
            catch { }
            try
            {
                // Andere Mods (z.B. "Fry That!"): Karten-Gerichte mit "Chicken" oder "Nugget" im Namen.
                foreach (var c in KitchenLib.Customs.CustomGDO.GDOs.Values)
                {
                    if (c == null || c.ModID == ChickenModID || c.ModID == Mod.MOD_GUID) continue;
                    if (!(c.GameDataObject is Dish d)) continue;
                    string n = (c.UniqueNameID + " " + d.name).ToLower();
                    if ((n.Contains("chicken") || n.Contains("nugget")) && !cards.Contains(d)) cards.Add(d);
                }
            }
            catch { }
            if (cards.Count == 0) return;

            int packs = 0;
            foreach (var pack in gameData.Get<ModularUnlockPack>())
            {
                if (pack.Sorters == null || pack.Sorters.Any(x => x is ChickenSorter)) continue;
                pack.Sorters.Add(new ChickenSorter { Cards = cards });
                packs++;
            }
            Mod.LogInfo($"Pollos Hermanos: {cards.Count} Chicken-Karten in {packs} Kartenpaketen bevorzugt.");
        }
    }

    public class ChickenSorter : IUnlockSorter
    {
        public List<Unlock> Cards = new List<Unlock>();

        public void SortCards(ref List<Unlock> candidates, HashSet<int> current_cards, UnlockRequest request)
        {
            if (Random.value >= EasterEgg.Chance) return;
            candidates = candidates.OrderByDescending(c => Cards.Contains(c) ? 1 : 0).ToList();
        }
    }
}
