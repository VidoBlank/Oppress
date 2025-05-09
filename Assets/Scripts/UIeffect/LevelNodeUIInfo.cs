using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelNodeUIInfo : MonoBehaviour
{
    [Header("UI 组件")]
    // 用于显示关卡信息的 CanvasGroup
    public CanvasGroup uiCanvasGroup;
    // 显示关卡名称的文本控件
    public TextMeshProUGUI levelNameText;
    // 显示关卡信息的文本控件
    public TextMeshProUGUI levelInfoText;

    [Header("过渡设置")]
    // 渐变时长（秒）
    public float fadeDuration = 0.5f;

    [Header("编辑此关卡的文本信息 (在 Inspector 中设置)")]
    // 关卡名称（可在 Inspector 中编辑）
    public string levelNameOverride;
    // 关卡描述信息（多行文本，可在 Inspector 中编辑）
    [TextArea(3, 10)]
    public string levelInfoOverride;

    // 当前节点对应的 LevelNode 组件（要求 LevelNodeUIInfo 与 LevelNode 挂在同一 GameObject 上）
    private LevelNode levelNode;
    // 用于检测 InPath 状态变化
    private bool previousInPath;

    private void Awake()
    {
        // 获取同一 GameObject 上的 LevelNode 组件
        levelNode = GetComponent<LevelNode>();

        // 确保 CanvasGroup 初始化为隐藏状态
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
        // 检查当前节点是否处于路径中（InPath）状态变化
        if (levelNode.InPath != previousInPath)
        {
            previousInPath = levelNode.InPath;
            if (levelNode.InPath)
            {
                // 更新 UI 文本（使用在 Inspector 中编辑的文本）
                UpdateUITexts();
                // 淡入显示 UI
                StartCoroutine(FadeCanvasGroup(uiCanvasGroup, 0, 1, fadeDuration));
            }
            else
            {
                // 淡出隐藏 UI
                StartCoroutine(FadeCanvasGroup(uiCanvasGroup, uiCanvasGroup.alpha, 0, fadeDuration));
            }
        }
    }

    /// <summary>
    /// 更新 UI 显示的关卡名称和信息，使用 Inspector 中编辑的内容
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
    /// 协程：对 CanvasGroup 的 alpha 进行渐变
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
