using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Use "Player" to reference the Player instance
// state_switch() to change states
// state_reset() to reset current state

public class pState_Template : PlayerState
{
    public pState_Template(PlayerStateMachine machine) { Bind(machine); }

    // ON ENTER
    public override void OnEnter()
    {
        // On Enter
    }

    // ON MAIN LOOP
    public override void Tick()
    {
        // On Loop
    }

    // ON EXIT
    public override void OnExit()
    {
        // On Exit
    }

    // ON RESET
    public override void OnReset()
    {
        // On Reset
    }
}
