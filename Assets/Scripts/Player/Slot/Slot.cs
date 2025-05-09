using System;

[System.Serializable]
public class Slot
{
    public string slotName;           // 插槽的名称
    public SlotEffect slotEffect;     // 存储插槽效果的引用

    // 新增：记录该槽效果是否已被应用
    [NonSerialized]
    public bool isEffectApplied = false;

    public Slot(string name, SlotEffect effect)
    {
        this.slotName = name;
        this.slotEffect = effect;
    }

    public void ApplyEffect(PlayerController player)
    {
        // 如果效果未应用且槽有效果，就调用 ApplyEffect
        if (!isEffectApplied && slotEffect != null)
        {
            slotEffect.ApplyEffect(player);
            isEffectApplied = true;
            SaveSlotState(player);
        }
    }

    public void RemoveEffect(PlayerController player)
    {
        // 如果效果已应用且槽有效果，就调用 RemoveEffect
        if (isEffectApplied && slotEffect != null)
        {
            slotEffect.RemoveEffect(player);
            isEffectApplied = false;
            SaveSlotState(player);
        }
    }

    private void SaveSlotState(PlayerController player)
    {
        if (player != null)
        {
            // 保存槽状态到存档（如果需要的话）
            PlayerTeamManager.Instance.SaveAllUnits();
        }
    }
}
