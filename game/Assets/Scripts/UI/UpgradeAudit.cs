using ImmunologyTD.Units;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// Sprint 29. A public, read-only view of the progenitor upgrade
    /// catalog, for the verification harness.
    ///
    /// Same reasoning as <see cref="UiTextAudit"/>: the catalog is internal
    /// to the UI assembly and should stay that way, while the harness lives
    /// in the Editor assembly and only needs the answers. Widening the
    /// catalog itself cascades — its nested Row type would have to go
    /// public too, and then so would everything Row touches.
    ///
    /// What the harness checks through this: that no tower ships with an
    /// empty upgrade panel, and that every row actually writes a field.
    /// Both have been shipped wrong at least once.
    /// </summary>
    public static class UpgradeAudit
    {
        public readonly struct RowInfo
        {
            public readonly string Name;
            public readonly int Cap;
            public readonly int Price;

            public RowInfo(string name, int cap, int price)
            {
                Name = name; Cap = cap; Price = price;
            }
        }

        public static int RowCountFor(UnitKind kind) =>
            ProgenitorUpgradeCatalog.RowCountFor(kind);

        public static RowInfo[] RowsFor(UnitKind kind)
        {
            var rows = ProgenitorUpgradeCatalog.RowsFor(kind);
            var outRows = new RowInfo[rows.Length];
            for (int i = 0; i < rows.Length; i++)
                outRows[i] = new RowInfo(rows[i].Name, rows[i].Cap, rows[i].BasePrice);
            return outRows;
        }
    }
}
