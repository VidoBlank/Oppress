using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ripper : Enemy
{
    [Header("������������")]
    [Tooltip("�޸ĺ�Ĺ�����")]
    public int newAttackDamage = 25;

    [Tooltip("�޸ĺ�Ĺ������")]
    public float newAttackInterval = 1.5f;

    [Tooltip("����������������Ĺ�������")]
    public int attackThreshold = 3;

    [Tooltip("speed = 0 ���ӳ�ʱ��")]
    public float speedChangeDelay = 0.5f; // Ĭ�� 0.5s �ӳ�

    private int attackCount = 0; // ��¼��������

    /// <summary>
    /// ��д����������ǰ���ι������޸Ĺ������빥������������ӳٺ��ٶ���Ϊ0
    /// </summary>
    public override void Attacking(Unit unit)
    {
        base.Attacking(unit);

        attackCount++;

        if (attackCount >= attackThreshold)
        {
            // **�����޸Ĺ������빥�����**
            attackDamage = newAttackDamage;
            attackInterval = newAttackInterval;
            Debug.Log($"{gameObject.name} �� {attackCount} �ι����󣬹������޸�Ϊ {newAttackDamage}����������޸�Ϊ {newAttackInterval}");

            // **�ӳ��޸� speed = 0**
            StartCoroutine(DelayedSetSpeed());
        }
    }

    /// <summary>
    /// **�ӳ�һ��ʱ��󽫶��� Blend Tree `speed = 0`**
    /// </summary>
    private IEnumerator DelayedSetSpeed()
    {
        yield return new WaitForSeconds(speedChangeDelay);
        animator.SetFloat("speed", 0f);
        Debug.Log($"{gameObject.name} �� {speedChangeDelay} ��󽫶��� speed ��Ϊ 0");
    }
}
