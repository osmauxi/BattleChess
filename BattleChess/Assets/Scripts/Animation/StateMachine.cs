using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class StateMachine//PlayerStateMachine负责管理所有的状态的切换
{
    public State currentState { get; private set; }//意为一个值在访问它的时候时public状态，想改变它时时private状态，即只读状态

    public void Initialize(State _startState)//Initialize为构造函数名
    {
        currentState = _startState;
        currentState.Enter();//enter是playerstate中的enter函数，下同
    }

    public void ChangeState(State _newState)
    {//退出现在的状态，改变现在的状态，进入新的状态
        currentState.Exit();
        currentState = _newState;
        currentState.Enter();
    }
}
