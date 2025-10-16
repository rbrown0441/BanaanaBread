using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Netherkin/AI Actions/Flytrap Snap")]
public class Act_Snap : AIAction
{
    [Header("Trigger Window")]
    public float biteRange = 0.9f;
    public float arcDegrees = 120f;       // how wide the mouth arc is
    public float cooldown = 1.4f;         // seconds between bites

    [Header("Animator")]
    public string attackTrigger = "Attack";

    // Per-agent cooldown table (key = instanceID)
    static readonly Dictionary<int, float> _nextAllowed = new();

    public override float Score(Blackboard bb, Perception2D s)
    {
        var t = s.FindBestTarget();
        if (t == null) return 0f;

        int id = bb.self.GetInstanceID();
        float now = Time.time;
        if (_nextAllowed.TryGetValue(id, out float readyAt) && now < readyAt) return 0f;

        // In front arc?
        Vector2 to = (Vector2)t.position - (Vector2)bb.self.position;
        if (to.sqrMagnitude > biteRange * biteRange) return 0f;

        Vector2 fwd = bb.self.right; // 2D "forward" (x+)
        float cosHalf = Mathf.Cos(0.5f * arcDegrees * Mathf.Deg2Rad);
        if (Vector2.Dot(to.normalized, fwd) < cosHalf) return 0f;

        // High score if someone is in bite window
        return Mathf.Max(0f, baseWeight + 2f);
    }

    public override void Enter(Blackboard bb, Perception2D s)
    {
        var anim = bb.self.GetComponent<Animator>();
        if (anim && !string.IsNullOrEmpty(attackTrigger)) anim.SetTrigger(attackTrigger);

        _nextAllowed[bb.self.GetInstanceID()] = Time.time + cooldown;
    }

    public override void Tick(float dt, Blackboard bb, Perception2D s)
    {
        // Flytraps are largely stationary during bite; no per-tick logic needed.
        // You can optionally nudge slight aim by flipping toward the target:
        var t = s.FindBestTarget();
        if (!t) return;

        float dir = Mathf.Sign(t.position.x - bb.self.position.x);
        var ls = bb.self.localScale;
        bb.self.localScale = new Vector3(Mathf.Abs(ls.x) * (dir == 0 ? 1 : dir), ls.y, ls.z);
    }
}
