using System.Collections;
using UnityEngine;

public class AudioFadeIn : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource; // 目标音频源
    public float delay = 5f; // 延迟时间（秒）
    public float fadeDuration = 3f; // 音量渐变持续时间（秒）

    [Header("Options")]
    public bool enableEffect = true; // 勾选项，是否开启效果
    private bool effectTriggered = false; // 标记效果是否已触发

    private void Update()
    {
        // 检查 enableEffect 是否从未激活到激活状态
        if (enableEffect && !effectTriggered)
        {
            effectTriggered = true; // 防止重复触发
            if (audioSource == null)
            {
                Debug.LogError("AudioSource is not assigned!");
                return;
            }

            // 确保音量初始为0
            audioSource.volume = 0f;
            audioSource.playOnAwake = false;

            // 启动渐变音频协程
            StartCoroutine(PlayAudioWithFadeIn());
        }
    }

    private IEnumerator PlayAudioWithFadeIn()
    {
        // 等待指定延迟时间
        yield return new WaitForSeconds(delay);

        // 开始播放音频
        audioSource.Play();

        // 逐渐增加音量
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Clamp01(elapsed / fadeDuration) * 0.5f;
            yield return null;
        }

        // 确保音量为0.5
        audioSource.volume = 0.5f;
    }
}
