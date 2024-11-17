using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerIdleState : PlayerGroundState
{
    public PlayerIdleState(Player _player, PlayerStateMachine _stateMachie, string _animboolName) : base(_player, _stateMachie, _animboolName)
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
        if (xInput != 0 || yInput != 0)
        {
            stateMachine.ChangeState(player.moveState);//用状态机执行playerstatemachine下的changestate函数，赋值为player脚本下的movestate状态，即改变为move状态
        }
    }
}
