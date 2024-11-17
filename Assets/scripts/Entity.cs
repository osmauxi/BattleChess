using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("碰撞检测")]
    [SerializeField] protected Transform groundcheck;
    [SerializeField] protected float groundcheckdistance;
    [SerializeField] protected LayerMask groundmask;
    //底部地面检测方块
    [SerializeField] protected float BoxX;
    [SerializeField] protected float BoxY;
    [SerializeField] protected float BoxZ;

    public Rigidbody rb {  get; private set; }

    public virtual void Awake() 
    {
        rb = GetComponent<Rigidbody>();
    }
 

    public virtual void Start()
    {


    }
    public virtual bool isground() => Physics.BoxCast(groundcheck.position,new Vector3(BoxX,BoxY,BoxZ),Vector3.down, Quaternion.identity,groundmask);
    //Boxcast的返回值是bool此行将physics.Boxcast的返回值赋值给public的bool函数isground（）,isground（）可当作普通的bool值使用，这样少定义一个值

    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(groundcheck.position, new Vector3(BoxX, BoxY, BoxZ));
    }

    void Update()
    {

    }
}
