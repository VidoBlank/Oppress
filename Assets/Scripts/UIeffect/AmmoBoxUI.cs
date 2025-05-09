using UnityEngine;
using TMPro;  // 导入TextMeshPro命名空间
using System.Collections;

public class AmmoBoxUI : MonoBehaviour
{
    [Header("UI 组件")]
    public CanvasGroup canvasGroup; // CanvasGroup 控制 UI 显示/隐藏
    public TextMeshProUGUI ammoText; // 用于显示当前弹药量的 TextMeshProUGUI

    [Header("UI 参数")]
    public float fadeDuration = 0.5f; // UI 渐入/渐出的时间
    public float displayDuration = 3f; // UI 持续可见时间

    private AmmoBox ammoBox;
    private Coroutine fadeCoroutine;

    void Start()
    {
        ammoBox = GetComponent<AmmoBox>();

        if (ammoBox == null)
        {
            Debug.LogError("AmmoBoxUI: 未找到 AmmoBox 组件！");
            return;
        }

        if (canvasGroup == null || ammoText == null)
        {
            Debug.LogError("AmmoBoxUI: UI 组件未绑定！");
            return;
        }

        canvasGroup.alpha = 0; // 初始时隐藏 UI
        UpdateAmmoUI(); // 初始化弹药量显示
    }

    public void ShowAmmoUI()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeUI(1, fadeDuration)); // 渐入 UI
        UpdateAmmoUI(); // 更新弹药量 UI
        StartCoroutine(HideAfterDelay());
    }

    private void UpdateAmmoUI()
    {
        if (ammoBox != null)
        {
            ammoText.text = $"剩余弹药: {ammoBox.currentEnergy} "; // 更新文本内容
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration); // 等待 X 秒
        fadeCoroutine = StartCoroutine(FadeUI(0, fadeDuration)); // 渐隐 UI
    }

    private IEnumerator FadeUI(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}
