using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("状态")]
    public bool hasMoved;       // 是否已移动
    public bool hasActed;       // 是否已行动
    public Vector2Int gridPosition;
    public int movePoint;

    [Header("属性")]
    public int moveRange = 5;   // 移动范围
    public int attackRange = 2; // 攻击范围

    [Header("初始化检测地面")]
    [SerializeField] private float checkDistance;
    public GridSettings gridSettings;//获取脚下的那块地的occupied，来实时更新，单纯觉得让格子每帧进行碰撞检测有点费电脑^W^
    private RaycastHit hit;

    public List<GameObject> enemyList = new List<GameObject>();//两个list分别存敌人和友军
    public List<GameObject> allyList = new List<GameObject>();
    [SerializeField] private float attackRadius;
    protected virtual void Start()
    {
        TargetCheck();
    }
    public void InitializeGroundGrid()//初始化检测一遍地面grid的并给脚本赋值，修复空值问题
                                       //名字有点问题，因为下面的方法也用了一次这个方法
    {
        if (Physics.Raycast(transform.position, Vector3.down, out hit, checkDistance))
        {
            if (hit.collider.gameObject.CompareTag("ground"))
            {
                gridSettings = hit.collider.GetComponent<GridSettings>();
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

    //移动过程中进行敌人与友军检测并组成列表
    public void TargetCheck() 
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRadius);
        foreach (var units in colliders) 
        {
            //因为敌人也要分友军和敌人（考虑敌人会有支援技能？）所以两个列表给敌人和友军单位公用，Player单纯是tag自带我没改，直接当友军单位的tag了
            if (units.CompareTag("Player") && gameObject.tag == "Player" || units.CompareTag("Enemy") && gameObject.tag == "Enemy")
            {
                allyList.Add(units.gameObject);
            }
            else if (units.CompareTag("Enemy") && gameObject.tag == "Player" || units.CompareTag("Player") && gameObject.tag == "Enemy") 
            {
                enemyList.Add(units.gameObject);
            }
        }
    }
    //！！！！！！！！！！！！！！！！！
    //A*寻路写好之后把unit的移动逻辑重写
    //！！！！！！！！！！！！！！！！！
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - checkDistance, transform.position.z));
    }
}
