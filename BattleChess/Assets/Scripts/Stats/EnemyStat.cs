using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStat : CharacterStat
{
    public Enemy Enemy;
    protected override void Start()
    {
        base.Start();
        Enemy = GetComponent<Enemy>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
