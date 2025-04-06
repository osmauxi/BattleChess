using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    private Entity entity;
    private CharacterStat mystats;
    private RectTransform mytransfrom;
    //用于存储和操作矩形的位置、大小和锚定，并支持各种形式的缩放
    private Slider slider;
    //Slider控件允许用户可以通过鼠标来在预先确定的范围调节数值
    //用于制作实时的血条跳动

    private void Start()
    {
        mytransfrom = GetComponent<RectTransform>();
        entity = GetComponentInParent<Entity>();
        slider = GetComponentInChildren<Slider>();
        mystats = GetComponentInParent<CharacterStat>();

        slider.maxValue = mystats.maxhealth.GetValue();//血量最大值
        Invoke("UpdateHealthUI", .1f);
    }

    private void Update()
    {
        //UpdateHealthUI();
    }

    public void UpdateHealthUI()
    {
        slider.value = mystats.currenthealth;
    }
    private void OnDisable()
    {
    }
}
