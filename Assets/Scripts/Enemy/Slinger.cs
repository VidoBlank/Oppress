using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slinger : Enemy
{
    [Header("投射物设置")]
    [Tooltip("子弹预制体，必须包含 SlingerProjectile 组件")]
    public GameObject projectilePrefab;

    [Tooltip("发射点（muzzle）：子弹发射的位置，通常为敌人武器的枪口或手部位置")]
    public Transform muzzle;

    [Tooltip("子弹飞行时间（影响抛物线轨迹），可以根据目标距离调整")]
    public float projectileFlightTime = 1.0f;

    [Tooltip("子弹命中后爆炸范围半径")]
    public float projectileExplosionRadius = 2.0f;

    [Tooltip("最大同时存在的子弹数量")]
    public int maxSimultaneousProjectiles = 1;

    // 当前已发射且未销毁的子弹数量
    private int currentProjectileCount = 0;

    /// <summary>
    /// 重写攻击方法：无论目标类型如何，都从发射点发射一个子弹（前提是当前子弹数量未达到上限）
    /// </summary>
    /// <param name="targetUnit">目标单位</param>
    public override void Attacking(Unit targetUnit)
    {
        // 判断目标是否有效
        if (targetUnit == null)
            return;

        // 根据目标位置调整敌人的朝向
        if (targetUnit.transform.position.x < transform.position.x)
        {
            // 目标在敌人左侧
            ChangeDirecition(Direction.Left);
            transform.localScale = new Vector3(-1f, 1f, 1f);  // 翻转朝向（假设默认朝右）
        }
        else
        {
            // 目标在敌人右侧
            ChangeDirecition(Direction.Right);
            transform.localScale = new Vector3(1f, 1f, 1f);
        }

        // 如果当前子弹数量达到限制，则不发射新的子弹
        if (currentProjectileCount >= maxSimultaneousProjectiles)
        {
            return;
        }

        if (projectilePrefab != null)
        {
            // 如果设置了 muzzle，则子弹从 muzzle 处发射，否则使用敌人的位置
            Vector3 spawnPos = (muzzle != null) ? muzzle.position : transform.position;
            Quaternion spawnRot = (muzzle != null) ? muzzle.rotation : Quaternion.identity;

            // 实例化子弹
            GameObject projectileInstance = Instantiate(projectilePrefab, spawnPos, spawnRot);

            // 获取 SlingerProjectile 组件
            SlingerProjectile projectile = projectileInstance.GetComponent<SlingerProjectile>();
            if (projectile != null)
            {
                // 将目标位置、攻击伤害、飞行时间、爆炸范围以及发射者自身传递给子弹
                projectile.Initialize(targetUnit.transform.position, attackDamage, projectileFlightTime, projectileExplosionRadius, this);
            }
            else
            {
                Debug.LogWarning("实例化的子弹上未找到 SlingerProjectile 组件！");
            }

            // 播放攻击动画，并设置当前状态为攻击
            animator.SetTrigger("isAttacking");
            state = EnemyState.Attack;
            attackLocked = true;

            // 增加当前发射的子弹数量
            currentProjectileCount++;
        }
        else
        {
            Debug.LogWarning("Slinger 未设置 projectilePrefab！");
        }
    }


    /// <summary>
    /// 供 SlingerProjectile 调用，当子弹销毁时减少当前子弹计数
    /// </summary>
    public void ProjectileDestroyed()
    {
        currentProjectileCount--;
        if (currentProjectileCount < 0)
            currentProjectileCount = 0;
    }
}
