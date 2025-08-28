using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class UnitState : MonoBehaviour
{
    public Animator anim;
    public StateMachine stateMachine;
    public SkeletonAnimation spineAnim;
    //每个状态都有单独实例以便被直接调用在ChangeState()中
    #region States
    public IdleState idleState { get; private set; }
    public MoveState moveState { get; private set; }
    public AttackState attackState { get; private set; }
    public BeAttackState beAttackState { get; private set; }
    public DeathState deathState { get; private set; }
    #endregion

    public void Awake()
    {
        stateMachine = new StateMachine();
        //新建状态机，每个UnitState都对应一个独立的状态机来改变状态，满足了复用的需求
        anim = GetComponent<Animator>();
        idleState = new IdleState(stateMachine, this,"Idle");
        moveState = new MoveState(stateMachine, this, "Move");
        attackState = new AttackState(stateMachine, this, "Attack");
        beAttackState = new BeAttackState(stateMachine, this, "BeAttack");
        deathState = new DeathState(stateMachine, this, "Death");

        spineAnim = GetComponent<SkeletonAnimation>();
    }
    public void Start()
    {
        stateMachine.Initialize(idleState);
    }
    public void Update()
    {
        stateMachine.currentState.update();
    }

    public void SetIdle() 
    {
        stateMachine.currentState.Exit();
        stateMachine.ChangeState(idleState);
        spineAnim.AnimationState.SetAnimation(1,"idel animation",true);
    }

    public void Methed() { }
}

