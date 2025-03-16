using UnityEngine.Events;
using UnityEngine;
public class Unit : Entity
{
    [Header("状态")]
    public bool isSelected;     // 是否被选中

    [Header("事件")]
    public UnityEvent OnSelected;   // 选中事件
    public UnityEvent OnDeselected; // 取消选中事件

    private Material originalMaterial;
    private Renderer rend;
    private Color originalColor;

    protected override void Start()
    {
        base.Start();
        GetColorInf();
        InitializeGroundGrid();
        GetGroundGrid();
        //将地面格方法加入选中事件中
        OnSelected.AddListener(TileMap.Instance.GridUpEvent);
        OnDeselected.AddListener(TileMap.Instance.GridDownEvent);
    }

    private void GetColorInf()
    {
        rend = GetComponent<Renderer>();
        originalMaterial = rend.material;
        originalColor = rend.material.color;
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

    // 设置选中状态
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (selected)
        {
            rend.material.color = Color.yellow; // 高亮显示
            GetGroundGrid().CanMoveColorChange(GetGroundGrid(),movePoint);
            OnSelected?.Invoke();
        }
        else
        {
            rend.material.color = originalColor;   // 恢复原材质
            GetGroundGrid().MovedColorRestore(GetGroundGrid());//这里写这个是不移动但是取消选中时改变颜色
            OnDeselected?.Invoke();
        }
    }
}