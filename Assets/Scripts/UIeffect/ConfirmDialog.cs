using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConfirmDialog : MonoBehaviour
{
    public static ConfirmDialog Instance;

    [Header("组件")]
    public GameObject panel;              // 整个UI容器
    public TextMeshProUGUI messageText;   // 内容提示
    public Button yesButton;              // 确认按钮
    public Button noButton;               // 取消按钮

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false); // 初始化隐藏
    }

    /// <summary>
    /// 调用确认窗口
    /// </summary>
    /// <param name="message">显示内容</param>
    /// <param name="onConfirm">确认后执行</param>
    public void Show(string message, Action onConfirm)
    {
        panel.SetActive(true);
        messageText.text = message;

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
            panel.SetActive(false);
        });

        noButton.onClick.AddListener(() =>
        {
            panel.SetActive(false);
        });
    }
}
