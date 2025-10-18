using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{ 
    [SerializeField] private string nextScene;

    public void Escape()
    {
              
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
                        
    }

    public void NewGame()
    {
        SceneManager.LoadScene(nextScene);
    }
}