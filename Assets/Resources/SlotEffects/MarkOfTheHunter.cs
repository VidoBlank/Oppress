using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MarkOfTheHunter", menuName = "SlotEffects/MarkOfTheHunter")]
public class MarkOfTheHunter : SlotEffect
{
    [Header("每层减伤比例（例如0.05为5%）")]
    public float damageReductionPerStack = 0.05f;

    [Header("最多层数")]
    public int maxStacks = 3;

    private Dictionary<string, int> stackDict = new Dictionary<string, int>();
    private Dictionary<string, System.Action<PlayerController>> recoveryHandlers = new Dictionary<string, System.Action<PlayerController>>();

    public override void ApplyEffect(PlayerController player)
    {
        string id = player.symbol.unitID;

        if (!stackDict.ContainsKey(id))
            stackDict[id] = 0;

        if (recoveryHandlers.ContainsKey(id))
            return;

        System.Action<PlayerController> handler = (PlayerController p) =>
        {
            if (p != player) return;

            if (stackDict[id] < maxStacks)
            {
                stackDict[id]++;
                UpdateDamageReduction(player);
                Debug.Log($"[MarkOfTheHunter] {player.symbol.unitName} 恢复倒地后获得印记，当前层数: {stackDict[id]}");
            }
        };

        PlayerController.OnPlayerRecovered += handler;
        recoveryHandlers[id] = handler;

        // 初始也执行一次以同步状态
        UpdateDamageReduction(player);
    }

    public override void RemoveEffect(PlayerController player)
    {
        string id = player.symbol.unitID;

        if (recoveryHandlers.ContainsKey(id))
        {
            PlayerController.OnPlayerRecovered -= recoveryHandlers[id];
            recoveryHandlers.Remove(id);
        }

        stackDict.Remove(id);

        // 重置额外减伤（非基础属性）
        player.additionalDamageTakenMultiplierFromSlotEffect = 0f;
        player.TriggerStatsChanged();
    }

    private void UpdateDamageReduction(PlayerController player)
    {
        string id = player.symbol.unitID;
        int stacks = stackDict.TryGetValue(id, out int val) ? val : 0;
        float reduction = -damageReductionPerStack * stacks; // 负值表示减伤

        // 限制最大减伤（最小承伤为10%）
        reduction = Mathf.Max(reduction, -0.9f);

        player.additionalDamageTakenMultiplierFromSlotEffect = reduction;
        player.TriggerStatsChanged(); // 通知属性变化
    }

    public void ResetStacks(PlayerController player)
    {
        string id = player.symbol.unitID;
        stackDict[id] = 0;
        player.additionalDamageTakenMultiplierFromSlotEffect = 0f;
        player.TriggerStatsChanged();
    }
}
