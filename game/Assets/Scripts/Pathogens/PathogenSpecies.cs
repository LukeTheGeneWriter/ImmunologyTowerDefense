namespace ImmunologyTD.Pathogens
{
    /// <summary>
    /// Sprint 24. **A named pathogen species** -- the thing the adaptive
    /// system learns about.
    ///
    /// Until now <see cref="PathogenClass"/> did two unrelated jobs at once:
    /// it was the behavioural archetype (how does this thing move, hide and
    /// replicate?) *and* the key the knowledge ledger was indexed by.
    /// GAME_DESIGN.md §5 has always said knowledge is tracked "per pathogen
    /// species, not globally", with the explicit consequence that "a new
    /// species arriving in a late round meets a naive adaptive system" --
    /// and with only three classes, that could never happen. Learning about
    /// "intracellular virus" taught you about every virus forever.
    ///
    /// So species and class are now separate. A species names an organism
    /// and points at the class whose machinery it reuses; two species can
    /// share a class and still be learned about independently.
    ///
    /// **Ordinals 0-2 deliberately match <see cref="PathogenClass"/>'s**, so
    /// the pre-existing generic content and every harness that spoke in
    /// classes keeps meaning exactly what it meant before.
    ///
    /// **Roster size.** Species tags must sit at least 2r+1 apart (5 at
    /// the shipped match radius of 2) or one helper-T cell teaches two
    /// species at once. Sprint 29 measured the real ceiling by exhaustive
    /// search rather than estimating it: **8 bits admits at most FOUR such
    /// tags**, which is why the six-species 8-bit set was cross-teaching
    /// 21.9% of the time. At 10 bits these six sit 6 apart with headroom.
    /// Before adding a species, re-run the search for the WHOLE set -- see
    /// SpeciesProfile.Antigen.
    /// </summary>
    public enum PathogenSpecies
    {
        // -- The three generics. Same ordinals as PathogenClass. --
        GenericVirus = 0,
        GenericIntracellularBacterium = 1,
        GenericLargeBacterium = 2,

        // -- Sprint 24's named roster (Director, 2026-10-25). --

        /// <summary>Rotavirus A. Infects MATURE enterocytes at the villus
        /// tip and never the crypt stem cell, so the tissue repaves itself
        /// from the crypt end. Carries NSP4, a genuine viral enterotoxin,
        /// which damages neighbours it never infects.</summary>
        Rotavirus = 3,

        /// <summary>Vibrio cholerae. Never enters a cell and never takes a
        /// slot: it adheres and secretes. Cholera toxin subverts living
        /// enterocytes rather than killing them, loosens tight junctions,
        /// and the damage it causes releases lactate the organism feeds
        /// on.</summary>
        VibrioCholerae = 4,

        /// <summary>Attaching-effacing E. coli (EPEC-like). Intimate,
        /// irreversible adhesion, and it RESPIRES THE PRODUCTS OF
        /// INFLAMMATION -- host oxidants generate nitrate and tetrathionate
        /// that only a facultative anaerobe can use, so a neutrophil-heavy
        /// clear feeds the survivors.</summary>
        AttachingEffacingEcoli = 5,
    }

    /// <summary>
    /// Sprint 34. **How a virus leaves the cell it infected.** GAME_DESIGN.md
    /// §4b has always said spread "has two per-species modes" -- contact-chain
    /// or budding -- but <c>PathogenAgent.AssignVirusTraits</c> picked between
    /// them with a per-spawn coin flip, and said so in a comment: "there is no
    /// species roster yet". There has been one since Sprint 24. This is the
    /// seventh instance of the standing pattern (a value written on one side
    /// of a boundary and never read as that value on the other), so the fix
    /// is to let the roster own the choice.
    ///
    /// <c>Unspecified</c> keeps the coin flip, so the three generics behave
    /// exactly as they did before this change.
    /// </summary>
    public enum ViralExit
    {
        /// <summary>Roll it, as every virus did before Sprint 34.</summary>
        Unspecified = 0,
        /// <summary>Infects ONE adjacent healthy cell. Snaking path.</summary>
        ContactChain = 1,
        /// <summary>Periodically emits a free virion. Growing disk.</summary>
        Budding = 2,
    }

    /// <summary>
    /// The per-species table: what each species IS, and the handful of
    /// traits that make it behave unlike its classmates.
    ///
    /// Traits are deliberately plain numbers with a neutral default, so a
    /// species that does not opt in behaves exactly like the generic of its
    /// class and no call site needs a species check. Same discipline as
    /// <c>UnitProfile.EfferocytosisDebrisPerTick</c>, where "only macrophages
    /// clear debris" falls out of a zero rather than an if.
    /// </summary>
    public struct SpeciesProfile
    {
        public PathogenSpecies Species;
        public PathogenClass Class;
        public string DisplayName;
        public string ShortName;

        /// <summary>
        /// The antigen barcode (§5c), <see cref="AdaptiveTuning.BarcodeBits"/>
        /// bits wide. Sprint 29 widened it from 8 to 10.
        ///
        /// **The whole set is chosen together, not one tag at a time.**
        /// Tags must be at least 2r+1 apart (5 at the shipped r=2) for a
        /// helper-T cell to teach at most one species. These six sit at a
        /// minimum distance of SIX, found by exhaustive search, which also
        /// leaves headroom for the avidity upgrade to raise r to 3.
        ///
        /// Adding a species means re-running that search for the whole set,
        /// not picking a tag that "looks far enough" from the others --
        /// which is exactly how the 8-bit set ended up with 21.9% of
        /// helper-T tags teaching two species by accident.
        /// </summary>
        public int Antigen;

        // -- Traits. Zero/false = "behave like the generic". --

        /// <summary>Rotavirus. If > 0, this species can only infect host
        /// cells within this many coarse cells of the lumen -- the mature
        /// villus-tip enterocytes. It cannot touch the crypt end, which is
        /// why the tissue regrows underneath it instead of being wiped.
        /// 0 = no positional restriction.</summary>
        public int VillusTipTropismCells;

        /// <summary>Rotavirus NSP4. Damage per tick dealt to HEALTHY
        /// neighbours of an infected cell -- a viral enterotoxin, so harm
        /// spreads without infection spreading. 0 = none.</summary>
        public float EnterotoxinNeighbourDamage;

        /// <summary>Cholera toxin. If > 0, adhered organisms of this species
        /// emit a toxin field of this strength instead of grazing the wall.
        /// The cells under it are SUBVERTED, not killed. 0 = none.</summary>
        public float ToxinFieldStrength;

        /// <summary>How far the toxin field reaches along the wall, in
        /// coarse cells.</summary>
        public float ToxinFieldRadiusCells;

        /// <summary>Attaching-effacing E. coli. Multiplier on this species'
        /// replication rate per unit of local inflammation -- the organism
        /// respiring the products of the host's own response. 0 = this
        /// species gains nothing from inflammation, which is every other
        /// species.</summary>
        public float InflammationFuelFactor;

        /// <summary>Intimate adhesion (the intimin/Tir pedestal). Multiplier
        /// on how strongly this species resists being flushed -- both IgA
        /// enchainment sweeping it away and goblet-cell sloughing. 1 =
        /// ordinary adhesion.</summary>
        public float FlushResistance;

        // -- Sprint 34 traits (docs/research/pathogens_viruses_02.md). --

        /// <summary>Which of §4b's two spread modes this species uses.
        /// <see cref="ViralExit.Unspecified"/> = roll it, the pre-Sprint-34
        /// behaviour.</summary>
        public ViralExit Exit;

        /// <summary>
        /// The fraction of this species' exits that leave the host cell
        /// ALIVE, which suppresses §4b's spontaneous burn-out by the same
        /// fraction. 0 = ordinary lytic virus.
        ///
        /// Rotavirus releases essentially all of its progeny non-lytically
        /// with no measured permeability change at 72 h, so an ignored
        /// rotavirus focus does not burst and resolve on its own. §4b wants
        /// viral infections self-limiting if wholly ignored, and rotavirus
        /// still is -- but by the CRYPT REPAVING UNDERNEATH IT (the villus-
        /// tip tropism it already has), not by burning out. The two traits
        /// compose into one organism: a plague that keeps shedding until the
        /// player kills the cell, under tissue that keeps rebuilding.
        /// </summary>
        public float NonLyticReleaseFraction;

        /// <summary>
        /// How much one organism of this species contributes to the §1b
        /// plaque crowd field. 0 = neutral, treated as 1 -- use
        /// <see cref="SpeciesRoster.CrowdWeightOf"/>, never the raw field.
        ///
        /// **Rotavirus arrives in vesicles.** 10-45% of stool virus is
        /// membrane-bound, each vesicle carries >10 virions, and stripping
        /// the vesicles cuts infectivity 72-83%: one arriving unit is
        /// already a dose. The measured multiplier is ~3.5x.
        ///
        /// **It is capped below <c>InvasionTuning.IngressCrowdFloor</c>
        /// (2.0) on purpose.** §1b Step 2 property 2 is LOCKED and declared
        /// load-bearing -- "ingress is something a CROWD does, never
        /// something a pathogen does... a trickle is arithmetically
        /// impossible, not merely unlikely" -- and a weight of 3.5 would let
        /// a lone virion charge a plaque, deleting that property. So the
        /// honest abstraction, in the roster's own "tropism literal, dose
        /// abstracted" style: rotavirus's minimum infectious dose is TWO
        /// organisms where a generic virus needs three, which is exactly the
        /// per-species knob §1b Step 2 promised the floor would become.
        /// </summary>
        public float PlaqueCrowdWeight;
    }

    /// <summary>Static roster lookup. Mutable-statics-with-a-reset shape, so
    /// a harness can override one species for a scenario and reset.</summary>
    public static class SpeciesRoster
    {
        public static readonly int Count =
            System.Enum.GetValues(typeof(PathogenSpecies)).Length;

        private static SpeciesProfile[] table;

        public static SpeciesProfile Get(PathogenSpecies s)
        {
            if (table == null) ResetToDefaults();
            return table[(int)s];
        }

        public static PathogenClass ClassOf(PathogenSpecies s) => Get(s).Class;

        public static string NameOf(PathogenSpecies s) => Get(s).DisplayName;

        public static string ShortNameOf(PathogenSpecies s) => Get(s).ShortName;

        /// <summary>Sprint 34. This species' §1b crowd contribution, with
        /// zero normalised to 1 so an un-opted-in species counts as one
        /// organism. Call sites must use this, not the raw field.</summary>
        public static float CrowdWeightOf(PathogenSpecies s)
        {
            float w = Get(s).PlaqueCrowdWeight;
            return w <= 0f ? 1f : w;
        }

        /// <summary>The generic species standing in for a bare class -- the
        /// bridge for code and harnesses that still speak in classes.</summary>
        public static PathogenSpecies GenericFor(PathogenClass c)
        {
            switch (c)
            {
                case PathogenClass.IntracellularVirus: return PathogenSpecies.GenericVirus;
                case PathogenClass.IntracellularBacterium: return PathogenSpecies.GenericIntracellularBacterium;
                default: return PathogenSpecies.GenericLargeBacterium;
            }
        }

        public static void ResetToDefaults()
        {
            table = new SpeciesProfile[Count];

            table[(int)PathogenSpecies.GenericVirus] = new SpeciesProfile
            {
                Species = PathogenSpecies.GenericVirus,
                Class = PathogenClass.IntracellularVirus,
                DisplayName = "Virus",
                ShortName = "Virus",
                Antigen = 0b0000000000,
                FlushResistance = 1f,
            };

            table[(int)PathogenSpecies.GenericIntracellularBacterium] = new SpeciesProfile
            {
                Species = PathogenSpecies.GenericIntracellularBacterium,
                Class = PathogenClass.IntracellularBacterium,
                DisplayName = "Intracellular bacterium",
                ShortName = "Intra",
                Antigen = 0b0000111111,
                FlushResistance = 1f,
            };

            table[(int)PathogenSpecies.GenericLargeBacterium] = new SpeciesProfile
            {
                Species = PathogenSpecies.GenericLargeBacterium,
                Class = PathogenClass.LargeBacterium,
                DisplayName = "Gut bacterium",
                ShortName = "Bact",
                Antigen = 0b0111000111,
                FlushResistance = 1f,
            };

            // -- Rotavirus --------------------------------------------------
            // Tropism is the mechanic. Rotavirus infects mature enterocytes
            // at the villus tip and does NOT infect the Lgr5+ crypt stem
            // cell, so the crypt keeps producing replacements underneath an
            // infection it is immune to. On this board that reads as damage
            // that cannot reach the regrowth engine.
            table[(int)PathogenSpecies.Rotavirus] = new SpeciesProfile
            {
                Species = PathogenSpecies.Rotavirus,
                Class = PathogenClass.IntracellularVirus,
                DisplayName = "Rotavirus",
                ShortName = "Rota",
                // Part of the Sprint 29 10-bit set (min distance 6).
                Antigen = 0b1011011001,
                VillusTipTropismCells = 3,
                // NSP4: a real viral enterotoxin. Harm spreads to cells the
                // virus never enters, which is why rotavirus damage outruns
                // rotavirus infection.
                EnterotoxinNeighbourDamage = 0.12f,
                FlushResistance = 1f,
                // Sprint 34, from pathogens_viruses_02.md: rotavirus leaves
                // the cell NON-LYTICALLY and arrives IN VESICLES. Both were
                // corrections to what the game already shipped -- it had
                // been played as a cell-killer with an ordinary dose.
                Exit = ViralExit.Budding,
                NonLyticReleaseFraction = 1f,
                PlaqueCrowdWeight = 1.9f,
            };

            // -- Vibrio cholerae --------------------------------------------
            // The enterocyte lives and stops being yours. No host-cell slot
            // is ever taken, nothing is killed outright -- the wall is
            // subverted at a distance, and the damage feeds the organism.
            table[(int)PathogenSpecies.VibrioCholerae] = new SpeciesProfile
            {
                Species = PathogenSpecies.VibrioCholerae,
                Class = PathogenClass.LargeBacterium,
                DisplayName = "Vibrio cholerae",
                ShortName = "Vibrio",
                Antigen = 0b1101101010,
                ToxinFieldStrength = 0.9f,
                ToxinFieldRadiusCells = 2.5f,
                FlushResistance = 1f,
            };

            // -- Attaching-effacing E. coli ---------------------------------
            // The one organism on the board that PROFITS from the player's
            // own response. Also pedestal-adherent, so the two flush answers
            // the player already owns work poorly against it.
            table[(int)PathogenSpecies.AttachingEffacingEcoli] = new SpeciesProfile
            {
                Species = PathogenSpecies.AttachingEffacingEcoli,
                Class = PathogenClass.LargeBacterium,
                DisplayName = "A/E E. coli",
                ShortName = "AEEC",
                Antigen = 0b1110110100,
                InflammationFuelFactor = 1.6f,
                FlushResistance = 2.2f,
            };
        }
    }
}
