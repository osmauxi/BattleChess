using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.UI.CanvasScaler;

public class UnitActionSystem : MonoBehaviour
{
    public static UnitActionSystem Instance; // 单例

    [Header("引用")]
    public Camera mainCamera;           // 主摄像机
    public LayerMask unitLayer;         // 单位层级
    public LayerMask groundLayer;       // 地面层级
    public GameObject actionMenuUI;     // 行动菜单UI

    public Unit selectedUnit;          // 当前选中单位
    public Enemy choosedEne; // 当前技能选中敌人
    public Unit choosedUnit; // 当前技能选中友军
    [SerializeField]private float correctedDis;//人物卡进地面的距离修正
    [Header("路径列表")]
    public List<Vector3Int> pathList = new List<Vector3Int>();
    public float moveSpeed;

    private bool unitCatched;
    private Ray ray;
    private RaycastHit hit;
    private GridSettings currentGrid;

    private AttackManager attackManager => AttackManager.instance;//动态获取单例内存
    void Awake()
    {//单例模式
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    public void HandleSelectClick()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        //判断鼠标是否点击在UI上
        {
            return;
        }
        ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Input.GetMouseButtonDown(0))
        {
            // 检测是否点击单位
            unitCatched = Physics.Raycast(ray, out hit, Mathf.Infinity, unitLayer);
            if (unitCatched)
            {//Physics.Raycast用于在场景中发射一条射线，并检测该射线是否与其他物体发生碰撞。
                Unit unit = hit.collider.GetComponent<Unit>();
                if (unit != null)
                {
                    SelectUnit(unit);
                    return;
                }
            }
        }
    }

    private void Update()
    {
        ShowPath();
        //考虑过放到FixedUpdate里面来降低函数的调用次数，但是这样的话放开右键能不能检测到就看运气了
    }

    private void ShowPath()//单位选中后，显示移动范围，每帧获取选中地块的位置，然后结合寻路显示路径
    {
        GridSettings pastGrid;
        if(selectedUnit != null)
        {
            if (Input.GetMouseButton(1))
            {
                //有一些赋值上的问题，所以这东西很多不能写成局部变量，很多要提出函数体
                ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                bool isOnGround = Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer);
                pastGrid = currentGrid;
                if (isOnGround)
                {
                    currentGrid = hit.collider.GetComponent<GridSettings>();
                    if (currentGrid.isInMovementRange)
                    {
                        if (BattleInfUISet.Instance.EnemyUIVisible) 
                        {
                            BattleInfUISet.Instance.SetEnemyUIVisible(false,choosedEne);
                        }
                        GridSettings grid;
                        if (pastGrid != currentGrid)
                        {
                            grid = selectedUnit.GetGroundGrid();
                            grid.PathColorRestore(pathList);
                        }
                        GetPathAndChangeColor();
                    }
                    else if (currentGrid.occupiedbyEnemy && currentGrid.isInAttackRange) //右键敌人地块
                    {
                        choosedEne = currentGrid.GetTargetAbove<Enemy>();
                        BattleInfUISet.Instance.SetEnemyUIVisible(true,choosedEne);
                    }
                    else
                    {
                        //后面可以写个改变鼠标的函数表示位置不对
                    }

                }
            }
            else if (Input.GetMouseButtonUp(1))
            {
                GridSettings grid = selectedUnit.GetGroundGrid();
                //实在不想把一个语句写两遍啊....
                grid.PathColorRestore(pathList);
                GroundMoveCheck(ray, unitCatched);
                grid.GridPathStateChange();
            }
        }
    }

    private void GetPathAndChangeColor()
    {
        pathList = GridAchieve.instance.PathFind(hit.collider.transform.position);
        selectedUnit.GetGroundGrid().PathColorChange(pathList);
    }


    private void GroundMoveCheck(Ray ray, bool unitCatched)
    {
        RaycastHit hit;
        // 检测是否点击地面
        bool groundCatched = Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer);
        if (selectedUnit != null && groundCatched && !selectedUnit.isMoving)
        {
            GridSettings targetGrid = hit.collider.GetComponent<GridSettings>();
            if (selectedUnit.movePoint != 0) 
            {
                if (targetGrid.isInMovementRange & !targetGrid.occupiedbyUnit && !targetGrid.occupiedbyEnemy)
                //调用脚底下单元格的CanMoveCheck方法传入鼠标选中的方格的脚本信息，限制移动距离
                {
                    Vector3 movePos = new Vector3(hit.transform.position.x, hit.transform.position.y + correctedDis, hit.transform.position.z);//最终移动到的位置
                    selectedUnit.isMoving = true;
                    selectedUnit.GridOccupiedChange();//将移动前位置的单元格设为未占据
                    UnitMove(movePos, selectedUnit, selectedUnit.GetGroundGrid(), targetGrid);//移动实现
                }
                else if(targetGrid.occupiedbyUnit || targetGrid.occupiedbyEnemy && targetGrid.isInAttackRange)
                {
                    SkillUse(attackManager.skill, targetGrid);
                    selectedUnit.movePoint = 0;//攻击清零移动点
                    selectedUnit.hasActed = true;
                    DeselectUnit();
                }
            }
        }
        if (selectedUnit == true && !groundCatched && !unitCatched)
        {
            DeselectUnit();
            attackManager.CanMoveColorRestore();
        }

    }

    public void SkillUse(Skill skill,GridSettings targetGrid) //给attackmanager传skill到这个方法里进行使用
    {
        selectedUnit.state.stateMachine.ChangeState(selectedUnit.state.attackState);
        if (attackManager.WhatSkillType) //对敌
        {
            if (skill.range == AttackRangeType.Straight)
            {
                choosedEne.GetComponent<EnemyStat>().DoDamage(skill, selectedUnit.GetComponent<CharacterStat>());
                choosedEne.GetComponentInChildren<HealthBarUI>().UpdateHealthUI();
                AttackAllUnitOnPath(skill, targetGrid);
            }
            else 
            {
                choosedEne.GetComponent<EnemyStat>().DoDamage(skill,selectedUnit.GetComponent<CharacterStat>());
                choosedEne.GetComponentInChildren<HealthBarUI>().UpdateHealthUI();
            }
            BattleInfUISet.Instance.SetEnemyUIVisible(false, choosedEne);
        }
        else if (!attackManager.WhatSkillType) 

        {
            choosedUnit = targetGrid.GetTargetAbove<Unit>();
            //这里还没写回血函数
        }
        selectedUnit.state.stateMachine.ChangeState(selectedUnit.state.attackState);
        attackManager.AttackedStateRestore();
        attackManager.CanMoveColorRestore();
        selectedUnit.GetComponent<UnitStat>().currentMana -= skill.manaCost;//减少释放者的蓝
    }

    private void AttackAllUnitOnPath(Skill skill, GridSettings targetGrid)
    {
        Vector3Int targetPos = targetGrid.gridCellPosition;
        Vector3Int originPos = selectedUnit.GetGroundGrid().gridCellPosition;
        int GameProtect = 0;
        while (targetPos != originPos)
        {
            Vector3Int pos = originPos - targetGrid.gridCellPosition;
            GameProtect++;
            foreach (var a in targetGrid.gridList)
            {
                Vector3Int P = originPos - a.gridCellPosition;
                if (a.gridCellPosition == originPos) 
                {
                    targetPos = originPos;
                    break;
                }
                if ((pos.x == 0 && pos.z != 0 && P.x == 0 && pos.z - P.z == -1) || (pos.z == 0 && pos.x != 0 && P.z == 0 && pos.x - P.x == 1))
                {//不知道为什么z轴减下来是-1
                    targetGrid = a;
                    targetPos = a.gridCellPosition;
                    if (a.occupiedbyEnemy)
                    {
                        a.GetTargetAbove<Enemy>().GetComponent<EnemyStat>().DoDamage(skill, selectedUnit.GetComponent<CharacterStat>());
                        a.GetTargetAbove<Enemy>().GetComponentInChildren<HealthBarUI>().UpdateHealthUI();
                    }
                    else if (a.occupiedbyUnit)
                    {
                        a.GetTargetAbove<Unit>().GetComponent<UnitStat>().DoDamage(skill, selectedUnit.GetComponent<CharacterStat>());
                        a.GetTargetAbove<Unit>().GetComponentInChildren<HealthBarUI>().UpdateHealthUI();
                    }
                }
            }
            if (GameProtect >= 20) 
                //防止出问题无限循环卡死游戏
            {
                Debug.Log("Wrong");
                break;
            }
        }
    }

    private void UnitMove(Vector3 Mospos,Unit controlledUnit,GridSettings pathStart,GridSettings pathend)
    {
        selectedUnit.GetGroundGrid().MovedColorRestore(selectedUnit.GetGroundGrid());//这个是移动时稳定在移动前进行颜色还原
        StartCoroutine(MoveAlongPath(selectedUnit,pathend,selectedUnit));
        //不能直接调用MoveAlongPath方法而是用StartCoroutine来引用，不然出现不调用没反应的问题，携程不是很懂
        selectedUnit.state.stateMachine.ChangeState(selectedUnit.state.moveState);

    }
    private IEnumerator MoveAlongPath(Unit unit,GridSettings pathend,Unit controlledUnit)
        //解决鼠标连点卡死的问题，要事前储存selectedUnit和终点的GridSetting脚本，玩家好难伺候
    {
    // 普通方法而非协程。在 while 循环中直接连续执行移动逻辑时，Unity 的主线程会被完全阻塞,导致没画面卡死
        foreach (Vector3Int path in pathList)
        {
            Vector3 targetPos = GridAchieve.instance.allGridPos[path].transform.position;
            targetPos.y = unit.transform.position.y; // 保持Y轴一致

            // 逐步移动到当前路径点
            while(Vector3.Distance(unit.transform.position, targetPos) > 0.1f)
            {
                unit.transform.position = Vector3.MoveTowards(unit.transform.position,targetPos,moveSpeed * Time.deltaTime);
                yield return null;
            }
            int moveCost = GridAchieve.instance.allGridPos[path].GridMoveInformation();
             // 移动完成后扣除移动点数
            unit.movePoint -= moveCost;
            if (BattleInfUISet.Instance.AllayUIVisible)
                BattleInfUISet.Instance.updateAllayText();
        }
        controlledUnit.isMoving = false;
        controlledUnit.GetGroundGrid().occupiedbyUnit = true;
        //到终点之后将脚底物块设为占据
        //selectedUnit.GetGroundGrid().UnitScene<Unit>(selectedUnit.GetGroundGrid(),selectedUnit.sceneRange,out selectedUnit.unitList);
        //selectedUnit.GetGroundGrid().UnitScene<Enemy>(selectedUnit.GetGroundGrid(), selectedUnit.sceneRange, out selectedUnit.enemyList);
        selectedUnit.state.stateMachine.ChangeState(selectedUnit.state.idleState);
        if (controlledUnit.movePoint == 0)        
        {
            controlledUnit.hasActed = true;
            DeselectUnit();
        }
        pathend.CanMoveColorChange(pathend, controlledUnit.movePoint);
    }

    private bool CheckAnyMoving() 
        //有角色移动时卡好时间选中另一个角色走一样的位置会使两个角色叠在一个地块上
    {
        foreach (var unit in TurnBasedManager.Instance.playerUnits) 
        {
            if(unit.isMoving)
                return true;
        }
        return false;
    }

    // 选中单位
    private void SelectUnit(Unit unit)
    {
        //若当前已有选中单位，先让此单位不被选中
        if (selectedUnit != null) 
        {
            BattleInfUISet.Instance.SetAllayUIVisible(true);
            selectedUnit.SetSelected(false);
        }
        if (unit.isMoving || CheckAnyMoving()) 
        {
            //防止在移动时鼠标连点选中移动的单位
            return;
        }
        selectedUnit = unit;
        selectedUnit.SetSelected(true);
        ShowActionMenu(true);
    }

    // 取消选中
    private void DeselectUnit()
    {
        if (selectedUnit != null)
        {
            selectedUnit.SetSelected(false);
            selectedUnit = null;
        }
        if (AttackManager.instance.colorChangedGrids.Count != 0)
            AttackManager.instance.AttackedStateRestore();
        BattleInfUISet.Instance.SetAllayUIVisible(false);
        ShowActionMenu(false);
    }

    // 控制行动菜单显示
    private void ShowActionMenu(bool show)
    {
        if (actionMenuUI != null)
            actionMenuUI.SetActive(show);
    }

    //四个技能对应四个按钮，使用的话要加入
}