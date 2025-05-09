using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    public Enemy monster;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ���������ң����ж� tag �Ƿ�Ϊ "Player"
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
        // ��������ϰ�����ж� tag �Ƿ�Ϊ "Obstacle"
        else if (collision.CompareTag("Obstacle"))
        {
            // ȷ�� obstacle �� Obstacle ���ͣ����� Interactable
            if (collision.TryGetComponent<Obstacle>(out Obstacle obstacle))
            {
                if (!monster.obstacleTargets.Contains(obstacle))
                {
                    monster.obstacleTargets.Add(obstacle);  // ��� Obstacle ����
                }
            }
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        // �������뿪���Ƴ����Ŀ��
        if (collision.CompareTag("Player") && collision.GetComponent<Unit>())
        {
            Unit unit = collision.GetComponent<Unit>();
            if (monster.attackTargets.Contains(unit)) // ȷ��ֻ�Ƴ��Ѵ��ڵĶ���
            {
                monster.attackTargets.Remove(unit);
            }
        }
        // ����ϰ����뿪���Ƴ��ϰ���Ŀ��
        else if (collision.GetComponent<Obstacle>())
        {
            Obstacle obstacle = collision.GetComponent<Obstacle>();  // ȷ���� Obstacle ����
            if (monster.obstacleTargets.Contains(obstacle)) // ȷ��ֻ�Ƴ��Ѵ��ڵĶ���
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
        // ���û�����ɫ
        Gizmos.color = Color.yellow;

        // ���Ի�ȡ������� Collider2D
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            // �����Բ����ײ�壬�����Բ��
            if (col is CircleCollider2D circle)
            {
                // ���Ƶ�Բ����Ҫ���� offset��ƫ������
                Vector2 center = (Vector2)transform.position + circle.offset;
                Gizmos.DrawWireSphere(center, circle.radius);
            }
            // ����Ǻ�����ײ�壬����ƺ���
            else if (col is BoxCollider2D box)
            {
                Vector2 center = (Vector2)transform.position + box.offset;
                Gizmos.DrawWireCube(center, box.size);
            }
            // �������͵� Collider2D Ҳ���Ը�����Ҫ���л���
        }
    }
}
