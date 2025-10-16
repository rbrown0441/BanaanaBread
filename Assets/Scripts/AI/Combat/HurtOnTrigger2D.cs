using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtOnTrigger2D : MonoBehaviour
{
    [Header("Targets & Hit")]
    [SerializeField] LayerMask targetMask;   // e.g., Player
    [SerializeField] int damage = 1;
    [SerializeField] float pushStrength = 6f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & targetMask) == 0) return;

        // Knockback direction
        Vector2 dir = (other.transform.position - transform.position).normalized;

        // Try CharacterScript.Hurt first (compatible with your project)
        var cs = other.GetComponent<CharacterScript>();
        if (cs != null)
        {
            cs.Hurt(damage, dir * pushStrength);
            return;
        }

        
    }
}
