using UnityEngine;
using ImmunologyTD.Pathogens;

namespace ImmunologyTD.Rounds
{
    /// <summary>
    /// What round N is: a gut-themed **tagline** and the **class mix** of
    /// the pathogens its contaminated food item carries (Sprint 9, Director
    /// 2026-08-29). This is a light version of the long-deferred "round
    /// batch composition" backlog item — per-round class weights and flavour
    /// text, no boss rounds and no real difficulty curve yet. The batch
    /// **size** still comes from <c>EconomyTuning.BatchSizeForRound</c>.
    ///
    /// The weights are plain floats; <see cref="RoundDefinition.RollClass"/>
    /// normalises them, so "3, 1, 0" just means "mostly virus, some
    /// bacterium, no large bacterium."
    /// </summary>
    public struct RoundDefinition
    {
        public string Tagline;
        public float VirusWeight;
        public float BacteriumWeight;
        public float LargeBacteriumWeight;

        public PathogenClass RollClass()
        {
            float total = Mathf.Max(0f, VirusWeight) + Mathf.Max(0f, BacteriumWeight) + Mathf.Max(0f, LargeBacteriumWeight);
            if (total <= 0f) return PathogenClass.LargeBacterium;
            float r = Random.value * total;
            if (r < VirusWeight) return PathogenClass.IntracellularVirus;
            if (r < VirusWeight + BacteriumWeight) return PathogenClass.IntracellularBacterium;
            return PathogenClass.LargeBacterium;
        }
    }

    public static class RoundScript
    {
        // Hand-written opening rounds. Taglines are pure flavour and safe to
        // rewrite — they're data. Past the end of this list, ForRound falls
        // back to a procedural definition.
        private static readonly RoundDefinition[] Scripted =
        {
            new RoundDefinition { Tagline = "Bagged salad, E. coli O157",          VirusWeight = 0f, BacteriumWeight = 2f, LargeBacteriumWeight = 3f },
            new RoundDefinition { Tagline = "Undercooked egg, Salmonella",          VirusWeight = 0f, BacteriumWeight = 4f, LargeBacteriumWeight = 1f },
            new RoundDefinition { Tagline = "Contaminated water, poliovirus",       VirusWeight = 4f, BacteriumWeight = 1f, LargeBacteriumWeight = 0f },
            new RoundDefinition { Tagline = "Raw oysters, norovirus + Vibrio",      VirusWeight = 3f, BacteriumWeight = 1f, LargeBacteriumWeight = 2f },
            new RoundDefinition { Tagline = "Deli meat, Listeria",                  VirusWeight = 0f, BacteriumWeight = 3f, LargeBacteriumWeight = 2f },
            new RoundDefinition { Tagline = "Street-cart everything",               VirusWeight = 2f, BacteriumWeight = 2f, LargeBacteriumWeight = 2f },
        };

        public static RoundDefinition ForRound(int roundNumber)
        {
            int n = roundNumber < 1 ? 1 : roundNumber;
            if (n <= Scripted.Length) return Scripted[n - 1];

            // Procedural fallback: a rotating "spoiled leftovers, day N" with
            // an even three-way mix.
            return new RoundDefinition
            {
                Tagline = $"Spoiled leftovers, day {n}",
                VirusWeight = 2f,
                BacteriumWeight = 2f,
                LargeBacteriumWeight = 2f,
            };
        }
    }
}
