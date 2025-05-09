using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PeakPerformance", menuName = "SlotEffects/PeakPerformance")]
public class PeakPerformance : SlotEffect
{
    [Header("血量阈值（大于该百分比时维持增益）")]
    [Range(0f, 1f)]
    public float healthThreshold = 0.75f;

    [Header("攻速百分比加成（-0.15 表示攻速+15%）")]
    [Range(-1f, 1f)]
    public float attackDelayPercentBonus = -0.15f;

    [Header("移动速度百分比加成（0.1 表示+10%）")]
    [Range(-1f, 1f)]
    public float moveSpeedPercentBonus = 0.1f;


    [Header("技能冷却加成")]
    public float cooldownRateBonus = 0.2f;

    // ✅ 存储玩家的回调引用，避免重复绑定
    private readonly Dictionary<PlayerController, Action> statChangedCallbacks = new();

    public override void ApplyEffect(PlayerController player)
    {
        var state = player.GetSlotEffectState(this);
        if (state.isApplied) return;
        state.isApplied = true;

        // ✅ 如果已注册过，先解绑
        if (statChangedCallbacks.TryGetValue(player, out var existingCallback))
        {
            player.OnStatsChanged -= existingCallback;
        }

        // ✅ 创建可追踪委托对象（不要用匿名 lambda）
        Action callback = () => Tick(player);
        statChangedCallbacks[player] = callback;
        player.OnStatsChanged += callback;

        Tick(player);
    }

    public override void RemoveEffect(PlayerController player)
    {
        var state = player.GetSlotEffectState(this);
        if (!state.isApplied) return;
        state.isApplied = false;

        if (state.isActive)
        {
            RemoveBuff(player, state); // 改为传 state
        }


        // ✅ 安全解绑事件
        if (statChangedCallbacks.TryGetValue(player, out var callback))
        {
            player.OnStatsChanged -= callback;
            statChangedCallbacks.Remove(player);
        }
    }

    public override void Tick(PlayerController player)
    {
       

        var state = player.GetSlotEffectState(this);
        float hpRatio = player.health / player.TotalMaxHealth;

        if (hpRatio > healthThreshold)
        {
            if (!state.isActive)
            {
                ApplyBuff(player, state); // ✅ 改为传 state
            }
        }
        else
        {
            if (state.isActive)
            {
                RemoveBuff(player, state);
            }
        }
    }

    private void ApplyBuff(PlayerController player, SlotEffectState state)
    {
        state.isActive = true;

        player.percentAttackDelayFromSlotEffect += attackDelayPercentBonus;
        player.percentMovingSpeedFromSlotEffect += moveSpeedPercentBonus;
        player.additionalSkillCooldownRateFromSlotEffect += cooldownRateBonus;

        Debug.Log($"[PeakPerformance] {player.symbol.unitName} 触发高血量 Buff！");
        player.TriggerStatsChanged();
    }


    private void RemoveBuff(PlayerController player, SlotEffectState state)
    {
        state.isActive = false;

        player.percentAttackDelayFromSlotEffect -= attackDelayPercentBonus;
        player.percentMovingSpeedFromSlotEffect -= moveSpeedPercentBonus;
        player.additionalSkillCooldownRateFromSlotEffect -= cooldownRateBonus;

        Debug.Log($"[PeakPerformance] {player.symbol.unitName} 移除高血量 Buff");
        player.TriggerStatsChanged();
    }


}
