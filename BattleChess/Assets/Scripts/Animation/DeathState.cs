using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathState : State
{
    public DeathState(StateMachine stateMachine, UnitState unit, string animBollNAme) : base(stateMachine, unit, animBollNAme)
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

    public override void update()
    {
        base.update();
    }
}
