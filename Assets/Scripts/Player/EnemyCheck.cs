using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCheck : MonoBehaviour
{
    private PlayerController playerController;
    private CircleCollider2D attackRange;  // 攻击范围碰撞体
    private float cachedRadius;

    private int frameCount = 0;
    private const int checkInterval = 5;   // 每5帧检测一次

    private void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
        attackRange = playerController.attribute.attackRange;

        // ✅ 缓存实际攻击半径（考虑缩放）
        cachedRadius = attackRange.radius * Mathf.Abs(attackRange.transform.lossyScale.x);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (!playerController.enemys.Contains(collision.gameObject))
            {
                playerController.enemys.Add(collision.gameObject);
                Debug.Log($"敌人 {collision.gameObject.name} 进入攻击范围");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (playerController.enemys.Contains(collision.gameObject))
            {
                playerController.enemys.Remove(collision.gameObject);
                Debug.Log($"敌人 {collision.gameObject.name} 离开攻击范围");
            }
        }
    }



    private bool IsColliderWithinAttackRange(Collider2D targetCollider)
    {
        if (playerController == null || targetCollider == null) return false;

        float distance = Vector2.Distance(
            playerController.transform.position,
            targetCollider.bounds.center);

        return distance <= playerController.TotalAttackRange + 0.5f;
    }

}
