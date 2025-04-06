using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarrerManager : MonoBehaviour
    //作为父类存放所有职业的共有属性
{
    protected Unit unit;
    protected virtual void Start()
    {
        unit = GetComponent<Unit>();
    }
    protected virtual void Update()
    {
        
    }
}
