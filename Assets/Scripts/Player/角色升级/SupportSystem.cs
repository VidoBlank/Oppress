using UnityEngine;

public class SupportLevelSystem : CharacterLevelSystem
{
    public override void SetAttributesByLevel(int level)
    {
        if (player == null) return;

        switch (level)
        {
          
            case 1:
                player.attribute.maxhealth = 150f;
                player.health = player.attribute.maxhealth;
                player.energy = player.attribute.maxEnergy;
                player.attribute.maxEnergy = 100f;
                player.attribute.knockdownHealth = 60f;
                player.attribute.interactspeed = 3f;
                player.attribute.recoveredspeed = 2.5f;
                
                player.attribute.damage = 5;
                player.attribute.attackRange.radius = 3.5f;
                player.attribute.attackDelay = 1.0f;
                player.attribute.bulletSpeed = 20f;
                break;

            case 2:
                player.attribute.maxhealth = 150f;
                player.attribute.maxEnergy = 100f;
                player.health = player.attribute.maxhealth;
                player.energy = player.attribute.maxEnergy;

                player.attribute.knockdownHealth = 60f;
                player.attribute.interactspeed = 3f;
                player.attribute.recoveredspeed = 2.5f;
                
                player.attribute.damage = 5;
                player.attribute.attackRange.radius = 3.5f;
                player.attribute.attackDelay = 1.0f;
                player.attribute.bulletSpeed = 20f;
                break;

            case 3:
                player.attribute.maxhealth = 150f;
                player.attribute.maxEnergy = 100f;
                player.health = player.attribute.maxhealth;
                player.energy = player.attribute.maxEnergy;

                player.attribute.knockdownHealth = 60f;
                player.attribute.interactspeed = 3f;
                player.attribute.recoveredspeed = 2.5f;
                
                player.attribute.damage = 5;
                player.attribute.attackRange.radius = 3.5f;
                player.attribute.attackDelay = 1.0f;
                player.attribute.bulletSpeed = 20f;
                break;

            case 4:
                player.attribute.maxhealth = 150f;
                player.attribute.maxEnergy = 100f;
                player.health = player.attribute.maxhealth;
                player.energy = player.attribute.maxEnergy;

                player.attribute.knockdownHealth = 60f;
                player.attribute.interactspeed = 3f;
                player.attribute.recoveredspeed = 2.5f;
                
                player.attribute.damage = 5;
                player.attribute.attackRange.radius = 3.5f;
                player.attribute.attackDelay = 1.0f;
                player.attribute.bulletSpeed = 20f;
                break;

            default:
                Debug.LogWarning($"{player.name} 的等级 {level} 未定义！");
                break;
        }

        player.health = Mathf.Clamp(player.health, 0, player.attribute.maxhealth);
        player.energy = Mathf.Clamp(player.energy, 0, player.attribute.maxEnergy);

        Debug.Log($"Sniper 等级 {level} 属性已设置");
    }
}
