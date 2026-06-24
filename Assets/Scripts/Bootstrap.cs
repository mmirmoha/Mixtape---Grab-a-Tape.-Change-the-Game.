using UnityEngine;

// Runtime entry point — the scene is empty; everything is built from code.
// Works even if Play is pressed in an Untitled scene with a default camera.
public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;

        foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            Object.Destroy(cam.gameObject);
        foreach (var al in Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
            Object.Destroy(al.gameObject);

        var go = new GameObject("GameManager");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<GameManager>();
    }
}
