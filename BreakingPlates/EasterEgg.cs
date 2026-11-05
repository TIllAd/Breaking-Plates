using System.Collections.Generic;
using System.Linq;
using KitchenData;
using KitchenLib.Utils;
using UnityEngine;

namespace BreakingPlates
{
    // Easter Egg: Ist auch "Chicken & Rice" installiert, werden dessen Karten bei der Kartenauswahl bevorzugt.
    // (Die Mods bleiben sonst völlig getrennt.)
    public static class EasterEgg
    {
        public const string ChickenModID = "TillAd.PlateUp.ChickenAndRice";
        public const float Chance = 0.35f;

        public static void Apply(GameData gameData)
        {
            List<Unlock> cards;
            try
            {
                cards = GDOUtils.GetCustomGameDataObjectsFromMod(ChickenModID)
                    .Select(c => c.GameDataObject).OfType<Unlock>().ToList();
            }
            catch { return; }
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
