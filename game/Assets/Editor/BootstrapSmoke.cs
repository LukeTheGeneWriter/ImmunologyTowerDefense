using UnityEngine;
using ImmunologyTD.Bootstrap;
using ImmunologyTD.Grid;

/// <summary>
/// Sprint 16: the one automatable signal the UI pass has
/// (docs/UI_DESIGN.md §9).
///
/// None of the rendered UI is headlessly testable -- it is Update()- and
/// event-driven, and Update() does not run in Editor batchmode. What *is*
/// testable is that booting the game constructs every view without
/// throwing: GameBootstrap.Awake now also creates the PanelSettings, the
/// UIDocument, and the whole initial visual tree (UiController.Build), so a
/// batchmode boot that finishes with zero errors covers "no view class
/// threw while building" -- a null slot, a bad Background.FromSprite on a
/// sprite that hasn't been rastered yet, a catalog index slip.
///
/// It is a smoke test, not a UI test. It cannot tell you the panel looks
/// right; it can tell you the panel exists and nothing exploded making it.
/// The Sprint 4 degenerate-band bug (a silent zero-width playfield that
/// rendered happily and reported nothing) is the other class of thing it
/// catches, since Awake's own WarnOnDegenerateBands logs an error.
///
/// Awake is invoked by reflection because edit-mode AddComponent does not
/// call it -- that is a property of the Editor, not of the bootstrap.
///
/// Run:
///   Unity.exe -batchmode -quit -projectPath &lt;repo&gt;\game
///             -executeMethod BootstrapSmoke.RunAll
/// </summary>
public static class BootstrapSmoke
{
    private static int errors;

    public static void RunAll()
    {
        errors = 0;
        Debug.Log("[BootstrapSmoke] Starting ...");

        Application.logMessageReceived += OnLog;
        GameObject go = null;
        try
        {
            go = new GameObject("SmokeBootstrap");
            go.AddComponent<BoardConfig>();
            var boot = go.AddComponent<GameBootstrap>();

            var awake = typeof(GameBootstrap).GetMethod(
                "Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (awake == null)
            {
                Debug.LogError("[BootstrapSmoke] FAIL -- GameBootstrap.Awake not found; the smoke test needs updating.");
            }
            else
            {
                awake.Invoke(boot, null);
            }
        }
        catch (System.Exception e)
        {
            // A TargetInvocationException here is the whole point of the
            // test: unwrap it so the log names the real failure.
            var real = e.InnerException ?? e;
            Debug.LogError($"[BootstrapSmoke] FAIL -- boot threw {real.GetType().Name}: {real.Message}\n{real.StackTrace}");
        }
        finally
        {
            Application.logMessageReceived -= OnLog;
            if (go != null) Object.DestroyImmediate(go);
        }

        if (errors == 0) Debug.Log("[BootstrapSmoke] PASS -- bootstrap + UI tree built with 0 errors.");
        Debug.Log($"[BootstrapSmoke] Done. {errors} error(s).");
    }

    private static void OnLog(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++;
    }
}
