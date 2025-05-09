using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FireMomentum", menuName = "SlotEffects/FireMomentum")]
public class FireMomentum : SlotEffect
{
    [Header("ÿ�ι������͵İٷֱȣ�2%��")]
    public float reductionPerStack = 0.02f;

    [Header("�����Ӳ���")]
    public int maxStacks = 8;

    [Header("����ʱ�䣨�룩���޹���������")]
    public float buffDuration = 4f;

    private class PlayerFireMomentumState
    {
        public int currentStacks = 0;
        public Coroutine resetCoroutine = null;
    }

    private Dictionary<string, PlayerFireMomentumState> states = new Dictionary<string, PlayerFireMomentumState>();

    public override void ApplyEffect(PlayerController player)
    {
        string id = player.symbol.unitID;

        if (!states.ContainsKey(id))
        {
            states[id] = new PlayerFireMomentumState();
        }

        player.OnNormalAttack += () => OnPlayerAttack(player);
    }

    public override void RemoveEffect(PlayerController player)
    {
        string id = player.symbol.unitID;

        player.OnNormalAttack -= () => OnPlayerAttack(player);

        if (states.ContainsKey(id))
        {
            var state = states[id];
            if (state.resetCoroutine != null)
                player.StopCoroutine(state.resetCoroutine);

            // ������й��ټӳ�
            player.additionalAttackDelayFromSlotEffect = 0f;
            player.TriggerStatsChanged();

            states.Remove(id);
        }
    }

    private void OnPlayerAttack(PlayerController player)
    {
        string id = player.symbol.unitID;
        if (!states.ContainsKey(id)) return;

        var state = states[id];

        state.currentStacks = Mathf.Min(state.currentStacks + 1, maxStacks);

        float reduction = player.attribute.attackDelay * reductionPerStack * state.currentStacks;
        player.additionalAttackDelayFromSlotEffect = -reduction;
        player.TriggerStatsChanged();

        Debug.Log($"[FireMomentum] {player.symbol.unitName} ����BUFF ������{state.currentStacks} ����������٣�{reduction:F3}s");

        if (state.resetCoroutine != null)
            player.StopCoroutine(state.resetCoroutine);

        state.resetCoroutine = player.StartCoroutine(ResetStacksAfterDelay(player, buffDuration));
    }

    private IEnumerator ResetStacksAfterDelay(PlayerController player, float delay)
    {
        yield return new WaitForSeconds(delay);

        string id = player.symbol.unitID;
        if (!states.ContainsKey(id)) yield break;

        var state = states[id];
        state.currentStacks = 0;
        player.additionalAttackDelayFromSlotEffect = 0f;
        player.TriggerStatsChanged();

        Debug.Log($"[FireMomentum] {player.symbol.unitName} BUFF��ʧ�����ٻ�ԭ");

        state.resetCoroutine = null;
    }
}
