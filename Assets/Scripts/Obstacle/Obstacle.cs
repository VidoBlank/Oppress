using UnityEngine;
using System.Collections.Generic;

public class Obstacle : MonoBehaviour
{
    [Header("障碍物属性")]
    public float maxHealth = 100f;  // 最大生命值
    public float currentHealth;

    private bool isDestroyed = false;  // 是否已被摧毁的标记
    private int attackCount = 0;  // 当前攻击次数

    [Header("攻击控制")]
    public int requiredHits = 5;  // 玩家攻击所需的总次数
    public float damagePerHitPercent = 0.2f;  // 每次攻击造成的伤害百分比，默认为最大生命值的20%

    private List<PlayerController> playersAttacking = new List<PlayerController>();  // 攻击过障碍物的玩家列表

    private void Start()
    {
        // 初始化当前生命值
        currentHealth = maxHealth;
    }

    // 受伤方法
    // 受伤方法
    public void TakeDamage(float damage, PlayerController player = null, bool isFromEnemy = false)
    {
        if (isDestroyed) return;

        float actualDamage;

        if (isFromEnemy)
        {
            // 来自怪物的伤害按传入的数值计算
            actualDamage = damage;
            Debug.Log($"障碍物受到怪物的攻击，伤害: {actualDamage}");
        }
        else
        {
            // 来自玩家的攻击，根据攻击次数来计算伤害
            actualDamage = maxHealth * damagePerHitPercent;
            Debug.Log($"障碍物受到玩家的攻击，固定伤害: {actualDamage}");
        }

        currentHealth -= actualDamage;

        Debug.Log($"障碍物当前生命值: {currentHealth}");

        // 如果是玩家攻击，记录攻击的玩家并增加攻击次数
        if (player != null && !playersAttacking.Contains(player))
        {
            playersAttacking.Add(player);
        }

        // 计数攻击次数，检查是否达到攻击次数要求
        if (!isFromEnemy)
        {
            attackCount++;
            if (attackCount >= requiredHits)
            {
                // 达到攻击次数要求后摧毁障碍物
                DestroyObstacle();
            }
        }

        if (currentHealth <= 0)
        {
            DestroyObstacle();
        }
    }


    // 障碍物被摧毁的处理
    private void DestroyObstacle()
    {
        isDestroyed = true;
        Debug.Log("障碍物已被摧毁");

        // 禁用所有攻击该障碍物的玩家的攻击功能
        foreach (var player in playersAttacking)
        {
            if (player != null)
            {
                player.isObstaclecanhit = false;  // 关闭玩家攻击障碍物的能力
            }
        }

        // 销毁障碍物
        Destroy(gameObject);
    }

    // 向障碍物添加伤害的方法
    public void ApplyDamageFromPlayer(PlayerController player)
    {
        if (player != null)
        {
            TakeDamage(player.TotalDamage, player); // 读取真实伤害
            Debug.Log($"玩家与障碍物交互，障碍物受到了 {player.TotalDamage} 点伤害");
        }
        else
        {
            Debug.LogError("PlayerController 为空，无法处理障碍物伤害");
        }
    }
}

