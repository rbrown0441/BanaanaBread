

/******************************************************************
 || Perception2D (begin) ||
Finds nearby targets (OverlapCircleNonAlloc, filters by FOV,
optional LOS ray against obstacleMask, with gizmo visualisation.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Perception2D : MonoBehaviour
{
    [Header("Ranges")]
    [SerializeField] float radius = 6f; // how far they can sense
    [SerializeField] float fovDeg = 110f; // field of view in degrees (centered on facing)

    [Header("Layers")]
    [SerializeField] LayerMask targetMask;   // enemies/actors/player
    [SerializeField] LayerMask obstacleMask; // solid world for LOS
    [SerializeField] LayerMask waterMask;


    [Header("Buffers")]
    [SerializeField] int maxHits = 16; //size of reusable buffer

    // Reused buffer to avoid per-frame allocations (GC)
    Collider2D[] _hits;


    void Awake()
    {
        //debug log
        {
            Debug.Log($"Perception2D Awake on {name}");
            _hits = new Collider2D[Mathf.Max(1, maxHits)];
        }
        // Allocate once. Avoid Garbage Collector churn
        _hits = new Collider2D[Mathf.Max(1, maxHits)];

    }

// Find the best target closest in FOV with LOS
public Transform FindBestTarget()
{
    int count = Physics2D.OverlapCircleNonAlloc(transform.position, radius, _hits, targetMask);

        Transform best = null;
        float bestD2 = float.PositiveInfinity;

        for (int i = 0; i < count; i++)
        { 
            var col = _hits[i];
            if (!col) continue;
            Transform t = col.transform;
            if (t == transform) continue;                 // no self
            if (t.gameObject.layer == gameObject.layer) continue; // same species/layer, skip


           
            Vector2 to = (Vector2)t.position - (Vector2)transform.position;

            if (!InFOV(to)) continue;
            if (!HasLOS((Vector2)transform.position, (Vector2)t.position)) continue;

            float d2 = to.sqrMagnitude;
            if (d2 < bestD2) { bestD2 = d2; best = t; }

        }
        return best;
    
    }

    //Is a vector inside our field-of-view?
    bool InFOV(Vector2 to)
    {
        if (to.sqrMagnitude < 0.0001f) return true; // standing on us

        // Determine facing from localScale.x (negative = facing left)
        float face = Mathf.Sign(transform.localScale.x == 0 ? 1f : transform.localScale.x);
        Vector2 fwd = (face >= 0f) ? Vector2.right : Vector2.left;

        float cosHalf = Mathf.Cos(0.5f * fovDeg * Mathf.Deg2Rad);
        return Vector2.Dot(to.normalized, fwd) > cosHalf;
    }

    // Is there a clear line between A and B (no walls)
    bool HasLOS(Vector2 a, Vector2 b)
    {
        Vector2 dir = b - a;
        float dist = dir.magnitude;
        if (dist <= 0.001f) return true;
        // Raycast only against obstacles 
        return !Physics2D.Raycast(a, dir / dist, dist, obstacleMask);
    }

    // Scene view debugging - draw radius and FOV cone
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);

        Vector3 p = transform.position;
        Vector3 r = transform.right * radius;

        Quaternion qA = Quaternion.Euler(0, 0, +fovDeg / 2f);
        Quaternion qB = Quaternion.Euler(0, 0, -fovDeg / 2f);

        Gizmos.DrawLine(p, p + qA * r);
        Gizmos.DrawLine(p, p + qB * r);

    }


}

        
