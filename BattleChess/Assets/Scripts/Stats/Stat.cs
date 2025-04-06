using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class Stat
//可拓展属性系统，属性用stat属性，能保留基础值的同时能动态的加减数值，在未来的装备系统等的加入留下了拓展空间
{
    [SerializeField] private int basevalue;

    public List<int> modifiers;

    public int GetValue()
    {
        int finalvalue = basevalue;

        foreach (int modifier in modifiers)
        {
            finalvalue += modifier;
            //遍历modifiers数组内所有的值加在finaovalue上,算出总值
            //用于各类buff或武器伤害的叠加
        }

        return finalvalue;
    }

    public void SetDefaultValue(int _value)
    {
        basevalue = _value;
    }

    public void AddModifier(int _modifier)
    {
        modifiers.Add(_modifier);
    }

    public void removemodifier(int _modifier)
    {
        modifiers.RemoveAt(_modifier);
    }
}


