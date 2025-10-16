using UnityEngine;

// Finds the "feet" of a prefab so the spawner can land it on the ground.
// Put this on a child Transform named "SpawnFoot" under the prefab root.
public class SpawnFootHint : MonoBehaviour
{
    [Tooltip("Random bury/poke around the ground contact (world units).")]
    public float minYOffset = -0.15f;   // bury a little
    public float maxYOffset = 0.10f;   // or poke a little

    [Tooltip("If true, final lift follows the surface normal up to this angle.")]
    public bool alignToSlope = false;

    [Range(0, 60)]
    public float slopeLimit = 35f;

#if UNITY_EDITOR
    // Visualize the foot and the allowed Y range in the Scene view
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position, 0.05f);

        Vector3 a = transform.position + Vector3.up * minYOffset;
        Vector3 b = transform.position + Vector3.up * maxYOffset;
        Gizmos.DrawLine(a, b);
    }
#endif
}
