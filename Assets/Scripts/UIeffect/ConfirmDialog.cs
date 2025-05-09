using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConfirmDialog : MonoBehaviour
{
    public static ConfirmDialog Instance;

    [Header("���")]
    public GameObject panel;              // ����UI����
    public TextMeshProUGUI messageText;   // ������ʾ
    public Button yesButton;              // ȷ�ϰ�ť
    public Button noButton;               // ȡ����ť

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false); // ��ʼ������
    }

    /// <summary>
    /// ����ȷ�ϴ���
    /// </summary>
    /// <param name="message">��ʾ����</param>
    /// <param name="onConfirm">ȷ�Ϻ�ִ��</param>
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
