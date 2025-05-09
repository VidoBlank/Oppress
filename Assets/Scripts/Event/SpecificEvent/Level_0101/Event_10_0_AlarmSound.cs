using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cinemachine;
using FronkonGames.Glitches.Interferences;

public class Event_10_0_AlarmSound : BaseEvent
{
    [Header("Cinemachine设置")]
    public CinemachineVirtualCamera virtualCamera; // 需要抖动的虚拟摄像机
    public float shakeDuration = 2f; // 抖动持续时间
    public float shakeAmplitude = 1f; // 抖动幅度
    public float shakeFrequency = 1f; // 抖动频率

    [Header("Interferences设置")]
    public Interferences interferences; // Interferences 管线
    public float interferencesIntensity = 0.8f; // Interferences 强度

    [Header("音效设置")]
    public AudioClip alarmSound; // 警报音效
    public AudioSource audioSource; // 音效播放源

    [Header("文本设置")]
    public TMP_Text eventText; // 用于显示事件文本的TextMeshPro组件
    public List<string> textSegments; // 文本段落数组
    public float textDisplayInterval = 3f; // 每段文本显示的间隔时间

    [Header("事件触发设置")]
    public float eventStartDelay = 5f; // 延迟时间，单位秒
    public bool enableTestMode = false; // 是否启用测试模式

    private bool lastTestModeState = false; // 上次测试模式的状态
    private CinemachineBasicMultiChannelPerlin noiseComponent; // 摄像机噪声组件
    private bool isInterferencesActive = false; // Interferences 是否启用

    private void Update()
    {
        // 检测测试模式是否被启用
        if (enableTestMode && !lastTestModeState)
        {
            lastTestModeState = true;
            StartCoroutine(TestAllEffects());
        }
        else if (!enableTestMode && lastTestModeState)
        {
            lastTestModeState = false;
        }
    }

    public override void EnableEvent()
    {
        StartCoroutine(DelayedEventStart());
    }

    private IEnumerator DelayedEventStart()
    {
        // 等待指定的延迟时间
        yield return new WaitForSeconds(eventStartDelay);

        // 启动事件主逻辑
        StartCoroutine(EventSequence());
    }

    private IEnumerator EventSequence()
    {
        // 获取 Cinemachine 的噪声组件
        if (virtualCamera != null)
        {
            noiseComponent = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        // 启动摄像机抖动和 Interferences 效果
        if (noiseComponent != null && interferences != null)
        {
            // 设置摄像机抖动参数
            noiseComponent.m_AmplitudeGain = shakeAmplitude;
            noiseComponent.m_FrequencyGain = shakeFrequency;

            // 启用 Interferences
            interferences.settings.intensity = interferencesIntensity;
            interferences.SetActive(true);
            isInterferencesActive = true;

            // 播放警报音效
            if (audioSource != null && alarmSound != null)
            {
                audioSource.clip = alarmSound;
                audioSource.Play();

                // 启动淡出音效逻辑
                StartCoroutine(FadeOutAudioAfterDelay(20f));
            }

            // 持续抖动
            yield return new WaitForSeconds(shakeDuration);

            // 停止摄像机抖动
            noiseComponent.m_AmplitudeGain = 0f;
            noiseComponent.m_FrequencyGain = 0f;

            // 停止 Interferences 效果
            interferences.SetActive(false);
            isInterferencesActive = false;
        }

        // 显示逐段文本
        if (eventText != null && textSegments != null && textSegments.Count > 0)
        {
            foreach (string segment in textSegments)
            {
                eventText.text = segment;
                yield return new WaitForSeconds(textDisplayInterval);
            }
        }

        // 最后一段文本显示后，结束事件
        EndEvent();
    }

    private IEnumerator FadeOutAudioAfterDelay(float delay)
    {
        // 等待指定延迟时间
        yield return new WaitForSeconds(delay);

        if (audioSource != null)
        {
            float initialVolume = audioSource.volume; // 记录当前音量
            float fadeDuration = 2f; // 淡出时间
            float elapsedTime = 0f;

            // 逐渐降低音量
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(initialVolume, 0f, elapsedTime / fadeDuration);
                yield return null;
            }

            // 完成后停止播放音效
            audioSource.volume = 0f;
            audioSource.Stop();
            Debug.Log("音效音量已从当前值淡出到 0，并停止播放。");
        }
        else
        {
            Debug.LogWarning("未找到 AudioSource 组件，无法进行淡出操作。");
        }
    }

    private IEnumerator TestAllEffects()
    {
        Debug.Log("开始测试所有效果");

        // 启动摄像机抖动和 Interferences 效果
        if (virtualCamera != null)
        {
            noiseComponent = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noiseComponent != null)
            {
                noiseComponent.m_AmplitudeGain = shakeAmplitude;
                noiseComponent.m_FrequencyGain = shakeFrequency;

                // 启用 Interferences
                if (interferences != null)
                {
                    interferences.settings.intensity = interferencesIntensity;
                    interferences.SetActive(true);
                    isInterferencesActive = true;
                }

                // 播放警报音效
                if (audioSource != null && alarmSound != null)
                {
                    audioSource.clip = alarmSound;
                    audioSource.Play();

                    // 启动淡出音效逻辑
                    StartCoroutine(FadeOutAudioAfterDelay(20f));
                }

                yield return new WaitForSeconds(shakeDuration);

                // 停止摄像机抖动
                noiseComponent.m_AmplitudeGain = 0f;
                noiseComponent.m_FrequencyGain = 0f;

                // 停止 Interferences 效果
                if (interferences != null)
                {
                    interferences.SetActive(false);
                    isInterferencesActive = false;
                }
            }
        }

        // 显示测试文本
        if (eventText != null && textSegments != null && textSegments.Count > 0)
        {
            foreach (string segment in textSegments)
            {
                eventText.text = segment;
                yield return new WaitForSeconds(textDisplayInterval);
            }
        }

        Debug.Log("测试结束");
    }
}
