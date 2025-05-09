using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "AdrenalineRush", menuName = "SlotEffects/AdrenalineRush")]
public class AdrenalineRush : SlotEffect
{
    [Header("Ѫ����ֵ��С�ڸðٷֱ�ʱ������")]
    [Range(0f, 1f)]
    public float healthThreshold = 0.4f;

    [Header("�������ʱ�䣨�룩")]
    public float buffDuration = 10f;

    [Header("�ƶ��ٶȼӳɣ��̶�ֵ��")]
    public float additionalMoveSpeed = 1.5f;

    [Header("��������ϵ����0.25=����+25%��")]
    public float attackSpeedMultiplier = 1.25f;

    [Header("���˼ӳɣ�����ֵ��")]
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

        // �Ѵ������ٴ���
        var state = player.GetSlotEffectState(this);
        if (state.hasUsedThisBattle) return;

        Debug.Log($"[AdrenalineRush] {player.symbol.unitName} Ѫ������{healthThreshold * 100}%��������������Ч����");

        // ��������
        player.additionalMovingSpeedFromSlotEffect += additionalMoveSpeed;
        player.additionalAttackDelayFromSlotEffect -= player.attribute.attackDelay * (1f - 1f / attackSpeedMultiplier);
        player.additionalDamageTakenMultiplierFromSlotEffect += additionalDamageReductionMultiplier;

        SetTriggerState(player);

        // �Զ���ԭ
        player.StartCoroutine(RemoveBuffAfterDuration(player, buffDuration));
    }

    public override void RemoveEffect(PlayerController player)
    {
        // �����Ƴ� = ��ԭ
        player.additionalMovingSpeedFromSlotEffect -= additionalMoveSpeed;
        player.additionalAttackDelayFromSlotEffect += player.attribute.attackDelay * (1f - 1f / attackSpeedMultiplier);
        player.additionalDamageTakenMultiplierFromSlotEffect -= additionalDamageReductionMultiplier;

        Debug.Log($"[AdrenalineRush] {player.symbol.unitName} �������ؽ��������Ի�ԭ");
    }

    private IEnumerator RemoveBuffAfterDuration(PlayerController player, float duration)
    {
        yield return new WaitForSeconds(duration);
        RemoveEffect(player);
    }
}
