using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarrerManager : MonoBehaviour
    //作为父类存放所有职业的共有属性
{
    protected Unit unit;
    protected Enemy enemy;
    protected virtual void Start()
    {
        unit = GetComponent<Unit>();

        enemy = GetComponent<Enemy>();
        //因为不想新开脚本所以就这样写一起了，稍微有点隐患，Unit中enemy会是空，Enemy中unit会是空，注意不要混用就不会出事
    }
    protected virtual void Update()
    {
        
    }
}

