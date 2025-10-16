using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        UnityEngine.Debug.Log($"[Menu] LoadScene('{sceneName}')\n" + new StackTrace(1, true));
        SceneManager.LoadScene(sceneName);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
