using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Transform overridePoint; // optional: leave empty to use this object's transform

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var cs = other.GetComponent<CharacterScript>();
        if (cs == null) return;

        cs.SetCheckpoint(overridePoint ? overridePoint : transform);
    }

    // (optional) draw a gizmo so you can see it in-scene
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((overridePoint ? overridePoint.position : transform.position), 0.25f);
    }
}
