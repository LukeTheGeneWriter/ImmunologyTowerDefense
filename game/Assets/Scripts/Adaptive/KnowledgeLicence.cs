using ImmunologyTD.Pathogens;
using ImmunologyTD.Units;

namespace ImmunologyTD.Adaptive
{
    /// <summary>
    /// Sprint 23. **The first thing in the game that reads knowledge and
    /// changes behaviour.** Until now `KnowledgeLedger` only drove the HUD
    /// readout -- every rung showed as "learned - planned" because no
    /// mechanic consumed it (GAME_DESIGN.md §5, "No rung's capability is
    /// built yet").
    ///
    /// Two separate questions, deliberately kept apart:
    ///
    ///  1. **Is the tower licensed at all?** (<see cref="IsTowerLicensed"/>)
    ///     Any one species clearing the rung switches the progenitor on.
    ///     The Director's ruling (2026-10-25): you buy a CTL progenitor
    ///     whenever you like, and the rung is the key that turns it on --
    ///     so the spend is a decision made in advance, not a reward handed
    ///     out automatically.
    ///
    ///  2. **May this unit act on THIS species?** (<see cref="RecognisesSpecies"/>)
    ///     MHC-I restriction is species-specific, so a CTL walking past a
    ///     species the system has never characterised should do nothing.
    ///     That is the biologically correct reading and it makes a mixed
    ///     round a real decision -- your CTLs ignore the novel pathogen.
    ///     **Flagged for the Director** (he was asked and has not ruled);
    ///     <see cref="PerSpeciesRecognition"/> flips it to the lenient
    ///     reading in one write if he prefers that.
    /// </summary>
    public static class KnowledgeLicence
    {
        /// <summary>
        /// The live ledger, set once by <c>GameBootstrap</c>. Process-global
        /// and null-safe, the same pattern as <c>GutWallFeatures.Current</c>
        /// and <c>EconomyHooks.PayForKill</c> -- threading a ledger through
        /// every <c>SearchUnit.Initialize</c> would have churned the
        /// signature in fifteen harnesses to deliver one reference that is
        /// the same for every unit on the board.
        ///
        /// **Null means "licence everything"**, so an existing harness that
        /// never sets it keeps testing exactly what it tested before.
        /// </summary>
        public static KnowledgeLedger Ledger;

        /// <summary>False = a licensed tower's units act on every species
        /// regardless of that species' own knowledge. True (default) = each
        /// unit only acts on species that have themselves cleared the rung.</summary>
        public static bool PerSpeciesRecognition = true;

        /// <summary>Which rung each gated kind waits on. A kind absent from
        /// this switch is ungated -- note that covers the NK/ILC1 and the
        /// gamma-delta IEL on purpose: both are innate.</summary>
        public static KnowledgeCapability? RungFor(UnitKind kind)
        {
            switch (kind)
            {
                case UnitKind.CytotoxicT: return KnowledgeCapability.CytotoxicTCells;
                case UnitKind.PlasmaCell: return KnowledgeCapability.NeutralizingAntibodies;
                default: return null;
            }
        }

        /// <summary>True if this progenitor may emit at all. An ungated kind
        /// is always licensed; a null ledger (harness) licenses everything,
        /// matching how a null wallet makes placement free.</summary>
        public static bool IsTowerLicensed(UnitKind kind, KnowledgeLedger ledger)
        {
            var rung = RungFor(kind);
            if (rung == null) return true;
            if (ledger == null) return true;

            // Sprint 25: iterate SPECIES. This read PathogenClassCount and
            // cast to PathogenClass, which meant a tower could only ever be
            // licensed by one of the three generics -- so learning
            // rotavirus to 100% would never have switched a CTL tower on.
            // A leftover from the half-finished class->species migration.
            for (int i = 0; i < SpeciesRoster.Count; i++)
            {
                if (KnowledgeLadder.IsUnlocked(rung.Value, ledger.Get((PathogenSpecies)i))) return true;
            }
            return false;
        }

        /// <summary>True if a unit of this kind may act on this species.</summary>
        public static bool RecognisesSpecies(UnitKind kind, PathogenSpecies species, KnowledgeLedger ledger)
        {
            var rung = RungFor(kind);
            if (rung == null) return true;            // innate: no licence needed
            if (!PerSpeciesRecognition) return true;  // lenient reading
            if (ledger == null) return true;          // harness
            return KnowledgeLadder.IsUnlocked(rung.Value, ledger.Get(species));
        }

        /// <summary>Convenience overloads reading <see cref="Ledger"/>.</summary>
        public static bool IsTowerLicensed(UnitKind kind) => IsTowerLicensed(kind, Ledger);

        public static bool RecognisesSpecies(UnitKind kind, PathogenSpecies species) =>
            RecognisesSpecies(kind, species, Ledger);

        public static void ResetToDefaults()
        {
            PerSpeciesRecognition = true;
            Ledger = null;
        }
    }
}
