using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class TileMap : MonoBehaviour
{
    public static TileMap Instance;
    private Vector3 originalPos;//获取默认位置，整一个出现时往上移动的过度效果
    private Vector3 TargetPos;
    [SerializeField] private float MoveSpeed;//给插值函数用的移动速度
    public bool up = false;
    public bool down = false;
    void Awake()
    {//单例模式
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    void Start()
    {
        originalPos = transform.position;
        TargetPos = new Vector3(originalPos.x, originalPos.y + 0.2f, originalPos.z);
    }

    // Update is called once per frame
    void Update()
    {
        GridUp();
        GridDown();
    }

    private void GridDown()//隐藏移动格
    {
        if (down)
        {
            transform.position = Vector3.Lerp(transform.position, originalPos, MoveSpeed * Time.deltaTime);
        }
    }

    private void GridUp()//显示移动格
    {
        if (up)
        {
            transform.position = Vector3.Lerp(transform.position, TargetPos, MoveSpeed * Time.deltaTime);
        }
    }

    public void GridUpEvent() 
    {
        up = true;
        down = false;
    }

    public void GridDownEvent() 
    {
        down = true;
        up = false;
    }
}
