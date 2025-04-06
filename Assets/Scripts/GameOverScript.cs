using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScript : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button gameQuitButton;
    [SerializeField] private int currentLives;

    private bool isGameOver = false;

    void Start()
    {
        gameOverPanel.SetActive(false);
        currentLives = FindFirstObjectByType<CharacterScript>().lives;
    }

    // void Update(){
    //     if(currentLives == 0 && !isGameOver){
    //         isGameOver = true;
    //         StartCoroutine(GameOverSequence());
    //     }

    //     UpdateLives();
    // }

    public void UpdateLives(){
        currentLives = FindFirstObjectByType<CharacterScript>().lives;
        if(currentLives == 0){
            gameOverPanel.SetActive(true);

        // Freeze time
            Time.timeScale = 0f;

            retryButton.onClick.AddListener(Retry);
            gameQuitButton.onClick.AddListener(GameOver);
        }
    }

//TODO: Check to see if the game will finish or go back to main menu
    public void GameOver(){
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    public void Retry(){
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator GameOverSequence()
    {
        Debug.Log("Game Over Triggered!");
        gameOverPanel.SetActive(true);

        // Freeze time
        Time.timeScale = 0f;

        retryButton.onClick.AddListener(Retry);
        gameQuitButton.onClick.AddListener(GameOver);

        yield return null;
    }

}