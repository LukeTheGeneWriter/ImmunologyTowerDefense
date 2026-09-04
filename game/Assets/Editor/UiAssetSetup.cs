using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Creates the one UI asset this project ships: `Assets/Resources/ITD_PanelSettings.asset`.
///
/// Sprint 16 built the whole front end from code specifically to avoid
/// hand-authored UI assets, and `docs/UI_DESIGN.md` §7 predicted the one
/// thing that might force an asset anyway. It did — but not for the
/// reason the spec expected. A `PanelSettings` created at runtime with
/// `ScriptableObject.CreateInstance` has no **text settings**, and in a
/// player build UI Toolkit's text shaper then dereferences null on every
/// label, every frame:
///
///   ICU Data not available. ... It will not be present on PanelSettings
///   created at runtime, so make sure the build contains at least one
///   PanelSettings asset
///   NullReferenceException at UITKTextHandle.ShapeText
///
/// This never appears in the Editor or in batchmode, only in the built
/// player, which is exactly why the Windows build + headless launch is
/// part of the definition of done and not an optional last step.
///
/// So: one `PanelSettings` asset, created here by script (not by hand in
/// an inspector), carrying the editor-assigned text settings and the
/// default runtime theme. Everything else stays code — no `.uxml`, no
/// `.uss`, no UI Builder. `GameBootstrap.BuildUiRoot` loads it from
/// Resources and falls back to the runtime-created instance if it is
/// missing, so a fresh clone still boots (with unstyled text) rather than
/// hard-failing.
///
/// Re-run after deleting the asset:
///   Unity.exe -batchmode -quit -projectPath &lt;repo&gt;\game
///             -executeMethod UiAssetSetup.CreatePanelSettings
/// </summary>
public static class UiAssetSetup
{
    public const string AssetPath = "Assets/Resources/ITD_PanelSettings.asset";
    private const string ThemePath = "Assets/UI Toolkit/UnityThemes/UnityDefaultRuntimeTheme.tss";

    [MenuItem("ImmunologyTD/Create Panel Settings asset")]
    public static void CreatePanelSettings()
    {
        Directory.CreateDirectory("Assets/Resources");

        var settings = AssetDatabase.LoadAssetAtPath<PanelSettings>(AssetPath);
        bool isNew = settings == null;
        if (isNew) settings = ScriptableObject.CreateInstance<PanelSettings>();

        settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        settings.referenceResolution = new Vector2Int(1920, 1080);
        settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
        settings.match = 0.5f;
        settings.sortingOrder = 100;
        settings.clearColor = false;

        // The default runtime theme is Unity's own generated asset (it
        // appears the moment a PanelSettings exists). We do not style
        // against it -- every element is explicit -- but assigning it is
        // what stops unstyled text from falling back to nothing.
        var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
        if (theme != null) settings.themeStyleSheet = theme;
        else Debug.LogWarning($"[UiAssetSetup] No theme at {ThemePath}; text may render unstyled.");

        if (isNew) AssetDatabase.CreateAsset(settings, AssetPath);
        else EditorUtility.SetDirty(settings);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[UiAssetSetup] {(isNew ? "Created" : "Updated")} {AssetPath} (theme: {(theme != null ? "assigned" : "MISSING")}).");
    }
}
