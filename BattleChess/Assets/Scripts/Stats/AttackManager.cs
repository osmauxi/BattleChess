using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class AttackManager : MonoBehaviour
{
    public static AttackManager instance;

    public GameObject DamageNumPrefab;
    public Vector3 offset;

    private Unit SelectedUnit;
    public List<GridSettings> colorChangedGrids = new List<GridSettings>();//存储改过颜色的地块，方便后期处理
    public Skill skill;

    public bool WhatSkillType;//标注是对敌技能还是对友技能,f为对友，t为对敌

    private List<Enemy> canBeattackedEnemy = new List<Enemy>();
    private List<Unit> canBeattackedUnit = new List<Unit>();
    void Awake()
    {
        // 单例模式初始化
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PrepareExecuteSkill(int skillIndex)//执行技能
    {
        SelectedUnit = UnitActionSystem.Instance.selectedUnit;
        if (SelectedUnit.movePoint == 0)
            return;
        skill = SelectedUnit.skills[skillIndex];
        if (skill == null)
        {
            Debug.Log("Skill Lost");
            return;
        }
        SelectedUnit.isAttacking = true;
        BattleInfUISet.Instance.SetSkillText(skill);
        if (skill.type == SkillType.Heal)
        {
            AttackRangeCheck<Unit>(skill.attackRange);
            if (colorChangedGrids.Count == 0)//没找到能打的人的情况 
            {
                return;
            }
            WhatSkillType = false;
        }
        else
        {
            AttackRangeCheck<Enemy>(skill.attackRange);
            if (colorChangedGrids.Count == 0)//没找到能打的人的情况 
            {
                return;
            }
            WhatSkillType = true;
        }
        SelectedUnit.GetComponent<UnitStat>().currentMana -= skill.manaCost;//减少释放者的蓝
    }
    private void AttackRangeCheck<T>(int attackRange)
    {
        //先更新一遍列表
        SelectedUnit.GetGroundGrid().UnitScene<Unit>(SelectedUnit.GetGroundGrid(), skill.attackRange, out SelectedUnit.unitList);
        SelectedUnit.GetGroundGrid().UnitScene<Enemy>(SelectedUnit.GetGroundGrid(), skill.attackRange, out SelectedUnit.enemyList);
        GridSettings currentGrid;
        if (typeof(T) == typeof(Unit))
        {
            foreach (var unit in SelectedUnit.unitList)
            {
                if (AttackRangeCheck(unit.gridPosition) <= attackRange)
                {
                    currentGrid = unit.GetGroundGrid();
                    currentGrid.CanAttackColorChange();
                    colorChangedGrids.Add(currentGrid);
                    unit.canBeAttacked = true;
                    currentGrid.isInAttackRange = true;
                    canBeattackedUnit.Add(unit);
                }

            }
        }
        else
        {
            foreach (var enemy in SelectedUnit.enemyList)
            {
                if (AttackRangeCheck(enemy.gridPosition) <= attackRange)
                {
                    currentGrid = enemy.GetGroundGrid();
                    currentGrid.CanAttackColorChange();
                    colorChangedGrids.Add(currentGrid);
                    enemy.canBeAttacked = true;
                    currentGrid.isInAttackRange = true;
                    canBeattackedEnemy.Add(enemy);
                }

            }
        }
    }

    public void AttackedColorRestore()
    {
        foreach (var a in colorChangedGrids) 
        {
            a.CanAttackColorRestore();
            a.isInMovementRange = false;
        }
        colorChangedGrids.Clear();
        foreach (var b in canBeattackedEnemy) 
        {
            b.canBeAttacked = false;
        }
        foreach (var c in canBeattackedEnemy)
        {
            c.canBeAttacked = false;
        }
        canBeattackedUnit.Clear();
        canBeattackedEnemy.Clear();
    }
    //private IEnumerator AttackInputCheck() 
    //{
    //    if (!Input.GetKey(KeyCode.Mouse1)) 
    //    {
    //        yield return 0;
    //    }
    //    //还原地块颜色
    //}
    private int AttackRangeCheck(Vector3Int unitPos)//之后可能还会根据攻击方式来进行细分吧
    {
        return Mathf.Abs(SelectedUnit.gridPosition.x - unitPos.x) + Mathf.Abs(SelectedUnit.gridPosition.z - unitPos.z);
    }
}
