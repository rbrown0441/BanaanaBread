// DamageFlash.cs
using UnityEngine;
using System.Collections;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] Color flashColor = Color.red;
    [SerializeField] float flashTime = 0.15f;
    [SerializeField] bool includeChildren = true;

    SpriteRenderer[] renders;
    Color[] orig;

    void Awake()
    {
        renders = includeChildren ? GetComponentsInChildren<SpriteRenderer>(true)
                               : new[] { GetComponent<SpriteRenderer>() };
        orig = new Color[renders.Length];
        for (int i = 0; i < renders.Length; i++) orig[i] = renders[i].color;
    }

    public void Flash() { StartCoroutine(CoFlash()); }
    public void FlashFromDamage(int _) { Flash(); } // lets you wire UnityEvent<int>
    IEnumerator CoFlash()
    {
        for (int i = 0; i < renders.Length; i++) renders[i].color = flashColor;
        yield return new WaitForSeconds(flashTime);
        for (int i = 0; i < renders.Length; i++) renders[i].color = orig[i];
    }
}

public class DelayDestroyOnDeath : MonoBehaviour
{
    public float delay = 0.25f;
    public void DoDestroy() { Destroy(gameObject, delay); }
}