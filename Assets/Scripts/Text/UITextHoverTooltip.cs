using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class UITextHoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("要显示的说明文字")]
    [TextArea]
    public string tooltipMessage = "这是说明内容";

    [Header("Tooltip UI组件")]
    public GameObject tooltipObject; // 包含 TextMeshProUGUI 的物体
    public TextMeshProUGUI tooltipText;

    private void Start()
    {
        if (tooltipObject != null)
        {
            tooltipObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipObject != null && tooltipText != null)
        {
            tooltipText.text = tooltipMessage;
            tooltipObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipObject != null)
        {
            tooltipObject.SetActive(false);
        }
    }
}
