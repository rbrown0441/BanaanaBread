using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Aerial

public class pState_Aerial : PlayerState
{
    public pState_Aerial(PlayerStateMachine machine) { Bind(machine); }

    // Set Velocity Threshold for swapping from Jump M to Jump D
    private float velocity_threshold = -1f;


    // ON ENTER
    public override void OnEnter()
    {
        // Get Player Velocity
        var y_vel = player.body.velocity.y;
        // Change Sprite
        if (y_vel > 0)
        {
            player.sprite_change("Diza_JumpU");
        }
        else if (y_vel > velocity_threshold)
        {
            player.anim.Play("Diza_JumpM");
        }
        else if (y_vel < velocity_threshold)
        {
            player.anim.Play("Diza_JumpD");
        }
    }


    // ON MAIN LOOP
    public override void Tick()
    {
        // Get Player Velocity
        var y_vel = player.body.velocity.y;
        // Update Sprite
        if (y_vel > 0)
        {
            player.sprite_change("Diza_JumpU");
        }
        else if (y_vel > velocity_threshold)
        {
            player.anim.Play("Diza_JumpM");
        }
        else if (y_vel < velocity_threshold)
        {
            player.anim.Play("Diza_JumpD");
        }
        // Landing, return to Idle
        if (player.isGrounded)
        {
            // Landing Event
            player.event_landed();
            // Check if Trying to Run
            if (player.actionMoveLeft.IsPressed() || player.actionMoveRight.IsPressed())
            {
                // Run
                player.state_switch<pState_Run>();
            }
            else
            {
                // Dampen X Velocity
                player.mod_vel_x(0.25f);
                // Return to Idle
                player.state_switch<pState_Idle>();
            }
        }
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
