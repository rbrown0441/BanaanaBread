using System;
using UnityEngine;
using UnityEngine.Events;

public class InteractiveObject : MonoBehaviour
{
    private bool _inRange;
    public CharacterScript Character;

    [SerializeField] public UnityEvent OnTriggerEnter;
    [SerializeField] public UnityEvent OnTriggerExit;
    [SerializeField] public UnityEvent OnInteract;

    public bool Interactable;

    // NEW: optional built-in “press key to interact”
    [Header("Optional Key Interaction")]
    [SerializeField] bool useKeyInteract = true;
    [SerializeField] KeyCode interactKey = KeyCode.E;

    void Awake()
    {
        Interactable = true;
    }

    // NEW: poll for key while player is in range
    void Update()
    {
        if (useKeyInteract && (Input.GetKeyDown(interactKey)
        || Input.GetKeyDown(KeyCode.JoystickButton0)   // A / Cross (old-input pads)
        || Input.GetButtonDown("Submit")))             // if mapped in Project Settings > Input
        {
            Interact(); // will invoke OnInteract
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out CharacterScript character))
        {
            if (Interactable)
            {
                Character = character;
                _inRange = true;
                OnTriggerEnter.Invoke(); // show "Press E" UI, etc.
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (Interactable && other.TryGetComponent(out CharacterScript character))
        {
            _inRange = false;
            OnTriggerExit.Invoke(); // hide "Press E" UI
        }
    }

    // CHANGED: make this public so events & code can call it
    public void Interact()
    {
        if (!_inRange || !Interactable) return;
        OnInteract.Invoke();
    }
}
