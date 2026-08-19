using UnityEngine;

namespace ImmunologyTD.Units
{
    public enum UnitKind { Macrophage, Neutrophil }

    /// <summary>
    /// Per-unit-type tuning. Each type gets its own configurable speed in
    /// fine tiles per tick, per GAME_DESIGN.md section 7 ("per-cell step
    /// length... required by the 7x7 choice") -- deliberately not a single
    /// shared constant, since migration speed genuinely differs by cell
    /// type (neutrophils are among the fastest migrating leukocytes,
    /// macrophages markedly slower).
    /// </summary>
    [System.Serializable]
    public class UnitProfile
    {
        public UnitKind Kind;
        public string DisplayName;
        [Min(1)] public int FineTilesPerTick = 1;
        [Min(1)] public int FootprintFineTiles = 3;
        public Color Color = Color.white;
    }
}
