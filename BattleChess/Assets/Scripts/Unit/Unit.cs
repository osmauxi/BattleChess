using UnityEngine.Events;
using UnityEngine;
using System.Collections.Generic;
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

    // 设置选中状态
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (selected)
        {
            BattleInfUISet.Instance.SetAllayUIVisible(true);
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

    public override void GridOccupiedChange()
    {
        base.GridOccupiedChange();
        gridSettings.occupiedbyUnit = false;
    }
}