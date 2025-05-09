using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class UITextHoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Ҫ��ʾ��˵������")]
    [TextArea]
    public string tooltipMessage = "����˵������";

    [Header("Tooltip UI���")]
    public GameObject tooltipObject; // ���� TextMeshProUGUI ������
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
