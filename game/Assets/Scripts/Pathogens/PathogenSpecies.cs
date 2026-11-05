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
    /// 21.9% of the time.
    ///
    /// **Sprint 35 widened the barcode from 10 bits to 12**, ahead of the
    /// roster that needs it. Ten bits admits at most **12** species at r=2
    /// (constructed 12 against a Hamming sphere-packing ceiling of 18) and
    /// `ROSTER_EXPANSION_SPEC.md` §1 brings the roster to **16**, which
    /// does not fit. Eleven bits is the minimum; 12 was chosen for the
    /// headroom (20 constructed). The 16-tag set is already here --
    /// <see cref="SpeciesRoster.ReservedAntigens"/> holds the ten not yet
    /// built -- so a new species takes its tag from a set whose 120 pairs
    /// were verified together, rather than one that "looks far enough" from
    /// the others. That habit is how the 8-bit set happened.
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
    /// Sprint 35: **a pathogen can look like two things.** Four of the ten
    /// organisms in `ROSTER_EXPANSION_SPEC.md` change silhouette on the
    /// board -- *C. perfringens* arms itself, *Giardia* has a trophozoite
    /// and a cyst, *Cryptosporidium* an oocyst and a blister -- and until
    /// now a sprite was chosen once at spawn and never revisited.
    ///
    /// Deliberately generic, and deliberately only two: the meaning of
    /// <see cref="Alternate"/> is a per-species fact documented on the
    /// profile field that triggers it, not something this enum pretends to
    /// name for everyone. A/E *E. coli*'s pedestal is the first shipped
    /// user (<see cref="SpeciesProfile.AnchorsWhenAdhered"/>).
    /// </summary>
    public enum PathogenForm
    {
        Primary = 0,
        Alternate = 1,
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
        /// helper-T cell to teach at most one species. The live six and the
        /// ten reserved in <see cref="SpeciesRoster.ReservedAntigens"/> sit
        /// at a minimum distance of SIX across all 120 pairs.
        ///
        /// **Correction (Sprint 35).** This comment used to claim the set
        /// "leaves headroom for the avidity upgrade to raise r to 3". It
        /// does not, and never did: a set 6 apart needs 7 to be safe at
        /// r=3, and at 10 bits r=3 admitted exactly **two** species -- a
        /// Plotkin bound, not a search limit. At 12 bits it admits four.
        /// `AdaptiveTuning.MatchMaxHammingDistance`'s own comment has this
        /// right and says the cross-teaching is deliberately bought; the
        /// two comments contradicted each other for six sprints and this
        /// was the wrong one.
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

        // -- Sprint 35 ---------------------------------------------------

        /// <summary>
        /// The debut card's third line (`ROSTER_EXPANSION_SPEC.md` §4.2):
        /// **the one thing that will not work**, stated as a negative.
        ///
        /// A positive hint ("use IgA") deletes the puzzle. A negative
        /// warning tells the player their habit is about to fail without
        /// handing them the replacement, which is the Bloons convention for
        /// the lead bloon -- the game says *sharp will not pop this* and
        /// lets the player find the rest.
        ///
        /// Every warning here names a mechanic that is actually
        /// implemented. A warning about a trait the code does not have
        /// would be the taglines-promising-Salmonella mistake again.
        /// </summary>
        public string Warning;

        /// <summary>
        /// **An applicability flag, not a multiplier** (BACKLOG, from the
        /// Giardia antigen pass). §1d's PRR -> immunogenic-apoptosis
        /// upgrade raises the debris an infected cell yields when it dies
        /// well. *Vibrio cholerae* never enters a cell at all, so that row
        /// does nothing against it -- and an upgrade that silently does
        /// nothing is the exact dead-feature pattern this project keeps
        /// finding. The flag lets the UI say so instead.
        ///
        /// It also pre-answers two of the ten: *Giardia* never infects a
        /// cell either, and *Cryptosporidium*'s infected cells are
        /// survivin-protected against apoptosis.
        /// </summary>
        public bool NeverEntersCells;

        /// <summary>
        /// A/E *E. coli*. Once this species is adhered to the gut wall it
        /// draws in its <see cref="PathogenForm.Alternate"/> form -- the
        /// actin pedestal (spec §5.4's two-form organisms, and the first
        /// shipped user of that path).
        ///
        /// Biologically the pedestal is not carried around: it is
        /// assembled *under* an intimately attached bacterium, intimin
        /// binding Tir, effacing the microvilli beneath it. So the
        /// silhouette changes exactly when the fact about the organism
        /// changes -- which is the only honest reason for a sprite to
        /// change at all.
        /// </summary>
        public bool AnchorsWhenAdhered;

        /// <summary>
        /// *Vibrio cholerae*. **Correction from the Sprint 34 research**
        /// (`pathogens_bacteria_01.md`): the built organism steered toward
        /// the wall, and the measured truth is the opposite -- *V. cholerae*
        /// swims, and NON-chemotactic mutants out-compete wild type across
        /// the small intestine. "Run straight and get everywhere" beats
        /// "steer at the wall", and it is cheaper to compute than what it
        /// replaces.
        /// </summary>
        public bool SwimsStraight;
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
        /// <summary>The debut card's warning line, or the empty string
        /// for a species with none authored.</summary>
        public static string WarningOf(PathogenSpecies s) => Get(s).Warning ?? "";

        /// <summary>
        /// The ten 12-bit tags held for the organisms in
        /// `ROSTER_EXPANSION_SPEC.md` §1 that are **specified and not
        /// built**. They are here rather than in the doc because the
        /// property that matters -- minimum pairwise distance 6 across all
        /// 120 pairs of the full 16 -- is only true of the set as a whole,
        /// and a harness can check a C# array. A tag chosen later, against
        /// the six live ones only, could sit 6 from each of them and 3 from
        /// a sibling nobody compared it to.
        ///
        /// Order matches the spec's table. Nothing reads these in play;
        /// adding the species is what promotes one into
        /// <see cref="SpeciesProfile.Antigen"/>.
        /// </summary>
        public static readonly (string name, int antigen)[] ReservedAntigens =
        {
            ("Norovirus GII.4",          0b011100111010),
            ("Astrovirus",               0b100000101110),
            ("Sapovirus",                0b100101000101),
            ("Clostridium perfringens",  0b101001110011),
            ("Staph aureus toxin",       0b101110100000),
            ("STEC O157:H7",             0b101111011110),
            ("ETEC",                     0b110010010010),
            ("Giardia",                  0b110111101011),
            ("Cryptosporidium",          0b111001001000),
            ("Cyclospora",               0b111010111101),
        };

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
                Antigen = 0b000011011001,
                FlushResistance = 1f,
                Warning = "Your phagocytes cannot touch it once it is inside one of your own cells.",
            };

            table[(int)PathogenSpecies.GenericIntracellularBacterium] = new SpeciesProfile
            {
                Species = PathogenSpecies.GenericIntracellularBacterium,
                Class = PathogenClass.IntracellularBacterium,
                DisplayName = "Intracellular bacterium",
                ShortName = "Intra",
                Antigen = 0b000110110111,
                FlushResistance = 1f,
                Warning = "Flushing the lumen will not reach this one.",
            };

            table[(int)PathogenSpecies.GenericLargeBacterium] = new SpeciesProfile
            {
                Species = PathogenSpecies.GenericLargeBacterium,
                Class = PathogenClass.LargeBacterium,
                DisplayName = "Gut bacterium",
                ShortName = "Bact",
                Antigen = 0b001000010100,
                FlushResistance = 1f,
                Warning = "No tricks, and no mercy either. If this one gets through, it is because nothing was there.",
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
                Antigen = 0b010000100001,
                VillusTipTropismCells = 3,
                // Both halves of this are implemented traits:
                // NonLyticReleaseFraction suppresses burn-out, and
                // EnterotoxinNeighbourDamage harms cells it never infects.
                Warning = "Ignoring a focus will not make it burn out — and the damage will outrun the infection.",
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
                Antigen = 0b010110001100,
                ToxinFieldStrength = 0.9f,
                ToxinFieldRadiusCells = 2.5f,
                FlushResistance = 1f,
                Warning = "Hunting for dying cells will not find it. The wall it has taken looks perfectly healthy.",
                // It never takes a host-cell slot, so §1d's
                // immunogenic-apoptosis row has nothing to act on.
                NeverEntersCells = true,
                // Sprint 35 correction: non-chemotactic mutants out-compete
                // wild type across the small intestine.
                SwimsStraight = true,
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
                Antigen = 0b011011000111,
                InflammationFuelFactor = 1.6f,
                FlushResistance = 2.2f,
                Warning = "A messy clear feeds the survivors, and once it is anchored it will not flush.",
                AnchorsWhenAdhered = true,
            };
        }
    }
}
