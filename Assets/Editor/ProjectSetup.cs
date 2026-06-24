using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ProjectSetup
{
    const string ScenePath = "Assets/Scenes/Main.unity";

    [InitializeOnLoadMethod]
    static void OnLoad()
    {
        EditorApplication.delayCall += () =>
        {
            if (!Application.isPlaying) EnsureScene();
        };
    }

    static void EnsureScene()
    {
        PlayerSettings.productName = "Mixtape";
        PlayerSettings.companyName = "Mani";

        if (!File.Exists(ScenePath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[Mixtape] Created " + ScenePath);
        }

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
    }

    // Batchmode target: refuses to run if any script has compile errors.
    public static void CompileCheck()
    {
        EnsureScene();
        Debug.Log("[Mixtape] CompileCheck OK");
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }
}
