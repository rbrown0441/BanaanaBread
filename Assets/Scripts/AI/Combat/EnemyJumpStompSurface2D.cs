using UnityEngine;

public class EnemyJumpStompSurface2D : MonoBehaviour
{
    [Header("Bounce / damage when doing a ground-pound stomp")]
    public float stompBounceVelocity = 8f;
    public int stompDamage = 1;

    [Header("Bounce / damage when landing on head (normal jump)")]
    public float headJumpBounceVelocity = 4f;
    public int headJumpDamage = 0;

    [Tooltip("Optional override; if null we'll look up in the parent.")]
    public Health2D healthOverride;

    public Health2D GetHealth()
    {
        if (healthOverride) return healthOverride;
        return GetComponentInParent<Health2D>();
    }
}
