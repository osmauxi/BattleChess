using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AttackState : State
{
    public AttackState(StateMachine stateMachine, UnitState unit, string animBollNAme) : base(stateMachine, unit, animBollNAme)
    {
    }

    public override void Enter()
    {
        SetSpineAnim("attack animation", false);
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
