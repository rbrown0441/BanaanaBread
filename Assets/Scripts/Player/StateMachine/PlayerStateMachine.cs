using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerStateMachine : MonoBehaviour
{
    // Reference Player
    [Header("Refs")]
    [SerializeField] private Player _player;
    public Player player => _player;

    // State Status
    public PlayerState Current { get; private set; }
    public PlayerState Previous { get; private set; }

    // State Registry
    private readonly Dictionary<Type, PlayerState> _states = new();

    void Awake()
    {
        if (_player == null) _player = GetComponent<Player>();

        // Register States
        Register(new pState_Idle(this));
        Register(new pState_Run(this));
        Register(new pState_Aerial(this));

        // Set Initial State
        state_switch<pState_Idle>();
    }

    void Update()
    {
        Current?.Tick();
    }

    void FixedUpdate()
    {
        Current?.FixedTick();
    }

    void LateUpdate()
    {
        Current?.LateTick();
    }

    // Helper: State Switch
    public void state_switch<T>() where T : PlayerState
    {
        var type = typeof(T);
        if (!_states.TryGetValue(type, out var next))
        {
            Debug.LogError($"[StateMachine] State not registered: {type.Name}");
            return;
        }
        InternalChangeState(next);
    }

    // Helper: State Switch
    public void state_switch(Type stateType)
    {
        if (!typeof(PlayerState).IsAssignableFrom(stateType))
        {
            Debug.LogError($"[StateMachine] {stateType} is not a PlayerState.");
            return;
        }
        if (!_states.TryGetValue(stateType, out var next))
        {
            Debug.LogError($"[StateMachine] State not registered: {stateType.Name}");
            return;
        }
        InternalChangeState(next);
    }

    // Helper: State Reset
    public void state_reset()
    {
        if (Current != null) return;
        Current.OnReset();
    }

    // Helper: Forced Reset
    public void state_reset_to<T>() where T : PlayerState
    {
        state_switch<T>();
        Current?.OnReset();
    }

    // Internal: Register State
    private void Register(PlayerState state)
    {
        var t = state.GetType();
        if (_states.ContainsKey(t))
        {
            Debug.LogWarning($"[SateMachine] Duplicate registration: {t.Name}");
            return;
        }
        state.Bind(this);
        _states.Add(t, state);
        state.OnRegistered();
    }

    // Internal: Change State
    private void InternalChangeState(PlayerState next)
    {
        if (next == Current) return;

        Current?.OnExit();
        Previous = Current;
        Current = next;
        Current?.OnEnter();
    }
}


