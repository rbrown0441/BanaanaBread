using UnityEngine;
using UnityEngine.Events;

public class Health2D : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] int maxHp = 3;
    [SerializeField] bool destroyOnDeath = true;
    [SerializeField] float deathDestroyDelay = 0.25f;   // delay so death flash is visible
    [SerializeField] bool flashOnDeath = true;          // flash if DamageFlash exists

    [Header("Events")]
    public UnityEvent<int> OnDamaged; // dmg amount
    public UnityEvent OnDied;         // fired when hp <= 0

    int hp;
    bool _dead;
    Rigidbody2D rb;

    void Awake()
    {
        hp = maxHp;
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeHit(int dmg, Vector2 impulse)
    {
        if (_dead) return;
        dmg = Mathf.Max(0, dmg);

        if (dmg > 0)
        {
            hp -= dmg;
            OnDamaged?.Invoke(dmg);           // damage flash, hit reactions, etc.
        }

        if (rb) rb.AddForce(impulse, ForceMode2D.Impulse);

        if (hp <= 0) StartCoroutine(CoHandleDeath());
    }

    System.Collections.IEnumerator CoHandleDeath()
    {
        if (_dead) yield break;
        _dead = true;

        // tell listeners first (sound, score, etc.)
        OnDied?.Invoke();

        // built-in death flash (if component exists)
        if (flashOnDeath)
        {
            var df = GetComponent<DamageFlash>();
            if (df != null) df.Flash();
        }

        // destroy (optionally after a delay)
        if (destroyOnDeath)
        {
            if (deathDestroyDelay > 0f) yield return new WaitForSeconds(deathDestroyDelay);
            Destroy(gameObject);
        }
    }
}
