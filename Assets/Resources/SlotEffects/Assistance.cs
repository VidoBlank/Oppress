using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "SlotEffects/Assistance")]
public class Assistance : SlotEffect
{
    [Header("减少恢复时间（秒）")]
    public float recoverSpeedBoost = 1.5f;

    [Header("救援后额外恢复生命（%）")]
    [Range(1f, 100f)]
    public float extraHealPercent = 20f;

    private void OnEnable()
    {
        onlyApplyInBattle = false; // 战斗外也生效
    }

    public override void ApplyEffect(PlayerController player)
    {
        var state = player.GetSlotEffectState(this);

        if (state.hasUsedThisBattle)
        {
            Debug.Log($"[Assistance] {player.symbol.unitName} 已应用，不重复。");
            return;
        }

        player.additionalRecoverSpeedFromSlotEffect -= recoverSpeedBoost;
        player.OnPlayerRescueSuccess += OnRescueSuccess;

        Debug.Log($"[Assistance] {player.symbol.unitName} 恢复速度加快 -{recoverSpeedBoost} 秒");

        state.hasUsedThisBattle = true;
    }

    public override void RemoveEffect(PlayerController player)
    {
        player.additionalRecoverSpeedFromSlotEffect += recoverSpeedBoost;
        player.OnPlayerRescueSuccess -= OnRescueSuccess;

        Debug.Log($"[Assistance] {player.symbol.unitName} 移除效果，恢复速度恢复");
    }

    private void OnRescueSuccess(PlayerController rescuer, PlayerController rescued)
    {
        if (rescued == null || rescued.isDead) return;

        Debug.Log($"[Assistance] {rescuer.symbol.unitName} 救援 {rescued.symbol.unitName} 成功，准备回血");

        rescuer.StartCoroutine(DelayHeal(rescued));
    }

    private IEnumerator DelayHeal(PlayerController rescued)
    {
        yield return new WaitForSeconds(0.5f);

        float healAmount = rescued.TotalMaxHealth * (extraHealPercent / 100f);
        rescued.RecoverHealth(healAmount);

        Debug.Log($"[Assistance] {rescued.symbol.unitName} 额外恢复 {healAmount:F1} HP（{extraHealPercent}%）");
    }
}
