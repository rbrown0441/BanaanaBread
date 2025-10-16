using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Netherkin/AI Actions/Frog Raid Sunflowers")]
public class Act_RaidSunflowers : AIAction
{
    [Header("Weights")]
    public float dayMul = 1.2f;
    public float twilightMul = 1.0f;
    public float nightMul = 0.2f;

    [Header("Chase")]
    public float runSpeed = 3.0f;
    public float stopRange = 0.8f;     // start attack inside this

    [Header("Animator")]
    public string runBool = "Walking"; // reuse walk bool if you like
    public string attackTrigger = "Attack";

    public override float Score(Blackboard bb, Perception2D s)
    {
        // Needs a target to make sense
        var t = s.FindBestTarget();
        if (t == null) return 0f;

        // Only care outside of the frog’s own species
        if (t.gameObject.layer == bb.self.gameObject.layer) return 0f;

        float mul = bb.phase == DayPhase.Day ? dayMul
                  : bb.phase == DayPhase.Twilight ? twilightMul
                  : nightMul;

        // Prefer closer targets (inverse of distance^0.5), capped
        float d = Vector2.Distance(bb.self.position, t.position);
        float proximity = 1f / Mathf.Max(0.5f, Mathf.Sqrt(d)); // 0.0..~1.4

        float score = baseWeight * mul + proximity;
        return Mathf.Max(0f, score);
    }

    public override void Enter(Blackboard bb, Perception2D s)
    {
        var anim = bb.self.GetComponent<Animator>();
        if (anim && !string.IsNullOrEmpty(runBool)) anim.SetBool(runBool, true);
    }

    public override void Tick(float dt, Blackboard bb, Perception2D s)
    {
        var target = s.FindBestTarget();
        var anim = bb.self.GetComponent<Animator>();

        if (!target)
        {
            if (anim && !string.IsNullOrEmpty(runBool)) anim.SetBool(runBool, false);
            return;
        }

        // Horizontal chase (keep current y)
        var pos = bb.self.position;
        float dir = Mathf.Sign(target.position.x - pos.x);

        // Face the target
        var ls = bb.self.localScale;
        bb.self.localScale = new Vector3(Mathf.Abs(ls.x) * (dir == 0 ? 1 : dir), ls.y, ls.z);

        // Move toward x until in range, then trigger attack
        float dist = Mathf.Abs(target.position.x - pos.x);
        if (dist > stopRange)
        {
            float nextX = pos.x + dir * runSpeed * dt;
            bb.self.position = new Vector3(nextX, pos.y, pos.z);
            if (anim && !string.IsNullOrEmpty(runBool)) anim.SetBool(runBool, true);
        }
        else
        {
            if (anim && !string.IsNullOrEmpty(attackTrigger)) anim.SetTrigger(attackTrigger);
            if (anim && !string.IsNullOrEmpty(runBool)) anim.SetBool(runBool, false);
        }
    }

    public override void Exit(Blackboard bb, Perception2D s)
    {
        var anim = bb.self.GetComponent<Animator>();
        if (anim && !string.IsNullOrEmpty(runBool)) anim.SetBool(runBool, false);
    }
}
