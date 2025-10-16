using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SpawnerV2 — one spawner that can:
/// • Spawn multiple species ("Entries").
/// • For each species: use Spawn Points (exact pegs) OR Local Areas (multiple BoxCollider2D zones).
/// • Optional weights per Local Area (higher = chosen more often).
/// • Optional clustering (pick an anchor, sprinkle neighbors around it).
/// • Global Area Box can be left empty if every Entry uses Local Areas.
/// Designer notes:
/// - "Min/Max Count" is the TOTAL for that species across all its areas.
/// - If any Spawn Points are assigned for an Entry, that Entry ignores areas and uses those pegs.
/// - BoxCollider2D authoring areas should be IsTrigger = ON (or disable the component) so they never block gameplay.
/// </summary>
public class SpawnerV2 : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        [Header("What")]
        [Tooltip("Prefab to spawn (the 'species').")]
        public GameObject prefab;

        [Tooltip("Total count across ALL areas (min..max).")]
        public int minCount = 2;
        public int maxCount = 5;

        [Header("Exact pegs (optional)")]
        [Tooltip("If you drop any Transforms here, this Entry uses THESE positions instead of areas.")]
        public Transform[] spawnPoints;

        [Header("Local Areas (optional)")]
        [Tooltip("Per-Entry zones this species can appear in. If empty, we fall back to the global Area Box, then center/halfExtents.")]
        public BoxCollider2D[] localAreas;

        [Tooltip("Weights for localAreas (same length as localAreas; leave empty for equal weights). Higher weight = chosen more often.")]
        public float[] localAreaWeights;

        [Header("Clustering (optional)")]
        [Tooltip("If ON: we pick a cluster anchor in one area and sprinkle neighbors around it.")]
        public bool clustered = false;

        [Tooltip("How many per cluster (min..max). Overall total still respects min/max Count above.")]
        public int clusterSizeMin = 3;
        public int clusterSizeMax = 6;

        [Tooltip("Radius around the anchor to sprinkle cluster members.")]
        public float clusterRadius = 1.5f;
    }

    [Header("Authoring Area (either BoxCollider2D OR center/halfExtents)")]
    [Tooltip("Optional global area for random sampling when an Entry has no Local Areas/Spawn Points.")]
    public BoxCollider2D areaBox; // leave empty if all Entries use Local Areas
    [Tooltip("Used only if no BoxCollider2D is provided anywhere.")]
    public Vector2 center = Vector2.zero;
    [Tooltip("Used only if no BoxCollider2D is provided anywhere.")]
    public Vector2 halfExtents = new Vector2(12, 6);

    [Header("Camera Exclusion (avoid pop-in on first screen)")]
    [Tooltip("If ON, we avoid spawning inside the current camera rect (expanded by padding).")]
    public bool excludeCameraView = true;
    [Tooltip("Grow the camera rect by this amount when excluding.")]
    public float exclusionPadding = 0.5f;

    [Header("Placement")]
    [Tooltip("We raycast DOWN on this layer to 'land' spawns on the ground.")]
    public LayerMask groundMask;
    [Tooltip("How far down we search for ground when snapping.")]
    public float groundSnapMax = 10f;
    [Tooltip("We refuse positions that would overlap colliders on these layers (keeps spacing).")]
    public LayerMask collisionMask;
    [Tooltip("Minimum spacing from other colliders when collisionMask is set.")]
    public float avoidRadius = 0.3f;
    [Tooltip("Max tries per item to find a legal placement.")]
    public int maxPlacementTries = 15;

    [Header("What to spawn (one row per species)")]
    public Entry[] entries;

    [Header("Per-phase multipliers (set all to 1 if you don’t want quantity changes)")]
    public float dayMult = 1f, twilightMult = 1f, nightMult = 1f;

    // ---------- Lifecycle ----------
    void Start() => DoSpawn();

    /// <summary>Public helper for doors/tunnels: wipe children and respawn fresh.</summary>
    public void ClearAndRespawn()
    {
        for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
        DoSpawn();
    }

    // ---------- Core ----------
    // ---------- Core ----------
    void DoSpawn()
    {
        // Quantity multiplier by time-of-day (keep at 1 to leave counts alone)
        float mult = (StepTime.Phase == DayPhase.Day) ? dayMult :
                    (StepTime.Phase == DayPhase.Twilight) ? twilightMult : nightMult;

        // Precompute camera rect once
        Rect camRect = default;
        if (excludeCameraView && Camera.main) camRect = GetOrthoCameraWorldRect(Camera.main, exclusionPadding);

        // Global area fallback rect
        Rect globalAreaRect = GetAreaRect();

        foreach (var e in entries)
        {
            if (!e?.prefab) continue;

            int total = Mathf.RoundToInt(Random.Range(e.minCount, e.maxCount + 1) * mult);
            if (total <= 0) continue;

            // Cluster bookkeeping (one cluster at a time)
            Vector2 clusterAnchor = Vector2.zero;
            int clusterLeft = 0;

            for (int i = 0; i < total; i++)
            {
                bool placed = false;

                for (int tries = 0; tries < maxPlacementTries && !placed; tries++)
                {
                    Vector2 pos;

                    // 1) Spawn Points override everything (exact pegs)
                    if (e.spawnPoints != null && e.spawnPoints.Length > 0)
                    {
                        var p = e.spawnPoints[Random.Range(0, e.spawnPoints.Length)];
                        pos = p ? (Vector2)p.position : (Vector2)transform.position;
                    }
                    else
                    {
                        // 2) Choose a position from Local Areas or Global Area
                        if (e.clustered)
                        {
                            // If starting a new cluster, pick a fresh anchor inside an area
                            if (clusterLeft <= 0)
                            {
                                int clusterSize = Mathf.Max(1, Random.Range(e.clusterSizeMin, e.clusterSizeMax + 1));
                                clusterLeft = clusterSize;
                                clusterAnchor = SampleFromLocalAreasOrGlobal(e, globalAreaRect);
                            }
                            // Sprinkle around the anchor
                            pos = clusterAnchor + Random.insideUnitCircle * e.clusterRadius;
                            clusterLeft--;
                        }
                        else
                        {
                            // Simple random pick
                            pos = SampleFromLocalAreasOrGlobal(e, globalAreaRect);
                        }
                    }

                    // ----------------------------------------------------------------
                    // Ground snap (feet-aware) with CircleCast fallback + diagnostics
                    // ----------------------------------------------------------------
                    if (groundMask.value != 0)
                    {
                        // Cast from slightly above the candidate in case we're inside thin tiles
                        Vector2 origin = pos + Vector2.up * 0.5f;

                        // 1) Try a precise ray straight down
                        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundSnapMax, groundMask);

                        // 2) If we missed (thin edges / tiny gaps), try a small-radius circle cast
                        if (!hit.collider)
                            hit = Physics2D.CircleCast(origin, 0.08f, Vector2.down, groundSnapMax, groundMask);

                        if (hit.collider)
                        {
                            // Find a SpawnFootHint child on the prefab (put it at the feet under the prefab root)
                            var hint = e.prefab.GetComponentInChildren<SpawnFootHint>(true);

                            // Distance from prefab root to the feet
                            float footToRootY;
                            if (hint != null)
                            {
                                // Measure hint relative to the prefab root so nesting/scales don't matter
                                float localYAtRoot = e.prefab.transform.InverseTransformPoint(hint.transform.position).y;
                                footToRootY = -localYAtRoot; // positive lift from root up to feet
                            }
                            else
                            {
                                // Fallback: sprite half-height (handles scaled child renderers)
                                var sr = e.prefab.GetComponentInChildren<SpriteRenderer>();
                                footToRootY = (sr != null && sr.sprite != null)
                                    ? sr.sprite.bounds.extents.y * Mathf.Abs(sr.transform.lossyScale.y)
                                    : 0f;
                            }

                            // Flavor bury/poke per clone
                            float minY = 0f, maxY = 0f;
                            if (hint != null) { minY = hint.minYOffset; maxY = hint.maxYOffset; }
                            float yOff = (minY <= maxY) ? Random.Range(minY, maxY) : 0f;

                            // Final: feet on ground + tiny safety lift + flavor
                            pos = hit.point + Vector2.up * (0.02f + footToRootY + yOff);

                            // debug: green when we snapped
                            Debug.DrawRay(origin, Vector2.down * (hit.distance), Color.green, 0.25f);
                        }
                        else
                        {
                            // debug: red ray shows we didn’t find ground → the 'too-high' cases
                            Debug.DrawRay(origin, Vector2.down * groundSnapMax, Color.red, 0.25f);
                            // Uncomment for logs if you need:
                            // Debug.LogWarning($"[SpawnerV2] No ground hit under {e.prefab.name} at {pos}. Check groundMask & colliders.");
                        }
                    }

                    // ----------------------------------------------------------------

                    // Optional: spacing check
                    if (collisionMask.value != 0 && Physics2D.OverlapCircle(pos, avoidRadius, collisionMask))
                    {
                        continue; // too close → retry
                    }

                    // Optional: camera exclusion (only meaningful for area/cluster samples)
                    if (excludeCameraView && Camera.main && camRect.Contains(pos))
                    {
                        continue; // inside camera → retry
                    }

                    // Finally place
                    Instantiate(e.prefab, pos, Quaternion.identity, transform);
                    placed = true;
                }
            }
        }
    }


    // ---------- Sampling helpers ----------
    /// <summary>Pick a random point inside a BoxCollider2D's world AABB (assumes no rotation).</summary>
    Vector2 RandomPointIn(BoxCollider2D box)
    {
        var b = box.bounds;
        return new Vector2(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.max.y));
    }

    /// <summary>Fallback when no collider is provided: pick inside a simple rect.</summary>
    Vector2 RandomPointInRect(Rect r)
    {
        return new Vector2(Random.Range(r.xMin, r.xMax), Random.Range(r.yMin, r.yMax));
    }

    /// <summary>
    /// Returns a sample position for this Entry by:
    /// - choosing one of its Local Areas using weights (if any), or
    /// - using the global area, or
    /// - falling back to center/halfExtents.
    /// </summary>
    Vector2 SampleFromLocalAreasOrGlobal(Entry e, Rect globalAreaRect)
    {
        // Prefer per-entry Local Areas if present
        if (e.localAreas != null && e.localAreas.Length > 0)
        {
            int idx = WeightedPickIndex(e.localAreaWeights, e.localAreas.Length);
            // Guard against null element
            for (int k = 0; k < e.localAreas.Length; k++)
            {
                var area = e.localAreas[(idx + k) % e.localAreas.Length];
                if (area) return RandomPointIn(area);
            }
        }
        // Else, use global Area Box if assigned
        if (areaBox) return RandomPointIn(areaBox);
        // Else, center/halfExtents
        return RandomPointInRect(globalAreaRect);
    }

    /// <summary>
    /// Pick an index [0..count-1] using weights. If weights are null/short/zero → uniform random.
    /// Negative weights are treated as zero. Guaranteed to return a value.
    /// </summary>
    int WeightedPickIndex(float[] weights, int count)
    {
        if (count <= 0) return 0;
        if (weights == null || weights.Length == 0)
        {
            return Random.Range(0, count);
        }
        float total = 0f;
        for (int i = 0; i < count; i++)
        {
            float w = (i < weights.Length) ? Mathf.Max(0f, weights[i]) : 1f; // default 1 if missing
            total += w;
        }
        if (total <= 0f) return Random.Range(0, count);
        float r = Random.value * total, acc = 0f;
        for (int i = 0; i < count; i++)
        {
            float w = (i < weights.Length) ? Mathf.Max(0f, weights[i]) : 1f;
            acc += w;
            if (r <= acc) return i;
        }
        return count - 1; // safety
    }

    // ---------- Area + Camera helpers ----------
    /// <summary>Global authored area as a Rect in world coords (used as fallback).</summary>
    Rect GetAreaRect()
    {
        if (areaBox)
        {
            var b = areaBox.bounds;
            return Rect.MinMaxRect(b.min.x, b.min.y, b.max.x, b.max.y);
        }
        var min = center - halfExtents;
        var max = center + halfExtents;
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    /// <summary>Orthographic camera world rect, with padding. Safe fallback for perspective cameras.</summary>
    static Rect GetOrthoCameraWorldRect(Camera cam, float pad)
    {
        if (!cam.orthographic)
        {
            var bl3 = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
            var tr3 = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
            var min = new Vector2(Mathf.Min(bl3.x, tr3.x) - pad, Mathf.Min(bl3.y, tr3.y) - pad);
            var max = new Vector2(Mathf.Max(bl3.x, tr3.x) + pad, Mathf.Max(bl3.y, tr3.y) + pad);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        var c = (Vector2)cam.transform.position;
        var minO = new Vector2(c.x - halfW - pad, c.y - halfH - pad);
        var maxO = new Vector2(c.x + halfW + pad, c.y + halfH + pad);
        return Rect.MinMaxRect(minO.x, minO.y, maxO.x, maxO.y);
    }

    // ---------- Gizmos ----------
    void OnDrawGizmosSelected()
    {
        // Global spawn area
        Gizmos.color = Color.yellow;
        DrawRect(GetAreaRect());

        // Camera exclusion
        if (excludeCameraView && Camera.main)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
            DrawRect(GetOrthoCameraWorldRect(Camera.main, exclusionPadding));
        }

        // Per-entry Local Areas (tinted by entry index)
        if (entries != null)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                var e = entries[i];
                if (e?.localAreas == null) continue;
                Color c = Color.HSVToRGB((i * 0.2f) % 1f, 0.8f, 1f); c.a = 0.85f;
                Gizmos.color = c;
                foreach (var a in e.localAreas)
                {
                    if (!a) continue;
                    var b = a.bounds;
                    DrawRect(Rect.MinMaxRect(b.min.x, b.min.y, b.max.x, b.max.y));
                }
            }
        }
    }
    static void DrawRect(Rect r)
    {
        var a = new Vector3(r.xMin, r.yMin, 0);
        var b = new Vector3(r.xMax, r.yMin, 0);
        var c = new Vector3(r.xMax, r.yMax, 0);
        var d = new Vector3(r.xMin, r.yMax, 0);
        Gizmos.DrawLine(a, b); Gizmos.DrawLine(b, c); Gizmos.DrawLine(c, d); Gizmos.DrawLine(d, a);
    }
}
