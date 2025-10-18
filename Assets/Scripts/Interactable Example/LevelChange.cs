using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChange : MonoBehaviour
{
    [SerializeField]
    private string nextLevelName;
    
    public void ChangeLevel()
    {
        SceneManager.LoadScene(nextLevelName);
    }
}