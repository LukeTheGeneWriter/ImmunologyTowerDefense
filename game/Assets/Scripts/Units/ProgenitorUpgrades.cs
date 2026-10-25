using UnityEngine;
using ImmunologyTD.Adaptive;

namespace ImmunologyTD.Units
{
    /// <summary>
    /// Sprint 18: what each progenitor upgrade row actually DOES -- the
    /// model-side half of <c>UI.ProgenitorUpgradeCatalog</c> (which owns the
    /// names, effect text, prices and caps). Row indices here must line up
    /// with the catalog's; <c>Sprint18Verification</c> buys every row of
    /// every kind and asserts the field moved, so a reorder on one side trips
    /// it.
    ///
    /// Every write is recomputed from the BASE value + level, never
    /// incremented in place. Applying the same level twice is a no-op, and
    /// the per-kind defaults (UnitProfile for innate, the AdaptiveTuning
    /// statics for adaptive) stay the one place the base numbers live.
    /// </summary>
    public static class ProgenitorUpgrades
    {
        // Macrophage
        public const float EfferocytosisPerLevel = 0.03f;
        public const int MacrophageKillLimitPerLevel = 8;
        public const int ContactRadiusPerLevel = 1;
        // Neutrophil
        public const float DegranulationCutPerLevel = 0.5f;
        public const float DegranulationFloor = 1f;
        public const int NeutrophilKillLimitPerLevel = 2;
        public const int NeutrophilSpeedPerLevel = 1;
        // Dendritic cell
        public const int DcPresentationsPerLevel = 2;
        public const int DcSpeedPerLevel = 1;
        public const float DcBiteCutPerLevel = 0.08f;
        // Helper-T
        /// <summary>Sprint 25: 4 -> 2. The base cap fell from 8 to 4, so a
        /// +4 step was now a doubling per level -- the upgrade would have
        /// gone from meaningless (it never bound) straight to dominant. +2
        /// on a base of 4 is a felt step that stays in proportion.</summary>
        public const int ClonalCapPerLevel = 2;
        /// <summary>Clonal expansion also speeds this tower's helper-T
        /// production by (1 + this · level). Without it the cap bump alone is
        /// inert: at the default 20s lifespan and 3.5s interval a tower holds
        /// ~6 cells, under the base cap of 8, so raising the cap changes
        /// nothing. Proliferation is what clonal expansion *is*, so the rate
        /// is the honest lever; the cap stays the ceiling.</summary>
        public const float ClonalRatePerLevel = 0.5f;
        public const int MatchDistancePerLevel = 1;
        public const float LifespanCutPerLevel = 4f;

        // -- Sprint 29: the four towers that shipped with no upgrades --

        public const float CtlGranzymePerLevel = 0.012f;
        public const int CtlSpeedPerLevel = 1;
        public const int CtlSynapseReach = 1;

        public const float NkAuraRatePerLevel = 0.008f;
        public const int NkAuraRadiusPerLevel = 1;
        public const float NkNkg2dBonus = 0.008f;

        public const float GdPatrolPerLevel = 0.6f;
        public const float GdRepairCutPerLevel = 0.55f;
        public const float GdRepairFloor = 1.6f;
        public const int GdNichePerLevel = 1;

        public const float PlasmaIgaCutPerLevel = 0.25f;
        public const float PlasmaSecretionFloor = 0.35f;
        public const float PlasmaSecretoryComponentPerLevel = 1.5f;
        public const int PlasmaNichePerLevel = 1;

