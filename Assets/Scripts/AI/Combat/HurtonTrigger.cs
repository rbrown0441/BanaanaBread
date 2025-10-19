using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HurtTrigger : MonoBehaviour
{
    public LayerMask targetMask;
    public int damage = 1;         // tweak per enemy
    public Vector2 knockback = new Vector2(2f, 2f);

    private Collider2D col;

    void Reset()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other) => TryHit(other);
    void OnTriggerStay2D(Collider2D other) => TryHit(other);

    void TryHit(Collider2D other)
    {
        // filter by targetMask
        if (((1 << other.gameObject.layer) & targetMask.value) == 0) return;

        // Try Health2D targets first
        var hp = other.GetComponentInParent<Health2D>();
        if (hp)
        {
            Vector2 dir = (other.bounds.center - col.bounds.center).normalized;
            hp.TakeHit(damage, dir * knockback.magnitude);
            return;
        }

        // Try PlayerHitProxy (your player bridge)
        var proxy = other.GetComponentInParent<PlayerHitProxy>();
        if (proxy)
        {
            Vector2 dir = (other.bounds.center - col.bounds.center).normalized;
            proxy.TakeHit(damage, dir * knockback.magnitude);
            return;
        }
    }
}
