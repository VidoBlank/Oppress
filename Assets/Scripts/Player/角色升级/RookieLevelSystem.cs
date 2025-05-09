using UnityEngine;

public class RookieLevelSystem : MonoBehaviour
{
    /// <summary>
    /// ���� Rookie (0��) ��ͨ������
    /// </summary>
    public void SetRookieAttributes(PlayerController player)
    {
        player.attribute.maxhealth = 80f;
        player.attribute.maxEnergy = 80f;
       
        
        player.attribute.damage = 5;
        player.attribute.attackRange.radius = 5f;
        player.attribute.attackDelay = 1f;
       

        Debug.Log("���� Rookie (0��) ��ͨ������");
    }
}
