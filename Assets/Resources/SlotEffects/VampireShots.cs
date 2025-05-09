using UnityEngine;

[CreateAssetMenu(fileName = "VampireShots", menuName = "SlotEffects/VampireShots")]
public class VampireShots : SlotEffect
{
    [Header("吸血比例（造成伤害的百分比）")]
    [Range(0f, 1f)]
    public float lifestealPercent = 0.03f;

    public override void ApplyEffect(PlayerController player)
    {
        // 由 Bullet 系统决定是否调用，不需要注册事件
    }

    public override void RemoveEffect(PlayerController player)
    {
        // 同样不需要卸载事件
    }

    public void TriggerLifesteal(PlayerController player, float damage)
    {
        if (!CanTrigger(player)) return;

        float healAmount = damage * lifestealPercent;

        // ✅ 加入最小值保护（避免 0 血回血失败）
        if (healAmount < 1f)
            healAmount = 1f;

        // ✅ 使用封装的恢复方法，确保日志和最大血量判断统一
        player.RecoverHealth(healAmount);

        SetTriggerState(player); // 注册本场战斗已触发
        Debug.Log($"[VampireShots] {player.symbol.unitName} 吸血成功 +{Mathf.RoundToInt(healAmount)}，当前HP：{player.health}/{player.TotalMaxHealth}");
    }
}
