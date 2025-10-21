using System.Collections;
using System.Collections.Generic;
using System.Xml;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

// Run

public class pState_Run : PlayerState
{
    public pState_Run(PlayerStateMachine machine) { Bind(machine); }


    // ON ENTER
    public override void OnEnter()
    {
        // Change Sprite
        player.sprite_change("Diza_Run");
        // Determine Input Direction
        float horizontal_input = player.get_input_horizontal();
        // Set Facing Dir
        if (horizontal_input != 0) player.transform.localScale = new Vector3(horizontal_input, 1, 1);
        // Get Current Horizontal Speed
        float horizontal_speed = player.body.velocity.x;
        // Set Initial Speed
        if (Mathf.Abs(horizontal_speed) < player.run_speed_init)
        {
            player.body.velocity = new Vector2(player.run_speed_init * horizontal_input, player.body.velocity.y);
        }
    }


    // ON MAIN LOOP
    public override void FixedTick()
    {
        // Determine Input Direction
        float horizontal_input = player.get_input_horizontal();
        // Move Player
        float step = player.run_accel * Time.fixedDeltaTime;
        player.body.velocity = Vector2.MoveTowards(player.body.velocity, new Vector2(player.run_speed_max * horizontal_input, player.body.velocity.y), step);
        //player.set_vel_x(player.run_speed_max * horizontal_input, player.run_accel, Time.fixedDeltaTime);
        Debug.Log($"dt={Time.fixedDeltaTime:0.0000}");
        // Set Faced Dir
        if (horizontal_input != 0) player.change_facing(Mathf.RoundToInt(horizontal_input));
        // Update Current Animation
        if (player.facing != Mathf.Sign(player.body.velocity.x))
        {
            // Apply Extra Friction
            player.apply_friction(player.ground_fric*3, Time.fixedDeltaTime);
            // Sprite
            player.sprite_change("Diza_Turnaround", 0.5f);
        }
        else
        {
            // Sprite
            player.sprite_change("Diza_Run");
        }
        // Jump
        if (player.actionMoveJump.IsPressed()) player.event_jump();
        // Stop Running
        if (!player.actionMoveLeft.IsPressed() && !player.actionMoveRight.IsPressed())
        {
            // Dampen X Velocity
            player.mod_vel_x(0.25f);
            // Switch State
            player.state_switch<pState_Idle>();
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
