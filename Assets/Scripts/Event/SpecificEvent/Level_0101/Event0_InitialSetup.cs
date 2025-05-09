using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Event0_InitialSetup : BaseEvent
{
    [Header("第一阶段：Logo 动画")]
    public GameObject logoUI;
    public Animator logoAnimator;
    public string logoStartTrigger = "logostart";
    public float logoDuration = 3f;

    [Header("第二阶段：语言选择")]
    public GameObject languageSelectionUI;
    public Button chineseButton;
    public Button englishButton;

    [Header("第三阶段：Gamma 调整")]
    public GameObject gammaAdjustmentUI;
    public Material targetMaterial;
    public Slider gammaSlider;
    public Button confirmButton;
    public VolumeProfile volumeProfile;
    public float minPostExposure = -1f;
    public float maxPostExposure = 1f;

    private ColorAdjustments colorAdjustments;
    private float postExposureValue = 0f;

    [Header("淡出效果")]
    public GameObject allUI;
    public GameObject Effect;
    public float fadeDuration = 1f;

    private const string PostExposureKey = "PostExposure";

    public override void EnableEvent()
    {
        base.EnableEvent();
        StartCoroutine(InitialSetupSequence());
    }

    private IEnumerator InitialSetupSequence()
    {
        // 第一阶段：显示 Logo 动画
        if (logoUI != null)
        {
            logoUI.SetActive(true);
            if (logoAnimator != null && !string.IsNullOrEmpty(logoStartTrigger))
            {
                logoAnimator.SetTrigger(logoStartTrigger);
            }
        }
        yield return new WaitForSeconds(logoDuration);
        if (logoUI != null) logoUI.SetActive(false);

        // 第二阶段：语言选择
        if (languageSelectionUI != null)
        {
            languageSelectionUI.SetActive(true);
            chineseButton.onClick.AddListener(() => OnLanguageSelected("Chinese"));
            englishButton.onClick.AddListener(() => OnLanguageSelected("English"));

            yield return new WaitUntil(() => !languageSelectionUI.activeSelf);
        }

        // 第三阶段：Gamma 调整
        if (gammaAdjustmentUI != null && volumeProfile != null && volumeProfile.TryGet(out colorAdjustments))
        {
            gammaAdjustmentUI.SetActive(true);

            // 从 PlayerPrefs 加载保存的 Post Exposure 值
            float savedPostExposure = PlayerPrefs.GetFloat(PostExposureKey, 0f);
            OnGammaSliderChanged(PostExposureToSliderValue(savedPostExposure));
            gammaSlider.value = PostExposureToSliderValue(savedPostExposure);

            gammaSlider.onValueChanged.AddListener(OnGammaSliderChanged);

            confirmButton.onClick.AddListener(() =>
            {
                SaveSettings();
                StartCoroutine(FadeOutUIAndEndEvent());
            });

            yield return new WaitUntil(() => !gammaAdjustmentUI.activeSelf);
        }
    }

    private void OnLanguageSelected(string language)
    {
        Debug.Log($"语言已选择：{language}");
        languageSelectionUI.SetActive(false);
    }

    private void OnGammaSliderChanged(float value)
    {
        // 调整滑条值，以 0.5 为中心
        float normalizedValue = (value - 0.5f) * 2f; // 将滑条值范围 [0, 1] 转换为 [-1, 1]

        // 计算 Post Exposure 值，保证滑条值为 0.5 时为 0
        postExposureValue = Mathf.Lerp(minPostExposure, maxPostExposure, 0.5f + (normalizedValue / 2f));

        // 调整 Post Exposure
        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.Override(postExposureValue);
        }

        // 调整目标材质亮度，使用非线性函数平滑高亮度值
        if (targetMaterial != null)
        {
            float adjustedBrightness = Mathf.Pow(2, postExposureValue); // 原始亮度
            float smoothedBrightness = Mathf.Sqrt(adjustedBrightness);  // 使用平方根函数平滑亮度增长
            Color adjustedColor = new Color(smoothedBrightness, smoothedBrightness, smoothedBrightness, 1f);
            targetMaterial.SetColor("_Color", adjustedColor);
        }

        Debug.Log($"已更新 Post Exposure 值: {postExposureValue}, 原始亮度: {Mathf.Pow(2, postExposureValue)}, 平滑亮度: {Mathf.Sqrt(Mathf.Pow(2, postExposureValue))}");
    }





    private void SaveSettings()
    {
        // 保存当前 Post Exposure 值到 PlayerPrefs
        PlayerPrefs.SetFloat(PostExposureKey, postExposureValue);
        PlayerPrefs.Save();

        Debug.Log($"设置已保存: PostExposure={postExposureValue}");
    }

    private void ApplySettingsToAllScenes()
    {
        // 检查 VolumeProfile 是否包含 ColorAdjustments
        if (volumeProfile.TryGet(out colorAdjustments))
        {
            // 从 PlayerPrefs 加载保存的 Post Exposure 值
            float savedPostExposure = Mathf.Clamp(
                PlayerPrefs.GetFloat(PostExposureKey, 0f),
                minPostExposure,
                maxPostExposure
            );

            // 应用到 Post Exposure
            colorAdjustments.postExposure.Override(savedPostExposure);

            // 调整目标材质亮度
            if (targetMaterial != null)
            {
                float adjustedBrightness = Mathf.Pow(2, savedPostExposure);
                Color adjustedColor = new Color(adjustedBrightness, adjustedBrightness, adjustedBrightness, 1f);
                targetMaterial.SetColor("_Color", adjustedColor);
            }

            Debug.Log($"已应用全局设置: PostExposure={savedPostExposure}");
        }
        else
        {
            Debug.LogError("VolumeProfile 中未找到 ColorAdjustments 组件！");
        }
    }

    private IEnumerator FadeOutUIAndEndEvent()
    {
        if (allUI != null)
        {
            CanvasGroup canvasGroup = allUI.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = allUI.AddComponent<CanvasGroup>();
            }

            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            allUI.SetActive(false);
            Effect.SetActive(false);
        }

        ApplySettingsToAllScenes();

        EndEvent();
    }

    private float PostExposureToSliderValue(float postExposure)
    {
        // 将 Post Exposure 值映射到滑条值（0~1）
        return Mathf.InverseLerp(minPostExposure, maxPostExposure, postExposure);
    }
}
