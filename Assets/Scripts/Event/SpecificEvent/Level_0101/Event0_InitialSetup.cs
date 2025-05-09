using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Event0_InitialSetup : BaseEvent
{
    [Header("��һ�׶Σ�Logo ����")]
    public GameObject logoUI;
    public Animator logoAnimator;
    public string logoStartTrigger = "logostart";
    public float logoDuration = 3f;

    [Header("�ڶ��׶Σ�����ѡ��")]
    public GameObject languageSelectionUI;
    public Button chineseButton;
    public Button englishButton;

    [Header("�����׶Σ�Gamma ����")]
    public GameObject gammaAdjustmentUI;
    public Material targetMaterial;
    public Slider gammaSlider;
    public Button confirmButton;
    public VolumeProfile volumeProfile;
    public float minPostExposure = -1f;
    public float maxPostExposure = 1f;

    private ColorAdjustments colorAdjustments;
    private float postExposureValue = 0f;

    [Header("����Ч��")]
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
        // ��һ�׶Σ���ʾ Logo ����
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

        // �ڶ��׶Σ�����ѡ��
        if (languageSelectionUI != null)
        {
            languageSelectionUI.SetActive(true);
            chineseButton.onClick.AddListener(() => OnLanguageSelected("Chinese"));
            englishButton.onClick.AddListener(() => OnLanguageSelected("English"));

            yield return new WaitUntil(() => !languageSelectionUI.activeSelf);
        }

        // �����׶Σ�Gamma ����
        if (gammaAdjustmentUI != null && volumeProfile != null && volumeProfile.TryGet(out colorAdjustments))
        {
            gammaAdjustmentUI.SetActive(true);

            // �� PlayerPrefs ���ر���� Post Exposure ֵ
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
        Debug.Log($"������ѡ��{language}");
        languageSelectionUI.SetActive(false);
    }

    private void OnGammaSliderChanged(float value)
    {
        // ��������ֵ���� 0.5 Ϊ����
        float normalizedValue = (value - 0.5f) * 2f; // ������ֵ��Χ [0, 1] ת��Ϊ [-1, 1]

        // ���� Post Exposure ֵ����֤����ֵΪ 0.5 ʱΪ 0
        postExposureValue = Mathf.Lerp(minPostExposure, maxPostExposure, 0.5f + (normalizedValue / 2f));

        // ���� Post Exposure
        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.Override(postExposureValue);
        }

        // ����Ŀ��������ȣ�ʹ�÷����Ժ���ƽ��������ֵ
        if (targetMaterial != null)
        {
            float adjustedBrightness = Mathf.Pow(2, postExposureValue); // ԭʼ����
            float smoothedBrightness = Mathf.Sqrt(adjustedBrightness);  // ʹ��ƽ��������ƽ����������
            Color adjustedColor = new Color(smoothedBrightness, smoothedBrightness, smoothedBrightness, 1f);
            targetMaterial.SetColor("_Color", adjustedColor);
        }

        Debug.Log($"�Ѹ��� Post Exposure ֵ: {postExposureValue}, ԭʼ����: {Mathf.Pow(2, postExposureValue)}, ƽ������: {Mathf.Sqrt(Mathf.Pow(2, postExposureValue))}");
    }





    private void SaveSettings()
    {
        // ���浱ǰ Post Exposure ֵ�� PlayerPrefs
        PlayerPrefs.SetFloat(PostExposureKey, postExposureValue);
        PlayerPrefs.Save();

        Debug.Log($"�����ѱ���: PostExposure={postExposureValue}");
    }

    private void ApplySettingsToAllScenes()
    {
        // ��� VolumeProfile �Ƿ���� ColorAdjustments
        if (volumeProfile.TryGet(out colorAdjustments))
        {
            // �� PlayerPrefs ���ر���� Post Exposure ֵ
            float savedPostExposure = Mathf.Clamp(
                PlayerPrefs.GetFloat(PostExposureKey, 0f),
                minPostExposure,
                maxPostExposure
            );

            // Ӧ�õ� Post Exposure
            colorAdjustments.postExposure.Override(savedPostExposure);

            // ����Ŀ���������
            if (targetMaterial != null)
            {
                float adjustedBrightness = Mathf.Pow(2, savedPostExposure);
                Color adjustedColor = new Color(adjustedBrightness, adjustedBrightness, adjustedBrightness, 1f);
                targetMaterial.SetColor("_Color", adjustedColor);
            }

            Debug.Log($"��Ӧ��ȫ������: PostExposure={savedPostExposure}");
        }
        else
        {
            Debug.LogError("VolumeProfile ��δ�ҵ� ColorAdjustments �����");
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
        // �� Post Exposure ֵӳ�䵽����ֵ��0~1��
        return Mathf.InverseLerp(minPostExposure, maxPostExposure, postExposure);
    }
}
