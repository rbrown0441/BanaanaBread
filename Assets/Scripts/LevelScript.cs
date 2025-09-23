using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScript : MonoBehaviour
{
    [SerializeField] GameObject PauseMenu;
    // Start is called before the first frame update
    void Start()
    {
        PauseMenu.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            togglePause();
        }

    }

    void togglePause()
    {
        if (PauseMenu.activeSelf == false)
        { 
          Time.timeScale = 0;
            PauseMenu.SetActive(true);
        }
        else
        {
            Time.timeScale = 1.0f;
            PauseMenu.SetActive(false);
        }


    }

    public void Escape()
    {

        Console.WriteLine("escape");


    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
                                    Application.Quit();
    #endif

    }
}
