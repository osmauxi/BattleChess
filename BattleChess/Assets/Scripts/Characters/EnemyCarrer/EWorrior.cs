using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EWorrior : CarrerManager
{
    public Skill SlashingStrike = new Skill(SkillType.PhysicAttack, AttackRangeType.Default, "SlashingStrike", 0, 0, 1, 1.0f, attackParams: new AttackParams { damage = 10, buffType = null, buffDuration = 0 });
    //{damage = 10,buffType = null,buffDuration = 0}对象的初始化器，能在创建对象时就进行赋值，相当于创建一个 HealParams 对象并对其字段进行初始化
    //: 可用于指定命名参数。命名参数允许你通过参数名来传递参数，而不用按照参数在方法定义中的顺序。
    //attackParams是在构造函数里声明好的AttackParams变量，在构造函数中被声明为null这个技能需要用到这个类，就new了一个新的，初始化好了的AttackParams类赋给他
    //这样整个构造函数内声明的几个null类只有我需要用到的类型得到了实例
    public Skill FireStrike = new Skill(SkillType.AllRoundAttack, AttackRangeType.Straight, "FireStrike", 1, 20, 2, 1.2f, allRoundAttackParams: new AllRoundAttackParams { icedamage = 0, firedamage = 10, damage = 5, buffType = "Ignited", buffDuration = 2 });
    protected override void Start()
    {
        base.Start();
        enemy.skills.Add(SlashingStrike);
        enemy.skills.Add(FireStrike);
    }
}
