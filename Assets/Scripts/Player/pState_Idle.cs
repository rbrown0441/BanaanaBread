using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Idle

public class pState_Idle : PlayerState
{
    public pState_Idle(PlayerStateMachine machine) { Bind(machine); }


    // ON ENTER
    public override void OnEnter()
    {
        // Change Sprite
        player.sprite_change("Diza_Idle", 0.25f);
    }


    // ON MAIN LOOP
    public override void FixedTick()
    {
        // Apply Friction
        player.apply_friction(player.ground_fric, Time.fixedDeltaTime);
        // Jump
        if (player.actionMoveJump.IsPressed()) player.event_jump();
        // Run
        if (player.actionMoveLeft.IsPressed() || player.actionMoveRight.IsPressed())
        {
            player.state_switch<pState_Run>();
        }
        // Fall
        if (!player.isGrounded) player.state_switch<pState_Aerial>();
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
