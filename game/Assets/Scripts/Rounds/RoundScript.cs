using UnityEngine;
using ImmunologyTD.Pathogens;

namespace ImmunologyTD.Rounds
{
    /// <summary>One organism in a round's cast, and how much of the batch it
    /// makes up. Weights are plain floats and need not sum to anything --
    /// <see cref="RoundDefinition.RollSpecies"/> normalises them.</summary>
    [System.Serializable]
    public struct SpeciesWeight
    {
        public PathogenSpecies Species;
        public float Weight;

        public SpeciesWeight(PathogenSpecies species, float weight)
        {
            Species = species;
            Weight = weight;
        }
    }

    /// <summary>
    /// What round N is: a gut-themed **tagline**, the **cast** of organisms
    /// its contaminated food item carries, and how hard it leans on the
    /// wall. The batch **size** comes from
    /// <c>EconomyTuning.BatchSizeForRound</c>, scaled by
    /// <see cref="WallPressure"/>.
    ///
    /// ## Sprint 32: the cast is SPECIES, not classes
    ///
    /// Sprint 9 wrote this as three class weights, which was right at the
    /// time -- there were three pathogen classes and nothing else. Sprints
    /// 24-25 then introduced named species, and this file was never
    /// revisited. The consequence, found in Sprint 32: a round rolled a
    /// <see cref="PathogenClass"/>, <c>PathogenAgent.Initialize</c> resolved
    /// a bare class to the GENERIC species of that class, and so **rotavirus,
    /// Vibrio cholerae and A/E E. coli could never appear in play at all** --
    /// three organisms with working traits, sprites and knowledge-HUD rows
    /// that no playtest had ever seen. The taglines, meanwhile, promised
    /// Salmonella, poliovirus and Listeria, none of which are in the roster.
    ///
    /// The class fields are kept and still work (a definition with no cast
    /// behaves exactly as it did in Sprint 9), because two harnesses build
    /// class-weighted rounds directly to test the roll.
    /// </summary>
    public struct RoundDefinition
    {
        public string Tagline;

        /// <summary>Sprint 32: the organisms this round actually delivers.
        /// Null or empty falls back to the class weights below.</summary>
        public SpeciesWeight[] Cast;

        /// <summary>Multiplier on the round's batch size -- how much traffic
        /// arrives at the wall. **Zero means 1.0**, so a definition written
        /// before this field existed is unchanged.
        ///
        /// This is the lever for the Sprint 22 IgA miss. The Director never
        /// perceived secretory IgA, and the diagnosis was explicitly NOT
        /// that IgA is too weak: IgA acts in the lumen, so if the tissue
        /// defence clears everything before pressure builds at the wall, the
        /// player never sees the thing IgA prevents. A defence against a
        /// problem you never have is invisible by construction. Later rounds
        /// therefore have to crowd the wall.</summary>
        public float WallPressure;

        // -- Pre-Sprint-32 class weights. Used when Cast is empty. --
        public float VirusWeight;
        public float BacteriumWeight;
        public float LargeBacteriumWeight;

        public float EffectiveWallPressure => WallPressure <= 0f ? 1f : WallPressure;

        public bool HasCast => Cast != null && Cast.Length > 0;

        /// <summary>Rolls one organism from the cast. Falls back to the
        /// generic of a class-weighted roll, which is exactly what every
        /// round did before Sprint 32.</summary>
        public PathogenSpecies RollSpecies()
        {
            if (!HasCast) return SpeciesRoster.GenericFor(RollClass());

            float total = 0f;
            for (int i = 0; i < Cast.Length; i++) total += Mathf.Max(0f, Cast[i].Weight);
            if (total <= 0f) return Cast[0].Species;

            float r = Random.value * total;
            float running = 0f;
            for (int i = 0; i < Cast.Length; i++)
            {
                running += Mathf.Max(0f, Cast[i].Weight);
                if (r <= running) return Cast[i].Species;
            }
            return Cast[Cast.Length - 1].Species;
        }

        /// <summary>The class of one rolled organism. Kept for callers and
        /// harnesses that still speak in classes; with a cast present the
        /// class is DERIVED from the species rather than rolled separately,
        /// so the two can never disagree.</summary>
        public PathogenClass RollClass()
        {
            if (HasCast) return SpeciesRoster.ClassOf(RollSpecies());

            float total = Mathf.Max(0f, VirusWeight) + Mathf.Max(0f, BacteriumWeight) + Mathf.Max(0f, LargeBacteriumWeight);
            if (total <= 0f) return PathogenClass.LargeBacterium;
            float r = Random.value * total;
            if (r < VirusWeight) return PathogenClass.IntracellularVirus;
            if (r < VirusWeight + BacteriumWeight) return PathogenClass.IntracellularBacterium;
            return PathogenClass.LargeBacterium;
        }
    }

