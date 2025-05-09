using UnityEngine;

public class RookieLevelSystem : MonoBehaviour
{
    /// <summary>
    /// 设置 Rookie (0级) 的通用属性
    /// </summary>
    public void SetRookieAttributes(PlayerController player)
    {
        player.attribute.maxhealth = 80f;
        player.attribute.maxEnergy = 80f;
       
        
        player.attribute.damage = 5;
        player.attribute.attackRange.radius = 5f;
        player.attribute.attackDelay = 1f;
       

        Debug.Log("设置 Rookie (0级) 的通用属性");
    }
}
