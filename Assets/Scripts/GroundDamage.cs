using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Vector2 = UnityEngine.Vector2;

public class GroundDamage : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private float pushStrength;

    [SerializeField] private GameObject player;
    private CharacterScript playerScript;
    void Start() {
        playerScript = player.GetComponent<CharacterScript>();
    }

    // Handle damaging player    
    void OnTriggerStay2D(Collider2D collider) {
        if (collider.gameObject == player) {
            Vector2 pushDirection = collider.transform.position - transform.position;
            pushDirection.Normalize();
            playerScript.Hurt(damage, pushDirection * pushStrength);
        }
    }
}