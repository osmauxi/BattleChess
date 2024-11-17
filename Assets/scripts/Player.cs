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
        //����PlayerStateMachine�ű��е�Initialize��������idlestate������������idle��Ϊ��ʼ״̬
        //start������һ֡ʱ��idlestate����PlayerStateMachine�ű���Initialize��������ʱ _startState = idlestate��Ȼ�� _startState��ֵ����currentstate��
        //���currentstate��ֵΪidlestate��ֵ��string��Ӧ��animator�����õ�boolֵ��idle����ִ��playerstate�µ�enter������������idle������animBoolName������Ϊtrue��
        //ʹ����״̬��ű�״̬�����idle
    }

    void Update()
    {
        stateMachine.currentState.Update();
        //currentState��PLayerState��һ��ʵ�������浱ǰ״̬��Ϣ����һ��״̬��Ϣ
        //��Update(ÿ֡����)�����Update����(ֻ�Ǻ�������Ϊû�м̳�Monobehavior)
        //ʹ��ÿ֡���ò�ͬ״̬��Update����(��PlayerState����д����Ҳ��ִ��PlayerState���Update(����))
    }
}
