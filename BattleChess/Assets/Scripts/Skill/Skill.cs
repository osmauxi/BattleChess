using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
public enum SkillType 
{
    Heal,
    PhysicAttack,
    MagicAttack,
    AllRoundAttack
}
#region

//普通类不能被序列化，也就无法在inspector中可见，为了解决这个问题就要使用[System.Serializable]
[System.Serializable]
public class HealParams 
{
    public int healAmount;
    public string buffType;
    public int buffDuration;
}
[System.Serializable]
public class AttackParams
{
    public int damage;
    public string buffType;
    public int buffDuration;
}
[System.Serializable]
public class AllRoundAttackParams//物法混伤
{
    public int damage;
    public int firedamage;
    public int icedamage;
    public string buffType;
    public int buffDuration;
}
[System.Serializable]
public class MagicAttackParams
{
    public int firedamage;
    public int icedamage;
    public string buffType;
    public int buffDuration;
}

#endregion
[System.Serializable]
public class Skill
    //设计为一个角色固定几个技能，每个技能一个实例
    //专门的技能类，这样实现技能功能时便可以直接配置好
{
    //这里用readonly标明每个技能一定会有的基础值，readonly是必须在声明时就要赋值的值
    public readonly SkillType type;
    public readonly string name;
    public readonly int skillNum;//技能编号
    public readonly int attackRange;
    public readonly int manaCost;
    public readonly float skillDamageFix;//伤害补正
    public int damage;
    public int firedamage;
    public int icedamage;
    public int healAmount;
    public string buffType;
    public int buffDuration;
    //构造函数内需要直接传值的只有readonly类的值，也就是所有技能都有的信息，剩下的值分装在类里面，这样的好处是创建一个技能时
    //只需要传入对应需要的数值就可以了，而不是Skill skill(10，2，0，0，0，0，0，0，0)这样的灾难
    //并且灵活，新种类的技能只需要新写一个类，然后在构造函数里引用并写入switch函数就行了
    public Skill(SkillType type, string name, int skillNum,int manaCost, int attackRange, float skillDamageFix,
         HealParams healParams = null,AttackParams attackParams = null,AllRoundAttackParams allRoundAttackParams = null, MagicAttackParams magicAttackParams = null)
    //这些类声明一个技能时不会用完，只会用其中一个，所以优先设为null
    {
        this.type = type;
        this.name = name;
        this.skillNum = skillNum;
        this.manaCost = manaCost;
        this.attackRange = attackRange;
        this.skillDamageFix = skillDamageFix;
        switch (type) 
        {
            case SkillType.Heal:
                healAmount = healParams.healAmount;
                buffType = healParams.buffType;
                buffDuration = healParams.buffDuration;
                break;
            case SkillType.PhysicAttack:
                damage = attackParams.damage;
                buffType = attackParams.buffType;
                buffDuration = attackParams.buffDuration;
                break;
            case SkillType.MagicAttack:
                icedamage = magicAttackParams.icedamage;
                firedamage = magicAttackParams.firedamage;
                buffType = magicAttackParams.buffType;
                buffDuration = magicAttackParams.buffDuration;
                break;
            case SkillType.AllRoundAttack:
                icedamage = allRoundAttackParams.icedamage;
                firedamage = allRoundAttackParams.firedamage;
                damage = allRoundAttackParams.damage;
                buffType = allRoundAttackParams.buffType;
                buffDuration = allRoundAttackParams.buffDuration;
                break;
        }
    }
}

