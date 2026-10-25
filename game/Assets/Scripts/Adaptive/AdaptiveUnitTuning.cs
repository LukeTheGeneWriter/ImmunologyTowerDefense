using UnityEngine;

namespace ImmunologyTD.Adaptive
{
    /// <summary>
    /// Sprint 18: the per-tower upgrade layer for the two ADAPTIVE kinds --
    /// the dendritic cell and the helper-T cell.
    ///
    /// Innate towers already own a mutable <c>UnitLifecycleTuning</c> per
    /// tower (GAME_DESIGN.md §6d), so an upgrade is a write to one tower's
    /// field. The adaptive numbers live on <see cref="AdaptiveTuning"/> as
    /// process-wide statics, and the Sprint 16 catalog cited those statics
    /// directly -- wiring the rows that way would have made buying an
    /// upgrade on one DC niche upgrade every DC niche on the board, which is
    /// not what a per-niche upgrade panel promises.
    ///
    /// This class holds the upgrade *bonuses* only, not absolute values.
    /// Each effective value is the current <see cref="AdaptiveTuning"/>
    /// static plus this tower's bonus, clamped -- so the statics stay the
    /// single source of the base numbers (a harness that overrides one still
    /// sees its override), and the upgrade rides on top of whatever the base
    /// happens to be.
    ///
    /// Same live-reference rule as the innate kinds (Director, 2026-08-21):
    /// every DC / helper-T holds its tower's instance, so an upgrade applies
    /// instantly to the cells already fielded. A null instance (harness, or a
    /// cell built without a tower) reads the bare statics.
    /// </summary>
    public class AdaptiveUnitTuning
    {
        /// <summary>Extra presentations a DC gets per antigen cargo.</summary>
        public int DcPresentationsBonus;
        /// <summary>Extra fine tiles a DC patrols per tick.</summary>
        public int DcFineTilesBonus;
        /// <summary>Subtracted from a DC's debris bite (smaller bite).</summary>
        public float DcDebrisBiteReduction;
        /// <summary>Extra Hamming distance a helper-T's pairing tolerates.</summary>
        public int MatchDistanceBonus;
        /// <summary>Seconds cut from a helper-T's lifespan.</summary>
        public float LymphocyteLifespanReduction;
        /// <summary>Multiplies this tower's emission RATE (divides its
        /// interval). 1 = the AdaptiveTuning default. Helper-T "Clonal
        /// expansion" raises it -- see ProgenitorUpgrades.ClonalRatePerLevel.</summary>
        public float EmissionRateMultiplier = 1f;

        /// <summary>Floor on the DC's sampled debris per bite -- the catalog's
        /// "floor 0.1". Below this a DC would barely dent the debris it
        /// samples, and sampling must still count as clearing (§1c).</summary>
        public const float DcDebrisBiteFloor = 0.1f;
        /// <summary>Ceiling on match distance -- the catalog's "cap 4". At 4
        /// of 8 bits, 163 of 256 tags match (~64%); beyond that the barcode
        /// stops meaning anything.</summary>
        public const int MatchDistanceCeiling = 4;
        /// <summary>Floor on helper-T lifespan -- the catalog's "floor 6".
        /// Shorter than ~4 pairing windows and a cell dies before it can
        /// usefully be visited.</summary>
        public const float LymphocyteLifespanFloor = 6f;

        public static int PresentationsPerCargo(AdaptiveUnitTuning t) =>
            AdaptiveTuning.DcPresentationsPerCargo + (t?.DcPresentationsBonus ?? 0);

        public static int DcFineTilesPerTick(AdaptiveUnitTuning t) =>
            AdaptiveTuning.DcFineTilesPerTick + (t?.DcFineTilesBonus ?? 0);

        public static float DcDebrisSamplePerBite(AdaptiveUnitTuning t) =>
            t == null
                ? AdaptiveTuning.DcDebrisSamplePerBite
                : Mathf.Max(DcDebrisBiteFloor, AdaptiveTuning.DcDebrisSamplePerBite - t.DcDebrisBiteReduction);

        /// <summary>Sprint 29: levels of the memory-T row on this tower.
        /// Each level adds a share of <see cref="AdaptiveTuning.MemoryRecallBonus"/>,
        /// scaled by how well the species is already known.</summary>
        public int MemoryRecallLevel;

        /// <summary>Probability that a MISSED pairing is rescued by memory,
        /// given current knowledge of the cargo's species (0..1 of
        /// KnowledgeMax). Zero without the upgrade, so an un-upgraded node
        /// is unchanged.</summary>
        public static float MemoryRecallChance(AdaptiveUnitTuning t, float knowledgeFraction)
        {
            if (t == null || t.MemoryRecallLevel <= 0) return 0f;
            float perLevel = AdaptiveTuning.MemoryRecallBonus / MemoryRecallCap;
            return Mathf.Clamp01(perLevel * t.MemoryRecallLevel) * Mathf.Clamp01(knowledgeFraction);
        }

        /// <summary>Levels at which the memory row reaches the full bonus.</summary>
        public const int MemoryRecallCap = 3;

        public static int MatchMaxHammingDistance(AdaptiveUnitTuning t) =>
            t == null
                ? AdaptiveTuning.MatchMaxHammingDistance
                : Mathf.Min(MatchDistanceCeiling, AdaptiveTuning.MatchMaxHammingDistance + t.MatchDistanceBonus);

        public static float LymphocyteLifespanSeconds(AdaptiveUnitTuning t) =>
            t == null
                ? AdaptiveTuning.LymphocyteLifespanSeconds
                : Mathf.Max(LymphocyteLifespanFloor, AdaptiveTuning.LymphocyteLifespanSeconds - t.LymphocyteLifespanReduction);
    }
}
