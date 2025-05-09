using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie_05 : Enemy
{
    [Header("狂暴模式设置")]
    [Tooltip("进入狂暴模式的 HP 阈值")]
    public float berserkHpThreshold = 50f;

    [Tooltip("狂暴前摇时间（rage 持续时间）")]
    public float rageDuration = 2.0f;

    [Tooltip("狂暴模式的攻击力")]
    public int berserkAttackDamage = 50;

    [Tooltip("狂暴模式的攻击间隔")]
    public float berserkAttackInterval = 0.8f;

    [Tooltip("狂暴模式的移动速度")]
    public float berserkMovingSpeed = 3.5f;

    [Tooltip("狂暴模式的 slowFactor（降低减速效果影响）")]
    public float berserkSlowFactor = 0.9f;

    [Tooltip("狂暴前摇期间的伤害减免比例（0.5 = 受到 50% 伤害）")]
    public float damageReductionFactor = 0.5f;

    [Header("动画 & 视觉效果")]
    [Tooltip("控制 Blend Tree `speed` 变量的变化时间")]
    public float animationTransitionTime = 1.0f;

    [Tooltip("指定要变化的模型对象")]
    public Transform berserkModelObject;  // **指定要变化的模型**

    [Tooltip("狂暴状态下的放大比例（相对原始尺寸）")]
    public float berserkScaleMultiplier = 1.2f;

    [Tooltip("模型放大过渡时间")]
    public float scaleTransitionTime = 1.0f;

    private bool isBerserk = false; // 标记是否已经进入狂暴模式
    private bool isRaging = false; // 标记是否在 `rage` 动画状态
    private float originalAttackDamage;
    private float originalAttackInterval;
    private float originalMovingSpeed;
    private float originalSlowFactor;
    private Vector3 originalScale; // 记录原始模型大小
    private Coroutine animationTransitionCoroutine;
    private Coroutine scaleCoroutine;

    private void Start()
    {
        // 获取 Animator
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        // **确保 berserkModelObject 不为空，否则默认使用当前对象**
        if (berserkModelObject == null)
        {
            berserkModelObject = transform;
            Debug.LogWarning($"{gameObject.name}: berserkModelObject 为空，默认使用本体");
        }

        // 记录原始属性
        originalAttackDamage = attackDamage;
        originalAttackInterval = attackInterval;
        originalMovingSpeed = movingSpeed;
        originalSlowFactor = slowFactor;
        originalScale = berserkModelObject.localScale; // 记录 **指定模型** 的原始大小
    }

    /// <summary>
    /// 受伤时触发，检测是否需要进入狂暴模式
    /// </summary>
    public override void ReduceHp(float num)
    {
        if (isRaging) // 在 rage 动画状态下，不执行普通受击逻辑
        {
            num *= damageReductionFactor; // 伤害减免
        }

        base.ReduceHp(num);

        // 检测是否进入狂暴模式
        if (!isBerserk && !isRaging && HP <= berserkHpThreshold)
        {
            StartCoroutine(RagePhase());
        }
    }

    /// <summary>
    /// 进入狂暴前摇（rage 状态），在此期间无法移动，减少受到的伤害，并且指定模型逐渐放大
    /// </summary>
    private IEnumerator RagePhase()
    {
        isRaging = true;
        animator.SetTrigger("rage"); // 触发 `rage` 动画
        Debug.Log($"{gameObject.name} 进入狂暴前摇状态！无法移动，伤害减免 {damageReductionFactor * 100}%");

        // 禁止移动
        float previousSpeed = movingSpeed;
        movingSpeed = 0;

        // **模型逐渐放大**
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(ScaleOverTime(berserkModelObject, originalScale * berserkScaleMultiplier, scaleTransitionTime));

        // 等待 `rageDuration` 结束
        yield return new WaitForSeconds(rageDuration);

        // 恢复移动速度
        movingSpeed = previousSpeed;

        // 进入真正的狂暴模式
        EnterBerserkMode();
    }

    /// <summary>
    /// 进入狂暴模式
    /// </summary>
    private void EnterBerserkMode()
    {
        isBerserk = true;
        isRaging = false; // 解除 `rage` 状态

        // 修改属性
        attackDamage = berserkAttackDamage;
        attackInterval = berserkAttackInterval;
        movingSpeed = berserkMovingSpeed;
        slowFactor = berserkSlowFactor;

        Debug.Log($"{gameObject.name} 进入狂暴模式！攻击力: {attackDamage}, 移动速度: {movingSpeed}, 攻击间隔: {attackInterval}");

        // **平滑过渡 Blend Tree `speed` 变量**
        if (animationTransitionCoroutine != null)
        {
            StopCoroutine(animationTransitionCoroutine);
        }
        animationTransitionCoroutine = StartCoroutine(SmoothAnimationSpeed(1.0f));
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
    /// **平滑调整指定模型的大小**
    /// </summary>
    private IEnumerator ScaleOverTime(Transform target, Vector3 targetScale, float duration)
    {
        if (target == null) yield break; // 确保对象不为空

        float elapsedTime = 0f;
        Vector3 startScale = target.localScale;

        while (elapsedTime < duration)
        {
            target.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        target.localScale = targetScale;
    }
}
