using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class State
//事实上，很多方法可以直接移到各个状态脚本中来执行，但是状态机作为后来加入的东西，要与之前代码合并重构实在淘神，故而只作为改变动画状态的脚本
{
    protected StateMachine stateMachine;
    protected UnitState unit;
    protected bool triggerCalled;
    protected string animBoolName;

    public State(StateMachine stateMachine, UnitState unit, string animBollNAme)
    //作为所有状态的父类，这个状态机是基于SetBool来的，所有状态都由Animator中的布尔值来进行转换，状态需要指定状态机，状态对象和Animator中对应的布尔值
    {
        this.stateMachine = stateMachine;
        this.unit = unit;
        this.animBoolName = animBollNAme;
    }

    public virtual void update()
    {

    }

    //进入状态是改变对应的bool为ture，Animator中进入此状态，退出后设为false，Animator进入Exit，这样就不用在Animator里拉蜘蛛网
    public virtual void Enter()
    {
        triggerCalled = false;
        unit.anim.SetBool(animBoolName, true);
    }

    public virtual void Exit()
    {
        unit.anim.SetBool(animBoolName, false);
    }

    public virtual void SetSpineAnim(string name, bool loop)
    {
        if (unit.spineAnim)
            unit.spineAnim.AnimationState.SetAnimation(1, name, loop);
    }
}

    
