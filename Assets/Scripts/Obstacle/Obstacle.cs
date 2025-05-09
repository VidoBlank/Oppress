using UnityEngine;
using System.Collections.Generic;

public class Obstacle : MonoBehaviour
{
    [Header("�ϰ�������")]
    public float maxHealth = 100f;  // �������ֵ
    public float currentHealth;

    private bool isDestroyed = false;  // �Ƿ��ѱ��ݻٵı��
    private int attackCount = 0;  // ��ǰ��������

    [Header("��������")]
    public int requiredHits = 5;  // ��ҹ���������ܴ���
    public float damagePerHitPercent = 0.2f;  // ÿ�ι�����ɵ��˺��ٷֱȣ�Ĭ��Ϊ�������ֵ��20%

    private List<PlayerController> playersAttacking = new List<PlayerController>();  // �������ϰ��������б�

    private void Start()
    {
        // ��ʼ����ǰ����ֵ
        currentHealth = maxHealth;
    }

    // ���˷���
    // ���˷���
    public void TakeDamage(float damage, PlayerController player = null, bool isFromEnemy = false)
    {
        if (isDestroyed) return;

        float actualDamage;

        if (isFromEnemy)
        {
            // ���Թ�����˺����������ֵ����
            actualDamage = damage;
            Debug.Log($"�ϰ����ܵ�����Ĺ������˺�: {actualDamage}");
        }
        else
        {
            // ������ҵĹ��������ݹ��������������˺�
            actualDamage = maxHealth * damagePerHitPercent;
            Debug.Log($"�ϰ����ܵ���ҵĹ������̶��˺�: {actualDamage}");
        }

        currentHealth -= actualDamage;

        Debug.Log($"�ϰ��ﵱǰ����ֵ: {currentHealth}");

        // �������ҹ�������¼��������Ҳ����ӹ�������
        if (player != null && !playersAttacking.Contains(player))
        {
            playersAttacking.Add(player);
        }

        // ������������������Ƿ�ﵽ��������Ҫ��
        if (!isFromEnemy)
        {
            attackCount++;
            if (attackCount >= requiredHits)
            {
                // �ﵽ��������Ҫ���ݻ��ϰ���
                DestroyObstacle();
            }
        }

        if (currentHealth <= 0)
        {
            DestroyObstacle();
        }
    }


    // �ϰ��ﱻ�ݻٵĴ���
    private void DestroyObstacle()
    {
        isDestroyed = true;
        Debug.Log("�ϰ����ѱ��ݻ�");

        // �������й������ϰ������ҵĹ�������
        foreach (var player in playersAttacking)
        {
            if (player != null)
            {
                player.isObstaclecanhit = false;  // �ر���ҹ����ϰ��������
            }
        }

        // �����ϰ���
        Destroy(gameObject);
    }

    // ���ϰ�������˺��ķ���
    public void ApplyDamageFromPlayer(PlayerController player)
    {
        if (player != null)
        {
            TakeDamage(player.TotalDamage, player); // ��ȡ��ʵ�˺�
            Debug.Log($"������ϰ��ｻ�����ϰ����ܵ��� {player.TotalDamage} ���˺�");
        }
        else
        {
            Debug.LogError("PlayerController Ϊ�գ��޷������ϰ����˺�");
        }
    }
}

