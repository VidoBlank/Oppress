using UnityEngine;

public class MedicLevelSystem : CharacterLevelSystem
{
    // ҽ�Ʊ��ɳ�����
    private float healthIncrementPerLevel = 20f;     // �������гɳ�
    private float energyIncrementPerLevel = 30f;     // �����ɳ��ϸߣ����ϼ���Ƶ��
    private int damageIncrementPerLevel = 0;         // ������΢�������򲻱�

    public override void SetAttributesByLevel(int level)
    {
        if (player == null) return;

        // ��¼��ǰ����ֵ����������
        float healthRatio = player.health / player.attribute.maxhealth;
        float energyRatio = player.energy / player.attribute.maxEnergy;

        switch (level)
        {
            case 1:
                // ��ʼ���ԣ��޶���ӳ�
                break;

            case 2:
                player.attribute.maxhealth += healthIncrementPerLevel;
                player.attribute.maxEnergy += energyIncrementPerLevel;
                player.attribute.damage += damageIncrementPerLevel;
                break;

            case 3:
                player.attribute.maxhealth += healthIncrementPerLevel;
                player.attribute.maxEnergy += energyIncrementPerLevel;
                player.attribute.damage += damageIncrementPerLevel;
                break;

            default:
                Debug.LogWarning($"{player.name} �ĵȼ� {level} δ���壡");
                return;
        }

        // �������ָ���ǰ����ֵ������
        player.health = Mathf.Clamp(player.attribute.maxhealth * healthRatio, 0, player.attribute.maxhealth);
        player.energy = Mathf.Clamp(player.attribute.maxEnergy * energyRatio, 0, player.attribute.maxEnergy);

        // ����Ӧ�ò��Ч��
        if (player.slots != null)
        {
            foreach (var slot in player.slots)
            {
                slot.ApplyEffect(player);
            }
        }

        Debug.Log($"[MedicLevelSystem] {player.symbol.unitName} �ȼ� {level} ������ɣ�" +
                  $"�������ֵ��{player.attribute.maxhealth}�����������{player.attribute.maxEnergy}����������{player.attribute.damage}");
    }
}
