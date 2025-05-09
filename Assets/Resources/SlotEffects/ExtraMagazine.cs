using UnityEngine;

[CreateAssetMenu(fileName = "ExtraMagazine", menuName = "SlotEffects/ExtraMagazine")]
public class ExtraMagazine : SlotEffect
{
    [Header("增加的最大能量")]
    public float extraEnergy = 20f;

    private void OnEnable()
    {
        onlyApplyInBattle = false;  // 是否仅在战斗中应用
    }

    public override void ApplyEffect(PlayerController player)
    {
        var state = player.GetSlotEffectState(this);

        // 如果已触发过则不重复应用
        if (state.hasUsedThisBattle)
        {
            Debug.Log($"[ExtraMagazine] {player.symbol.unitName} 已应用备用弹匣，不再重复。");
            return;
        }

        player.additionalMaxEnergyFromSlotEffect += extraEnergy;
        player.energy = Mathf.Min(player.energy + extraEnergy, player.TotalMaxEnergy);

        Debug.Log($"[ExtraMagazine] {player.symbol.unitName} 最大能量 +{extraEnergy} → {player.TotalMaxEnergy}，当前能量为 {player.energy}");

        state.hasUsedThisBattle = true;
    }

    public override void RemoveEffect(PlayerController player)
    {
        player.additionalMaxEnergyFromSlotEffect -= extraEnergy;
        player.energy = Mathf.Min(player.energy, player.TotalMaxEnergy);
        Debug.Log($"[ExtraMagazine] {player.symbol.unitName} 移除备用弹匣，最大能量恢复 → {player.TotalMaxEnergy}");
    }
}
