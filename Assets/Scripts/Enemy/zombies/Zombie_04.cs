using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie_04 : Enemy
{
    [Header("新减速系数")]
    [Tooltip("当Hp减少时，新设定的减速比例（例如 0.3 表示30% 速度）")]
    public float newSlowFactor = 0.3f;

    [Header("新移动速度")]
    [Tooltip("当Hp减少时，新设定的移动速度")]
    public float newMovingSpeed = 2.0f;

    [Header("动画过渡时间")]
    [Tooltip("控制 Blend Tree `speed` 变量的变化时间")]
    public float animationTransitionTime = 1.0f;

    private float originalMovingSpeed;
    private Coroutine animationTransitionCoroutine;
    private Coroutine restoreSpeedCoroutine;

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        originalMovingSpeed = movingSpeed;
    }

    /// <summary>
    /// 受伤时触发：立即修改移动速度 & slowFactor，平滑调整 Blend Tree `speed` 变量
    /// </summary>
    public override void ReduceHp(float num)
    {
        base.ReduceHp(num);

        // **立即** 修改移动速度 & slowFactor
        slowFactor = newSlowFactor;
        movingSpeed = newMovingSpeed;

        // **平滑** 过渡 Blend Tree `speed` 变量
        if (animationTransitionCoroutine != null)
        {
            StopCoroutine(animationTransitionCoroutine);
        }
        animationTransitionCoroutine = StartCoroutine(SmoothAnimationSpeed(1.0f));

        // **启动恢复速度的协程**（等待 slowDuration 后恢复）
        if (restoreSpeedCoroutine != null)
        {
            StopCoroutine(restoreSpeedCoroutine);
        }
        restoreSpeedCoroutine = StartCoroutine(RestoreSpeed());
    }

    /// <summary>
    /// 平滑修改 Blend Tree 的 `speed` 变量
    /// </summary>
    private IEnumerator SmoothAnimationSpeed(float targetBlendSpeed)
    {
        float elapsedTime = 0f;
        float startBlendSpeed = animator.GetFloat("speed");

        while (elapsedTime < animationTransitionTime)
        {
            float blendValue = elapsedTime / animationTransitionTime;

            animator.SetFloat("speed", Mathf.Lerp(startBlendSpeed, targetBlendSpeed, blendValue));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        animator.SetFloat("speed", targetBlendSpeed);
    }

    /// <summary>
    /// 过 slowDuration 秒后，立即恢复移动速度，动画 `speed` 平滑回 0
    /// </summary>
    private IEnumerator RestoreSpeed()
    {
        yield return new WaitForSeconds(slowDuration);

        // **立即** 恢复移动速度
        movingSpeed = originalMovingSpeed;

        // **平滑** 过渡 Blend Tree `speed` 变量
        if (animationTransitionCoroutine != null)
        {
            StopCoroutine(animationTransitionCoroutine);
        }
        animationTransitionCoroutine = StartCoroutine(SmoothAnimationSpeed(0f)); // 
    }
}
