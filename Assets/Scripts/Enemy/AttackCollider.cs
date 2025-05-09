using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    public Enemy monster;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 如果碰到玩家，则判断 tag 是否为 "Player"
        if (collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent<Unit>(out Unit unit) &&
                collision.TryGetComponent<PlayerController>(out PlayerController player))
            {
                if (!player.isKnockedDown && !player.isDead)
                {
                    if (!monster.attackTargets.Contains(unit))
                    {
                        monster.attackTargets.Add(unit);
                    }
                }
            }
        }
        // 如果碰到障碍物，则判断 tag 是否为 "Obstacle"
        else if (collision.CompareTag("Obstacle"))
        {
            // 确保 obstacle 是 Obstacle 类型，而非 Interactable
            if (collision.TryGetComponent<Obstacle>(out Obstacle obstacle))
            {
                if (!monster.obstacleTargets.Contains(obstacle))
                {
                    monster.obstacleTargets.Add(obstacle);  // 添加 Obstacle 对象
                }
            }
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        // 如果玩家离开，移除玩家目标
        if (collision.CompareTag("Player") && collision.GetComponent<Unit>())
        {
            Unit unit = collision.GetComponent<Unit>();
            if (monster.attackTargets.Contains(unit)) // 确保只移除已存在的对象
            {
                monster.attackTargets.Remove(unit);
            }
        }
        // 如果障碍物离开，移除障碍物目标
        else if (collision.GetComponent<Obstacle>())
        {
            Obstacle obstacle = collision.GetComponent<Obstacle>();  // 确保是 Obstacle 类型
            if (monster.obstacleTargets.Contains(obstacle)) // 确保只移除已存在的对象
            {
                monster.obstacleTargets.Remove(obstacle);
            }
        }
    }

    private void Start()
    {
        if (transform.parent.GetComponent<Enemy>() != null)
        {
            monster = transform.parent.GetComponent<Enemy>();
        }
    }
    private void OnDrawGizmosSelected()
    {
        // 设置绘制颜色
        Gizmos.color = Color.yellow;

        // 尝试获取本对象的 Collider2D
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            // 如果是圆形碰撞体，则绘制圆形
            if (col is CircleCollider2D circle)
            {
                // 绘制的圆心需要考虑 offset（偏移量）
                Vector2 center = (Vector2)transform.position + circle.offset;
                Gizmos.DrawWireSphere(center, circle.radius);
            }
            // 如果是盒形碰撞体，则绘制盒子
            else if (col is BoxCollider2D box)
            {
                Vector2 center = (Vector2)transform.position + box.offset;
                Gizmos.DrawWireCube(center, box.size);
            }
            // 其他类型的 Collider2D 也可以根据需要进行绘制
        }
    }
}
