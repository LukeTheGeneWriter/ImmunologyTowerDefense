using ImmunologyTD.Pathogens;

namespace ImmunologyTD.Adaptive
{
    /// <summary>
    /// The 8-bit antigen barcode of GAME_DESIGN.md §5c. Real TCR:MHC
    /// recognition is a 9mer with 20 options per position (~5e11
    /// combinations); the Director's simplification is an 8-bit binary
    /// barcode -- 256 values, small enough to match by chance in a game,
    /// large enough that a match reads as specificity rather than a
    /// formality.
    ///
    /// A tag is a plain <see cref="byte"/> (0..255). This class is the
    /// side-effect-free math on tags -- pulled out like
    /// <see cref="Units.Chemotaxis"/> so Assets/Editor/AdaptiveVerification.cs
    /// can assert it directly without any GameObjects.
    /// </summary>
    public static class Antigen
    {
        /// <summary>A fresh random 8-bit tag. Every helper-T cell is born
        /// with one of these (§5c step 1); every meeting re-rolls nothing --
        /// the tag is fixed for that cell's life.</summary>
        public static byte RandomTag() => (byte)UnityEngine.Random.Range(0, 256);

        /// <summary>Number of bit positions at which two tags differ --
        /// popcount(a XOR b). 0 = identical, 8 = complementary. Manual
        /// 8-iteration loop rather than System.Numerics.BitOperations, which
        /// isn't guaranteed on Unity's scripting runtime.</summary>
        public static int HammingDistance(byte a, byte b)
        {
            int x = a ^ b;
            int count = 0;
            while (x != 0)
            {
                count += x & 1;
                x >>= 1;
            }
            return count;
        }

        /// <summary>True iff the two tags are close enough to teach on a
        /// pairing -- Hamming distance at most
        /// <see cref="AdaptiveTuning.MatchMaxHammingDistance"/>. At the
        /// default threshold of 2 this is 37 of 256 possible partner tags
        /// (C(8,0)+C(8,1)+C(8,2)), ≈ 14.5% of random pairings.</summary>
        public static bool IsMatch(byte a, byte b) =>
            HammingDistance(a, b) <= AdaptiveTuning.MatchMaxHammingDistance;

        /// <summary>The fixed antigen a given pathogen class presents.
        /// Species key = <see cref="PathogenClass"/> until a real
        /// species roster exists (SPRINT_PLAN.md item 1).</summary>
        public static byte ForClass(PathogenClass c) => AdaptiveTuning.AntigenForClass(c);
    }
}
