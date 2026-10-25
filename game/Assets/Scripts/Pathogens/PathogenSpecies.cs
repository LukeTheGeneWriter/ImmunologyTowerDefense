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
    /// Roster size warning (docs/CELL_KINETICS_REFERENCE.md): the 8-bit
    /// antigen barcode ceilings the useful roster at roughly 8-10 species
    /// before two species start teaching each other by accident. We are at
    /// six. Past about eight, the barcode needs to grow to 16 bits -- and
    /// note §5c's claim that the Hamming threshold is the dial to reach for
    /// first is backwards; length is the binding constraint.
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

        /// <summary>The 8-bit antigen barcode (§5c).</summary>
        public byte Antigen;

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
                Antigen = 0b01011100,
                FlushResistance = 1f,
            };

            table[(int)PathogenSpecies.GenericIntracellularBacterium] = new SpeciesProfile
            {
                Species = PathogenSpecies.GenericIntracellularBacterium,
                Class = PathogenClass.IntracellularBacterium,
                DisplayName = "Intracellular bacterium",
                ShortName = "Intra",
                Antigen = 0b10100011,
                FlushResistance = 1f,
            };

            table[(int)PathogenSpecies.GenericLargeBacterium] = new SpeciesProfile
            {
                Species = PathogenSpecies.GenericLargeBacterium,
                Class = PathogenClass.LargeBacterium,
                DisplayName = "Gut bacterium",
                ShortName = "Bact",
                Antigen = 0b11110000,
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
                // >=4 bits from every other tag, so one species' lessons do
                // not accidentally teach another (§5c's separation rule).
                //
                // This was 0b00110011 and the Sprint 24 harness rejected it:
                // only 2 bits from the generic intracellular bacterium, so
                // the two would have taught each other. Caught at species
                // SIX, which is exactly the roster ceiling the research
                // predicted -- hand-picking separable 8-bit tags gets hard
                // fast, and past ~8-10 species the barcode has to grow.
                Antigen = 0b00001111,
                VillusTipTropismCells = 3,
                // NSP4: a real viral enterotoxin. Harm spreads to cells the
                // virus never enters, which is why rotavirus damage outruns
                // rotavirus infection.
                EnterotoxinNeighbourDamage = 0.12f,
                FlushResistance = 1f,
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
                Antigen = 0b01100110,
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
                Antigen = 0b10011001,
                InflammationFuelFactor = 1.6f,
                FlushResistance = 2.2f,
            };
        }
    }
}
