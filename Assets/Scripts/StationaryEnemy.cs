using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Vector2 = UnityEngine.Vector2;

public class StationaryEnemy : MonoBehaviour
{
    // Enemy stats
    [SerializeField] private int damage;
    [SerializeField] private float pushStrength;

    // Make sure to set player in editor
    [SerializeField] private GameObject player;
    private CharacterScript playerScript;

    void Start() {
        playerScript = player.GetComponent<CharacterScript>();
    }

    // Handle damaging player    
    void OnCollisionEnter2D(Collision2D collider) {
        DamagePlayer(collider);
    }
    void OnCollisionStay2D(Collision2D collider) {
        DamagePlayer(collider);
    }
    void DamagePlayer(Collision2D collider) {
        if (collider.gameObject == player) {
            Vector2 pushDirection = collider.transform.position - transform.position;
            pushDirection.Normalize();
            playerScript.Hurt(damage, pushDirection * pushStrength);
        }
    }
}
