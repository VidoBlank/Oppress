using UnityEngine;
using DG.Tweening;

public class DeployElevatorController : MonoBehaviour
{
    [Header("电梯设置")]
    public GameObject elevator;       // 指定的电梯 GameObject
    public float moveDuration = 2f;   // 电梯移动持续时间（秒）
    public float moveDistance = 5f;   // 电梯向下移动的距离（单位）

    [Header("管理器设置")]
    public MonoBehaviour[] managerComponents;  // 指定的管理器脚本组件数组

    [Header("音频设置")]
    public AudioSource targetAudioSource;        // 目标 AudioSource（用于替换音乐、pitch 过渡和 volume 淡出）
    public AudioClip replacementClip;            // 指定的音乐片段，用于替换目标 AudioSource 的 clip
    public float targetPitch = 1.2f;               // 目标 pitch 值
    public float pitchTransitionDuration = 0.5f;   // pitch 过渡时间（秒）
    public float volumeFadeDuration = 1.0f;        // volume 淡出持续时间（秒）

    public AudioSource secondaryAudioSource;       // 新添加的 AudioSource，仅进行 volume 淡出

    /// <summary>
    /// 部署按钮点击后调用此方法
    /// </summary>
    public void OnDeployButtonClicked()
    {
        if (elevator == null)
        {
            Debug.LogError("未指定电梯 GameObject！");
            return;
        }

        // 替换目标 AudioSource 的 clip 为指定的音乐，并播放
        if (targetAudioSource != null && replacementClip != null)
        {
            targetAudioSource.clip = replacementClip;
            targetAudioSource.Play();
        }

        // 触发部署时，快速将目标 AudioSource 的 pitch 过渡到指定值
        if (targetAudioSource != null)
        {
            DOTween.To(() => targetAudioSource.pitch, x => targetAudioSource.pitch = x, targetPitch, pitchTransitionDuration);
        }

        // 播放电梯上的 AudioSource
        AudioSource elevatorAudio = elevator.GetComponent<AudioSource>();
        if (elevatorAudio != null)
        {
            elevatorAudio.Play();
            secondaryAudioSource.Play();
        }

        // 计算电梯目标位置（向下移动 moveDistance 个单位）
        Vector3 targetPos = elevator.transform.position - new Vector3(0, moveDistance, 0);

        // 使用 DOTween 执行电梯移动动画，使用 Ease.OutCubic 实现先快后慢的缓冲效果
        elevator.transform.DOMoveY(targetPos.y, moveDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                // 电梯移动完成后，开启所有指定的管理器脚本组件
                if (managerComponents != null)
                {
                    foreach (var comp in managerComponents)
                    {
                        if (comp != null)
                        {
                            comp.enabled = true;
                        }
                    }
                }

                // 在电梯移动完成后，将目标 AudioSource 的 volume 在 volumeFadeDuration 秒内过渡回 0
                if (targetAudioSource != null)
                {
                    DOTween.To(() => targetAudioSource.volume, x => targetAudioSource.volume = x, 0f, volumeFadeDuration);
                }

                // 对新添加的 secondaryAudioSource 执行 volume 淡出效果
                if (secondaryAudioSource != null)
                {
                    DOTween.To(() => secondaryAudioSource.volume, x => secondaryAudioSource.volume = x, 0f, volumeFadeDuration);
                }
            });
    }
}
