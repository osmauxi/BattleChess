using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//回合制管理器
// 回合状态枚举
public enum TurnState
{
    PlayerTurnStart,    // 玩家回合开始
    PlayerControlled,   // 玩家控制阶段
    EnemyTurnStart,     // 敌人回合开始
    EnemyUnitActing,    // 敌人单位行动中
    TurnEnding          // 回合结束过渡
}

// 单位基础类
[System.Serializable]
//Serializable: 用于 自定义的，非 abstract 的类. 结构体等 , 使这类型也能序列化
//SerializeField: 用于 非public 类型(如private),  使非public 类型也能序列化
public class TurnBasedManager : MonoBehaviour
{
    // 单例实例
    public static TurnBasedManager Instance { get; private set; }

    [Header("回合设置")]
    public float aiActionDelay = 0.5f;  // AI行动间隔
    public int currentRound = 1;        // 当前回合数

    // 单位列表存储需要进行移动的所有单位
    [SerializeField] private List<Unit> playerUnits = new List<Unit>();
    [SerializeField] private List<Enemy> enemyUnits = new List<Enemy>();

    // 当前状态
    [SerializeField] private TurnState currentState = TurnState.PlayerTurnStart;

    void Awake()
    {
        // 单例模式初始化
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 初始化单位列表
        FindAllUnits();
        StartCoroutine(TurnCycle());
    }

    // 主回合循环协程
    private IEnumerator TurnCycle()
    //想要让一个物体逐渐消失，我们希望方法可以一次调用便可在程序后续执行中实现我们想要的效果
    {
        while (true)
        {
            switch (currentState)
            {
                case TurnState.PlayerTurnStart:
                    yield return HandlePlayerTurnStart();
                    break;

                case TurnState.PlayerControlled:
                    yield return HandlePlayerUnitSelection();
                    break;

                case TurnState.EnemyTurnStart:
                    yield return HandleEnemyTurnStart();
                    break;

                case TurnState.EnemyUnitActing:
                    yield return HandleEnemyActions();
                    break;

                case TurnState.TurnEnding:
                    yield return HandleTurnEnd();
                    break;
            }
            yield return null;
        }
    }

    #region 玩家回合处理
    // 玩家回合开始阶段
    private IEnumerator HandlePlayerTurnStart()//先重置
    {
        Debug.Log($"玩家回合开始 - 第{currentRound}回合");

        // 重置所有玩家单位状态
        foreach (var unit in playerUnits)
        {
            unit.GetComponent<Unit>().hasActed = false;
        }

        // 切换到单位选择状态
        currentState = TurnState.PlayerControlled;
        yield return null;
    }

    // 玩家单位选择阶段
    private IEnumerator HandlePlayerUnitSelection()
    {
        while (!CheckAllPlayerUnitsActed())
        {
            UnitActionSystem.Instance.HandleMouseClick();
            yield return null;
            //yield return null;让脚本停顿一帧，这里就是所有单位没有移动完就一直等
        }
        currentState = TurnState.EnemyTurnStart;
    }
    #endregion

    #region 敌人回合处理
    // 敌人回合开始阶段
    private IEnumerator HandleEnemyTurnStart()
    {
        Debug.Log($"敌人回合开始");
        currentState = TurnState.EnemyUnitActing;
        yield return new WaitForSeconds(1f); // 过渡等待
    }

    // 处理所有敌人行动
    private IEnumerator HandleEnemyActions()
    {
        foreach (var enemy in enemyUnits)
        {
            // 跳过被消灭的单位
            if (enemy == null) continue;

            // 执行AI决策
            yield return StartCoroutine(ExecuteEnemyAI(enemy));

            // 行动间隔
            yield return new WaitForSeconds(aiActionDelay);
        }

        // 结束回合
        currentState = TurnState.TurnEnding;
    }

    // 执行单个敌人AI逻辑
    private IEnumerator ExecuteEnemyAI(Enemy enemy)
    {

        // 简单的AI示例：移动到最近玩家单位
        Unit nearestPlayer = FindNearestPlayerUnit(enemy.transform.position);

        if (nearestPlayer != null)
        {
            /*
            // 计算移动路径
            List<Vector2Int> path = Pathfinding.FindPath(
                enemy.gridPosition,
                nearestPlayer.gridPosition,
                enemy.movementRange
            );

            // 执行移动
            if (path != null && path.Count > 0)
            {
                yield return StartCoroutine(MoveUnitAlongPath(enemy, path));
            }
        */
        }

        // 可以在此处添加攻击逻辑
        yield return null;

    }
    #endregion

    #region 通用方法
    // 初始化单位列表
    private void FindAllUnits()
    {
        FindAllAlly();
        FindAllEnemy();
    }
    //想写泛型的，结果FindObjectsOfType不接受T做参
    private void FindAllAlly()
    {
        Unit[] allUnits = FindObjectsOfType<Unit>();
        //获取当前场景中所有Unit脚本的实例并加入表中，哈人的功能
        foreach (var unit in allUnits)
        {
            if (unit is Unit)
                playerUnits.Add(unit);
        }
    }
    private void FindAllEnemy()
    {
        Enemy[] allUnits = FindObjectsOfType<Enemy>();
        //获取当前场景中所有Unit脚本的实例并加入表中，哈人的功能
        foreach (var unit in allUnits)
        {
            if (unit is Enemy)
                enemyUnits.Add(unit);
        }
    }

    // 检查所有玩家单位是否完成行动
    private bool CheckAllPlayerUnitsActed()
    {
        foreach (var unit in playerUnits)
        {
            if (!unit.hasActed) return false;
        }
        return true;
    }
    /*
    // 单位沿路径移动协程
    private IEnumerator MoveUnitAlongPath(Unit unit, List<Vector2Int> path)
    {

        foreach (var step in path)
        {
            Vector3 targetPos = GridSystem.Instance.GridToWorld(step);
            while (Vector3.Distance(unit.transform.position, targetPos) > 0.1f)
            {
                unit.transform.position = Vector3.MoveTowards(
                    unit.transform.position,
                    targetPos,
                    5f * Time.deltaTime
                );

                yield return null;
            }
            unit.gridPosition = step;
        }
    }
    */
    // 查找最近玩家单位
    private Unit FindNearestPlayerUnit(Vector3 enemyPosition)
    {
        Unit nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (var playerUnit in playerUnits)
        {
            float dist = Vector3.Distance(enemyPosition, playerUnit.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = playerUnit;
            }
        }
        return nearest;
    }

    // 回合结束处理
    private IEnumerator HandleTurnEnd()
    {
        Debug.Log($"第{currentRound}回合结束");
        currentRound++;
        yield return new WaitForSeconds(1f);
        currentState = TurnState.PlayerTurnStart; // 开始新回合
    }
    #endregion
}