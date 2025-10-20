using UnityEngine;

[CreateAssetMenu(menuName = "Netherkin/AI Actions/Hostile Fallback")]
public class HostileFallback : AIAction
{
    [Header("Attack")]
    public float attackRange = 1.2f;
    public float attackCooldown = 0.8f;
    public int damage = 1;
    public float knockback = 2f;

    [Tooltip("Animator Trigger to fire when we attack (leave empty to skip animation).")]
    public string attackTrigger = "Attack";

    float _cooldown;

    public override float Score(Blackboard bb, Perception2D s)
    {
        // Only compete when we actually see a target
        return s.FindBestTarget() ? baseWeight : 0f;
    }

    public override void Tick(float dt, Blackboard bb, Perception2D s)
    {
        _cooldown -= dt;

        var target = s.FindBestTarget();
        if (!target) return;

        // Face the target
        var pos = (Vector2)bb.self.position;
        var tpos = (Vector2)target.position;
        var dirX = Mathf.Sign(tpos.x - pos.x);
        if (dirX != 0)
        {
            var ls = bb.self.localScale;
            bb.self.localScale = new Vector3(Mathf.Abs(ls.x) * dirX, ls.y, ls.z);
        }

        // Only attack when in range and cooldown ready
        if (_cooldown > 0f) return;
        float dist = Vector2.Distance(pos, tpos);
        if (dist > attackRange) return;

        // (Optional) play attack anim
        if (!string.IsNullOrEmpty(attackTrigger))
        {
            var anim = bb.self.GetComponent<Animator>();
            if (anim) anim.SetTrigger(attackTrigger); // NOTE: your controller must have this Trigger
        }

        // Apply damage directly to Health2D
        var hp = target.GetComponentInParent<Health2D>();
        if (hp)
        {
            Vector2 kb = (tpos - pos).normalized * knockback;
            hp.TakeHit(damage, kb);
        }

        _cooldown = attackCooldown;
    }
}
