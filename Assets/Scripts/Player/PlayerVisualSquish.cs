using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerVisualSquish : MonoBehaviour
{
    [Header("SpriteRenderer/Animator")]
    [SerializeField] private Transform visual;

    [Header("Parameters")]
    [SerializeField] private float default_duration = 0.15f;
    [SerializeField] private float default_elasticity = 0.10f;

    // Internal
    Vector3 base_scale_abs;
    float current_routine_id;

    void Awake()
    {
        if (visual == null)
        {
            // Try to auto-find child
            var t = transform.Find("Visual");
            // Fallback
            visual = t != null ? t : transform;
        }
        // Record Base Scale
        base_scale_abs = new Vector3(Mathf.Abs(visual.localScale.x), Mathf.Abs(visual.localScale.y), Mathf.Abs(visual.localScale.z));
    }

    // Squash/stretch, then ease back
    public void squish(float xMul, float yMul, float duration = -1f, float elasticity = -1f)
    {
        if (duration < -0f) duration = default_duration;
        if (elasticity < 0f) elasticity = default_elasticity;
        current_routine_id += 1f;
        // Snap to Target
        visual.localScale = new Vector3(base_scale_abs.x * xMul, base_scale_abs.y * yMul, base_scale_abs.z);
        // Return to Base
        StartCoroutine(return_to_base(duration, elasticity, current_routine_id));
    }

    IEnumerator return_to_base(float duration, float elasticity, float myToken)
    {
        Vector3 from = visual.localScale;
        // Slight Overshoot
        Vector3 over = new Vector3(base_scale_abs.x * (1f + elasticity), base_scale_abs.y * (1f - elasticity * 0.6f), base_scale_abs.z);

        float t = 0f;
        while (t < duration)
        {
            if (myToken != current_routine_id) yield break;
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);
            // Current -> Overshoot -> Base
            Vector3 mid = Vector3.LerpUnclamped(from, over, Ease.OutQuad(a));
            Vector3 s = Vector3.LerpUnclamped(mid, base_scale_abs, Ease.InQuad(a));
            // Stay positive :)
            s.x = Mathf.Abs(s.x);
            s.y = Mathf.Abs(s.y);
            // Scale
            visual.localScale = s;
            yield return null;
        }
        visual.localScale = base_scale_abs;
    }

    // Hard Reset Helper
    public void reset_scale() => visual.localScale = base_scale_abs;

    // Easing Helpers
    static class Ease
    {
        public static float OutQuad (float x) => 1f - (1f - Mathf.Clamp01(x)) * (1f - Mathf.Clamp01(x));
        public static float InQuad  (float x) => Mathf.Clamp01(x) * Mathf.Clamp01(x);
    }
}
