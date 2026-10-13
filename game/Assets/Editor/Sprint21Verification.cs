using UnityEngine;
using ImmunologyTD.Adaptive;
using ImmunologyTD.Bootstrap;
using ImmunologyTD.Pathogens;
using ImmunologyTD.Rendering;
using ImmunologyTD.UI;

/// <summary>
/// Sprint 21 verification: the parts of the cosmetic pass that are pure
/// logic (docs/COSMETIC_PASS_SPEC.md §0, §4, §1.3). Everything visual is
/// the Director's eye.
///
///  1. KnowledgeWatcher: priming fires nothing; a +3% teach is one Match; a
///     crossing of a rung threshold is an Unlock with the right index; two
///     rungs crossed at once are two Unlocks; a drop is a Loss; nothing
///     changes when the ledger revision doesn't.
///  2. The honesty rule: no rung is Built yet, and the capstone sits at 90%.
///  3. FitCamera's gutter: >= 15% of the screen above and below the board
///     at 16:9, 21:9 and 4:3, and a no-op at 16:9.
///  4. The node body (rounded rect, corner 0.30H) contains the helper-T
///     arena's corners (86% of the node rect) -- the old bean ellipse did not.
///  5. The new UI sprites and organ canvases build, and the plate frame is
///     a 9-slice with the expected border.
///
/// Run (Linux): tools/unity.sh Sprint21Verification.RunAll
/// </summary>
public static class Sprint21Verification
{
    private static int passed;
    private static int failed;

    public static void RunAll()
    {
        passed = 0;
        failed = 0;
        Debug.Log("[Sprint21Verification] Starting ...");
        AdaptiveTuning.ResetToDefaults();

        RunWatcher();
        RunHonesty();
        RunGutter();
        RunNodeContainsArena();
        RunSprites();

        Debug.Log($"[Sprint21Verification] Done. {passed} passed, {failed} failed.");
    }

    private static void Check(string label, bool cond)
    {
        if (cond) { passed++; Debug.Log($"[Sprint21Verification] PASS -- {label}"); }
        else { failed++; Debug.LogError($"[Sprint21Verification] FAIL -- {label}"); }
    }

    private static void RunWatcher()
    {
        var ledger = new KnowledgeLedger();
        ledger.Add(PathogenClass.IntracellularVirus, 6f);
        var w = new KnowledgeWatcher();
        Check("first poll primes and fires nothing (no replay of earlier matches)", w.Poll(ledger) == 0);
        Check("no change -> no events", w.Poll(ledger) == 0);

        ledger.Add(PathogenClass.IntracellularVirus, 3f);   // 9%: no rung
        int n = w.Poll(ledger);
        Check("a +3% teach is exactly one Match", n == 1 && w.Events[0].Kind == KnowledgeWatcher.Kind.Match
              && w.Events[0].Species == (int)PathogenClass.IntracellularVirus && Mathf.Approximately(w.Events[0].Delta, 3f));

        ledger.Add(PathogenClass.IntracellularVirus, 3f);   // 12%: crosses CTL at 10
        n = w.Poll(ledger);
        Check("crossing 10% is a Match plus an Unlock of rung 0 (CTL)",
            n == 2 && w.Events[0].Kind == KnowledgeWatcher.Kind.Match &&
            w.Events[1].Kind == KnowledgeWatcher.Kind.Unlock && w.Events[1].RungIndex == 0);

        ledger.Add(PathogenClass.LargeBacterium, 31f);       // 0 -> 31: crosses 10, 20, 30
        n = w.Poll(ledger);
        int unlocks = 0, lastRung = -1;
        for (int i = 0; i < n; i++) if (w.Events[i].Kind == KnowledgeWatcher.Kind.Unlock) { unlocks++; lastRung = w.Events[i].RungIndex; }
        Check("a jump across three thresholds is three Unlocks, in order", unlocks == 3 && lastRung == 2);

        ledger.Add(PathogenClass.LargeBacterium, -5f);
        n = w.Poll(ledger);
        Check("a drop is a Loss (the future mutation hook)", n == 1 && w.Events[0].Kind == KnowledgeWatcher.Kind.Loss);
    }

