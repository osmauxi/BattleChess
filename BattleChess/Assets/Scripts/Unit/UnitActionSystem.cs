using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

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
                        UnitMove(movePos, selectedUnit, targetGrid.GridMoveInformation());//移动实现
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

    private void UnitMove(Vector3 Mospos,Unit controlledUnit,int moveCost)
    {
        selectedUnit.GetGroundGrid().MovedColorRestore(selectedUnit.GetGroundGrid());//这个是移动时稳定在移动前进行颜色还原
        pathList=GridAchieve.instance.PathFind(Mospos);
        //这个后续换成更具体的移动
        //controlledUnit.transform.position = Mospos;
        //！！！！！！！！！！！！！！！！！！！！！
        selectedUnit.movePoint -= moveCost;
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