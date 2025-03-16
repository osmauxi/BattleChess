using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMoveCostSet : MonoBehaviour
    //因为字典的更新必须在GridSettings更新之后才能获取准确值，但移动消耗必须等在gridSettings脚本执行前进行传值，相互矛盾，所以功能从GridAchieve中切出来独立
{
    public static GridMoveCostSet instance;
    //从GridSettings中把这些值换过来搞成静态，因为这些都是基本值，每个网格实例都会用，不会因为网格变化而改变值
    public static int plainGridMoveCosts;
    public static int ruggedGridMoveCosts;
    public static int controlledGridMoveCosts;//企划书里有类似文明6的控制区设定，即相邻两个单位友军或敌军的格子会变成控制区，敌人/友军移动消耗更大
    public static int hinderedGridMoveCosts;
    //静态字段属于类不属于实例，inspector中视图只修改非静态值(属于实例的值)，所以赋值要麻烦一点 
    [SerializeField] private int _plainGridMoveCosts;
    [SerializeField] private int _ruggedGridMoveCosts;
    [SerializeField] private int _controlledGridMoveCosts;
    [SerializeField] private int _hinderedGridMoveCosts;
    private void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        plainGridMoveCosts = _plainGridMoveCosts;
        ruggedGridMoveCosts = _ruggedGridMoveCosts;
        controlledGridMoveCosts = _controlledGridMoveCosts;
        hinderedGridMoveCosts = _hinderedGridMoveCosts;
    }
}
