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

        /// <summary>**Sprint 33: how many organisms this round delivers**,
        /// authored per round. 0 means "use the economy's linear baseline x
        /// <see cref="WallPressure"/>", which is what the procedural tail
        /// past the script does.
        ///
        /// A hand-authored 40-round script has to own its own volume curve,
        /// the way Bloons' round tables do. The alternative was discovered
        /// by arithmetic: the economy baseline (16, +6 a round) times a
        /// rising pressure multiplier hits the batch ceiling of 90 at round
        /// 8, so **every round from 8 to 40 would have delivered exactly 90
        /// organisms** and the whole back half of the curve would have been
        /// flat however the multiplier was tuned.
        ///
        /// `GAME_DESIGN.md` §5d always said the linear growth was a
        /// placeholder and that a real difficulty curve was out of
        /// framework scope; this is where that curve now lives.</summary>
        public int Batch;

        /// <summary>**Sprint 33: how the batch arrives.** The contaminated
        /// food item drops its cargo in this many bursts as it transits the
        /// lumen; 0 means <see cref="InvasionTuning.FoodItemBurstCount"/>,
        /// the global default of 4.
        ///
        /// This is the round-design lever that is NOT volume, and it is the
        /// one that reads most strongly at the wall. Bursts land at the
        /// bolus's current lane, so **1 burst of 80 is a rush against one
        /// stretch of wall, while 7 bursts of 11 is a trickle smeared along
        /// the whole thing** — the same count, a different problem. It is
        /// this game's answer to Bloons' grouped rushes, and it is the
        /// honest form of "wall pressure": organisms per clump, not
        /// organisms per round.</summary>
        public int Bursts;

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

        /// <summary>Bursts this round uses, resolving 0 to the global
        /// default.</summary>
        public int EffectiveBursts =>
            Bursts > 0 ? Bursts : Mathf.Max(1, ImmunologyTD.Pathogens.InvasionTuning.FoodItemBurstCount);

        /// <summary>Organisms per clump — <see cref="Batch"/> divided by
        /// <see cref="EffectiveBursts"/>. The quantity that actually
        /// describes pressure at the wall, and the one the 40-round script's
        /// difficulty envelope is built from. 0 for a round with no authored
        /// batch.</summary>
        public float OrganismsPerClump =>
            Batch > 0 ? Batch / (float)Mathf.Max(1, EffectiveBursts) : 0f;

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
    /// The **40-round script** (Sprint 33, Director: "40 rounds with a bunch
    /// of different mixes and matches, reference Bloons TD").
    ///
    /// ## What Bloons actually does, and what carries over
    ///
    /// Forty rounds is Bloons TD 6's Easy length, and its round table has
    /// four habits worth stealing:
    ///
    /// 1. **A new enemy debuts, then recurs in quantity.** Nothing appears
    ///    once. A type arrives in a small dose against a board the player
    ///    already handles, and comes back in bulk two or three rounds later.
    /// 2. **Difficulty is a sawtooth, not a ramp.** A hard round is followed
    ///    by an easier one. The envelope rises; individual rounds dip. This
    ///    is what makes a spike legible as a spike.
    /// 3. **Grouped rushes.** The same count delivered tightly is a
    ///    different problem from the same count spread out — and it is a
    ///    *delivery* property, not a new enemy. This is the single biggest
    ///    source of variety in Bloons' back half.
    /// 4. **The player can see what is coming.** The round number and the
    ///    callout for a new type are both up before the round starts.
    ///
    /// All four are here. (3) is <see cref="RoundDefinition.Bursts"/>:
    /// bursts land at the bolus's current lane, so one burst of 88 hits one
    /// stretch of wall while seven bursts of 12 smear along the whole
    /// thing. (4) is the buy-phase caption, which already previewed the next
    /// round's tagline and now also names any organism debuting in it
    /// (<see cref="DebutsInRound"/>).
    ///
    /// ## What does NOT carry over, and the honest limit of this script
    ///
    /// Bloons has ~20 bloon types by round 40. **This game has six
    /// organisms**, three of them generic. So rounds 20-40 differ by
    /// proportion, volume and delivery shape — not by new content. The
    /// script is built to make those three levers carry as far as they can,
    /// and the back half is where the ranked shortlist in
    /// `PATHOGEN_ROSTER.md` (astrovirus, CMV as a boss, norovirus,
    /// *Shigella*, *Salmonella*) would actually change the game rather than
    /// re-weight it. Read rounds 20-40 as a frame waiting for content, not
    /// as a finished difficulty curve.
    ///
    /// ## The three rules the harness enforces
    ///
    /// 1. **One new named organism at a time** — debuts at rounds 6
    ///    (*Vibrio cholerae*), 11 (rotavirus) and 15 (A/E *E. coli*), with
    ///    the three generic classes established in rounds 1-3.
    /// 2. **Everything recurs.** No organism appears in only one round,
    ///    because knowledge is per species (§5c) and the adaptive rungs fire
    ///    per species — a parade of novelties keeps the whole adaptive arm
    ///    dark, which is close to what the Sprint 22 playtest reported.
    /// 3. **The envelope rises and individual rounds dip.** Measured on
    ///    organisms-per-clump (batch ÷ bursts): 4 at round 1, 17 by round 5,
    ///    26 by 17, 33 by 28, and 88 at round 40 — the finale is the whole
    ///    batch in a single burst.
    ///
    /// Taglines name only organisms that are actually in the roster. Before
    /// Sprint 32 they promised *Salmonella*, poliovirus and *Listeria*, none
    /// of which exist here, which is the kind of drift that hid a much worse
    /// bug for eight sprints.
    /// </summary>
    public static class RoundScript
    {
        // Short aliases, so a 40-row table stays readable.
        private const PathogenSpecies Bact = PathogenSpecies.GenericLargeBacterium;
        private const PathogenSpecies Vir = PathogenSpecies.GenericVirus;
        private const PathogenSpecies Intra = PathogenSpecies.GenericIntracellularBacterium;
        private const PathogenSpecies Vibrio = PathogenSpecies.VibrioCholerae;
        private const PathogenSpecies Rota = PathogenSpecies.Rotavirus;
        private const PathogenSpecies Aeec = PathogenSpecies.AttachingEffacingEcoli;

        private static SpeciesWeight W(PathogenSpecies s, float weight) => new SpeciesWeight(s, weight);

        private static RoundDefinition R(string tagline, int batch, int bursts, params SpeciesWeight[] cast)
            => new RoundDefinition { Tagline = tagline, Batch = batch, Bursts = bursts, Cast = cast };

        private static readonly RoundDefinition[] Scripted =
        {
            // -- Act 1 (1-5): the three classes, and the first rush. ------
            // One enemy, a slow trickle: adhesion, the mill at the wall, and
            // macrophages eating. Nothing to read yet.
            R("Bagged salad, gut bacteria", 16, 4, W(Bact, 3)),
            // A virus behaves nothing like a bacterium -- it takes a cell
            // rather than a slot -- so it is the next thing to meet.
            R("Undercooked egg, and something viral", 22, 4, W(Bact, 3), W(Vir, 1)),
            // The third class: an invader that gets inside a cell and stays.
            R("Reheated rice, an invader among them", 26, 5, W(Bact, 3), W(Vir, 1), W(Intra, 1)),
            // Same cast, spread as thin as it goes. The quiet round before
            // the first spike, and a chance to see the whole wall at once.
            R("Unwashed berries, a long thin trickle", 30, 6, W(Bact, 3), W(Vir, 1), W(Intra, 1)),
            // **First rush.** Same organisms, two clumps instead of six.
            R("Street-cart sauce, all at once", 34, 2, W(Bact, 4), W(Vir, 1)),

            // -- Act 2 (6-14): the named organisms arrive. ---------------
            // **Vibrio cholerae debuts**, and it is the gentlest of the three
            // to meet first: it never kills a cell and never takes a slot.
            // Its lesson is that hunting for dying cells is not enough,
            // because subverted wall looks perfectly healthy.
            R("Raw oysters, Vibrio cholerae", 32, 4, W(Vibrio, 3), W(Bact, 2), W(Vir, 1)),
            R("Brackish ice, more of the same", 38, 5, W(Vibrio, 3), W(Bact, 2), W(Intra, 1)),
            // A deliberately viral round: the tissue answer, not the wall one.
            R("Contaminated water, mostly viral", 34, 6, W(Vir, 4), W(Bact, 1), W(Vibrio, 1)),
            R("Buffet tongs, a bit of everything", 42, 4, W(Bact, 2), W(Vir, 2), W(Intra, 1), W(Vibrio, 2)),
            R("Oyster platter, one heavy dose", 46, 2, W(Vibrio, 4), W(Bact, 2)),
            // **Rotavirus debuts.** It eats the villus tip and can never
            // touch the crypt, so damage creeps one way while the board
            // repaves from the other: WHERE a thing can reach starts to
            // matter.
            R("Nursery water bottle, rotavirus", 40, 4, W(Rota, 3), W(Vir, 1), W(Bact, 1)),
            R("Shared teething ring, rotavirus again", 44, 5, W(Rota, 3), W(Vir, 2), W(Vibrio, 1)),
            R("Soft cheese, invaders inside the cells", 46, 4, W(Intra, 3), W(Bact, 2), W(Rota, 1)),
            R("Picnic leftovers, nothing exotic", 40, 6, W(Bact, 3), W(Vir, 2), W(Intra, 2)),

            // -- Act 3 (15-27): the last organism, and composition. ------
            // **A/E E. coli debuts, deliberately last.** It respires the
            // products of the host's own response, so a messy
            // neutrophil-heavy clear feeds the survivors -- the one strategy
            // in the research with no answer anywhere in the roster. It
            // should land on a player who has settled into a routine.
            R("Street-cart mince, attaching-effacing E. coli", 44, 4, W(Aeec, 3), W(Bact, 1), W(Intra, 1)),
            R("Dripping burger press, E. coli and friends", 50, 5, W(Aeec, 3), W(Bact, 2), W(Vir, 1)),
            // A clump of the one organism that resists being flushed: IgA
            // enchainment and goblet sloughing both work poorly on it.
            R("Unrinsed sprouts, in one clump", 52, 2, W(Aeec, 4), W(Bact, 2)),
            // The two organisms that never present as dying cells, together.
            R("Oyster and ice, nothing looks sick", 48, 5, W(Vibrio, 3), W(Rota, 2), W(Bact, 1)),
            R("Daycare door handles, a viral week", 52, 4, W(Rota, 3), W(Vir, 3), W(Bact, 1)),
            // Milestone: the full roster, no novelty, tightened up.
            R("Catered wedding, the full menu", 58, 3,
              W(Bact, 2), W(Vir, 2), W(Intra, 1), W(Vibrio, 2), W(Rota, 2), W(Aeec, 1)),
            R("Home cooking, a quiet week", 48, 6, W(Bact, 3), W(Vir, 2), W(Intra, 1)),
            R("Minced beef, inflamed ground", 56, 4, W(Aeec, 3), W(Bact, 3), W(Intra, 1)),
            R("Food truck at rush hour", 60, 2, W(Aeec, 2), W(Bact, 2), W(Vibrio, 2)),
            R("Villus tips, chewed from the top", 56, 5, W(Rota, 4), W(Vir, 2), W(Bact, 1)),
            R("The wall is alive, and not yours", 58, 4, W(Vibrio, 4), W(Bact, 2), W(Intra, 1)),
            // Almost nothing extracellular: everything worth killing is
            // inside a cell, which is the stress-sensing arm's round.
            R("Nothing in the lumen, everything inside", 60, 5, W(Intra, 3), W(Vir, 3), W(Rota, 2)),
            R("Thin soup, spread thinner", 52, 7, W(Bact, 3), W(Vir, 2), W(Vibrio, 2), W(Intra, 1)),

            // -- Act 4 (28-40): volume, concentration, and the finale. ---
            R("Two deliveries, one wall", 66, 2, W(Aeec, 3), W(Bact, 2), W(Rota, 2)),
            R("Offal and oysters", 64, 4, W(Aeec, 3), W(Intra, 3), W(Vibrio, 2)),
            R("Festival weekend, everything at once", 70, 3,
              W(Bact, 2), W(Vir, 2), W(Intra, 2), W(Vibrio, 2), W(Rota, 2), W(Aeec, 2)),
            R("Convalescence, a lighter plate", 60, 6, W(Bact, 3), W(Vir, 2), W(Rota, 1)),
            // Toxin and fuel: rotavirus damages neighbours it never infects,
            // and A/E E. coli respires the mess that leaves behind.
            R("Toxin, and something that feeds on it", 68, 4, W(Rota, 3), W(Aeec, 3), W(Bact, 1)),
            R("Neither of them kills a cell", 70, 5, W(Vibrio, 4), W(Aeec, 3), W(Bact, 1)),
            R("One bolus, one stretch of wall", 74, 2, W(Aeec, 3), W(Bact, 3), W(Vir, 2)),
            R("A viral storm", 72, 4, W(Rota, 4), W(Vir, 4), W(Intra, 1)),
            R("Plain fare, and plenty of it", 66, 6, W(Bact, 4), W(Vir, 2), W(Intra, 2)),
            R("A siege, all bacterial", 78, 3, W(Bact, 3), W(Intra, 3), W(Aeec, 3)),
            R("Everything, delivered twice", 82, 2,
              W(Bact, 2), W(Vir, 2), W(Intra, 2), W(Vibrio, 2), W(Rota, 2), W(Aeec, 2)),
            R("The last quiet plate", 84, 5,
              W(Bact, 2), W(Vir, 2), W(Intra, 2), W(Vibrio, 2), W(Rota, 2), W(Aeec, 2)),
            // **Round 40.** The whole batch in a single burst, against one
            // stretch of wall, with every organism in it. There is no boss
            // unit to put here -- the designed one (CMV, four independent
            // MHC-I mechanisms, a ~96h lytic cycle) is still unbuilt in
            // `PATHOGEN_ROSTER.md` -- so the finale is made of
            // concentration rather than of a new enemy. If a boss is ever
            // built, this is its slot.
            R("All of it, in one mouthful", 88, 1,
              W(Bact, 2), W(Vir, 2), W(Intra, 2), W(Vibrio, 3), W(Rota, 3), W(Aeec, 3)),
        };

        /// <summary>Highest wall pressure the procedural tail will reach.
        /// Past here a round gets longer and more varied, not denser --
        /// density is bounded by what the wall mechanics can express before
        /// the lumen simply saturates.</summary>
        public const float MaxProceduralWallPressure = 2.5f;

        public static int ScriptedRounds => Scripted.Length;

        /// <summary>
        /// Organisms making their FIRST scripted appearance in this round,
        /// in roster order. Bloons calls out a new bloon type before the
        /// round it debuts in; this is the data behind that callout, and
        /// `HudView` shows it in the buy-phase caption, which already
        /// previewed the next round's tagline.
        ///
        /// Returns an empty array for a round that introduces nothing, and
        /// for anything past the script.
        /// </summary>
        public static PathogenSpecies[] DebutsInRound(int roundNumber)
        {
            if (roundNumber < 1 || roundNumber > Scripted.Length)
                return System.Array.Empty<PathogenSpecies>();

            var seenBefore = new System.Collections.Generic.HashSet<PathogenSpecies>();
            for (int r = 1; r < roundNumber; r++)
            {
                var earlier = Scripted[r - 1].Cast;
                if (earlier == null) continue;
                for (int i = 0; i < earlier.Length; i++)
                    if (earlier[i].Weight > 0f) seenBefore.Add(earlier[i].Species);
            }

            var cast = Scripted[roundNumber - 1].Cast;
            if (cast == null) return System.Array.Empty<PathogenSpecies>();

            var debuts = new System.Collections.Generic.List<PathogenSpecies>();
            for (int i = 0; i < cast.Length; i++)
            {
                if (cast[i].Weight <= 0f) continue;
                if (seenBefore.Contains(cast[i].Species)) continue;
                if (debuts.Contains(cast[i].Species)) continue;
                debuts.Add(cast[i].Species);
            }
            debuts.Sort((a, b) => ((int)a).CompareTo((int)b));
            return debuts.ToArray();
        }

        /// <summary>The player-facing callout for a round's debuts, or null
        /// when it introduces nothing. "NEW: Vibrio cholerae".</summary>
        public static string DebutLabelFor(int roundNumber)
        {
            var debuts = DebutsInRound(roundNumber);
            if (debuts.Length == 0) return null;

            var names = new string[debuts.Length];
            for (int i = 0; i < debuts.Length; i++) names[i] = SpeciesRoster.NameOf(debuts[i]);
            return "NEW: " + string.Join(", ", names);
        }

        public static RoundDefinition ForRound(int roundNumber)
        {
            int n = roundNumber < 1 ? 1 : roundNumber;
            if (n <= Scripted.Length) return Scripted[n - 1];

            // Procedural tail past round 40: the full roster, at the
            // baseline-times-pressure volume the script does not author, in
            // a middling delivery shape. Deliberately NOT round 40's single
            // burst -- the finale is a spike, and a spike repeated forever
            // is just a plateau.
            //
            // Note the honest limitation: the economy baseline is far above
            // the batch ceiling by round 41, so every tail round delivers
            // the ceiling (90) whatever the pressure says. Past 40 the game
            // does not get harder, it just continues. See BACKLOG.md.
            int past = n - Scripted.Length;
            return new RoundDefinition
            {
                // The ROUND number, not the count past the script:
                // RoundVerification asserts the procedural tagline names the
                // round, and the HUD round bar shows this string.
                Tagline = $"Spoiled leftovers, day {n}",
                WallPressure = Mathf.Min(
                    MaxProceduralWallPressure,
                    1.7f + 0.1f * past),
                Bursts = 3,
                Cast = Scripted[Scripted.Length - 1].Cast,
            };
        }
    }
}
