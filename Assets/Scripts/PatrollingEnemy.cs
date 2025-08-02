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
        
        playerScript = FindFirstObjectByType<CharacterScript>();
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
    // void OnCollisionEnter2D(Collision2D collider) {
    //     DamagePlayer(collider);
    // }
    // void OnCollisionStay2D(Collision2D collider) {
    //   //  DamagePlayer(collider);
    // }
    void OnCollisionEnter2D(Collision2D collider) {
        Debug.Log($"Getting Attacked: {playerScript.IsAttacking}");
        if (collider.gameObject == player && playerScript.IsAttacking) StartCoroutine(TakeDamage());
        else DamagePlayer(collider.gameObject);
    }
    
    void DamagePlayer(GameObject gameObject)
    {
        if (gameObject == player)
        {
            Vector2 pushDirection = gameObject.transform.position - transform.position;
            pushDirection.Normalize();
            playerScript.Hurt(damage, pushDirection * pushStrength);
        }
    }
    
    [SerializeField] private int health;

    private IEnumerator TakeDamage()
    {
        var renderer = GetComponent<SpriteRenderer>();
        Color originalColor = renderer.color;

        renderer.color = Color.red;

        yield return new WaitForSeconds(0.15f);

        renderer.color = originalColor;

        yield return new WaitForSeconds(0.1f);

        health -= 1;
        if (health < 1) Destroy(gameObject);
    }
}
