using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GridSettings : MonoBehaviour
{ 
    
    public enum GridType
    {
        plainGrid,//可移动格子，消耗一个移动点
        ruggedGrid,//可移动丘陵，消耗两个移动点
        hinderGrid,//不可移动阻碍格，无法通过,我想阻碍格直接不做就行了，没必要在这里弄 //现在想到寻路算法可能要用到，所以还是启用吧
        trapGrid,//可移动陷阱格，可通过
        interactGrid//可移动(?)交互格
    }
    [Header("占据检测")]
    public bool occupied;//是否被占据
    [SerializeField]private float checkDistance;
    [Header("地块信息")]
    public GridType gridType = GridType.plainGrid;
    [SerializeField] private int finalMovingCosts;
    public Vector3Int gridCellPosition;
    public bool isInMovementRange = false;
    [Header("移动约束")]
    public List<GridSettings> gridList = new List<GridSettings>();
    [SerializeField] private float gridCheckDistance;
    private Material originalMaterial;
    private Renderer rend;
    private Color originalColor;
    [Header("寻路相关")]
    // 初始化队列用于广度优先搜索,存储要访问的地块和剩余的移动点
    private Queue<(GridSettings gridSetting, int movepoint)> queue = new Queue<(GridSettings gridSettings, int remainingPoints)>();
    //Enqueue()方法往队列中加入一个值，Dequeue()让一个值出队，先进先出后进后出
    // 初始化访问集合，用于记录已经访问过的地图格
    private HashSet<GridSettings> visited = new HashSet<GridSettings>();
    //HashSet<int> set = new HashSet<int>(); 例子
    //set.UnionWith(list1);
    //set.UnionWith(list2);
    //UnionWith 方法将 list1 和 list2 中的元素添加到 HashSet<int> 中，HashSet<int> 会自动去除重复元素。豪用！


    private void Start()
    {
        rend = GetComponent<Renderer>();
        originalMaterial = rend.material;
        originalColor = rend.material.color;
        OccupiedCheck();
        GridTypeSettings();
        FourWayGridDetection();

    }
    private void GridTypeSettings()
    {
        switch (gridType)
        {
            case GridType.plainGrid:
                SetUpGridInformation(GridMoveCostSet.plainGridMoveCosts);
                break;
            case GridType.ruggedGrid:
                SetUpGridInformation(GridMoveCostSet.ruggedGridMoveCosts);
                break;
            case GridType.trapGrid:
                SetUpGridInformation(GridMoveCostSet.controlledGridMoveCosts);
                break;
            case GridType.interactGrid:
                SetUpGridInformation(GridMoveCostSet.hinderedGridMoveCosts);
                break;
        }
    }

    private void SetUpGridInformation(int moveCost) //设置grid类型后进行基础信息传值
    {
        finalMovingCosts = moveCost;
    }

    public int GridMoveInformation() //给其他脚本获取finalMovingCosts信息
    {
        return finalMovingCosts;
    }
    public void OccupiedCheck()
    {
        RaycastHit hit;//存储碰撞体信息
        occupied = Physics.Raycast(transform.position, Vector3.up, out hit, checkDistance);
        if (occupied)
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                occupied = true;
            }
            else
            {
                occupied = false;
            }
        }
    }

    

    public void CanMoveColorChange(GridSettings gridSettings,int movePoint) 
    {
        // 将角色当前所在的地图格加入队列，初始剩余行动点为角色的行动点
        queue.Enqueue((gridSettings, movePoint));
        visited.Add(gridSettings);
        // 标记当前地图格不在移动范围内（因为角色已经在该位置）
        gridSettings.isInMovementRange = false;

        // 开始BFS广度优先搜索
        while (queue.Count > 0)
        {
            // 从队列中取出一个地图格和对应的剩余行动点
            var (current, remaining) = queue.Dequeue();

            // 遍历当前地图格的周围的四个地块，进行依次访问
            foreach (GridSettings neighbor in current.gridList)
            {
                // 检查邻居是否未被访问过，并且剩余行动点足够移动到该邻居
                if (!visited.Contains(neighbor) && remaining >= neighbor.finalMovingCosts)
                {
                    // 标记该邻居在移动范围内
                    neighbor.isInMovementRange = true;

                    //高亮具象化 后面要改###########################
                    neighbor.rend.material.color = Color.red;
                    //##############################################

                    // 将该邻居加入访问集合
                    visited.Add(neighbor);
                    // 计算移动到该邻居后剩余的行动点
                    int newRemaining = remaining - neighbor.finalMovingCosts;
                    // 将该邻居和新的剩余行动点加入队列，继续搜索
                    queue.Enqueue((neighbor, newRemaining));
                }
            }
        }
    }

    public void MovedColorRestore(GridSettings gridSettings) //移动后恢复颜色
    {
        foreach (var a in visited)
        //用foreach就不让我在循环中清除值(不能访问一个删除一个)，用for的话visited作为HashSet集合是无序的没有索引，好在HashSet有个删除全部的方法
        {
            a.rend.material.color = originalColor;
        }
            visited.Clear();
            //将颜色还原的同时清空列表
    }

    public bool CanMoveCheck(GridSettings targetGrid) //约束单位只能进行一格范围内的移动
    {
        FourWayGridDetection();
        if (gridList.Contains(targetGrid))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void FourWayGridDetection()
    {
        gridList.Clear();

        // 检测四个方向并填充 gridList
        CheckDirection(Vector3.forward);
        CheckDirection(Vector3.back);
        CheckDirection(Vector3.left);
        CheckDirection(Vector3.right);
    }

    private void CheckDirection(Vector3 direction)//射线检测某个方向，检测到Grid就把他加到list里面
    {
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, gridCheckDistance))
        {
            if (hit.collider.CompareTag("ground"))
            {
                GridSettings grid = hit.collider.GetComponent<GridSettings>();
                if (grid != null)
                    gridList.Add(grid);
            }
        }
    }

    private void Update()
    {
        
    }

    private void OnDrawGizmos()//Physics.Raycast射线检测的可视化
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position,new Vector3(transform.position.x, transform.position.y+checkDistance,transform.position.z));
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y, transform.position.z + gridCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y, transform.position.z - gridCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x - gridCheckDistance, transform.position.y, transform.position.z));
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + gridCheckDistance, transform.position.y, transform.position.z));
    }


}
