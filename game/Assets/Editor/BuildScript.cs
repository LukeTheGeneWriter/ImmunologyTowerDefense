using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using ImmunologyTD.Bootstrap;

/// <summary>
/// Build entry points. Run from PowerShell via:
///   Unity.exe -batchmode -quit -projectPath <path> -executeMethod BuildScript.BuildWindows
///   Unity.exe -batchmode -quit -projectPath <path> -executeMethod BuildScript.BuildWebGL
/// Both ensure the Sprint 1 scene exists and is in Build Settings before
/// building, so no manual Editor step is needed first. As of Sprint 1 the
/// scene is Assets/Scenes/Sprint1.unity (renamed from Sprint0.unity) and
/// carries a single GameBootstrap object that builds the rest of the
/// scene at runtime -- see Assets/Editor/SceneSetup.cs and
/// Assets/Scripts/Bootstrap/GameBootstrap.cs.
/// </summary>
public static class BuildScript
{
    private const string ScenePath = "Assets/Scenes/Sprint1.unity";

    private static string EnsureSceneExists()
    {
        if (!File.Exists(ScenePath))
        {
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new UnityEngine.GameObject("GameBootstrap");
            go.AddComponent<GameBootstrap>();
            EditorSceneManager.SaveScene(scene, ScenePath);
        }
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        return ScenePath;
    }

    public static void BuildWindows()
    {
        var scenePath = EnsureSceneExists();
        var outputDir = Path.Combine("Builds", "Windows");
        Directory.CreateDirectory(outputDir);
        var report = BuildPipeline.BuildPlayer(
            new[] { scenePath },
            Path.Combine(outputDir, "ImmunologyTowerDefense.exe"),
            BuildTarget.StandaloneWindows64,
            BuildOptions.None);
        LogResult("Windows", report);
    }

    public static void BuildWebGL()
    {
        var scenePath = EnsureSceneExists();
        var outputDir = Path.Combine("Builds", "WebGL");
        Directory.CreateDirectory(outputDir);
        var report = BuildPipeline.BuildPlayer(
            new[] { scenePath },
            outputDir,
            BuildTarget.WebGL,
            BuildOptions.None);
        LogResult("WebGL", report);
    }

    private static void LogResult(string label, BuildReport report)
    {
        UnityEngine.Debug.Log(
            $"[BuildScript] {label} build result: {report.summary.result}, " +
            $"size: {report.summary.totalSize} bytes, errors: {report.summary.totalErrors}");
    }
}
