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
    //default移动
    private Queue<(GridSettings gridSetting, int movepoint)> queue = new Queue<(GridSettings gridSettings, int remainingPoints)>();
    public HashSet<GridSettings> visited = new HashSet<GridSettings>();
    private Color originalColor;
    public Enemy attackEnemy;

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
        if (visited.Count > 0)
            CanMoveColorRestore();
        if (SelectedUnit.movePoint == 0)
            return;
        if (SelectedUnit.GetComponent<UnitStat>().currentMana - skill.manaCost < 0)
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
            UAttackRangeCheck<Unit>(skill.attackRange);
            if (canBeattackedUnit.Count == 0)//没找到能打的人的情况 
            {
                return;
            }
            WhatSkillType = false;
        }
        else
        {
            UAttackRangeCheck<Enemy>(skill.attackRange);
            if (canBeattackedEnemy.Count == 0)//没找到能打的人的情况 
            {
                return;
            }
            WhatSkillType = true;
        }
    }
    public void EPrepareExecuteSkill(Skill _skill,Enemy enemy)
    {
        if (_skill == null)
        {
            Debug.Log("Skill Lost");
            return;
        }
        if (visited.Count > 0)
            CanMoveColorRestore();
        if (enemy.movePoint == 0)
            return;
        if (enemy.GetComponent<EnemyStat>().currentMana - _skill.manaCost < 0)
            return;
        attackEnemy = enemy;
        skill = _skill;
        if (_skill.type == SkillType.Heal)
        {
            EAttackRangeCheck<Enemy>(_skill.attackRange);
            if (canBeattackedUnit.Count == 0)//没找到能打的人的情况 
            {
                return;
            }
            WhatSkillType = false;
        }
        else
        {
            EAttackRangeCheck<Unit>(_skill.attackRange);
            if (canBeattackedEnemy.Count == 0)//没找到能打的人的情况 
            {
                return;
            }
            WhatSkillType = true;
        }
        

        //!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //enemy.GetComponent<EnemyStat>().currentMana -= _skill.manaCost;//减少释放者的蓝
    }
    private void UAttackRangeCheck<T>(int attackRange)
    {
        originalColor = SelectedUnit.GetGroundGrid().rend.material.color;
        SelectedUnit.GetGroundGrid().MovedColorRestore(SelectedUnit.GetGroundGrid());
        if (skill.range == AttackRangeType.Default)
        {
            CanMoveColorChangeDefault<T>(SelectedUnit.GetGroundGrid(), skill.attackRange);
        }
        else if (skill.range == AttackRangeType.Straight)
        {
            CanMoveColorChangeStraight<T>(SelectedUnit.GetGroundGrid(), skill.attackRange);
        }
    }
    private void EAttackRangeCheck<T>(int attackRange) 
    {
        originalColor = attackEnemy.GetGroundGrid().rend.material.color;
        attackEnemy.GetGroundGrid().MovedColorRestore(attackEnemy.GetGroundGrid());
        if (skill.range == AttackRangeType.Default)
        {
            CanMoveColorChangeDefault<T>(attackEnemy.GetGroundGrid(), skill.attackRange);
        }
        else if (skill.range == AttackRangeType.Straight)
        {
            CanMoveColorChangeStraight<T>(attackEnemy.GetGroundGrid(), skill.attackRange);
        }
    }

    public void AttackedStateRestore()//改攻击状态的
    {
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
    //private int GetNormalizedDistance(Vector3Int unitPos)//之后可能还会根据攻击方式来进行细分吧
    //{
    //    return Mathf.Abs(SelectedUnit.gridPosition.x - unitPos.x) + Mathf.Abs(SelectedUnit.gridPosition.z - unitPos.z);
    //}
    #region DefaultARange
    public void CanMoveColorChangeDefault<T>(GridSettings gridSettings, int attackPoint)
    {
        // 将角色当前所在的地图格加入队列，初始剩余行动点为角色的行动点
        queue.Enqueue((gridSettings, attackPoint));
        visited.Add(gridSettings);
        // 标记当前地图格不在移动范围内（因为角色已经在该位置）

        // 开始BFS广度优先搜索
        while (queue.Count > 0)
        {
            // 从队列中取出一个地图格和对应的剩余行动点
            var (current, remaining) = queue.Dequeue();

            // 遍历当前地图格的周围的四个地块，进行依次访问
            foreach (GridSettings neighbor in current.gridList)
            {
                // 检查邻居是否未被访问过，并且剩余行动点足够移动到该邻居
                if (!visited.Contains(neighbor) && remaining > 0)
                {
                    //高亮具象化 后面要改###########################
                    neighbor.rend.material.color = Color.yellow;
                    //##############################################

                    if (typeof(T) == typeof(Unit) && neighbor.occupiedbyUnit)
                    {
                        canBeattackedUnit.Add(neighbor.GetTargetAbove<Unit>());

                        neighbor.rend.material.color = Color.black;

                        neighbor.isInAttackRange = true;
                    }
                    else if (typeof(T) == typeof(Enemy) && neighbor.occupiedbyEnemy)
                    {
                        canBeattackedEnemy.Add(neighbor.GetTargetAbove<Enemy>());

                        neighbor.rend.material.color = Color.black;

                        neighbor.isInAttackRange = true;
                    }
                    // 将该邻居加入访问集合
                    visited.Add(neighbor);
                    // 计算移动到该邻居后剩余的行动点
                    int newRemaining = remaining - 1;
                    // 将该邻居和新的剩余行动点加入队列，继续搜索
                    queue.Enqueue((neighbor, newRemaining));
                }
            }
        }
    }
    #endregion
    #region StraightRange
    public void CanMoveColorChangeStraight<T>(GridSettings gridSettings, int attackPoint)
    {
        queue.Enqueue((gridSettings, attackPoint));
        visited.Add(gridSettings);

        while (queue.Count > 0)
        {
            var (current, remaining) = queue.Dequeue();
            foreach (var neighbor in current.gridList)
            {
                //将邻居与原点比坐标，xz轴都有数说明不在一条直线上
                Vector3Int Dir = gridSettings.gridCellPosition - neighbor.gridCellPosition;
                // 仅处理十字方向（x或y变化量为±1，另一个为0）
                if (Mathf.Abs(Dir.x)> 0 && Mathf.Abs(Dir.z) > 0)
                {
                    continue; // 跳过非十字方向的邻居
                }

                // 检查邻居是否未被访问且剩余行动点足够
                if (!visited.Contains(neighbor) && remaining > 0)
                {
                    // 高亮显示（后续可调整颜色逻辑）
                    neighbor.rend.material.color = Color.yellow;

                    // 检测攻击目标
                    if (typeof(T) == typeof(Unit) && neighbor.occupiedbyUnit)
                    {
                        canBeattackedUnit.Add(neighbor.GetTargetAbove<Unit>());
                        neighbor.rend.material.color = Color.black;
                        neighbor.isInAttackRange = true;
                    }
                    else if (typeof(T) == typeof(Enemy) && neighbor.occupiedbyEnemy)
                    {
                        canBeattackedEnemy.Add(neighbor.GetTargetAbove<Enemy>());
                        neighbor.rend.material.color = Color.black;
                        neighbor.isInAttackRange = true;
                    }

                    visited.Add(neighbor);
                    int newRemaining = remaining - 1; // 移动消耗1点行动点
                    queue.Enqueue((neighbor, newRemaining));
                }

            }
        }
    }
    #endregion
    public void CanMoveColorRestore()
    {
        foreach (var a in visited)
        {
            a.rend.material.color = originalColor;
            a.isInMovementRange = false;
        }
        visited.Clear();
    }
}