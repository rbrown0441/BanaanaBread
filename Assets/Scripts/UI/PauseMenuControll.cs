using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] GameObject PauseMenu;
    // Start is called before the first frame update
    void Start()
    {
        if (PauseMenu) PauseMenu.SetActive(false);
        // start unpaused
        Time.timeScale = 1f;
    }
   

    // Update is called once per frame
    void Update()
    {
        // Open/close with keyboard Escape or common pad pause buttons
        if (Input.GetKeyDown(KeyCode.Escape) ||
            Input.GetKeyDown(KeyCode.JoystickButton6) || // Start/Options on most pads
            Input.GetKeyDown(KeyCode.JoystickButton7))   // Back/Select on many pads
        {
            TogglePause();
        }

        // Let B/Circle act as "Cancel" ONLY when menu is already open
        if (PauseMenu && PauseMenu.activeSelf && Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            TogglePause(); // close
        }
    }


    public void TogglePause()
    {
        if (!PauseMenu) return; 
        
        bool goingToPause =!PauseMenu.activeSelf;
        PauseMenu.SetActive(goingToPause);
        Time.timeScale = goingToPause ? 0f : 1f;
    }
    //Resume button
    public void Resume()
    {
        if (!PauseMenu) return;
        PauseMenu.SetActive(false);
        Time.timeScale = 1f; 
    }

    // Wait button
    public void WaitOneStep()
    {
        // 1)unpause and advance normally
        Resume();

        //2) advance phase + popup
        StepTime.Advance();
        Debug.Log("WaitOneStep");
        //3 (optional) to test reopening pause menu after the popup or stay unpaused
        // TogglePause();
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