    private static void RunHonesty()
    {
        bool anyBuilt = false;
        foreach (var r in KnowledgeLadder.Rungs) anyBuilt |= r.Built;
        Check("no rung capability is built yet -> every reached rung shows 'planned'", !anyBuilt);
        Check("the capstone sits at 90%, above the last rung",
            Mathf.Approximately(KnowledgeLadder.CapstonePercent, 90f) &&
            KnowledgeLadder.CapstonePercent > KnowledgeLadder.Rungs[KnowledgeLadder.Rungs.Length - 1].ThresholdPercent);
    }

    private static void RunGutter()
    {
        var board = new Rect(-14f, -5.6f, 28f, 11.2f);    // the 2.5:1 Map 01 board
        foreach (var (name, aspect) in new[] { ("16:9", 16f / 9f), ("21:9", 21f / 9f), ("4:3", 4f / 3f) })
        {
            float half = GameBootstrap.CameraHalfHeight(board, aspect);
            float gutter = (half - board.height * 0.5f) / (2f * half);
            float halfWidth = half * aspect;
            Debug.Log($"[Sprint21Verification] {name}: gutter {gutter:P1} of screen height");
            Check($"{name}: each gutter is >= 15% of the screen", gutter >= GameBootstrap.UiGutterFraction - 1e-4f);
            Check($"{name}: the whole board is on screen", halfWidth >= board.width * 0.5f - 1e-4f);
        }
        float h169 = GameBootstrap.CameraHalfHeight(board, 16f / 9f);
        Check("16:9 is width-driven, unchanged from Sprint 20", Mathf.Approximately(h169, board.width * 0.5f * 1.04f / (16f / 9f)));
    }

    private static void RunNodeContainsArena()
    {
        // The node at its Map 01 proportions (base band 0.82 wide x 0.30 tall).
        float W = 3.6f, H = 2.2f;
        float ax = 0.43f * W, ay = 0.43f * H;    // the arena's corner, from the node centre
        float corner = 0.30f * H;
        float rounded = OrganShapes.Canvas.RoundedDist(ax, ay, 0f, 0f, W * 0.5f, H * 0.5f, corner);
        float ellipse = (ax / (W * 0.5f)) * (ax / (W * 0.5f)) + (ay / (H * 0.5f)) * (ay / (H * 0.5f));
        Check("the new rounded node body contains the helper-T arena's corner", rounded <= 0f);
        Check("...which the old bean ellipse did not (the bug it fixes)", ellipse > 1f);
    }

    private static void RunSprites()
    {
        Check("UI icons build", SpriteShapes.IconBolt != null && SpriteShapes.IconHeart != null && SpriteShapes.IconStar != null
                               && SpriteShapes.HatchTile != null && SpriteShapes.BuildPad != null && SpriteShapes.SelectRing != null);
        var plate = SpriteShapes.PlateFrame;
        Check("the plate frame is a 9-slice with a 48 px border", plate != null && Mathf.Approximately(plate.border.x, 48f));
        var pads = new Vector2[5];
        for (int i = 0; i < 5; i++) pads[i] = new Vector2(0f, (2 - i) * 0.95f);
        var m = OrganShapes.BuildMarrow(new Vector2(2.2f, 4.85f), pads, 0.76f, 0.95f);
        Check("the marrow builds three layers at the block's aspect",
            m.Bone != null && m.Space != null && m.Shadow != null &&
            Mathf.Abs(m.Bone.bounds.size.x - 2.2f) < 0.02f && Mathf.Abs(m.Bone.bounds.size.y - 4.85f) < 0.05f);
        var n = OrganShapes.BuildNode(new Vector2(3.6f, 2.2f));
        Check("the node builds four layers, padded for its vessels",
            n.Body != null && n.Vessels != null && n.Follicles != null &&
            Mathf.Abs(n.Body.bounds.size.x - 3.6f * 1.24f) < 0.02f && Mathf.Abs(n.Body.bounds.size.y - 2.2f * 1.18f) < 0.05f);

        // A pad cavity is really cut: the bone is transparent at a pad centre.
        var tex = m.Bone.texture;
        var c = tex.GetPixel(tex.width / 2, tex.height / 2);
        Check("the bone has a cavity behind the middle pad", c.a < 0.1f);
    }
}
