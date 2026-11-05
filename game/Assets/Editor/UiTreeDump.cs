using System.IO;
using UnityEditor;
using UnityEngine;
using ImmunologyTD.Bootstrap;
using ImmunologyTD.Grid;
using ImmunologyTD.Rounds;
using ImmunologyTD.UI;

/// <summary>
/// Sprint 35: boots the real game headlessly and prints the whole UI tree,
/// plus what the HUD caption says in each round phase.
///
/// This is the diagnostic companion to <c>UiVisibilityVerification</c>: the
/// harness asserts, this one *shows*, which is what you want when the
/// question is "why does the player not see this" rather than "is it still
/// true". Same relationship as <c>SpriteShapeDump</c> to the sprite checks.
///
/// Writes `game/Logs/ui_tree.txt` as well as the log, because the tree is
/// long and grepping a file beats scrolling an Editor log.
///
///   tools/unity.sh UiTreeDump.DumpAll
/// </summary>
public static class UiTreeDump
{
    [MenuItem("ImmunologyTD/Diagnose/Dump the UI tree")]
    public static void DumpAll()
    {
        UiProbe.Clear();
        var go = new GameObject("UiTreeDumpBootstrap");
        try
        {
            go.AddComponent<BoardConfig>();
            var boot = go.AddComponent<GameBootstrap>();
            typeof(GameBootstrap)
                .GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(boot, null);

            var ui = Object.FindFirstObjectByType<UiController>();
            var rounds = Object.FindFirstObjectByType<RoundController>();
            if (ui == null || rounds == null)
            {
                Debug.LogError($"[UiTreeDump] boot produced ui={ui != null} rounds={rounds != null}");
                return;
            }

            var sb = new System.Text.StringBuilder();

            sb.Append("=== phase captions ===\n");
            ui.RefreshViews();
            Report(sb, "opening buy phase", rounds);

            rounds.StartRound();
            ui.RefreshViews();
            Report(sb, "active", rounds);

            // Back to a buy phase the honest way: run the clock until the
            // round resolves. A phase forced by reflection would prove
            // nothing about what the player meets.
            for (int i = 0; i < 20000 && rounds.Phase == RoundPhase.Active; i++) rounds.Tick(0.05f);
            ui.RefreshViews();
            Report(sb, $"buy phase after round 1 (phase={rounds.Phase})", rounds);

            sb.Append($"\nlayout pass ran: {UiProbe.HasLayout}\n");
            sb.Append($"panel root rect: {UiProbe.RectOf("UiRoot-container")} (root worldBound)\n");
            foreach (var n in new[] { UiNames.HudCard, UiNames.HudCaption, UiNames.HudStartButton })
                sb.Append($"  {n}: {UiProbe.RectOf(n)}\n");

            sb.Append("\n=== tree ===\n");
            sb.Append(UiProbe.Dump(14));

            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/ui_tree.txt", sb.ToString());
            Debug.Log(sb.ToString());
            Debug.Log("[UiTreeDump] wrote Logs/ui_tree.txt");
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }

    private static void Report(System.Text.StringBuilder sb, string where, RoundController rounds)
    {
        string text = UiProbe.TextOf(UiNames.HudCaption);
        string hidden = UiProbe.HiddenBecause(UiNames.HudCaption);
        sb.Append($"[{where}] round={rounds.RoundNumber} phase={rounds.Phase}\n");
        sb.Append($"    caption text   : {(text == null ? "<NO ELEMENT>" : "\"" + text + "\"")}\n");
        sb.Append($"    caption shown  : {(hidden == null ? "yes" : "NO -- " + hidden)}\n");
        sb.Append($"    caption count  : {UiProbe.Count(UiNames.HudCaption)}\n");
        if (!string.IsNullOrEmpty(text))
            sb.Append($"    renderable     : {UiProbe.IsRenderable(text)}\n");
    }
}
