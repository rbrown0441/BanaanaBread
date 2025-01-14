using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Vector2 = UnityEngine.Vector2;

public class PatrollingEnemy : MonoBehaviour
{
    // The two points that the player moves between
    [SerializeField] private float pointA; 
    [SerializeField] private float pointB;
    
    // Enemy stats
    [SerializeField] private float speed;
    [SerializeField] private int damage;
    [SerializeField] private float pushStrength;

    // Make sure to set the player in the editor
    [SerializeField] private GameObject player;
    private CharacterScript playerScript;
    private float targetPoint;

    void Start() {
        playerScript = player.GetComponent<CharacterScript>();
        targetPoint = pointA;
    }

    // Move back and forth between the points
    void Update() {
        transform.position = Vector2.MoveTowards(transform.position, new Vector2 (targetPoint, transform.position.y), speed * Time.deltaTime);
        if (Vector2.Distance(transform.position, new Vector2 (targetPoint, transform.position.y)) < 0.1f) {
            if (targetPoint == pointA)
                targetPoint = pointB;
            else
                targetPoint = pointA;
            transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
        }
    }

    //Handle damaging the player when they touch
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
