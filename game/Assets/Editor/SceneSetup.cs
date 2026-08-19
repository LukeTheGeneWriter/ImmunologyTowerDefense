using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ImmunologyTD.Bootstrap;

/// <summary>
/// Builds Sprint 1's scene from code: a single GameObject carrying
/// GameBootstrap, which in turn builds the camera, board visualization,
/// unit/pathogen pools, spawns, and HUD at runtime (see GameBootstrap.cs).
/// Written as an Editor script rather than hand-authoring scene YAML
/// because there's no interactive Editor session in this workflow --
/// run via
///   Unity.exe -batchmode -quit -projectPath <path> -executeMethod SceneSetup.RebuildSprint1Scene
/// or the menu item below from inside the Editor. Re-run any time the
/// scene needs to be reset to a clean single-bootstrap-object state.
/// </summary>
public static class SceneSetup
{
    public const string Sprint1ScenePath = "Assets/Scenes/Sprint1.unity";

    [MenuItem("ImmunologyTD/Rebuild Sprint 1 Scene")]
    public static void RebuildSprint1Scene()
    {
        Directory.CreateDirectory("Assets/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var go = new GameObject("GameBootstrap");
        go.AddComponent<GameBootstrap>();

        EditorSceneManager.SaveScene(scene, Sprint1ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Sprint1ScenePath, true) };
        AssetDatabase.SaveAssets();

        Debug.Log("[SceneSetup] Sprint1 scene rebuilt at " + Sprint1ScenePath);
    }
}
