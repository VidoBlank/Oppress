using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelNodeUIInfo : MonoBehaviour
{
    [Header("UI ���")]
    // ������ʾ�ؿ���Ϣ�� CanvasGroup
    public CanvasGroup uiCanvasGroup;
    // ��ʾ�ؿ����Ƶ��ı��ؼ�
    public TextMeshProUGUI levelNameText;
    // ��ʾ�ؿ���Ϣ���ı��ؼ�
    public TextMeshProUGUI levelInfoText;

    [Header("��������")]
    // ����ʱ�����룩
    public float fadeDuration = 0.5f;

    [Header("�༭�˹ؿ����ı���Ϣ (�� Inspector ������)")]
    // �ؿ����ƣ����� Inspector �б༭��
    public string levelNameOverride;
    // �ؿ�������Ϣ�������ı������� Inspector �б༭��
    [TextArea(3, 10)]
    public string levelInfoOverride;

    // ��ǰ�ڵ��Ӧ�� LevelNode �����Ҫ�� LevelNodeUIInfo �� LevelNode ����ͬһ GameObject �ϣ�
    private LevelNode levelNode;
    // ���ڼ�� InPath ״̬�仯
    private bool previousInPath;

    private void Awake()
    {
        // ��ȡͬһ GameObject �ϵ� LevelNode ���
        levelNode = GetComponent<LevelNode>();

        // ȷ�� CanvasGroup ��ʼ��Ϊ����״̬
        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 0;
            uiCanvasGroup.interactable = false;
            uiCanvasGroup.blocksRaycasts = false;
        }
        previousInPath = levelNode.InPath;
    }

    private void Update()
    {
        if (levelNode.IsPass)
        {
            uiCanvasGroup.alpha = 0;
            uiCanvasGroup.interactable = false;
            uiCanvasGroup.blocksRaycasts = false;
            return;
        }
        // ��鵱ǰ�ڵ��Ƿ���·���У�InPath��״̬�仯
        if (levelNode.InPath != previousInPath)
        {
            previousInPath = levelNode.InPath;
            if (levelNode.InPath)
            {
                // ���� UI �ı���ʹ���� Inspector �б༭���ı���
                UpdateUITexts();
                // ������ʾ UI
                StartCoroutine(FadeCanvasGroup(uiCanvasGroup, 0, 1, fadeDuration));
            }
            else
            {
                // �������� UI
                StartCoroutine(FadeCanvasGroup(uiCanvasGroup, uiCanvasGroup.alpha, 0, fadeDuration));
            }
        }
    }

    /// <summary>
    /// ���� UI ��ʾ�Ĺؿ����ƺ���Ϣ��ʹ�� Inspector �б༭������
    /// </summary>
    private void UpdateUITexts()
    {
        if (levelNameText != null)
        {
            levelNameText.text = levelNameOverride;
        }
        if (levelInfoText != null)
        {
            levelInfoText.text = levelInfoOverride;
        }
    }

    /// <summary>
    /// Э�̣��� CanvasGroup �� alpha ���н���
    /// </summary>
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        cg.alpha = startAlpha;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }
        cg.alpha = endAlpha;
        cg.interactable = (endAlpha > 0);
        cg.blocksRaycasts = (endAlpha > 0);
    }
}
