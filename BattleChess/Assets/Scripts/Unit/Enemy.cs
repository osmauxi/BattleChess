using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using static UnityEngine.UI.CanvasScaler;

public class Enemy : Entity
{
    private CharacterStat stat;
    [Header("巡视设置")]
    [SerializeField] private bool tourPointsSettingMode;
    [SerializeField] private List<GridSettings> tourPoints = new List<GridSettings>();
    //列表是引用变量，复制是引用赋值，不是值赋值，我说我删Points的值为什么tourPoints的值在少，赛博鬼打墙了
    public bool Move = false;
    public bool isRunning = false;
    [SerializeField]private List<GridSettings> Points = new List<GridSettings>();
    [Header("寻敌设置")]
    public Unit targetUnit;
    [SerializeField] private float CPV;//移动权重衡量总值
    [SerializeField] private List<Vector3Int> movePath = new List<Vector3Int>();
    [Header("决策系数")]//取0-1
    public float basealpha = 0.4f;    // 攻击权重
    public float basebeta = 0.3f;     // 防御权重
    public float basegamma = 0.5f;    // 危险规避
    public float enemyInfluence = 0.15f;//敌人和友军的权重计算偏差
    public float allySupportFactor = 0.1f;
    private float currentAlpha;
    private float currentBeta;
    private float currentgamma;
    [Header("攻击设定")]
    [SerializeField] private bool isMelee = true;
    private int maxAttackRange;
    private Skill chosedSkill;
    private Skill farestSkill;
    private float currentCooldown;

    private bool isReady => currentCooldown <= 0;
    protected override void Start()
    {
        base.Start();
        stat = GetComponent<CharacterStat>();
        GetGroundGrid().UnitScene<Unit>(GetGroundGrid(), sceneRange, out unitList);
        GetGroundGrid().UnitScene<Enemy>(GetGroundGrid(), sceneRange, out enemyList);
        MakeDecision();
        GetMaxAttackRange();
    }

    private void GetMaxAttackRange()
    {
        maxAttackRange = 0;
        foreach (var skill in skills)
        {
            if (skill.attackRange > maxAttackRange) 
            {
                maxAttackRange = skill.attackRange;
                farestSkill = skill;
            }
        }
    }

