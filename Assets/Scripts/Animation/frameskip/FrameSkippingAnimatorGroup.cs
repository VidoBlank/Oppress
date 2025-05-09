using UnityEngine;
using System.Collections.Generic;

public class FrameSkippingAnimatorGroup : MonoBehaviour
{
    public GameObject[] parentObjects;      // 多个父物体数组
    public float totalDuration = 1f;        // 动画的总时长（秒）
    public int targetFramesPerSecond = 12;  // 目标帧率（fps）
    public bool enableFrameSkipping = true; // 在Inspector中启用或禁用抽帧效果

    private float frameDuration;            // 每帧的时长
    private float timeAccumulator = 0f;     // 用于累积时间
    private List<Animator> animators = new List<Animator>();

    void Start()
    {
        frameDuration = totalDuration / targetFramesPerSecond;
        FindAllAnimators(); // 查找所有Animator

        // 初始设置，禁用自动播放
        SetAnimatorsSpeed(1f);
    }

    void Update()
    {
        if (enableFrameSkipping)
            ApplyFrameSkipping();
        else
            SetAnimatorsSpeed(1f); // 正常速度播放
    }

    // 查找所有Animator
    void FindAllAnimators()
    {
        foreach (GameObject parentObject in parentObjects)
        {
            if (parentObject == null) continue;
            animators.AddRange(parentObject.GetComponentsInChildren<Animator>());
        }
    }

    // 应用抽帧效果
    void ApplyFrameSkipping()
    {
        timeAccumulator += Time.deltaTime;
        if (timeAccumulator >= frameDuration)
        {
            foreach (Animator animator in animators)
            {
                if (animator == null || !animator.gameObject.activeInHierarchy) continue; // 检查Animator是否为null或已销毁，且父对象是否激活

                animator.speed = 1f;
                animator.Update(frameDuration);  // 播放一帧
                animator.speed = 0f;  // 暂停
            }
            timeAccumulator %= frameDuration; // 重置时间
        }
    }

    // 设置Animator播放速度
    void SetAnimatorsSpeed(float speed)
    {
        foreach (Animator animator in animators)
        {
            if (animator == null || !animator.gameObject.activeInHierarchy) continue; // 检查Animator是否为null或已销毁，且父对象是否激活
            animator.speed = speed;
        }
    }

    // Inspector中控制启用/禁用抽帧效果
    public void ToggleFrameSkipping(bool enabled)
    {
        enableFrameSkipping = enabled;
        if (!enabled) SetAnimatorsSpeed(1f); // 恢复正常速度
    }
}
