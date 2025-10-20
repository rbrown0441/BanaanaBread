using UnityEngine;

// -----------------------------------------------------------------------------
// AttackRelay
// -----------------------------------------------------------------------------
// Purpose: Controls when the HurtBox collider (child trigger) is active during
// an attack animation. This version does NOT use animation events.
// Instead, it checks the Animator’s current state and normalized time.
//
// Usage:
// • Put this script on the enemy’s root object (same one that has the Animator).
// • Drag that Animator into the "Animator" field (or leave blank to auto-grab).
// • Drag the HurtBox child’s BoxCollider2D into "Hurt Collider".
// • In "Attack State Name", type the exact Animator state name for the attack.
// • Adjust "Window Open" and "Window Close" to match the active hit window.
//
// Example: if the attack clip lasts 1 second and the actual bite should hit
// between 0.3s and 0.6s, set WindowOpen = 0.3 and WindowClose = 0.6.
// -----------------------------------------------------------------------------

public class AttackRelay : MonoBehaviour
{
    [Header("References")]
    public Animator animator;          // Animator on this object
    public Collider2D hurtCollider;    // The trigger collider on HurtBox child

    [Header("Attack Timing")]
    [Tooltip("Animator state name for this attack (case-sensitive)")]
    public string attackStateName = "Sunflower_Attack";
    [Range(0f, 1f)] public float windowOpen = 0.18f;
    [Range(0f, 1f)] public float windowClose = 0.42f;
    [Tooltip("Animator layer index (usually 0)")]
    public int layerIndex = 0;

    // --- internal ---
    bool lastState;

    void Reset()
    {
        // Auto-fill references when added
        animator = GetComponent<Animator>();
        var child = transform.Find("HurtBox");
        if (child != null)
            hurtCollider = child.GetComponent<Collider2D>();
    }

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        // Start disabled until attack plays
        if (hurtCollider != null)
            hurtCollider.enabled = false;
    }

    void Update()
    {
        if (animator == null || hurtCollider == null)
            return;

        var state = animator.GetCurrentAnimatorStateInfo(layerIndex);

        // Are we in the attack animation?
        bool inAttack = state.IsName(attackStateName);
        bool shouldEnable = false;

        if (inAttack)
        {
            // Normalized time is 0 → 1 across the clip length
            float t = state.normalizedTime % 1f;
            shouldEnable = (t >= windowOpen && t <= windowClose);
        }

        // Toggle collider only when state changes
        if (shouldEnable != lastState)
        {
            hurtCollider.enabled = shouldEnable;
            lastState = shouldEnable;
        }
    }
}
