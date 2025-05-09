using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SkillCooldownManager : MonoBehaviour
{
    [Header("绑定 PlayerController（用于获取当前装备技能）")]
    public PlayerController playerController;

    [Header("冷却背景图（完成时闪烁效果）")]
    public Image cooldownBGImage;

    [Header("技能冷却 Slider（仅一个）")]
    public Slider cooldownSlider;

    [Header("技能图标 Image（仅一个）")]
    public Image skillImage;

    [Header("颜色设置")]
    [Tooltip("冷却状态下的颜色（深灰）")]
    public Color cooldownColor = new Color(0.29f, 0.29f, 0.29f, 1f);
    [Tooltip("冷却完成时的颜色（白色）")]
    public Color readyColor = Color.white;
    [Tooltip("技能激活中（例如持续技能）的颜色")]
    public Color activeColor = Color.cyan;
    [Tooltip("背景图冷却中的颜色")]
    public Color bgCooldownColor = new Color(0.15f, 0.15f, 0.15f, 1f);
    [Tooltip("激活状态背景色")]
    public Color bgActiveColor = new Color(0.1f, 0.3f, 0.3f, 1f);

    // 状态
    private bool isCoolingDown = false;
    private bool isActive = false; // 记录技能是否处于激活状态
    private float currentCooldown = 0f;
    private float currentTimer = 0f;
    private Color originalBGColor;
    private Vector3 originalScale;
    private Sequence activeSequence; // 用于持续技能的动画序列

    private void Awake()
    {
        // 在Awake中记录原始缩放，避免在Start之前需要使用
        if (skillImage != null)
        {
            originalScale = skillImage.transform.localScale;
        }
    }

    private void Start()
    {
        if (cooldownSlider != null)
        {
            cooldownSlider.value = 0f;
            cooldownSlider.gameObject.SetActive(false);
        }

        if (skillImage != null)
        {
            skillImage.color = readyColor;
            originalScale = skillImage.transform.localScale;
        }

        if (cooldownBGImage != null)
        {
            // 强制让 originalBGColor = 脚本里设置的 bgCooldownColor
            originalBGColor = bgCooldownColor;
            // 同步到当前显示
            cooldownBGImage.color = bgCooldownColor;
        }
    }

    private void Update()
    {
        if (!isCoolingDown || currentCooldown <= 0f || playerController == null)
            return;

        currentTimer += Time.deltaTime;
        float ratio = 1f - (currentTimer / currentCooldown);
        ratio = Mathf.Clamp01(ratio);

        if (cooldownSlider != null)
            cooldownSlider.value = ratio;

        if (ratio <= 0f)
        {
            isCoolingDown = false;

            if (skillImage != null && !isActive) // 只有在非激活状态才恢复颜色
                skillImage.color = readyColor;

            // 冷却完成，背景图恢复颜色并闪烁
            if (cooldownBGImage != null && !isActive)
            {
                cooldownBGImage.color = originalBGColor;
                PlayCooldownFinishedEffect();
            }
        }
    }

    /// <summary>
    /// 启动冷却（用于瞬发技能）
    /// </summary>
    public void StartCooldownNow(IPlayerSkill skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("StartCooldownNow 被调用但技能为空");
            return;
        }

        // 停止任何正在播放的激活动画
        if (activeSequence != null)
        {
            activeSequence.Kill();
            activeSequence = null;
        }

        // 确保技能不再显示为激活状态
        isActive = false;

        currentCooldown = skill.Cooldown;
        currentTimer = 0f;
        isCoolingDown = true;

        if (cooldownSlider != null)
        {
            cooldownSlider.gameObject.SetActive(true);
            cooldownSlider.value = 1f;
        }

        if (skillImage != null)
        {
            skillImage.color = cooldownColor;
            // 确保图标恢复原始大小
            skillImage.transform.localScale = originalScale;
        }

        if (cooldownBGImage != null)
            cooldownBGImage.color = bgCooldownColor;
    }

    /// <summary>
    /// 设置图标为"激活中"状态视觉效果（持续技能）
    /// </summary>
    public void SetSkillActiveVisual(bool active)
    {
        if (skillImage == null) return;

        // 更新状态跟踪
        isActive = active;

        // 停止任何正在播放的激活动画
        if (activeSequence != null)
        {
            activeSequence.Kill();
            activeSequence = null;
        }

        if (active)
        {
            // 激活状态 - 设置颜色并添加脉冲动画效果
            skillImage.color = activeColor;
            if (cooldownBGImage != null)
                cooldownBGImage.color = bgActiveColor;

            // 创建新的动画序列 - 循环脉冲效果
            activeSequence = DOTween.Sequence();

            // 缩放和颜色交替变化的脉冲动画
            activeSequence.Append(skillImage.transform.DOScale(originalScale * 1.2f, 0.4f).SetEase(Ease.OutQuad));
            activeSequence.Join(skillImage.DOColor(activeColor, 0.4f).SetEase(Ease.OutQuad));

            activeSequence.Append(skillImage.transform.DOScale(originalScale * 1.1f, 0.4f).SetEase(Ease.InQuad));
            activeSequence.Join(skillImage.DOColor(new Color(activeColor.r, activeColor.g, activeColor.b, 0.8f), 0.4f).SetEase(Ease.InQuad));

            // 设置为循环播放
            activeSequence.SetLoops(-1, LoopType.Restart);

            // 添加背景效果
            if (cooldownBGImage != null)
            {
                DOTween.Sequence()
                    .Append(cooldownBGImage.DOColor(bgActiveColor, 0.2f))
                    .SetEase(Ease.OutQuad);
            }
        }
        else
        {
            // 非激活状态 - 恢复正常颜色和大小
            skillImage.color = isCoolingDown ? cooldownColor : readyColor;
            skillImage.transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutQuad);

            // 恢复背景色
            if (cooldownBGImage != null)
            {
                cooldownBGImage.DOColor(isCoolingDown ? bgCooldownColor : originalBGColor, 0.2f)
                    .SetEase(Ease.OutQuad);
            }
        }
    }

    /// <summary>
    /// 取消冷却 UI（例如技能取消使用）
    /// </summary>
    public void CancelCooldown()
    {
        isCoolingDown = false;
        currentCooldown = 0f;
        currentTimer = 0f;

        if (cooldownSlider != null)
            cooldownSlider.gameObject.SetActive(false);

        if (skillImage != null && !isActive) // 只有在非激活状态才恢复颜色
            skillImage.color = readyColor;

        if (cooldownBGImage != null && !isActive) // 只有在非激活状态才恢复背景色
            cooldownBGImage.color = originalBGColor;
    }

    /// <summary>
    /// 冷却完成闪烁背景图（白色 → 原色）
    /// </summary>
    private void PlayCooldownFinishedEffect()
    {
        if (cooldownBGImage == null || isActive) return; // 在技能激活状态下不播放冷却完成效果

        float duration = 0.25f;
        Color flashColor = Color.white;

        cooldownBGImage.DOColor(flashColor, duration).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                if (!isActive) // 再次检查，确保在效果完成前技能没有被激活
                    cooldownBGImage.DOColor(originalBGColor, duration).SetEase(Ease.InQuad);
            });
    }

    /// <summary>
    /// 显示UI时同步冷却条
    /// </summary>
    public void SyncCooldown()
    {
        if (playerController == null || playerController.equippedSkill == null)
            return;

        // 检查是否是持续型技能并且正处于激活状态
        if (playerController.equippedSkill.IsSustained && playerController.equippedSkill.IsActive)
        {
            // 如果当前是激活状态但UI未显示激活效果，则更新UI
            if (!isActive)
            {
                SetSkillActiveVisual(true);
            }
            return;
        }

        // 先检查冷却状态
        float cooldown = playerController.equippedSkill.Cooldown;
        float timer = playerController.equippedSkillCooldownTimer;

        if (cooldown <= 0f || timer >= cooldown)
        {
            // 技能已经冷却完毕
            cooldownSlider.gameObject.SetActive(false);

            // 只有在非激活状态才恢复默认颜色
            if (!isActive)
            {
                skillImage.color = readyColor;
                if (cooldownBGImage != null)
                    cooldownBGImage.color = originalBGColor;
            }

            isCoolingDown = false;
            return;
        }

        // 技能还在冷却中
        cooldownSlider.gameObject.SetActive(true);
        float ratio = 1f - (timer / cooldown);
        ratio = Mathf.Clamp01(ratio);
        cooldownSlider.value = ratio;

        // 只有在非激活状态才显示冷却颜色
        if (!isActive)
        {
            skillImage.color = cooldownColor;
            if (cooldownBGImage != null)
                cooldownBGImage.color = bgCooldownColor;
        }

        isCoolingDown = true;
        currentCooldown = cooldown;
        currentTimer = timer;
    }

    /// <summary>
    /// 检查技能是否在冷却中
    /// </summary>
    public bool IsInCooldown()
    {
        return isCoolingDown;
    }

    /// <summary>
    /// 检查技能是否处于激活状态
    /// </summary>
    public bool IsSkillActive()
    {
        return isActive;
    }

    /// <summary>
    /// 获取当前冷却比例 (0-1)，0表示已完成冷却，1表示刚开始冷却
    /// </summary>
    public float GetCooldownRatio()
    {
        if (!isCoolingDown || currentCooldown <= 0f)
            return 0f;

        return 1f - (currentTimer / currentCooldown);
    }

    /// <summary>
    /// 获取剩余冷却时间（秒）
    /// </summary>
    public float GetRemainingCooldownTime()
    {
        if (!isCoolingDown || currentCooldown <= 0f)
            return 0f;

        return currentCooldown - currentTimer;
    }

    /// <summary>
    /// 显示或隐藏整个冷却UI
    /// </summary>
    public void SetUIVisibility(bool visible)
    {
        if (skillImage != null)
            skillImage.gameObject.SetActive(visible);

        if (cooldownBGImage != null)
            cooldownBGImage.gameObject.SetActive(visible);

        if (cooldownSlider != null && isCoolingDown)
            cooldownSlider.gameObject.SetActive(visible);
    }

    private void OnDisable()
    {
        // 确保停止所有动画
        if (activeSequence != null)
        {
            activeSequence.Kill();
            activeSequence = null;
        }

        DOTween.Kill(skillImage);
        DOTween.Kill(cooldownBGImage);
    }
}