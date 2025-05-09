using UnityEngine;

public abstract class SlotEffect : ScriptableObject
{
    [Header("通用限制（可选）")]
    [Tooltip("是否在同一场战斗只能触发一次")]
    public bool oncePerBattle = false;

    [Tooltip("触发后的冷却时间（秒）")]
    public float cooldown = 0f;

    [Header("是否仅在关卡内生效")]
    public bool onlyApplyInBattle = true;

    // 每帧调用，适合条件型/持续型天赋（如血量检测类）
    public virtual void Tick(PlayerController player)
    {
        if (!CanTrigger(player)) return;
    }

    protected bool CanTrigger(PlayerController player)
    {
        if (onlyApplyInBattle && !player.isInBattle) return false;

        var state = player.GetSlotEffectState(this);
        if (oncePerBattle && state.hasUsedThisBattle) return false;
        if (Time.time < state.nextAvailableTime) return false;

        return true;
    }

    protected void SetTriggerState(PlayerController player)
    {
        var state = player.GetSlotEffectState(this);
        if (oncePerBattle) state.hasUsedThisBattle = true;
        state.nextAvailableTime = Time.time + cooldown;

        Debug.Log($"[SlotEffect] {name} 触发成功，冷却至 {state.nextAvailableTime:F1}");
    }

    public abstract void ApplyEffect(PlayerController player);
    public abstract void RemoveEffect(PlayerController player);
}

[System.Serializable]
public class SlotEffectState
{
    public bool hasUsedThisBattle = false;
    public bool isApplied = false;
    public bool isActive = false;
    public float nextAvailableTime = 0f;

    public void ResetBattleState()
    {
        hasUsedThisBattle = false;
        nextAvailableTime = 0f;
        // ⚠️ isApplied 与 isActive 不应在此重置！
    }

#if UNITY_EDITOR
    public void DebugValidate(string effectName)
    {
        if (isApplied && isActive && hasUsedThisBattle && Time.time < nextAvailableTime)
        {
            Debug.LogWarning($"[SlotEffectState] 状态异常：{effectName} 当前状态可能冲突");
        }
    }
#endif
}
