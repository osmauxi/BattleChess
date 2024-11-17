using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMoveState : PlayerGroundState
{
    public PlayerMoveState(Player _player, PlayerStateMachine _stateMachie, string _animboolName) : base(_player, _stateMachie, _animboolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if (xInput != 0)
            rb.velocity = new Vector3(rb.velocity.x * player.movespeed * xInput,rb.velocity.y,rb.velocity.z);
        if(yInput != 0)
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * player.movespeed * yInput, rb.velocity.z);
        if (xInput == 0 && yInput == 0)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}