    /// <summary>
    /// The scripted opening rounds -- Sprint 32's **curriculum**.
    ///
    /// Three rules shaped this list, and they are design decisions rather
    /// than flavour:
    ///
    ///  1. **One new organism at a time, against a board you already
    ///     understand.** Species are learned independently (§5c), so round
    ///     order *is* teaching order.
    ///  2. **Organisms RECUR after they are introduced.** This is the part
    ///     that is easy to get wrong: knowledge is per species, and the
    ///     adaptive ladder's rungs (secretory IgA at 70%) only fire for a
    ///     species the player has actually accumulated knowledge on. A
    ///     parade of novelties would keep every species below its rungs
    ///     forever and the whole adaptive arm would stay dark -- which is
    ///     very close to what the Sprint 22 playtest reported.
    ///  3. **Wall pressure rises.** See <see cref="RoundDefinition.WallPressure"/>.
    ///     Rounds 1 and 2 stay at 1.0 deliberately: the economy harnesses
    ///     assert the unscaled batch size for those two rounds, and a
    ///     tutorial round should not be crowded anyway.
    ///
    /// Taglines now name what the round actually contains. Before Sprint 32
    /// they named organisms that were not in the game.
    /// </summary>
    public static class RoundScript
    {
        private static readonly RoundDefinition[] Scripted =
        {
            // R1 -- one familiar enemy, nothing else. Learn adhesion, the
            // mill at the wall, and that macrophages eat.
            new RoundDefinition
            {
                Tagline = "Bagged salad, gut bacteria",
                WallPressure = 1f,
                Cast = new[] { new SpeciesWeight(PathogenSpecies.GenericLargeBacterium, 3f) },
            },

            // R2 -- the second class. A virus behaves nothing like a
            // bacterium (it takes a cell rather than a slot), so it is the
            // next thing to meet, with the familiar organism still dominant.
            new RoundDefinition
            {
                Tagline = "Undercooked egg, bacteria and a virus",
                WallPressure = 1f,
                Cast = new[]
                {
                    new SpeciesWeight(PathogenSpecies.GenericLargeBacterium, 3f),
                    new SpeciesWeight(PathogenSpecies.GenericVirus, 1f),
                    new SpeciesWeight(PathogenSpecies.GenericIntracellularBacterium, 1f),
                },
            },

            // R3 -- *Vibrio cholerae*, the first named organism, and the
            // gentlest introduction of the three: it never kills a cell and
            // never takes a slot. Its whole lesson is that looking for dying
            // cells is not enough, because subverted wall looks healthy.
            new RoundDefinition
            {
                Tagline = "Raw oysters, Vibrio cholerae",
                WallPressure = 1.2f,
                Cast = new[]
                {
                    new SpeciesWeight(PathogenSpecies.VibrioCholerae, 3f),
                    new SpeciesWeight(PathogenSpecies.GenericLargeBacterium, 2f),
                    new SpeciesWeight(PathogenSpecies.GenericVirus, 1f),
                },
            },

            // R4 -- rotavirus. Teaches that WHERE a thing can reach matters:
            // it eats the villus tip and can never touch the crypt, so the
            // board regrows from one end while damage creeps the other way.
            new RoundDefinition
            {
                Tagline = "Nursery water bottle, rotavirus",
                WallPressure = 1.3f,
                Cast = new[]
                {
                    new SpeciesWeight(PathogenSpecies.Rotavirus, 3f),
                    new SpeciesWeight(PathogenSpecies.GenericVirus, 1f),
                    new SpeciesWeight(PathogenSpecies.VibrioCholerae, 1f),
                    new SpeciesWeight(PathogenSpecies.GenericLargeBacterium, 1f),
                },
            },

            // R5 -- A/E *E. coli*, deliberately last of the three and on a
            // crowded wall. It respires the products of the host's own
            // response, so a messy neutrophil-heavy clear feeds the
            // survivors. That is the one strategy in the research with no
            // answer anywhere in the roster, and it should land on a player
            // who has already settled into a routine.
            new RoundDefinition
            {
                Tagline = "Street-cart mince, attaching-effacing E. coli",
                WallPressure = 1.5f,
                Cast = new[]
                {
                    new SpeciesWeight(PathogenSpecies.AttachingEffacingEcoli, 3f),
                    new SpeciesWeight(PathogenSpecies.GenericLargeBacterium, 1f),
                    new SpeciesWeight(PathogenSpecies.Rotavirus, 1f),
                    new SpeciesWeight(PathogenSpecies.GenericIntracellularBacterium, 1f),
                },
            },

            // R6 -- everything, at pressure. Every species the player has
            // met, no novelty: the round that asks whether the routine
            // actually holds.
            new RoundDefinition
            {
                Tagline = "Street-cart everything",
                WallPressure = 1.7f,
                Cast = new[]
                {
                    new SpeciesWeight(PathogenSpecies.GenericLargeBacterium, 2f),
                    new SpeciesWeight(PathogenSpecies.GenericVirus, 2f),
                    new SpeciesWeight(PathogenSpecies.GenericIntracellularBacterium, 1f),
                    new SpeciesWeight(PathogenSpecies.VibrioCholerae, 2f),
                    new SpeciesWeight(PathogenSpecies.Rotavirus, 2f),
                    new SpeciesWeight(PathogenSpecies.AttachingEffacingEcoli, 2f),
                },
            },
        };

        /// <summary>Highest wall pressure the procedural tail will reach.
        /// Past here a round gets longer and more varied, not denser --
        /// density is bounded by what the wall mechanics can express before
        /// the lumen simply saturates.</summary>
        public const float MaxProceduralWallPressure = 2.5f;

        public static int ScriptedRounds => Scripted.Length;

        public static RoundDefinition ForRound(int roundNumber)
        {
            int n = roundNumber < 1 ? 1 : roundNumber;
            if (n <= Scripted.Length) return Scripted[n - 1];

            // Procedural tail: the full roster, with pressure still climbing
            // toward the cap. Reuses round 6's cast rather than inventing a
            // mix, so "past the script" means more of everything the player
            // has already learned, not a different game.
            int past = n - Scripted.Length;
            return new RoundDefinition
            {
                // The ROUND number, not the count past the script:
                // RoundVerification asserts the procedural tagline names the
                // round, and the HUD round bar shows this string.
                Tagline = $"Spoiled leftovers, day {n}",
                WallPressure = Mathf.Min(
                    MaxProceduralWallPressure,
                    Scripted[Scripted.Length - 1].EffectiveWallPressure + 0.1f * past),
                Cast = Scripted[Scripted.Length - 1].Cast,
            };
        }
    }
}
