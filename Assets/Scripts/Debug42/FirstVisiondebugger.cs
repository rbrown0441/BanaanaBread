using UnityEngine;

public class SenseFirstPing : MonoBehaviour
{
    public Perception2D senses;
    [Tooltip("Only 1 out of every N spawns will log/draw")]
    public int sampleEvery = 8;
    public bool drawLOS = true;

    static int _spawnCounter;
    bool _enabled;
    Transform last;
    float nextLog;

    void Awake()
    {
        _enabled = (++_spawnCounter % sampleEvery) == 0;
        if (!senses) senses = GetComponent<Perception2D>();
    }

    void Update()
    {
        if (!_enabled || !senses) return;

        // mild throttle
        if (Time.time < nextLog) return;
        nextLog = Time.time + 0.25f;

        var t = senses.FindBestTarget();
        bool hadAny = last != null, hasAny = t != null;

        if (!hadAny && hasAny)
        {
            Debug.Log($"[{name}] FIRST VISION: {t.name} at {Vector2.Distance(transform.position, t.position):0.00}m");
        }
        if (drawLOS && t)
        {
            Debug.DrawLine(transform.position, t.position, Color.red, 0.25f);
        }
        last = t;
    }
}
