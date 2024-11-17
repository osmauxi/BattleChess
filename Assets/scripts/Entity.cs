using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("��ײ���")]
    [SerializeField] protected Transform groundcheck;
    [SerializeField] protected float groundcheckdistance;
    [SerializeField] protected LayerMask groundmask;
    //�ײ������ⷽ��
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
    //Boxcast�ķ���ֵ��bool���н�physics.Boxcast�ķ���ֵ��ֵ��public��bool����isground����,isground�����ɵ�����ͨ��boolֵʹ�ã������ٶ���һ��ֵ

    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(groundcheck.position, new Vector3(BoxX, BoxY, BoxZ));
    }

    void Update()
    {

    }
}
