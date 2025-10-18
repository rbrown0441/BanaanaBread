using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopUpScript : MonoBehaviour
{
    [SerializeField] private GameObject Panel;

    void Start()
    {
        Panel.SetActive(false);
    }

    public void ActivatePanel()
    {
        Panel.SetActive(true);
        Time.timeScale = 0f;

    }
    public void GameOver()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        Panel.SetActive(false);
    }

}