using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MouseWorld : MonoBehaviour
{
    public static MouseWorld instance;
    //声明了本脚本类的实例instance

    [SerializeField] private LayerMask MousePositionLayerMask;

    private void Awake()
    {
        instance = this;
        //使instance实例实例化为本脚本？？？不知道，反正是赋值
    }
    private void Update()
    {
        transform.position = GetPosition();
    }

    public static Vector3 GetPosition()
    //static声明函数内部变量和方法使属于这个类而不是类的实例，允许不在脚本中实例化也能用.符号来访问内部变量
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //找到tag为maincamera的camera组件
        //ScreenPointToRay() 是Unity中Camera类的一个方法，用于将屏幕上的一个点转换为一条射线。这条射线的起点是摄像机在屏幕上对应的点，
        //方向是从摄像机出发指向那个点。这在进行射线命中检测时非常有用，特别是与用户界面和鼠标交互相关的场景中。
        Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, instance.MousePositionLayerMask);//声明raycastHit，本身使检测鼠标射线与设置层之间的碰撞
        //Raycast函数第二个值使out形式，故此只能也写out，out会使值丢失，并重新赋值，也就是会改变
        //RaycastHit 类是 Unity 中的一个结构，用于存储射线投射操作的结果。射线投射是一种常用的技术，
        //用于检测场景中的碰撞、获取碰撞点、获取碰撞对象的信息等。RaycastHit 提供了关于射线与场景中对象的交互信息，包括碰撞点、碰撞法线、碰撞对象等
        return raycastHit.point;
    }
  
}
