using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MarkOfTheHunter", menuName = "SlotEffects/MarkOfTheHunter")]
public class MarkOfTheHunter : SlotEffect
{
    [Header("ÿ����˱���������0.05Ϊ5%��")]
    public float damageReductionPerStack = 0.05f;

    [Header("������")]
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
                Debug.Log($"[MarkOfTheHunter] {player.symbol.unitName} �ָ����غ���ӡ�ǣ���ǰ����: {stackDict[id]}");
            }
        };

        PlayerController.OnPlayerRecovered += handler;
        recoveryHandlers[id] = handler;

        // ��ʼҲִ��һ����ͬ��״̬
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

        // ���ö�����ˣ��ǻ������ԣ�
        player.additionalDamageTakenMultiplierFromSlotEffect = 0f;
        player.TriggerStatsChanged();
    }

    private void UpdateDamageReduction(PlayerController player)
    {
        string id = player.symbol.unitID;
        int stacks = stackDict.TryGetValue(id, out int val) ? val : 0;
        float reduction = -damageReductionPerStack * stacks; // ��ֵ��ʾ����

        // ���������ˣ���С����Ϊ10%��
        reduction = Mathf.Max(reduction, -0.9f);

        player.additionalDamageTakenMultiplierFromSlotEffect = reduction;
        player.TriggerStatsChanged(); // ֪ͨ���Ա仯
    }

    public void ResetStacks(PlayerController player)
    {
        string id = player.symbol.unitID;
        stackDict[id] = 0;
        player.additionalDamageTakenMultiplierFromSlotEffect = 0f;
        player.TriggerStatsChanged();
    }
}
