using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverHandler : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    // 鼠标进入时触发
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("鼠标悬停中");
        YourCustomMethod();
    }

    // 鼠标离开时触发
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("鼠标离开");
    }

    private void YourCustomMethod()
    {
        // 在此处编写你的自定义逻辑
    }
}