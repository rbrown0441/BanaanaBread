using UnityEngine;
using System.Collections;

public class DamageFlash2D : MonoBehaviour
{
    [SerializeField] SpriteRenderer sprite;      // auto-grab if left empty
    [SerializeField] Color flashColor = Color.red;
    [SerializeField] float flashTime = 0.12f;

    Color _orig;
    void Awake()
    {
        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite) _orig = sprite.color;
    }

    public void Flash(int _unusedAmount)  // signature matches UnityEvent<int>
    {
        if (!sprite) return;
        StopAllCoroutines();
        StartCoroutine(DoFlash());
    }

    IEnumerator DoFlash()
    {
        sprite.color = flashColor;
        yield return new WaitForSeconds(flashTime);
        sprite.color = _orig;
    }
}
