using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerState
{
    protected PlayerStateMachine Machine { get; private set; }
    protected Player player => Machine.player;

    // Called upon Registration
    public virtual void OnRegistered() { }

    // State Machine injects itself here
    internal void Bind(PlayerStateMachine machine) => Machine = machine;

    // On Enter
    public virtual void OnEnter() { }

    // On Main Loop
    public virtual void Tick() { }

    // Fixed Update Loop
    public virtual void FixedTick() { }

    // Late Update Loop
    public virtual void LateTick() { }

    // On Exit
    public virtual void OnExit() { }

    // On Reset
    public virtual void OnReset() { }
}
