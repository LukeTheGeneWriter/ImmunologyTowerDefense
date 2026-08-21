namespace ImmunologyTD.Pathogens
{
    /// <summary>
    /// Running counts for the invasion loop -- what the HUD shows and what
    /// Assets/Editor/MapVerification.cs asserts against.
    ///
    /// A plain reference type (not a MonoBehaviour, not statics) passed by
    /// reference to whoever increments it, matching the TissueGrid /
    /// CytokineField pattern: a headless harness constructs one, hands it to
    /// the real production classes, and reads it afterwards, with no
    /// GameObjects and no play mode.
    ///
    /// SPRINT_PLAN.md item 9 scopes this deliberately: a pathogen reaching
    /// the base despawns and increments <see cref="ReachedBase"/>, and that
    /// is all. **The 100-life pool and the real lose condition are Sprint
    /// 5** -- do not add health subtraction here without reading
    /// GAME_DESIGN.md section 6c first.
    /// </summary>
    public class InvasionTally
    {
        /// <summary>Pathogens that reached the base band and despawned. The
        /// endzone counter -- Sprint 5 turns this into life loss.</summary>
        public int ReachedBase;

        /// <summary>Pathogens carried out of the downstream end of the lumen
        /// and excreted. Explicitly NOT a fail state
        /// (handoff-map01-intestine.md section 1's deliberate break from
        /// Bloons TD 6), tracked only so the Director can see how much of
        /// the flow never becomes his problem.</summary>
        public int Excreted;

        /// <summary>Pathogens that left the flow and colonised the gut
        /// interface.</summary>
        public int Adhesions;

        /// <summary>Boundary positions that have breached.</summary>
        public int Breaches;

        /// <summary>Pathogens released into tissue by breaches. Divided by
        /// <see cref="Breaches"/> this is the average burst size -- the
        /// number that tells you whether the mechanic is bursting or
        /// trickling.</summary>
        public int ReleasedIntoTissue;

        public void Reset()
        {
            ReachedBase = 0;
            Excreted = 0;
            Adhesions = 0;
            Breaches = 0;
            ReleasedIntoTissue = 0;
        }
    }
}
