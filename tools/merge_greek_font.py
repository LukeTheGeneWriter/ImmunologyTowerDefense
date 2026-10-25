#!/usr/bin/env python3
"""Merge Greek + scientific symbols into the shipped Nunito faces.

Why this exists
---------------
Immunology is written in Greek -- IFN-gamma, gamma-delta T cells, TNF-alpha,
IL-1beta -- and the shipped Nunito faces contain no Greek block at all, so
every one of those labels silently rendered with the letters missing. The
game ships to WebGL, where there are no system fonts to fall back on, so the
glyphs have to be IN the font we ship.

Both Nunito and Noto Sans are SIL OFL 1.1, both are 1000 units/em, and Noto
ships matching Regular/Bold/Black weights. Nunito declares no Reserved Font
Name, but the output is renamed anyway so nobody can mistake a modified file
for upstream Nunito.

Donor glyphs are SCALED to match Nunito's x-height (484 vs Noto's 536-553,
about 11% larger) so a gamma sits on the same optical line as the Latin
around it instead of looking oversized.

Run from the repo root:  python3 tools/merge_greek_font.py
"""
import sys, pathlib
from fontTools.ttLib import TTFont
from fontTools.pens.ttGlyphPen import TTGlyphPen
from fontTools.pens.transformPen import TransformPen
from fontTools.pens.recordingPen import DecomposingRecordingPen
from fontTools.misc.transform import Transform

REPO = pathlib.Path(__file__).resolve().parent.parent
FONT_DIR = REPO / "game/Assets/Resources/Fonts"

# Two donors per weight, tried in order. Noto Sans carries Greek but NOT the
# comparison operators or arrows -- those live in Noto Sans Math, which ships
# a single weight, so the maths symbols are the same shape at every weight.
# At the sizes this UI uses that is invisible, and it beats a tofu box.
WEIGHTS = [
    ("Regular", FONT_DIR / "Nunito-Regular.ttf",
     ["/usr/share/fonts/noto/NotoSans-Regular.ttf",
      "/usr/share/fonts/noto/NotoSansMath-Regular.ttf"]),
    ("Bold",    FONT_DIR / "Nunito-Bold.ttf",
     ["/usr/share/fonts/noto/NotoSans-Bold.ttf",
      "/usr/share/fonts/noto/NotoSansMath-Regular.ttf"]),
    ("Black",   FONT_DIR / "Nunito-Black.ttf",
     ["/usr/share/fonts/noto/NotoSans-Black.ttf",
      "/usr/share/fonts/noto/NotoSansMath-Regular.ttf"]),
]

# The whole Greek block, because "Greek is everywhere in immunology" and a
# hand-picked handful would just fail again on the next cytokine. Plus the
# scientific symbols that keep company with it.
WANTED = set(range(0x0370, 0x0400))                     # Greek and Coptic
WANTED |= {0x00B5}                                      # micro sign
WANTED |= {0x2190, 0x2191, 0x2192, 0x2193}              # arrows
WANTED |= {0x2212, 0x00B1, 0x00D7, 0x00F7}              # minus plus-minus times divide
WANTED |= {0x2248, 0x2260, 0x2264, 0x2265}              # approx, not-equal, <=, >=
WANTED |= {0x00B0, 0x2032, 0x2033}                      # degree, prime, double prime
WANTED |= {0x207A, 0x207B}                              # superscript + and -
WANTED |= set(range(0x2080, 0x208A))                    # subscript digits
WANTED |= set(range(0x2070, 0x2080))                    # superscript digits


def covered(font):
    cm = set()
    for t in font["cmap"].tables:
        cm |= set(t.cmap.keys())
    return cm


def merge_one(label, nunito_path, donor_paths):
    dst = TTFont(str(nunito_path))
    dst_x = dst["OS/2"].sxHeight
    dst_glyf = dst["glyf"]
    dst_hmtx = dst["hmtx"]
    added = 0
    scale = 1.0

    for donor_path in donor_paths:
        if not pathlib.Path(donor_path).exists():
            print(f"  !! donor missing: {donor_path}")
            continue
        src = TTFont(donor_path)
        src_x = src["OS/2"].sxHeight or dst_x
        scale = dst_x / float(src_x)

        have = covered(dst)
        src_cmap = {}
        for t in src["cmap"].tables:
            src_cmap.update(t.cmap)

        todo = sorted(cp for cp in WANTED if cp in src_cmap and cp not in have)
        if not todo:
            continue

        src_glyphs = src.getGlyphSet()
        added += _copy(todo, src_cmap, src_glyphs, src, dst, dst_glyf, dst_hmtx, scale)

    dst["maxp"].numGlyphs = len(dst_glyf.glyphs)
    return dst, added, scale


