using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slinger : Enemy
{
    [Header("Ͷ��������")]
    [Tooltip("�ӵ�Ԥ���壬������� SlingerProjectile ���")]
    public GameObject projectilePrefab;

    [Tooltip("����㣨muzzle�����ӵ������λ�ã�ͨ��Ϊ����������ǹ�ڻ��ֲ�λ��")]
    public Transform muzzle;

    [Tooltip("�ӵ�����ʱ�䣨Ӱ�������߹켣�������Ը���Ŀ��������")]
    public float projectileFlightTime = 1.0f;

    [Tooltip("�ӵ����к�ը��Χ�뾶")]
    public float projectileExplosionRadius = 2.0f;

    [Tooltip("���ͬʱ���ڵ��ӵ�����")]
    public int maxSimultaneousProjectiles = 1;

    // ��ǰ�ѷ�����δ���ٵ��ӵ�����
    private int currentProjectileCount = 0;

    /// <summary>
    /// ��д��������������Ŀ��������Σ����ӷ���㷢��һ���ӵ���ǰ���ǵ�ǰ�ӵ�����δ�ﵽ���ޣ�
    /// </summary>
    /// <param name="targetUnit">Ŀ�굥λ</param>
    public override void Attacking(Unit targetUnit)
    {
        // �ж�Ŀ���Ƿ���Ч
        if (targetUnit == null)
            return;

        // ����Ŀ��λ�õ������˵ĳ���
        if (targetUnit.transform.position.x < transform.position.x)
        {
            // Ŀ���ڵ������
            ChangeDirecition(Direction.Left);
            transform.localScale = new Vector3(-1f, 1f, 1f);  // ��ת���򣨼���Ĭ�ϳ��ң�
        }
        else
        {
            // Ŀ���ڵ����Ҳ�
            ChangeDirecition(Direction.Right);
            transform.localScale = new Vector3(1f, 1f, 1f);
        }

        // �����ǰ�ӵ������ﵽ���ƣ��򲻷����µ��ӵ�
        if (currentProjectileCount >= maxSimultaneousProjectiles)
        {
            return;
        }

        if (projectilePrefab != null)
        {
            // ��������� muzzle�����ӵ��� muzzle �����䣬����ʹ�õ��˵�λ��
            Vector3 spawnPos = (muzzle != null) ? muzzle.position : transform.position;
            Quaternion spawnRot = (muzzle != null) ? muzzle.rotation : Quaternion.identity;

            // ʵ�����ӵ�
            GameObject projectileInstance = Instantiate(projectilePrefab, spawnPos, spawnRot);

            // ��ȡ SlingerProjectile ���
            SlingerProjectile projectile = projectileInstance.GetComponent<SlingerProjectile>();
            if (projectile != null)
            {
                // ��Ŀ��λ�á������˺�������ʱ�䡢��ը��Χ�Լ������������ݸ��ӵ�
                projectile.Initialize(targetUnit.transform.position, attackDamage, projectileFlightTime, projectileExplosionRadius, this);
            }
            else
            {
                Debug.LogWarning("ʵ�������ӵ���δ�ҵ� SlingerProjectile �����");
            }

            // ���Ź��������������õ�ǰ״̬Ϊ����
            animator.SetTrigger("isAttacking");
            state = EnemyState.Attack;
            attackLocked = true;

            // ���ӵ�ǰ������ӵ�����
            currentProjectileCount++;
        }
        else
        {
            Debug.LogWarning("Slinger δ���� projectilePrefab��");
        }
    }


    /// <summary>
    /// �� SlingerProjectile ���ã����ӵ�����ʱ���ٵ�ǰ�ӵ�����
    /// </summary>
    public void ProjectileDestroyed()
    {
        currentProjectileCount--;
        if (currentProjectileCount < 0)
            currentProjectileCount = 0;
    }
}
