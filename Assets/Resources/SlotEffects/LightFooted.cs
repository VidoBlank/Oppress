using UnityEngine;

[CreateAssetMenu(fileName = "LightFooted", menuName = "SlotEffects/LightFooted")]
public class LightFooted : SlotEffect
{
    [Header("速度提升比例")]
    public float speedMultiplier = 1.05f;

    [Header("减少的最大生命值（固定值）")]
    public float maxHealthReduction = 10f;

    // 实际交互速度是原值除以速度倍数
    public float interactSpeedMultiplier => 1f / speedMultiplier;

    public override void ApplyEffect(PlayerController player)
    {
        var state = player.GetSlotEffectState(this);
        if (state.isApplied) return;
        state.isApplied = true;

        // 只修改属性加成，不修改状态值（血量）
        player.additionalMaxHealthFromSlotEffect -= maxHealthReduction;
        player.additionalMovingSpeedFromSlotEffect += player.attribute.movingSpeed * (speedMultiplier - 1f);
        player.additionalInteractSpeedFromSlotEffect -= player.attribute.interactspeed * (1f - interactSpeedMultiplier);

        player.TriggerStatsChanged();

        Debug.Log($"[LightFooted] 应用：-最大生命 {maxHealthReduction}，当前HP={player.health:F1}/{player.TotalMaxHealth:F1}");
    }

    public override void RemoveEffect(PlayerController player)
    {
        var state = player.GetSlotEffectState(this);
        if (!state.isApplied) return;
        state.isApplied = false;

        player.additionalMaxHealthFromSlotEffect += maxHealthReduction;
        player.additionalMovingSpeedFromSlotEffect -= player.attribute.movingSpeed * (speedMultiplier - 1f);
        player.additionalInteractSpeedFromSlotEffect += player.attribute.interactspeed * (1f - interactSpeedMultiplier);

        player.TriggerStatsChanged();

        Debug.Log($"[LightFooted] 移除：+最大生命 {maxHealthReduction}，当前HP={player.health:F1}/{player.TotalMaxHealth:F1}");
    }
}
