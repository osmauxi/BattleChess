using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    public float movespeed;
    #region States
    public PlayerStateMachine stateMachine {  get; private set; }
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    #endregion
    public override void Awake()
    {
        base.Awake();
        stateMachine = new PlayerStateMachine();
        idleState = new PlayerIdleState(this,stateMachine,"Idle");
        moveState = new PlayerMoveState(this, stateMachine, "Move");
    }
    public override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
        //访问PlayerStateMachine脚本中的Initialize函数，将idlestate赋给它，即将idle设为初始状态
        //start函数在一帧时将idlestate赋给PlayerStateMachine脚本下Initialize函数，此时 _startState = idlestate，然后 _startState将值赋给currentstate，
        //最后currentstate（值为idlestate的值，string对应了animator中设置的bool值“idle”）执行playerstate下的enter函数，并将“idle”赋给animBoolName，设置为true，
        //使动画状态与脚本状态都变成idle
    }

    void Update()
    {
        stateMachine.currentState.Update();
        //currentState是PLayerState的一个实例，储存当前状态信息和下一个状态信息
        //在Update(每帧调用)里调用Update函数(只是函数，因为没有继承Monobehavior)
        //使其每帧调用不同状态的Update函数(是PlayerState的重写，故也会执行PlayerState里的Update(函数))
    }
}
