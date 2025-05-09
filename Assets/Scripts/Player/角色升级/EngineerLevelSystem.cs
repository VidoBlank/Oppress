using UnityEngine;

public class EngineerLevelSystem : CharacterLevelSystem
{
    public override void SetAttributesByLevel(int level)
    {
        if (player == null) return;

        switch (level)
        {


            case 1:
                player.attribute.maxhealth = 150f;
                player.attribute.maxEnergy = 100f;

                player.attribute.damage = 5;
                player.attribute.attackRange.radius = 5f;

                break;

            case 2:
                player.attribute.maxhealth = 200f;
                player.attribute.maxEnergy = 125f;
          

                player.attribute.damage = 6;
                player.attribute.attackRange.radius = 5f;

                break;

            case 3:
                player.attribute.maxhealth = 250f;
                player.attribute.maxEnergy = 150f;


                player.attribute.damage = 7;
                player.attribute.attackRange.radius = 5f;

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
