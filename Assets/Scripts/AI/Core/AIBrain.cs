/***********************************************************
 || NETHERKIN — AIBrain (Utility Selector) ||
 Decides "what to do now?" by scoring a list of AIAction
 assets (ScriptableObjects). Highest score runs. Each
 frame, the current action's Tick() executes.

 - LOD thinking (near/mid/far) to save CPU
 - Visibility-aware (think slower when offscreen)
 - Blackboard per-agent memory will likely extend to two 
 - seperate scripts for extended and long term memory
 - Perception2D injected for sensory queries
 - Super-commented so the team can study extend perfect
************************************************************/


#region Using
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
#endregion

/************************************************************
 * AIAction (base)
 * A pluggable decision + behavior module.
 * - ScriptableObject: lives as a project asset, reused by many agents.
 * - IMPORTANT: keep per-agent, changing data OUT of the action
 *   (store that on the agent’s Blackboard). Actions should be
 *   mostly stateless / read-only config + logic.
 ************************************************************/

#region AIActionBase
public abstract class AIAction : ScriptableObject
{
    [Header("Scoring")]
    [Tooltip("Base score before action-specific math. Larger means more likely.")]
    [Range(0f, 10f)] public float baseWeight = 1f;

    /// <summary>
    /// Return a score >= 0. Higher wins. 0 means "never".
    /// Read bb (memory) and s (senses) — do NOT mutate agent state here.
    /// Keep it CHEAP: this is called for every candidate at think time.
    /// </summary>
    public virtual float Score(Blackboard bb, Perception2D s) => baseWeight;

    /// <summary>
    /// Called once when this action becomes current.
    /// Side effects welcome: set animator flags, reserve targets, publish events…
    /// </summary>
    public virtual void Enter(Blackboard bb, Perception2D s) { }

    /// <summary>
    /// Called every Update() while this action remains selected.
    /// Use dt for time-based motion / timers.
    /// </summary>
    public virtual void Tick(float dt, Blackboard bb, Perception2D s) { }

    /// <summary>
    /// Called once when a different action is selected.
    /// Undo transient setup (e.g., stop VFX, clear animator flags).
    /// </summary>
    public virtual void Exit(Blackboard bb, Perception2D s) { }
}
#endregion

/************************************************************
 * Blackboard (minimal)
 * Per-agent memory & references. Extend as needed.
 * Think of this like a tiny “context bag” passed to actions.
 ************************************************************/
#region Blackboard
[System.Serializable]
public class Blackboard
{
    // Who am I?
    public Transform self;

    // Who do I care about right now?
    public Transform player;   // simple: first thing tagged "Player" (replace with a proper tracker later)
    public Transform target;   // what current action is focusing on (prey, threat, goal)

    // World anchors
    public Vector3 home;       // spawn/home pos (return here when lost)
    public DayPhase phase;     // StepTime.Phase snapshot (can mirror the static value each think)

    // Personality knobs (tune per prefab in the Inspector of AIBrain)
    public float courage;      // higher = more likely to fight
    public float curiosity;    // higher = more likely to investigate

    // Cheap "Init" to seed fields
    public void Init(Transform t)
    {
        self = t;
        home = t.position;
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) player = go.transform;
        }
    }
}
#endregion

/************************************************************
 * AIBrain (MonoBehaviour)
 * 1) Periodically (not every frame) scores all actions.
 * 2) Picks highest score; switches if different.
 * 3) Ticks the current action every frame.
 *
 * LOD thinking:
 *   - near:   think often (responsive)
 *   - mid:    think slower
 *   - far:    think rarely
 *   - offscreen: optional extra slowdown
 ************************************************************/

#region AIBrain

public class AIBrain : MonoBehaviour
{   
    
    [Header("Injected & Data")]
    [Tooltip("Sensor helper for vision/LOS queries. If null, we GetComponent<Perception2D>().")]
    [SerializeField] Perception2D senses;

    [Tooltip("Pluggable action assets. Highest Score() wins. Order doesn't matter.")]
    [SerializeField] List<AIAction> actions = new List<AIAction>();

    [Tooltip("Animator used by actions (optional). If null we auto-grab.")]
    [SerializeField] Animator anim;


    [Header("Temperament (copied into Blackboard on Awake)")]
    [Tooltip("Higher = less likely to flee.")]
    [SerializeField] float courage = 0.5f;
    [Tooltip("Higher = more likely to investigate / wander.")]
    [SerializeField] float curiosity = 0.5f;

    [Header("Think Cadence (seconds)")]
    [Tooltip("How often to re-score actions when the player is close.")]
    [SerializeField] float nearThink = 0.20f;
    [Tooltip("How often when mid distance.")]
    [SerializeField] float midThink = 0.60f;
    [Tooltip("How often when far.")]
    [SerializeField] float farThink = 2.00f;
    [Tooltip("Random jitter multiplier range applied to chosen interval (prevents sync).")]
    [SerializeField] Vector2 jitter = new Vector2(0.85f, 1.15f);

