using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
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
    [SerializeField]private float correctedDis;//人物卡进地面的距离修正
    [Header("路径列表")]
    public List<Vector3Int> pathList = new List<Vector3Int>();
    [SerializeField] private float moveSpeed;


    void Awake()
    {//单例模式
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }


    
    // 处理鼠标点击
    public void HandleMouseClick()
        //总觉得怪怪的，有点影响我其它功能实现，之后看有没有机会改了吧
        //############################################################
    {
        /* 忽略UI点击
        if (EventSystem.current.IsPointerOverGameObject())
        EventSystem.current.IsPointerOverGameObject()判断鼠标是否点击在UI上
        {
            return;
        }
        */
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            // 检测是否点击单位
            bool unitCatched = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, unitLayer);
            if (unitCatched)
            {//Physics.Raycast用于在场景中发射一条射线，并检测该射线是否与其他物体发生碰撞。
                Unit unit = hit.collider.GetComponent<Unit>();
                if (unit != null)
                {
                    SelectUnit(unit);
                    return;
                }
            }

            // 检测是否点击地面
            bool groundCatched = Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer);
            if (selectedUnit != null && groundCatched)
            {
                GridSettings targetGrid = hit.collider.GetComponent<GridSettings>();
                if (targetGrid.isInMovementRange)
                //调用脚底下单元格的CanMoveCheck方法传入鼠标选中的方格的脚本信息，限制移动距离
                {
                    if (!targetGrid.occupied && selectedUnit.movePoint != 0)
                    {
                        Vector3 movePos = new Vector3(hit.transform.position.x, hit.transform.position.y + correctedDis, hit.transform.position.z);//最终移动到的位置

                        selectedUnit.GridOccupiedChange();//将移动前位置的单元格设为未占据
                        UnitMove(movePos, selectedUnit, selectedUnit.GetGroundGrid());//移动实现
                    }
                    else
                    {
                        Debug.Log("Cant Move");
                        DeselectUnit();
                    }
                }
                else//写的有点臃肿了，这里复用了懒得改
                {
                    Debug.Log("Cant Move");
                    DeselectUnit();
                }
            }

            if (selectedUnit == true && !groundCatched && !unitCatched)
            {
                DeselectUnit();
            }
        }
    }

    private void UnitMove(Vector3 Mospos,Unit controlledUnit,GridSettings pathStart)
    {
        selectedUnit.GetGroundGrid().MovedColorRestore(selectedUnit.GetGroundGrid());//这个是移动时稳定在移动前进行颜色还原
        pathList=GridAchieve.instance.PathFind(Mospos);
        StartCoroutine(MoveAlongPath(selectedUnit));
        //不能直接调用MoveAlongPath方法而是用StartCoroutine来引用，不然出现不调用没反应的问题，携程不是很懂
    }
    private IEnumerator MoveAlongPath(Unit unit)
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
            if (selectedUnit.movePoint == 0) 
            {
                selectedUnit.hasActed = true;
                DeselectUnit();
            }
        }
    }   

    // 选中单位
    private void SelectUnit(Unit unit)
    {
        //若当前已有选中单位，先让此单位不被选中
        if (selectedUnit != null)
            selectedUnit.SetSelected(false);

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
        ShowActionMenu(false);
    }

    // 控制行动菜单显示
    private void ShowActionMenu(bool show)
    {
        if (actionMenuUI != null)
            actionMenuUI.SetActive(show);
    }
}