using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using Cinemachine;
using FronkonGames.Glitches.Interferences;



public class GlitchEffect : MonoBehaviour
{
    //后处理
    public Volume globalVolume; // 后处理 Volume
    public TextMeshProUGUI glitchText; // 用于显示自检文本的 TextMeshPro 对象
    public CinemachineVirtualCamera virtualCamera; // Cinemachine Virtual Camera
    public float minFlashFrequency = 0.01f; // 最快闪烁频率
    public float maxFlashFrequency = 0.1f; // 最慢闪烁频率
    public float startExposure = -10f; // 起始曝光值
    public float endExposure = 1f; // 最终曝光

    private CinemachineBasicMultiChannelPerlin noise; // Cinemachine 噪声组件
    private float initialBloomThreshold = 1f; // Bloom 初始阈值
    private bool isGlitching = false;


    //管线引用
    public Interferences interferences;
    private bool isInterferencesActive = false;


    //自检进度条
    public Slider progressSlider; // 进度条
    public TextMeshProUGUI progressText; // 进度百分比文本
    public CanvasGroup progressCanvasGroup; // 控制进度条透明度的 CanvasGroup



    //自检文本
    [SerializeField]
    private List<string> checkItems = new List<string>
    {
        "正在初始化显示接口……",
        "显示初始化完毕。",
        "电力供应系统检查:评估反应堆输出稳定性……",
        "备用能源储备评估:计算剩余供电时间……",
        "能源分配策略：优先分配关键模块供电。",
        "自动防御系统检查：<color=yellow>异常，需要人工干预。</color>",
        "启用<color=yellow>ALPHA-B2应急协议</color>：正在调度资源……",
        "生命检测器扫描:功能正常",
        "环境毒性检测：毒素浓度：较低。",
        "空气过滤系统：运行正常。",
        "氧气浓度监测：水平已过滤安全范围内。",
        "二氧化碳过滤效率：已验证。",
        "重力模拟装置校准：确认内部重力平衡……",
        "温控系统检测：模块运行正常。",
        "环境控制：自动调节温度至安全水平……",
        "设施完整性扫描：正在分析损伤程度……",
        "外壁完整性检查：检测到轻微破损。",
        "应急密封协议：确认生效……",
        "主要支撑结构分析：稳定。",
        "内部通道扫描：确认可通行路径……",
        "隔离屏障状态检测：屏障完整性正常。",
        "核心处理器运行状态:重启逻辑核心……",
        "存储系统扫描：数据未受损。",
        "外部探测器状态更新：恢复环境扫描……",
        "生物活动监控更新:确认异常活动热点……",
        "系统诊断完成。所有关键功能运行正常。"
    };

