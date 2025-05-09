using UnityEngine;

[CreateAssetMenu(fileName = "ExtraMagazine", menuName = "SlotEffects/ExtraMagazine")]
public class ExtraMagazine : SlotEffect
{
    [Header("���ӵ��������")]
    public float extraEnergy = 20f;

    private void OnEnable()
    {
        onlyApplyInBattle = false;  // �Ƿ����ս����Ӧ��
    }

    public override void ApplyEffect(PlayerController player)
    {
        var state = player.GetSlotEffectState(this);

        // ����Ѵ��������ظ�Ӧ��
        if (state.hasUsedThisBattle)
        {
            Debug.Log($"[ExtraMagazine] {player.symbol.unitName} ��Ӧ�ñ��õ�ϻ�������ظ���");
            return;
        }

        player.additionalMaxEnergyFromSlotEffect += extraEnergy;
        player.energy = Mathf.Min(player.energy + extraEnergy, player.TotalMaxEnergy);

        Debug.Log($"[ExtraMagazine] {player.symbol.unitName} ������� +{extraEnergy} �� {player.TotalMaxEnergy}����ǰ����Ϊ {player.energy}");

        state.hasUsedThisBattle = true;
    }

    public override void RemoveEffect(PlayerController player)
    {
        player.additionalMaxEnergyFromSlotEffect -= extraEnergy;
        player.energy = Mathf.Min(player.energy, player.TotalMaxEnergy);
        Debug.Log($"[ExtraMagazine] {player.symbol.unitName} �Ƴ����õ�ϻ����������ָ� �� {player.TotalMaxEnergy}");
    }
}
