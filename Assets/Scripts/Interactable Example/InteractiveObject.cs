using System;
using UnityEngine;
using UnityEngine.Events;

public class InteractiveObject : MonoBehaviour
{
    // This boolean determines if this interaction can happen or not.
    private bool _inRange;
    public CharacterScript Character;
    
    [SerializeField]
    public UnityEvent OnTriggerEnter;
    [SerializeField]
    public UnityEvent OnTriggerExit;
    [SerializeField]
    public UnityEvent OnInteract;

    public bool Interactable;

    private void Awake()
    {
        // Makes sure the object always interactable, change this if something else is desired
        Interactable = true;
    }

    // When the player collides with this object, a boolean is set to signal that the player is in range.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Instead of using other.CompareTag like many tutorials, you can check if the other is the CharacterScript
        // Can use any function you need that is on the character, like Character.Hurt();
        // or have any other script get the CharacterScript reference from InteractiveObject
        
        if (other.TryGetComponent(out CharacterScript character)) 
        {
            if (Interactable)
            {
                Character = character;
                
                //character.playerInput.onInteract += Interact;     // Note, we are not using this kind of Interaction input in the CharacterScript

                
                _inRange = true;

                OnTriggerEnter.Invoke();
            }
        }
    }

    // When the player is no longer in contact with the object, the bool is unset.
    private void OnTriggerExit2D(Collider2D other)
    {
        if (Interactable)
        {
            if (other.TryGetComponent(out CharacterScript character))
            {
                _inRange = false;

                OnTriggerExit.Invoke();
            }
        }
    }

    // If the Character is still in range, allows the action to trigger an interaction
    // Note, we are not using this kind of Interaction input in the CharacterScript
    private void Interact()
    {
        if(!_inRange) return;
        OnInteract.Invoke();
        //character.playerInput.onInteract += Interact;     // Note, we are not using this kind of Interaction input in the CharacterScript
    }
    
}