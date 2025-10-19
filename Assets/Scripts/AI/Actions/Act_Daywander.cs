using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Netherkin/AI Actions/Day Wander")]


//Adjustment fields



public class Act_DayWander : AIAction
{
    [Header("Phase weighting (multiplies baseWeight)")]
    public float dayMul = 1.0f;
    public float twilightMul = 0.7f;
    public float nightMul = 0.3f;

    [Header("Wander motion")]
    public float wanderRadius = 2.5f;     // how far from home/x to sway
    public float wanderSpeed = 0.6f;     // how fast the sway target moves
    public float moveSpeed = 1.8f;     // actual horizontal move speed

    [Header("Animation (optional)")]
    public string SunflowerwalkBool = "Walking";

    public override float Score(Blackboard bb, Perception2D s)
    {
        float mul = bb.phase == DayPhase.Day ? dayMul
                  : bb.phase == DayPhase.Twilight ? twilightMul
                  : nightMul;
        float score = baseWeight * mul;

        // If a target is very close, let other actions (like attack/flee) win.
        if (s.FindBestTarget() != null) score -= 0.5f;

        return Mathf.Max(0f, score);
    }

    public override void Enter(Blackboard bb, Perception2D s)
    {
        var anim = bb.self.GetComponent<Animator>();
        if (anim && !string.IsNullOrEmpty(SunflowerwalkBool)) anim.SetBool(SunflowerwalkBool, true);
    }

    public override void Tick(float dt, Blackboard bb, Perception2D s)
    {
        // Stateless “random” sway using PerlinNoise keyed by instance id
        int seed = bb.self.GetInstanceID();
        float t = Time.time * wanderSpeed + seed * 0.123f;
        float off = (Mathf.PerlinNoise(t, 0.37f) - 0.5f) * 2f; // [-1..1]

        float targetX = bb.home.x + off * wanderRadius;

        var pos = bb.self.position;
        float nextX = Mathf.MoveTowards(pos.x, targetX, moveSpeed * dt);
        bb.self.position = new Vector3(nextX, pos.y, pos.z);

        // Face movement direction
        float dir = Mathf.Sign(targetX - pos.x);
        if (dir != 0)
        {
            var ls = bb.self.localScale;
            bb.self.localScale = new Vector3(Mathf.Abs(ls.x) * dir, ls.y, ls.z);
        }
    }

    public override void Exit(Blackboard bb, Perception2D s)
    {
        var anim = bb.self.GetComponent<Animator>();
        if (anim && !string.IsNullOrEmpty(SunflowerwalkBool)) anim.SetBool(SunflowerwalkBool, false);
    }
}
