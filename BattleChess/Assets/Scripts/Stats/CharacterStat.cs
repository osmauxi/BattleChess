using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class CharacterStat : MonoBehaviour
{
    private UnitActionSystem UnitActionSystem => UnitActionSystem.Instance;
    private AttackManager AttackManager => AttackManager.instance;
    [Header("offensive stats")]
    public Stat damage;
    public Stat critchance;//暴击率
    public Stat critpower;//暴击伤害

    [Header("defensive stats")]
    public Stat maxhealth;
    public Stat armor;
    public Stat magicresistance;
    public Stat evasion;//闪避

    public Stat maxMana;
    [Header("negative stats")]
    public bool isIgnited;//灼烧
    public bool isChilled;//冻僵
    public int buffLevel = 0;
    public int continuousRound;

    public int currenthealth;
    public int currentMana;

    private bool isCrit;
    protected virtual void Start()
    {
        critchance.SetDefaultValue(20);
        critpower.SetDefaultValue(150);
        currenthealth = maxhealth.GetValue();
        currentMana = maxMana.GetValue();
    }

    public void DoDamage(Skill skill,CharacterStat attackStat) // 计算伤害所要调用的函数
    {
        if(CouldEvade())
            return;

        int totalDamage = Mathf.RoundToInt((attackStat.damage.GetValue() + skill.damage - armor.GetValue() + 
            (skill.firedamage + skill.icedamage - magicresistance.GetValue()))* skill.skillDamageFix);
        if (attackStat.CouldCrit())
            //dodamage函数是由攻击对象传入技能调用被攻击对象的dodamage函数，这里比较绕，一定要搞清你在判断攻击者还是被攻击者的条件
            totalDamage = attackStat.GetCritDamage(totalDamage);
        Mathf.Clamp(totalDamage,0,int.MaxValue);
        SetSkillNegativeEffects(skill);
        DecreaceHealth(totalDamage);
    }

    private void DecreaceHealth(int value)
    {
        currenthealth -= value;
        DamageNumGenerate(value);
        if (currenthealth <= 0)
            Die();
    }

    private void Die()
    {
        Destroy(gameObject);
        gameObject.GetComponent<Entity>().GetGroundGrid().ResetAllOccupiedInf();
        TurnBasedManager.Instance.enemyUnits.Remove(GetComponent<Enemy>());
    }

    private void DamageNumGenerate(int damage)
    {
        GameObject Prefab = Instantiate(AttackManager.DamageNumPrefab, transform.position + AttackManager.offset, Quaternion.identity);
        Prefab.GetComponentInChildren<TextMeshProUGUI>().text = damage.ToString();
        if (isCrit) 
        {
            Animator anim = Prefab.GetComponent<Animator>();
            anim.SetBool("IsCrit",true);
        }
        if (isIgnited) 
        {
            Prefab.GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
        }
        Prefab.GetComponent<DestoryItSelf>().Invoke("DestoryIt", 1f);//延迟摧毁
    }

    private void SetSkillNegativeEffects(Skill skill)
        //buff种类Ignited Chilled以后再加
    {
        if (skill.buffType == null)
            return;

            if (skill.buffType == "Ignited")
            {
                isIgnited = true;
            }
            else if (skill.buffType == "Chilled")
            {
                isChilled = true;
            }
            if (isIgnited && isChilled) //这两种状态不同时存在
            {
                isChilled = false;
                isIgnited = false;
            }
            else
            {
                buffLevel++;
                if (TurnBasedManager.Instance.currentState == TurnState.PlayerControlled)
                    UnitActionSystem.choosedEne.GetComponent<CharacterStat>().continuousRound = skill.buffDuration;
                else if (TurnBasedManager.Instance.currentState == TurnState.EnemyUnitActing)
                    AttackManager.instance.attackEnemy.GetComponent<Enemy>().targetUnit.GetComponent<CharacterStat>().continuousRound = skill.buffDuration;
            }
        
    }

    public void SkillNegativeEffectsUse() 
    {
        if (isIgnited)
        {
            DecreaceHealth(Mathf.Clamp(5 * buffLevel, 0, 30));
        }
        else if (isChilled) 
        {
            GetComponent<Entity>().movePoint -= Mathf.Clamp(1 * buffLevel,0,3);
            armor.AddModifier(Mathf.Clamp(-5 * buffLevel, 0, 15));
        }
    }

    private int GetCritDamage(int totalDamage)
    {
        return totalDamage = totalDamage * critpower.GetValue() / 100;
    }
    private bool CouldCrit()//暴击检测 目前单纯除一百当概率
    {
        if (Random.Range(0, 100) <= critchance.GetValue())
        {
            isCrit = true;
            return true;
        }
        isCrit = false;
        return false;
    }
    private bool CouldEvade()//闪避检测 目前单纯除一百当概率
    {
        if(Random.Range(0,100) < evasion.GetValue())
            return true;
        return false;
    }
}