def _copy(todo, src_cmap, src_glyphs, src, dst, dst_glyf, dst_hmtx, scale):
    added = 0
    for cp in todo:
        src_name = src_cmap[cp]
        new_name = f"uni{cp:04X}"
        if new_name in dst_glyf.glyphs:
            continue
        # Decompose composites first -- many Greek glyphs are built from
        # components, and a component reference would point at a glyph id
        # that does not exist in the destination font.
        try:
            rec = DecomposingRecordingPen(src_glyphs)
            src_glyphs[src_name].draw(rec)
            pen = TTGlyphPen(None)
            rec.replay(TransformPen(pen, Transform(scale, 0, 0, scale, 0, 0)))
        except Exception as e:
            print(f"    skip U+{cp:04X}: {e}")
            continue
        dst_glyf[new_name] = pen.glyph()
        adv, lsb = src["hmtx"][src_name]
        dst_hmtx[new_name] = (int(round(adv * scale)), int(round(lsb * scale)))
        added += 1

    # Point every cmap subtable at the new glyphs.
    for t in dst["cmap"].tables:
        if t.isUnicode():
            for cp in todo:
                if f"uni{cp:04X}" in dst_glyf.glyphs:
                    t.cmap.setdefault(cp, f"uni{cp:04X}")
    return added


def rename(font, label):
    """Rename so a modified file can never be mistaken for upstream Nunito."""
    family = "ImmunoSans"
    full = f"{family} {label}"
    ps = f"{family}-{label}"
    for rec in font["name"].names:
        try:
            cur = rec.toUnicode()
        except Exception:
            continue
        if rec.nameID == 1:
            rec.string = family
        elif rec.nameID == 4:
            rec.string = full
        elif rec.nameID == 6:
            rec.string = ps
        elif rec.nameID == 3:
            rec.string = f"{ps};ImmunologyTD"
        elif rec.nameID == 0:
            rec.string = (cur + "  Modified for Immunology Tower Defense: Greek and "
                                "scientific symbols merged from Noto Sans (SIL OFL 1.1).")


def emit_coverage(paths):
    """Write the C# coverage table from what the merged fonts ACTUALLY have.

    Hand-maintaining an allowlist is how the original bug happened in
    reverse: the code believed something was renderable when it was not.
    Generating it from the shipped files means the audit cannot drift from
    the font, and a glyph the donor turned out not to carry (several maths
    and arrow symbols are in Noto Sans Math, not Noto Sans) is reported as
    unsupported instead of being quietly claimed.
    """
    sets = []
    for p in paths:
        f = TTFont(str(p))
        sets.append(covered(f))
    common = sorted(set.intersection(*sets))

    ranges = []
    for cp in common:
        if ranges and cp == ranges[-1][1] + 1:
            ranges[-1][1] = cp
        else:
            ranges.append([cp, cp])

    out = REPO / "game/Assets/Scripts/UI/UiFontCoverage.cs"
    body = []
    body.append("// <auto-generated> by tools/merge_greek_font.py -- do not edit by hand.")
    body.append("// Regenerate after changing the shipped fonts.")
    body.append("namespace ImmunologyTD.UI")
    body.append("{")
    body.append("    /// <summary>")
    body.append("    /// Codepoint ranges the shipped UI faces actually contain, generated")
    body.append("    /// from the font files themselves rather than written by hand.")
    body.append("    ///")
    body.append("    /// Only ranges present in ALL THREE weights are listed, so a label")
    body.append("    /// cannot render in Regular and vanish in Black.")
    body.append("    /// </summary>")
    body.append("    public static class UiFontCoverage")
    body.append("    {")
    body.append("        /// <summary>Flattened [start, end] inclusive pairs, ascending.</summary>")
    body.append("        public static readonly int[] Ranges =")
    body.append("        {")
    for a, b in ranges:
        body.append(f"            0x{a:04X}, 0x{b:04X},")
    body.append("        };")
    body.append("")
    body.append("        public static bool Contains(int cp)")
    body.append("        {")
    body.append("            int lo = 0, hi = Ranges.Length / 2 - 1;")
    body.append("            while (lo <= hi)")
    body.append("            {")
    body.append("                int mid = (lo + hi) / 2;")
    body.append("                if (cp < Ranges[mid * 2]) hi = mid - 1;")
    body.append("                else if (cp > Ranges[mid * 2 + 1]) lo = mid + 1;")
    body.append("                else return true;")
    body.append("            }")
    body.append("            return false;")
    body.append("        }")
    body.append("    }")
    body.append("}")
    out.write_text("\n".join(body) + "\n")
    print(f"  coverage: {len(ranges)} ranges, {len(common)} codepoints -> {out.name}")


def main():
    total = 0
    for label, nunito, donors in WEIGHTS:
        print(f"{label}:")
        got = merge_one(label, nunito, donors)
        if not got:
            return 1
        font, added, scale = got
        rename(font, label)
        out = FONT_DIR / f"ImmunoSans-{label}.ttf"
        font.save(str(out))
        cm = covered(TTFont(str(out)))
        ok = all(c in cm for c in (0x3B1, 0x3B2, 0x3B3, 0x3B4, 0x3BC, 0x41, 0xB7))
        print(f"  added {added} glyphs, donor scaled x{scale:.3f} -> {out.name} "
              f"({'greek OK' if ok else 'GREEK MISSING'})")
        total += added

    outs = [FONT_DIR / f"ImmunoSans-{w}.ttf" for w, _, _ in WEIGHTS]
    emit_coverage(outs)

    # Say plainly what the donor could NOT provide, rather than letting it
    # look like everything requested arrived.
    got = covered(TTFont(str(outs[0])))
    missing = sorted(cp for cp in WANTED if cp not in got)
    if missing:
        print("  requested but unavailable from this donor "
              "(they live in Noto Sans Math/Symbols, not Noto Sans):")
        print("    " + " ".join(f"U+{c:04X}" for c in missing))
    print(f"total glyphs added: {total}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