        /// <summary>Write row <paramref name="row"/>'s effect at
        /// <paramref name="level"/> onto one tower's tuning.
        /// <paramref name="profile"/> is the innate kind's defaults (null for
        /// adaptive); <paramref name="adaptiveBaseCap"/> is the adaptive
        /// kind's default MaxActiveChildren (0 for innate).</summary>
        public static void Apply(UnitKind kind, int row, int level,
            UnitLifecycleTuning tuning, AdaptiveUnitTuning adaptive,
            UnitProfile profile, int adaptiveBaseCap)
        {
            switch (kind)
            {
                case UnitKind.Macrophage:
                    if (tuning == null || profile == null) return;
                    if (row == 0) tuning.EfferocytosisDebrisPerTick = profile.EfferocytosisDebrisPerTick + EfferocytosisPerLevel * level;
                    else if (row == 1) tuning.KillLimit = profile.KillLimit + MacrophageKillLimitPerLevel * level;
                    else if (row == 2) tuning.ContactRadiusFineTiles = profile.ContactRadiusFineTiles + ContactRadiusPerLevel * level;
                    break;

                case UnitKind.Neutrophil:
                    if (tuning == null || profile == null) return;
                    if (row == 0) tuning.DegranulationBurstMultiplier =
                        Mathf.Max(DegranulationFloor, profile.DegranulationBurstMultiplier - DegranulationCutPerLevel * level);
                    else if (row == 1) tuning.KillLimit = profile.KillLimit + NeutrophilKillLimitPerLevel * level;
                    else if (row == 2) tuning.FineTilesPerTick = profile.FineTilesPerTick + NeutrophilSpeedPerLevel * level;
                    break;

                case UnitKind.DendriticCell:
                    if (adaptive == null) return;
                    if (row == 0) adaptive.DcPresentationsBonus = DcPresentationsPerLevel * level;
                    else if (row == 1) adaptive.DcFineTilesBonus = DcSpeedPerLevel * level;
                    else if (row == 2) adaptive.DcDebrisBiteReduction = DcBiteCutPerLevel * level;
                    break;

                case UnitKind.HelperT:
                    if (adaptive == null) return;
                    if (row == 0)
                    {
                        if (tuning != null) tuning.MaxActiveChildren = adaptiveBaseCap + ClonalCapPerLevel * level;
                        adaptive.EmissionRateMultiplier = 1f + ClonalRatePerLevel * level;
                    }
                    else if (row == 1) adaptive.MatchDistanceBonus = MatchDistancePerLevel * level;
                    else if (row == 2) adaptive.LymphocyteLifespanReduction = LifespanCutPerLevel * level;
                    else if (row == 3) adaptive.MemoryRecallLevel = level;
                    break;

                case UnitKind.CytotoxicT:
                    if (tuning == null || profile == null) return;
                    if (row == 0) tuning.StressSenseChancePerTick = profile.StressSenseChancePerTick + CtlGranzymePerLevel * level;
                    else if (row == 1) tuning.FineTilesPerTick = profile.FineTilesPerTick + CtlSpeedPerLevel * level;
                    else if (row == 2) tuning.ContactRadiusFineTiles = profile.ContactRadiusFineTiles + CtlSynapseReach * level;
                    break;

                case UnitKind.NaturalKiller:
                    if (tuning == null || profile == null) return;
                    if (row == 0) tuning.IfnGammaAuraChancePerTick = profile.IfnGammaAuraChancePerTick + NkAuraRatePerLevel * level;
                    else if (row == 1) tuning.IfnGammaAuraCoarseRadius = profile.IfnGammaAuraCoarseRadius + NkAuraRadiusPerLevel * level;
                    else if (row == 2) tuning.StressSenseChancePerTick = profile.StressSenseChancePerTick + NkNkg2dBonus * level;
                    break;

                case UnitKind.GammaDeltaT:
                    if (tuning == null || profile == null) return;
                    if (row == 0) tuning.BarrierPatrolRadiusCells = profile.BarrierPatrolRadiusCells + GdPatrolPerLevel * level;
                    else if (row == 1) tuning.BarrierRepairIntervalSeconds =
                        Mathf.Max(GdRepairFloor, profile.BarrierRepairIntervalSeconds - GdRepairCutPerLevel * level);
                    else if (row == 2) tuning.MaxActiveChildren = profile.MaxActiveChildren + GdNichePerLevel * level;
                    break;

                case UnitKind.PlasmaCell:
                    if (tuning == null || profile == null) return;
                    if (row == 0) tuning.AntibodySecretionIntervalSeconds = Mathf.Max(
                        PlasmaSecretionFloor,
                        profile.AntibodySecretionIntervalSeconds - PlasmaIgaCutPerLevel * level);
                    else if (row == 1) tuning.AntibodyLifespanBonusSeconds =
                        profile.AntibodyLifespanBonusSeconds + PlasmaSecretoryComponentPerLevel * level;
                    else if (row == 2) tuning.MaxActiveChildren = profile.MaxActiveChildren + PlasmaNichePerLevel * level;
                    break;
            }
        }
    }
}
