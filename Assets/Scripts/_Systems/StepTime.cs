

/***************************************************
|| StepTime - phase stepper (Day ? Twilight ? Night) ||
- Static Phase property + Advance() API
- Will add auto phase steps when the player crosses through opposite screen tunnels 
- C# event OnPhaseChanged (simple, no EventBus)
- Placeholder Popup text(unscaled-time safe) 
//**************************************************/


using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI; // May use TextMesh 

public enum DayPhase { Day = 0, Twilight = 1, Night = 2 }



public class StepTime : MonoBehaviour
{
    // ---static API ----
    public static DayPhase Phase { get; private set; } = DayPhase.Day;
    public static event Action<DayPhase> OnPhaseChanged;


    // Assign in Inspector (optional). If null, we just won't show a popup.
    [SerializeField] private Text popupText;
    [SerializeField] private float popupDuration = 1.2f;

    private static StepTime _inst;     // run coroutines for popup
    private const string SaveKey = "NK_Phase";  //PlayerPrefs key


    void Awake()
    {
        _inst = this;


        // Restore last saved phase (optional; remove if you prefer Day on boot)
        if (PlayerPrefs.HasKey(SaveKey))
            Phase = (DayPhase)PlayerPrefs.GetInt(SaveKey, (int)DayPhase.Day);

        // Notify listeners of current phase at startup
        SafeNotify();
    }


    // Rotate Day -> Twilight -> Night and notify everyone
    public static void Advance()
    {
        Phase = (DayPhase)(((int)Phase + 1) % 3);
        PlayerPrefs.SetInt(SaveKey, (int)Phase);
        PlayerPrefs.Save();
        SafeNotify();

        if (_inst != null && _inst.popupText != null)
            _inst.StartCoroutine(_inst.ShowPopupDeferred(Phase.ToString(), 0.0f)); //delay if the fade needs it)
    }



    // Directly set a phase (for testing)

    public static void SetPhase(DayPhase p)

    {
        Phase = p; 
        PlayerPrefs.SetInt(SaveKey, (int)Phase);
        PlayerPrefs.SetInt(SaveKey, (int)Phase);
        PlayerPrefs.Save();
        SafeNotify();

        if (_inst != null && _inst.popupText != null)
            _inst.StartCoroutine(_inst.ShowPopupDeferred(Phase.ToString(), 0.0f));

    }

    //Fire the event safely so one bad listener can't kill the others
    private static void SafeNotify()
    {
        try { OnPhaseChanged?.Invoke(Phase); }
        catch (Exception e) { Debug.LogException(e); }

    }



    // Optional: dev hotkeys (remove in production)
    // [  ] to step, Shift+[ / Shift+] to set explicit phase.
    void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.LeftBracket)) Advance();
        if (Input.GetKeyDown(KeyCode.RightBracket)) Advance();

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.LeftBracket)) SetPhase(DayPhase.Day);
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Backslash)) SetPhase(DayPhase.Twilight);
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.RightBracket)) SetPhase(DayPhase.Night);
#endif
    }


    // Popup helpers (unscaled time, optional small delay to avoid being hidden by fades)

    private IEnumerator ShowPopupDeferred(string text, float delay)
    {
        if (delay > 0f)
        {
            float t = delay;
            while (t > 0f) { t -= Time.unscaledDeltaTime; yield return null; }

        }
        yield return ShowPopup(text);
    }

    private IEnumerator ShowPopup(string text)
    { 
        if (popupText == null) yield break;

        popupText.gameObject.SetActive(true);
        popupText.text = text;


        //Full opaque
        var c = popupText.color; c.a = 1f; popupText.color = c;


        // Hold a bit
        float t = popupDuration;
        while (t > 0f) { t -= Time.unscaledDeltaTime; yield return null; }

        // Quick fade out
        float fade = 0.35f;
        while (fade > 0f)
        {
            fade -= Time.unscaledDeltaTime;
            c.a = Mathf.InverseLerp(0f, 0.35f, fade);
            popupText.color = c;
            yield return null;

        }
        
        
        popupText.gameObject.SetActive(false);  



    }



}






























