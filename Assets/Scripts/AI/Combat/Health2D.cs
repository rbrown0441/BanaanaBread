using UnityEngine;

public class Health2D : MonoBehaviour
{
    [SerializeField] int maxHp = 3;
    [SerializeField] bool destroyOnDeath = true;
    int hp;
    Rigidbody2D rb;

    void Awake() { hp = maxHp; rb = GetComponent<Rigidbody2D>(); }

    public void TakeHit(int dmg, Vector2 impulse)
    {
        hp -= Mathf.Max(0, dmg);
        if (rb) rb.AddForce(impulse, ForceMode2D.Impulse);
        if (hp <= 0 && destroyOnDeath) Destroy(gameObject);
    }
}
