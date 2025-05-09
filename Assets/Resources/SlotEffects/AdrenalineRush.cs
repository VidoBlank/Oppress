using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "AdrenalineRush", menuName = "SlotEffects/AdrenalineRush")]
public class AdrenalineRush : SlotEffect
{
    [Header("血量阈值（小于该百分比时触发）")]
    [Range(0f, 1f)]
    public float healthThreshold = 0.4f;

    [Header("增益持续时间（秒）")]
    public float buffDuration = 10f;

    [Header("移动速度加成（固定值）")]
    public float additionalMoveSpeed = 1.5f;

    [Header("攻速提升系数（0.25=攻速+25%）")]
    public float attackSpeedMultiplier = 1.25f;

    [Header("减伤加成（减少值）")]
    public float additionalDamageReductionMultiplier = -0.2f;

    private void OnEnable()
    {
        cooldown = 120f;
        onlyApplyInBattle = true;
    }

    public override void ApplyEffect(PlayerController player)
    {
        if (!CanTrigger(player)) return;

        float thresholdValue = player.TotalMaxHealth * healthThreshold;
        if (player.health > thresholdValue) return;

        // 已触发不再触发
        var state = player.GetSlotEffectState(this);
        if (state.hasUsedThisBattle) return;

        Debug.Log($"[AdrenalineRush] {player.symbol.unitName} 血量低于{healthThreshold * 100}%，触发肾上腺素效果！");

        // 增加属性
        player.additionalMovingSpeedFromSlotEffect += additionalMoveSpeed;
        player.additionalAttackDelayFromSlotEffect -= player.attribute.attackDelay * (1f - 1f / attackSpeedMultiplier);
        player.additionalDamageTakenMultiplierFromSlotEffect += additionalDamageReductionMultiplier;

        SetTriggerState(player);

        // 自动还原
        player.StartCoroutine(RemoveBuffAfterDuration(player, buffDuration));
    }

    public override void RemoveEffect(PlayerController player)
    {
        // 主动移除 = 还原
        player.additionalMovingSpeedFromSlotEffect -= additionalMoveSpeed;
        player.additionalAttackDelayFromSlotEffect += player.attribute.attackDelay * (1f - 1f / attackSpeedMultiplier);
        player.additionalDamageTakenMultiplierFromSlotEffect -= additionalDamageReductionMultiplier;

        Debug.Log($"[AdrenalineRush] {player.symbol.unitName} 肾上腺素结束，属性还原");
    }

    private IEnumerator RemoveBuffAfterDuration(PlayerController player, float duration)
    {
        yield return new WaitForSeconds(duration);
        RemoveEffect(player);
    }
}
