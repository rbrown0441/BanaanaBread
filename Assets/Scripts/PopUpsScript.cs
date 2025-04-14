using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopUpScript : MonoBehaviour
{
    [SerializeField] private GameObject Panel;
    // [SerializeField] private Button retryButton;
    // [SerializeField] private Button gameQuitButton;

    // [SerializeField] private int currentLives;

    // private bool isGameOver = false;

    void Start()
    {
        Panel.SetActive(false);
        // currentLives = FindFirstObjectByType<CharacterScript>().lives;
    }

    // public void UpdateLives(){
    //     currentLives = FindFirstObjectByType<CharacterScript>().lives;
    //     if(currentLives == 0){
    //         gameOverPanel.SetActive(true);

    //     // Freeze time
    //         Time.timeScale = 0f;

    //         retryButton.onClick.AddListener(Retry);
    //         gameQuitButton.onClick.AddListener(GameOver);
    //     }
    // }
    public void ActivatePanel(){
        Panel.SetActive(true);
        Time.timeScale = 0f;

    }
    public void GameOver(){
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    public void Retry(){
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}