    protected override void Update()
    {
        base.Update();
        TourPointSet();
    }
    #region TourLogic
    private void TourPointSet()
        //设好一个敌人后，自己开这个模式然后设定巡回点，然后把list复制给scene视图，也可以直接在scene视图拉对应地块的脚本
    {
        if (tourPointsSettingMode)
        {
            GetGroundGrid().CanMoveColorChange(GetGroundGrid(), movePoint);
            Ray ray = UnitActionSystem.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
            // 检测是否点击单位
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, UnitActionSystem.Instance.groundLayer) && Input.GetKeyDown(KeyCode.Mouse1))
            {
                tourPoints.Add(hit.collider.GetComponent<GridSettings>());
                GetGroundGrid().MovedColorRestore(GetGroundGrid());
                transform.position = new Vector3(hit.collider.transform.position.x, hit.collider.transform.position.y + 0.8f, hit.collider.transform.position.z);
                GetGroundGrid().CanMoveColorChange(GetGroundGrid(), movePoint);
            }
        }
    }
    public void TourAchieve()
    {
        if (tourPoints == null)
        {
            Debug.Log("Tour point not set");
        }
        else
        {
            isRunning = true;
            StartCoroutine(TourMoveLogic());
        }

    }
    private IEnumerator TourMoveLogic()
    //逻辑是提前给一个闭环的巡游list，敌人走完一个点就删一个点，删完了重新赋值，当前位置到下一个点之间用寻路来走
    //！！！
    //一个重要的问题是这个函数不能没有正在执行的判定，没有isRunning来阻止TourAchieve的调用的话，每帧都会有一个携程实例执行这个代码块，控制的还是同一个实例
    //导致的问题是物体跑的贼快，每行代码被疯狂执行，导致各种匪夷所思的错误，好奇的话可以把isRunning删了看看，比如一次跑完所有路径，倒欠几千行动点之类的东西
    {
        if (Points.Count == 0)
        {
            Points = new List<GridSettings>(tourPoints);
        }
        bool turnOut = false;
        GridSettings point = Points[0];
        Vector3 nowPos = transform.position;
        Vector3 targetPos = point.transform.position;
        targetPos.y = nowPos.y;
        //设置两点的坐标
        GetGroundGrid().occupiedbyEnemy = false;
        //走之前先把格子设为未占据
        List<Vector3Int> pathBetweenTwoPoints = GridAchieve.instance.PathFind(point.transform.position);
        foreach (Vector3Int movePath in pathBetweenTwoPoints)
        {
            Vector3 targetPosBetweenTwoPoints = GridAchieve.instance.allGridPos[movePath].transform.position;
            targetPosBetweenTwoPoints.y = transform.position.y; // 保持Y轴一致

            // 逐步移动到当前路径点
            while (Vector3.Distance(transform.position, targetPosBetweenTwoPoints) > .1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosBetweenTwoPoints, (UnitActionSystem.Instance.moveSpeed) * Time.deltaTime);
                yield return null;
            }
            GetGroundGrid().UnitScene<Unit>(GetGroundGrid(), sceneRange, out unitList);
            GetGroundGrid().UnitScene<Enemy>(GetGroundGrid(), sceneRange, out enemyList);
            movePoint -= GetGroundGrid().GridMoveInformation();
            if (movePoint == 0)
                //防止路径被阻挡时硬走完路导致倒欠移动点
                break;
            if (unitList.Count != 0)
            {
                turnOut = true;
                TargetChoose();
                break;
            }
        }
        if (!turnOut)
        {
            Points.Remove(point);
            EnemyMoveOver();
            GetGroundGrid().occupiedbyEnemy = true;
            //到终点之后将脚底物块设为占据
            isRunning = false;
        }
        else if (turnOut) 
        {
            turnOut = true;
            GetGroundGrid().occupiedbyEnemy = true;
            MakeDecision();
        }
    }

    private void EnemyMoveOver()//结束单位行动状态
    {
        hasActed = true;
        isMoving = false;
        Move = false;
    }
    #endregion
    private void TargetChoose() 
    {
        if (unitList.Count != 0) 
        {
            targetUnit = unitList[0];
        }
    }
    public void MakeDecision()//敌人决策设置
    {
        CalculateCPV();
        if (targetUnit != null)
        {
            float attackThreshold = 0.3f + 0.1f * (1 - (stat.currenthealth / stat.maxhealth.GetValue()));
            float retreatThreshold = 0.2f - 0.1f * (stat.damage.GetValue() / 100f);
            if (targetUnit != null)
            {
                attackThreshold -= 0.1f * targetUnit.GetComponent<CharacterStat>().currenthealth / targetUnit.GetComponent<CharacterStat>().maxhealth.GetValue();
            }
            //Debug.Log(attackThreshold);
            //Debug.Log(retreatThreshold);
            if (CPV > attackThreshold)
            {
                Debug.Log("Attack");
                ExecuteAttack();
            }
            else if (CPV > retreatThreshold)
            {
                Debug.Log("Defense");
                ExecuteDefensiveMove();
            }
            else
            {
                Debug.Log("elseDefense");
                ExecuteDefensiveMove();
            }
        }
        else 
        {
            if (Move && !isRunning)
            {
                TourAchieve();
            }
        }
    }

    private void ExecuteAttack() 
    {
        if (GetManhattanDistance(gridPosition,targetUnit.gridPosition) > maxAttackRange || !AttackRangeCheck()) 
        {
            int minDis = int.MaxValue;
            GridSettings Targetgrid = null;
            foreach (var grid in targetUnit.GetGroundGrid().gridList)
            {
                int Dis = GetManhattanDistance(gridPosition, grid.gridCellPosition);
                if (Dis < minDis)
                {
                    minDis = Dis;
                    Targetgrid = grid;
                }
            }
            movePath = GridAchieve.instance.PathFind(Targetgrid.transform.position);
            StartCoroutine(TryGetToGrid());
        }
        if (AttackRangeCheck())
            SkillAttack(chosedSkill);
    }
    private IEnumerator TryGetToGrid()
    {
        foreach (var moveGrid in movePath)
        {
            Vector3 targetPosBetweenTwoPoints = GridAchieve.instance.allGridPos[moveGrid].transform.position;
            targetPosBetweenTwoPoints.y = transform.position.y; // 保持Y轴一致
            GetGroundGrid().occupiedbyEnemy = false;
            // 逐步移动到当前路径点
            while (Vector3.Distance(transform.position, targetPosBetweenTwoPoints) > .1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosBetweenTwoPoints, (UnitActionSystem.Instance.moveSpeed) * Time.deltaTime);
                yield return null;
            }
            GetGroundGrid().UnitScene<Unit>(GetGroundGrid(), sceneRange, out unitList);
            GetGroundGrid().UnitScene<Enemy>(GetGroundGrid(), sceneRange, out enemyList);
            movePoint -= GetGroundGrid().GridMoveInformation();
            if (movePoint == 0) 
            {
                EnemyMoveOver();
                GetGroundGrid().occupiedbyEnemy = true;
                break;
            }
            if (AttackRangeCheck()) 
            {
                GetGroundGrid().occupiedbyEnemy = true;
                SkillAttack(chosedSkill);
            }
        }
    }
    private bool AttackRangeCheck() 
    {
        Skill bestSkill = null;
        float bestSKillUtility = 0f;
        foreach (Skill skill in skills) 
        {
            float Utility = CalculateUtility(stat,skill);
            if(bestSKillUtility < Utility) 
            {
                bestSKillUtility = Utility;
                bestSkill = skill;
            }
        }
        chosedSkill = bestSkill;
        AttackManager.instance.EPrepareExecuteSkill(chosedSkill,this);
        if (targetUnit.GetGroundGrid().isInAttackRange)
        {
            chosedSkill = bestSkill;
            return true;
        }
        return false;
    }

    public float CalculateUtility(CharacterStat stat,Skill skill)
    {
        if (!isReady) 
            return 0f;

        float utility = 1f;
        float distanceRatio = Mathf.Clamp01(skill.attackRange / GetManhattanDistance(gridPosition, targetUnit.gridPosition));
        // 伤害需求曲线
        float damageWeight = Mathf.Lerp(0.5f, 1.5f, 1 - targetUnit.GetComponent<CharacterStat>().currenthealth / targetUnit.GetComponent<CharacterStat>().maxhealth.GetValue());
        utility += (skill.damage + stat.damage.GetValue() + skill.firedamage + skill.icedamage)* skill.skillDamageFix * damageWeight * distanceRatio;
        // 治疗需求
        float healWeight = Mathf.Lerp(0, 2f, (0.3f - stat.currenthealth/stat.maxhealth.GetValue()) / 0.3f);
        utility += healWeight;
        // 距离适配（根据角色类型调整）
        float rangeBias = isMelee ?Mathf.Pow(distanceRatio, 2) :1 - Mathf.Pow(distanceRatio, 3); // 近战和远程偏好安全距离计算        
        // 资源管理（法力值保留阈值）
        float manaPreserve = Mathf.Clamp01(((float)stat.currentMana - (float)skill.manaCost) / (float)stat.maxMana.GetValue());
        return utility * rangeBias * manaPreserve;
    }
    private void SkillAttack(Skill skill)
    {
        if (hasActed)
            return;
        Debug.Log("SkillUsed");
        targetUnit.GetComponent<CharacterStat>().DoDamage(skill, stat);
        targetUnit.GetComponentInChildren<HealthBarUI>().UpdateHealthUI();
        AttackManager.instance.CanMoveColorRestore();
        EnemyMoveOver();
    }


    private void ExecuteDefensiveMove() 
        //逻辑 选最远的攻击技能距离进行移动，再进行攻击判定,有队友在身边时，找距离敌人最远的那一个靠近，走出了攻击距离就固定用buff技能
    {
        GetGroundGrid().UnitScene<Unit>(GetGroundGrid(), sceneRange, out unitList);
        GetGroundGrid().UnitScene<Enemy>(GetGroundGrid(), sceneRange, out enemyList);
        TryDefenseMove();
        /* 防御移动 */ 
    }

    private void TryDefenseMove()
    {
        if (enemyList.Count > 0)
        {
            Debug.Log("list");
            float maxDis = 0;
            Vector3 targetPos = Vector3.up;
            foreach (var unit in enemyList)
            {
                Vector3Int sePos = gridPosition;
                float CDis = GetManhattanDistance(sePos, unit.gridPosition);
                if (CDis > maxDis)
                {
                    maxDis = CDis;
                    targetPos = unit.GetGroundGrid().transform.position;
                    Debug.Log(targetPos);
                }
            }
            StartCoroutine("DenfenceMoveToUnit", targetPos);
        }
        else 
        {
            StartCoroutine("DenfenceMoveToUnit", GetGroundGrid().DefenceGridSelect(GetGroundGrid(), targetUnit.GetGroundGrid(), maxAttackRange).transform.position);
            //GridAchieve.instance.PathFind(GetGroundGrid().DefenceGridSelect(GetGroundGrid(),targetUnit.GetGroundGrid(),maxAttackRange).transform.position);
        }
    }

    private IEnumerator DenfenceMoveToUnit(Vector3 targetPos)
    {
        Debug.Log("startMove");
        bool turnOut = false;
        Vector3 nowPos = transform.position;
        targetPos.y = nowPos.y;
        //设置两点的坐标
        GetGroundGrid().occupiedbyEnemy = false;
        //走之前先把格子设为未占据
        List<Vector3Int> pathBetweenTwoPoints = GridAchieve.instance.PathFind(targetPos);
        foreach (Vector3Int movePath in pathBetweenTwoPoints)
        {
            Vector3 targetPosBetweenTwoPoints = GridAchieve.instance.allGridPos[movePath].transform.position;
            targetPosBetweenTwoPoints.y = transform.position.y; // 保持Y轴一致

            // 逐步移动到当前路径点
            while (Vector3.Distance(transform.position, targetPosBetweenTwoPoints) > .1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosBetweenTwoPoints, (UnitActionSystem.Instance.moveSpeed) * Time.deltaTime);
                yield return null;
            }
            GetGroundGrid().UnitScene<Unit>(GetGroundGrid(), sceneRange, out unitList);
            GetGroundGrid().UnitScene<Enemy>(GetGroundGrid(), sceneRange, out enemyList);
            movePoint -= GetGroundGrid().GridMoveInformation();
            if (movePoint == 0)
                //防止路径被阻挡时硬走完路导致倒欠移动点
                break;
            if ((float)stat.currenthealth / (float)stat.maxhealth.GetValue() <= 0.3) 
                //血量过低概率直接停下
            {
                if (UnityEngine.Random.Range(0, 100) >= 50) 
                {
                    turnOut = true;
                    if(ChosedSkillRangeCheck(farestSkill))
                        SkillAttack(farestSkill);
                }
            }
        }
        GetGroundGrid().occupiedbyEnemy = true;
        //到终点之后将脚底物块设为占据
        if (!turnOut)
        {
            EnemyMoveOver();
            isRunning = false;
        }
        else if (turnOut)
        {
            Debug.Log("Stop");
            turnOut = false;
            isRunning = false;
            if (AttackRangeCheck())
                SkillAttack(farestSkill);
            else
                EnemyMoveOver();
        }
    }
    private bool ChosedSkillRangeCheck(Skill skill)
    {
        AttackManager.instance.EPrepareExecuteSkill(chosedSkill, this);
        if (targetUnit.GetGroundGrid().isInAttackRange)
        {
            chosedSkill = skill;
            return true;
        }
        return false;
    }
    private void CalculateCPV()
    {
        currentBeta = CalculateDynamicBeta();//防御权重的设置
        currentAlpha = basealpha * CalculateDistanceFactor();
        currentgamma = basegamma;
        float healthRatio = Mathf.Clamp01(stat.currenthealth / stat.maxhealth.GetValue());
        float atkRatio = Mathf.Clamp01(stat.damage.GetValue() / 20f); // 假设基准攻击20
        float defRatio = Mathf.Clamp01((stat.armor.GetValue() + stat.magicresistance.GetValue()) / 20f);  // 假设基准防御20
        CPV = (currentAlpha * atkRatio) +(currentBeta * defRatio) -(currentgamma * (1 - healthRatio)); ;
    }
    private float CalculateDynamicBeta()
    {
        float allyWeight = enemyList.Count * allySupportFactor;
        float enemyWeight = unitList.Count * enemyInfluence;

        // 基础防御权重 + 敌人压力 - 友军支援
        float rawBeta = basebeta - enemyWeight + allyWeight;
        return Mathf.Clamp(rawBeta, 0.1f, 0.8f); 
    }
    private float CalculateDistanceFactor()
    {
        if (enemyList.Count == 0) 
            return 1f;

        // 计算到最近敌人的曼哈顿距离
        int minDistance = int.MaxValue;
        foreach (Unit unit in unitList)
        {
            int distance = GetManhattanDistance(unit.gridPosition, gridPosition);
            minDistance = Mathf.Min(minDistance, distance);
        }

        return GetNormalizedDistance(minDistance);
    }
    private float GetNormalizedDistance(int MahDis)
    {
        float Normalized = Mathf.Clamp01(1 - Mathf.Pow((float)MahDis / sceneRange, 0.5f));//用pow平方实现非线性的权重变化，.Clamp01将值限制在01之间                                                                                       
        return Mathf.Lerp(0.5f, 1.5f, Normalized);// 映射到0.5-1.5范围，近距离时系数更高
    }
    private int GetManhattanDistance(Vector3Int startPos, Vector3Int endPos)
    {
        return Mathf.Abs(startPos.x - endPos.x) + Mathf.Abs(startPos.z - endPos.z);
        //Abs求绝对值的，求出曼哈顿距离
    }
    public override void GridOccupiedChange() 
    {
        base.GridOccupiedChange();
        gridSettings.occupiedbyEnemy = false;
    }
}
