using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class Enemy : Entity
{
    [Header("巡视设置")]
    [SerializeField] private bool tourPointsSettingMode;
    [SerializeField] private List<GridSettings> tourPoints = new List<GridSettings>();
    //列表是引用变量，复制是引用赋值，不是值赋值，我说我删Points的值为什么tourPoints的值在少，赛博鬼打墙了
    public bool Move = false;
    public bool isRunning = false;
    [SerializeField]private List<GridSettings> Points = new List<GridSettings>();
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        TourPointSet();
        if (Move&&!isRunning) 
        {
            TourAchieve();
        }
    }

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
        GridSettings point = Points[0];
        Vector3 nowPos = transform.position;
        Vector3 targetPos = point.transform.position;
        targetPos.y = nowPos.y;
        //设置两点的坐标
        GetGroundGrid().occupied = false;
        //走之前先把格子设为未占据
        List<Vector3Int> pathBetweenTwoPoints = GridAchieve.instance.PathFind(point.transform.position);
        Debug.Log(pathBetweenTwoPoints);
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
            if(movePoint == 0)
                //防止路径被阻挡时硬走完路导致倒欠移动点
                break;
        }
        Points.Remove(point);
        //这三个条件本来是确认移动点为0的时候才触发，但是这个移动巡回点是开发者自订的，不会出现什么超出的问题，为了清除有时候巡回点走回原点时还剩移动点报错的情况，暂时就这样把
        hasActed = true;
        isMoving = false;
        Move = false;
        GetGroundGrid().occupied = true;
        //到终点之后将脚底物块设为占据
        isRunning = false;
    }
}
