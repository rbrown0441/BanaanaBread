// Assets/Editor/PlayStartSceneTools.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PlayStartSceneTools
{
    [MenuItem("Tools/PlayMode/Print Start Scene")]
    public static void PrintStartScene()
    {
        var s = EditorSceneManager.playModeStartScene;
        Debug.Log(s ? $"PlayModeStartScene = {AssetDatabase.GetAssetPath(s)}" : "PlayModeStartScene = <none>");
    }

    [MenuItem("Tools/PlayMode/Clear Start Scene")]
    public static void ClearStartScene()
    {
        EditorSceneManager.playModeStartScene = null;
        Debug.Log("Cleared Play Mode Start Scene (Play will start from the currently open scene).");
    }

    // Hook loggers so we see *who* loads scenes
    [RuntimeInitializeOnLoadMethod]
    static void HookLogging()
    {
        SceneManager.sceneLoaded += (sc, mode) => Debug.Log($"[LOG] sceneLoaded: {sc.path} ({mode})");
        SceneManager.activeSceneChanged += (oldSc, newSc) => Debug.Log($"[LOG] activeSceneChanged: {newSc.path}");
    }

    [MenuItem("Tools/PlayMode/Set Start Scene To Selected", true)]
    static bool ValidateSet() => Selection.activeObject is SceneAsset;

    [MenuItem("Tools/PlayMode/Set Start Scene To Selected")]
    public static void SetStartSceneToSelected()
    {
        var scene = (SceneAsset)Selection.activeObject;
        EditorSceneManager.playModeStartScene = scene;
        Debug.Log($"PlayModeStartScene set to: {AssetDatabase.GetAssetPath(scene)}");
    }
}
#endif

