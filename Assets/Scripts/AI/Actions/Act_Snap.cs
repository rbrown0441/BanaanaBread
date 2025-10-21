using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Netherkin/AI Actions/Flytrap Snap")]
public class Act_Snap : AIAction
{
    [Header("Trigger Window")]
    public float biteRange = 1.3f;                // reach
    [Range(0f, 360f)] public float arcDegrees = 200f;
    public float cooldown = 0.8f;

    [Header("Animator")]
    public string attackTrigger = "Attack";

    [Header("Facing")]
    [Tooltip("+scale visually faces RIGHT. Uncheck if +scale faces LEFT for this sprite.")]
    public bool positiveScaleFacesRight = false;   // your flytrap art faces left at +scale

    [Header("Debug")]
    public bool log;

    // per-agent cooldowns
    static readonly Dictionary<int, float> _nextAllowed = new();

    // Prefer Perception2D; fall back to a tiny physics query so Score>0 works even before sensing
    Transform FindTarget(Blackboard bb, Perception2D s)
    {
        var t = s ? s.FindBestTarget() : null;
        if (t) return t;

        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer >= 0)
        {
            var hits = Physics2D.OverlapCircleAll(bb.self.position, biteRange, 1 << playerLayer);
            if (hits.Length > 0) return hits[0].transform;
        }
        return null;
    }

    public override float Score(Blackboard bb, Perception2D s)
    {
        var t = FindTarget(bb, s);
        if (!t) return 0f;

        int id = bb.self.GetInstanceID();
        float now = Time.time;

        // cooldown check
        bool ready = !_nextAllowed.TryGetValue(id, out float coolUntil) || now >= coolUntil;
        if (!ready) return 0f;

        // distance gate
        Vector2 to = (Vector2)t.position - (Vector2)bb.self.position;
        if (to.sqrMagnitude > biteRange * biteRange) return 0f;

        // facing gate
        float sign = Mathf.Sign(bb.self.localScale.x == 0 ? 1f : bb.self.localScale.x);
        if (!positiveScaleFacesRight) sign = -sign;
        Vector2 fwd = (sign >= 0f) ? Vector2.right : Vector2.left;

        float cosHalf = Mathf.Cos(0.5f * arcDegrees * Mathf.Deg2Rad);
        float align = Vector2.Dot(to.normalized, fwd); // was "dot"
        if (align < cosHalf) return 0f;

        float result = baseWeight + 5f;

        if (log)
        {
            float dist = to.magnitude;
            Debug.Log(
                $"[Snap:{name}] RETURN score={result:0.00} (dist={dist:F2} dot={align:F2} need>={cosHalf:F2} ready={ready})",
                this
            );
        }

        return result;
    }




    public override void Enter(Blackboard bb, Perception2D s)
    {
        if (log) Debug.Log($"[Snap:{name}] ENTER", this);

        // Optional: first bite instantly on first enter
        var anim = bb.self.GetComponent<Animator>();
        if (anim && !string.IsNullOrEmpty(attackTrigger))
            anim.SetTrigger(attackTrigger);

        // Start cooldown so we don't spam a second bite immediately
        _nextAllowed[bb.self.GetInstanceID()] = Time.time + cooldown;
    }


    public override void Tick(float dt, Blackboard bb, Perception2D s)
    {
        // Find a target (Perception2D first; tiny fallback if needed)
        var t = s ? s.FindBestTarget() : null;
        if (!t)
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0)
            {
                var hits = Physics2D.OverlapCircleAll(bb.self.position, biteRange, 1 << playerLayer);
                if (hits.Length > 0) t = hits[0].transform;
            }
        }
        if (!t) return;

        // Face target
        float dir = Mathf.Sign(t.position.x - bb.self.position.x); // +1 right, -1 left
        if (!positiveScaleFacesRight) dir = -dir;
        var ls = bb.self.localScale;
        float wanted = Mathf.Abs(ls.x) * dir;
        if (!Mathf.Approximately(ls.x, wanted))
            bb.self.localScale = new Vector3(wanted, ls.y, ls.z);

        // Cooldown gate
        int id = bb.self.GetInstanceID();
        float now = Time.time;
        bool ready = !_nextAllowed.TryGetValue(id, out float coolUntil) || now >= coolUntil;
        if (!ready) return;

        // Distance + arc gates (same as Score)
        Vector2 to = (Vector2)t.position - (Vector2)bb.self.position;
        if (to.sqrMagnitude > biteRange * biteRange) return;

        float sign = Mathf.Sign(bb.self.localScale.x == 0 ? 1f : bb.self.localScale.x);
        if (!positiveScaleFacesRight) sign = -sign;
        Vector2 fwd = (sign >= 0f) ? Vector2.right : Vector2.left;

        float cosHalf = Mathf.Cos(0.5f * arcDegrees * Mathf.Deg2Rad);
        float align = Vector2.Dot(to.normalized, fwd);
        if (align < cosHalf) return;

        // Fire bite + reset cooldown
        var anim = bb.self.GetComponent<Animator>();
        if (anim && !string.IsNullOrEmpty(attackTrigger))
            anim.SetTrigger(attackTrigger);

        _nextAllowed[id] = now + cooldown;

        if (log)
            Debug.Log($"[Snap:{name}] FIRE (dist={to.magnitude:F2}, align={align:F2}) next={_nextAllowed[id]:F2}", this);
    }



}
