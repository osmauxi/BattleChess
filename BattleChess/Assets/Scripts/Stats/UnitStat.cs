using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStat : CharacterStat
{
    public Unit Unit;
    protected override void Start()
    {
        base.Start();
        Unit = GetComponent<Unit>();
    }

    void Update()
    {
        
    }
}
