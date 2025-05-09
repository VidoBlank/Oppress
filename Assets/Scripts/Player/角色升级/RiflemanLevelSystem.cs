using UnityEngine;

public class RiflemanLevelSystem : CharacterLevelSystem
{
    // ✅ 定义升级时增加的增量
    private float healthIncrementPerLevel = 25f;
    private float energyIncrementPerLevel = 25f;
    private int damageIncrementPerLevel = 1;

    public override void SetAttributesByLevel(int level)
    {
        if (player == null) return;

        // 记录当前生命值和能量比例
        float healthRatio = player.health / player.attribute.maxhealth;
        float energyRatio = player.energy / player.attribute.maxEnergy;

        // ✅ 计算升级后的属性（基于当前属性增加）
        switch (level)
        {
            case 1:
              
               
                break;

            case 2:
                player.attribute.maxhealth += healthIncrementPerLevel;   // 增加 25
                player.attribute.maxEnergy += energyIncrementPerLevel;  // 增加 25
                player.attribute.damage += damageIncrementPerLevel;     // 增加 1
               
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

        // ✅ 重新调整当前生命值和能量值，按比例更新
        player.health = Mathf.Clamp(player.attribute.maxhealth * healthRatio, 0, player.attribute.maxhealth);
        player.energy = Mathf.Clamp(player.attribute.maxEnergy * energyRatio, 0, player.attribute.maxEnergy);

        // ✅ 检查并重新应用天赋和插槽效果
        if (player.slots != null)
        {
            foreach (var slot in player.slots)
            {
                slot.ApplyEffect(player);
            }
        }

        Debug.Log($"[RiflemanLevelSystem] {player.symbol.unitName} 等级 {level} 升级完成，" +
                  $"最大生命值提升至 {player.attribute.maxhealth}，最大能量提升至 {player.attribute.maxEnergy}，攻击力提升至 {player.attribute.damage}");
    }
}
