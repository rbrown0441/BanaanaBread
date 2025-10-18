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
    void OnCollisionStay2D(Collision2D collider) {
        //DamagePlayer(collider);
    }
    
    // Handle damaging player    
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
