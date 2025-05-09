using UnityEngine;

public class MedicLevelSystem : CharacterLevelSystem
{
    // 医疗兵成长参数
    private float healthIncrementPerLevel = 20f;     // 生命适中成长
    private float energyIncrementPerLevel = 30f;     // 能量成长较高，保障技能频率
    private int damageIncrementPerLevel = 0;         // 攻击力微弱提升或不变

    public override void SetAttributesByLevel(int level)
    {
        if (player == null) return;

        // 记录当前生命值和能量比例
        float healthRatio = player.health / player.attribute.maxhealth;
        float energyRatio = player.energy / player.attribute.maxEnergy;

        switch (level)
        {
            case 1:
                // 初始属性，无额外加成
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
                Debug.LogWarning($"{player.name} 的等级 {level} 未定义！");
                return;
        }

        // 按比例恢复当前生命值与能量
        player.health = Mathf.Clamp(player.attribute.maxhealth * healthRatio, 0, player.attribute.maxhealth);
        player.energy = Mathf.Clamp(player.attribute.maxEnergy * energyRatio, 0, player.attribute.maxEnergy);

        // 重新应用插槽效果
        if (player.slots != null)
        {
            foreach (var slot in player.slots)
            {
                slot.ApplyEffect(player);
            }
        }

        Debug.Log($"[MedicLevelSystem] {player.symbol.unitName} 等级 {level} 升级完成，" +
                  $"最大生命值：{player.attribute.maxhealth}，最大能量：{player.attribute.maxEnergy}，攻击力：{player.attribute.damage}");
    }
}
