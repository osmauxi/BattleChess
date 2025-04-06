using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("状态")]
    public bool hasActed;       // 是否已行动
    public Vector3Int gridPosition;
    public int MaxMovepoint;
    //回合制管理器里需要刷新角色移动状态和移动点，所以要一个最大值存储，但是MaxMovepoint和movePoint都public总觉得不太好
    public int movePoint;

    [Header("属性")]
    public bool isMoving = false;
    public int sceneRange;

    [Header("初始化检测地面")]
    [SerializeField] private float checkDistance;
    public GridSettings gridSettings;//获取脚下的那块地的occupied，来实时更新，单纯觉得让格子每帧进行碰撞检测有点费电脑^W^
    private RaycastHit hit;
    [Header("攻击")]
    public bool canBeAttacked = false;
    public bool isAttacking = false;

    public List<Skill> skills = new List<Skill>();

    public List<Enemy> enemyList = new List<Enemy>();//两个list分别存敌人和友军
    public List<Unit> unitList = new List<Unit>();
    protected virtual void Start()
    {
        InitializeGroundGrid();
        GetGroundGrid();
        movePoint = MaxMovepoint;
    }
    protected virtual void Update()
    {
    }
    public void InitializeGroundGrid()//初始化检测一遍地面grid的并给脚本赋值，修复空值问题
                                       //名字有点问题，因为下面的方法也用了一次这个方法
    {
        if (Physics.Raycast(transform.position, Vector3.down, out hit, checkDistance))
        {
            if (hit.collider.gameObject.CompareTag("ground"))
            {
                gridSettings = hit.collider.GetComponent<GridSettings>();
                gridPosition = gridSettings.gridCellPosition;
            }
        }
    }
    public void MovedGroundCheck()//用于获取移动后当前地面的occupied值
    {
        bool check;//先检测上面有没有东西，再判断类型
        check = Physics.Raycast(transform.position, Vector3.down, out hit, checkDistance);
        if (check)
        {
            if (hit.collider.gameObject.CompareTag("ground"))
            {
                gridSettings = hit.collider.GetComponent<GridSettings>();
            }
        }
        else
        {
            Debug.LogError("未检测到有效地面！");
            gridSettings = null;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - checkDistance, transform.position.z));
    }

    public GridSettings GetGroundGrid() // 给其他脚本获取当前角色脚底的Grid
    {
        InitializeGroundGrid();//传值之前再检测一次对应的grid，防止检测地面格和实际位置不一致
        return gridSettings;
    }

    public void GridOccupiedChange() //修改当前脚下的gird的occupied值
    {
        MovedGroundCheck();
        gridSettings.occupied = false;
    }
}
