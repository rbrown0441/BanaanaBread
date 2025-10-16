using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchDamage : MonoBehaviour
{
    [SerializeField] LayerMask targetMask;     // e.g., Player
    [SerializeField] int healthDamage = 1;     // set 0 if you want stamina-only
    [SerializeField] float staminaDrain = 0f;  // set >0 for sunflowers; frogs can be 0
    [SerializeField] float pushStrength = 5f;
    [SerializeField] float tickInterval = 0.35f; // damage no more than this often

    float _nextTick;

    void OnCollisionStay2D(Collision2D c) { TryTouch(c.collider); }
    void OnTriggerStay2D(Collider2D c) { TryTouch(c); } // if your body collider isTrigger

    void TryTouch(Collider2D other)
    {
        if (Time.time < _nextTick) return;
        if (((1 << other.gameObject.layer) & targetMask) == 0) return;

        _nextTick = Time.time + tickInterval;

        Vector2 dir = (other.transform.position - transform.position).normalized;

        // Prefer your existing CharacterScript
        var cs = other.GetComponent<CharacterScript>();
        if (cs != null && healthDamage > 0)
            cs.Hurt(healthDamage, dir * pushStrength);

        // Optional stamina drain (if you add Stamina component later)
        var stamina = other.GetComponent<Stamina>();
        if (stamina != null && staminaDrain > 0)
            stamina.Drain(staminaDrain);
    }
}