    [Header("LOD Distances (world units, squared compare)")]
    [Tooltip("<= this: near. Use squared distance for cheap compare.")]
    [SerializeField] float nearDist = 10f;     // 10 units
    [Tooltip("<= this and > near: mid.")]
    [SerializeField] float midDist = 30f;     // 30 units
    // > mid: far

    [Header("Visibility Optimization")]
    [Tooltip("If true, think even slower when not visible by any camera.")]
    [SerializeField] bool slowWhenOffscreen = true;
    [Tooltip("Multiplier applied to interval when offscreen.")]
    [SerializeField] float offscreenMul = 1.5f;

    [Header("Debug")]
    [Tooltip("Log every selection with scores in the Console.")]
    [SerializeField] bool logDecisions = false;


    // Public blackboard (visible in Inspector for debugging)
    [HideInInspector] public Blackboard bb = new Blackboard();

    // --- runtime ---
    AIAction _current;
    float _nextThinkAt;
    bool _isVisible; // Unity will toggle this via OnBecameVisible/Invisible

    // Expose a read-only view of the current action (e.g., for UI badges)
    public AIAction CurrentAction => _current;

    // ?????????????????????????????????????????????????????????????????????????????

    void Awake()
    {
        if (!senses) senses = GetComponent<Perception2D>();
        // if field is null get sibling
        if (!anim) anim = GetComponent<Animator>();

        // Seed blackboard
        bb.Init(transform);
        bb.courage = courage;
        bb.curiosity = curiosity;
    }

    
    void OnEnable()
    {
        // Pick an initial action immediately
        _nextThinkAt = 0f;
    }

    // Unity calls these when the Renderer becomes (in)visible by any camera
    void OnBecameVisible()   { _isVisible = true; }
    void OnBecameInvisible() { _isVisible = false; }

    void Update()
    {
        // 1) Determine how often we should "think" (LOD by squared distance)
        float dist2 = 0f;
        if (bb.player)
            dist2 = (bb.player.position - transform.position).sqrMagnitude;

        // choose interval based on squared thresholds (avoid expensive sqrt)
        float near2 = nearDist * nearDist;
        float mid2  = midDist  * midDist;

        float interval = dist2 <= near2 ? nearThink
                         : dist2 <= mid2 ? midThink
                         :                  farThink;

        if (slowWhenOffscreen && !_isVisible) interval *= offscreenMul;

        // 2) Time to re-score?
        if (Time.time >= _nextThinkAt)
        {
            SelectAction();

            // Randomize next think time a little to avoid many agents syncing
            float j = Random.Range(jitter.x, jitter.y);
            _nextThinkAt = Time.time + interval * j;
        }

        // 3) Drive the current action every frame
        // OLD
        // _current?.Tick(Time.deltaTime, bb, senses);
        var act = _current;
        if (act != null) act.Tick(Time.deltaTime, bb, senses);
    }

    /// <summary>
    /// Scores all actions and switches if a different one wins.
    /// </summary>
    void SelectAction()
    {
        if (actions == null || actions.Count == 0) return;

        // Keep the StepTime phase mirrored on the blackboard for actions to use
        bb.phase = StepTime.Phase; // safe: static DayPhase property

        float bestScore = -1f;
        AIAction pick   = null;

        // Evaluate each candidate
        for (int i = 0; i < actions.Count; i++)
        {
            var a = actions[i];
            if (!a) continue;

            float s = Mathf.Max(0f, a.Score(bb, senses)); // clamp negative to 0
            if (s > bestScore)
            {
                bestScore = s;
                pick = a;
            }
        }

        // No viable action? keep current if it exists.
        if (pick == null) return;

        // If unchanged, do nothing (stick with it)
        if (pick == _current) return;

        // Transition: Exit old, Enter new
        var prev = _current;
        if (prev != null) prev.Exit(bb, senses);   // <-- correct: cleanup old
        _current = pick;
        _current.Enter(bb, senses);             // <-- setup new


        if (logDecisions)
        {
            string prevName = prev ? prev.name : "(none)";
            Debug.Log($"[{name}] AIBrain switch: {prevName} ? {_current.name} (bestScore={bestScore:0.00})");
        }
    }

    // ?????????????????????????????????????????????????????????????????????????????
    // Convenience helpers (optional)
    // ?????????????????????????????????????????????????????????????????????????????

    /// <summary>
    /// Immediately force a specific action (e.g., stun, scripted moment).
    /// </summary>
    public void ForceAction(AIAction specific)
    {
        if (!specific) return;
        if (_current == specific) return;        // already running; no churn
        var prev = _current;
        if (prev != null) prev.Exit(bb, senses);   // <-- cleanup old (was Tick)

        _current = specific;
        _current.Enter(bb, senses);

        // next think soon so we can recover control; set Infinity if you want to hard-lock
        _nextThinkAt = Time.time + nearThink;

        if (logDecisions)
        {
            var pn = prev ? prev.name : "(none)";
            Debug.Log($"[{name}] ForceAction: {pn} → {_current.name}");
        }
    }


    /// <summary>
    /// Bump next think to now (useful after blackboard changes).
    /// </summary>
    public void NudgeThinkNow() => _nextThinkAt = 0f;
}
#endregion