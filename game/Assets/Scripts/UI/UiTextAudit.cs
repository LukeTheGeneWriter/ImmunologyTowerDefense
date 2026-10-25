using System.Collections.Generic;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Units;
using ImmunologyTD.Adaptive;

namespace ImmunologyTD.UI
{
    /// <summary>
    /// Sprint 25. Walks every player-facing string and reports any that the
    /// shipped UI font cannot actually draw.
    ///
    /// **This exists because of a bug that shipped.** The gamma-delta T
    /// cell's shop card was named "γδ T cell (IEL)" and the NK card's blurb
    /// read "IFN-γ" — and Nunito contains no Greek block at all. UI Toolkit
    /// does not warn, does not error, and does not draw a missing-glyph
    /// box: the characters are simply absent. The cards rendered, slightly
    /// wrong, through a build, a sprite dump and a playtest, and were
    /// finally caught by the Director's eye.
    ///
    /// A public audit surface rather than making <see cref="UiTheme"/>
    /// public: the verification harness lives in the Editor assembly and
    /// only needs the answer, not the whole theme.
    /// </summary>
    public static class UiTextAudit
    {
        public readonly struct Offender
        {
            public readonly string Where;
            public readonly string Text;
            public readonly char Character;

            public Offender(string where, string text, char character)
            {
                Where = where; Text = text; Character = character;
            }

            public override string ToString() =>
                $"{Where}: '{Text}' contains U+{(int)Character:X4}";
        }

        /// <summary>Every unrenderable player-facing string. Empty = clean.</summary>
        public static List<Offender> FindUnrenderable()
        {
            var bad = new List<Offender>();

            void Probe(string where, string text)
            {
                if (UiTheme.IsRenderable(text)) return;
                bad.Add(new Offender(where, text, UiTheme.FirstUnrenderable(text)));
            }

            foreach (UnitKind k in System.Enum.GetValues(typeof(UnitKind)))
            {
                Probe($"DisplayName({k})", ProgenitorUpgradeCatalog.DisplayName(k));
                Probe($"PickerBlurb({k})", ProgenitorUpgradeCatalog.PickerBlurb(k));
                Probe($"LockedBlurb({k})", ProgenitorUpgradeCatalog.LockedBlurb(k));
                foreach (var row in ProgenitorUpgradeCatalog.RowsFor(k))
                {
                    Probe($"upgrade name({k})", row.Name);
                    Probe($"upgrade effect({k})", row.Effect);
                }
            }

            foreach (PathogenSpecies sp in System.Enum.GetValues(typeof(PathogenSpecies)))
            {
                Probe($"species name({sp})", SpeciesRoster.NameOf(sp));
                Probe($"species short({sp})", SpeciesRoster.ShortNameOf(sp));
            }

            foreach (var rung in KnowledgeLadder.Rungs) Probe("ladder rung", rung.ShortName);

            return bad;
        }

        /// <summary>Exposed so the harness can prove the guard itself works.
        /// A test that only ever passes is not a test.</summary>
        public static bool IsRenderable(string text) => UiTheme.IsRenderable(text);
    }
}