    private void Awake()
    {
        if (checkItems == null || checkItems.Count == 0)
        {
            InitializeDefaultCheckItems();
        }

        if (virtualCamera != null)
        {
            noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise == null)
            {
               
            }
        }
    }

   

    private void InitializeDefaultCheckItems()
    {
        checkItems = new List<string>
        {
            "正在初始化显示接口……",
            "显示初始化完毕。",
            "电力供应系统检查:评估反应堆输出稳定性……",
            "备用能源储备评估:计算剩余供电时间……",
            "能源分配策略：优先分配关键模块供电。",
            "自动防御系统检查：<color=yellow>异常，需要人工干预。</color>",
            "启用<color=yellow>ALPHA-B2应急协议</color>：正在调度资源……",
            "生命检测器扫描:功能正常",
            "环境毒性检测：毒素浓度：较低。",
            "空气过滤系统：运行正常。",
            "氧气浓度监测：水平已过滤安全范围内。",
            "二氧化碳过滤效率：已验证。",
            "重力模拟装置校准：确认内部重力平衡……",
            "温控系统检测：模块运行正常。",
            "环境控制：自动调节温度至安全水平……",
            "设施完整性扫描：正在分析损伤程度……",
            "外壁完整性检查：检测到轻微破损。",
            "应急密封协议：确认生效……",
            "主要支撑结构分析：稳定。",
            "内部通道扫描：确认可通行路径……",
            "隔离屏障状态检测：屏障完整性正常。",
            "核心处理器运行状态:重启逻辑核心……",
            "存储系统扫描：数据未受损。",
            "外部探测器状态更新：恢复环境扫描……",
            "生物活动监控更新:确认异常活动热点……",
            "系统诊断完成。所有关键功能运行正常。"
        };
    }

    public IEnumerator StartGlitchEffect()
    {
        if (isGlitching || globalVolume == null)
            yield break;

        isGlitching = true;
        globalVolume.weight = 1f;

        // 激活颜色调整效果
        if (globalVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
            colorAdjustments.active = true;

        if (globalVolume.profile.TryGet(out Bloom bloom))
        {
            initialBloomThreshold = bloom.threshold.value;
            bloom.active = true;
        }

        // 初始化进度条
        if (progressSlider != null)
        {
            progressSlider.value = 0f;
            progressCanvasGroup.alpha = 0f; // 设置为透明
            StartCoroutine(FadeCanvasGroup(progressCanvasGroup, 1f, 0.5f)); // 淡入
        }

        float minIntensity = 0.5f;
        float maxIntensity = 0.85f;

        int totalItems = checkItems.Count; // 总自检项目数

        for (int i = 0; i < totalItems; i++)
        {
            float currentFlashFrequency = i < totalItems / 2
                ? Random.Range(0.5f, 1.25f)
                : Random.Range(0.25f, 0.03f);

            if (globalVolume.profile.TryGet(out ColorAdjustments colorAdjustmentsEffect))
            {
                colorAdjustmentsEffect.postExposure.value = Random.Range(-5f, 5f);
            }

            // 更新进度条
            if (progressSlider != null)
            {
                float progress = (i + 1) / (float)totalItems;
                progressSlider.value = progress;

                if (progressText != null)
                    progressText.text = Mathf.RoundToInt(progress * 100) + "%";
            }

            // 显示自检项目
            string currentText = checkItems[i];
            glitchText.text = currentText;

            // 随机决定是否启用 Interferences
            if (Random.value > 0.35f)
            {
                float randomDuration = Random.Range(0.25f, 0.5f);
                StartCoroutine(EnableInterferencesTemporarily(randomDuration));
            }

            // 动态随机波动 Interferences 强度11
            if (isInterferencesActive && interferences.settings != null)
            {
                interferences.settings.intensity = Random.Range(minIntensity, maxIntensity);
            }

            yield return new WaitForSeconds(currentFlashFrequency);
        }

       

        if (globalVolume.profile.TryGet(out ColorAdjustments colorAdjustmentsFinal))
        {
            colorAdjustmentsFinal.postExposure.value = endExposure;
        }

        if (interferences != null)
        {
            interferences.SetActive(false);
            isInterferencesActive = false;
        }

        glitchText.text = "系统评估完成，Tau-Prime站点状态已更新。";
        yield return new WaitForSeconds(3f);
        glitchText.text = "";
        // 自检完成后隐藏进度条
        if (progressSlider != null)
        {
            StartCoroutine(FadeCanvasGroup(progressCanvasGroup, 0f, 0.75f)); // 淡出
            yield return new WaitForSeconds(0.75f); // 等待淡出完成
        }
        yield return StartCoroutine(FadeOutVolumeWeight());
        isGlitching = false;
    }









    ///////////////////////协程////////////////////////

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }



    private IEnumerator EnableInterferencesTemporarily(float duration)
    {
        if (interferences != null)
        {
            interferences.SetActive(true); // 启用 Interferences
            isInterferencesActive = true; // 更新状态
            yield return new WaitForSeconds(duration);
            interferences.SetActive(false); // 关闭 Interferences
            isInterferencesActive = false; // 更新状态
        }
    }



    private IEnumerator FadeOutVolumeWeight()
    {
        float fadeDuration = 0.35f; // 过渡时间
        float startWeight = globalVolume.weight;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            globalVolume.weight = Mathf.Lerp(startWeight, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        globalVolume.weight = 0f; // 确保最终值为 0
    }

    /// <summary>
    /// ///////////////////////////////////////
    /// </summary>



    //重置效果
    public void ResetPostProcessingEffects()
    {
        if (globalVolume == null || globalVolume.profile == null)
            return;

        if (globalVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            colorAdjustments.postExposure.value = 0f;
            colorAdjustments.active = false;
        }

        if (globalVolume.profile.TryGet(out Bloom bloom))
        {
            bloom.threshold.value = 0.85f;
            bloom.intensity.value = 1f;
            bloom.active = false;
        }

        globalVolume.weight = 0f;
    }
}
