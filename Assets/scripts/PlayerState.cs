using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState
{
    protected Player player;
    protected PlayerStateMachine stateMachine;
    private string animBoolName;

    protected Rigidbody rb;
    protected float xInput;
    protected float yInput;
    protected float zInput;

    public PlayerState(Player _player,PlayerStateMachine _stateMachie,string _animboolName)
    {
        this.player = _player;
        this.stateMachine = _stateMachie;
        this.animBoolName = _animboolName;

    }
    public virtual void Enter() 
    {
        if (player.rb != null)
            rb = player.rb;
        else
            Debug.Log("falled");
    }

    public virtual void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
    }

    public virtual void Exit() 
    {
    
    }
}
