using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish_01 : Enemy
{
    [Header("�״ι������޸ĵĲ���")]
    [Tooltip("�״ι������ƶ��ٶ�")]
    public float afterAttackSpeed = 1.1f;

    private bool hasAttacked = false; // ��¼�Ƿ��Ѿ����й���һ�ι���

    /// <summary>
    /// ��д������������һ�ι����ɹ���ֱ���޸� Blend Tree speed �������ƶ��ٶ�
    /// </summary>
    public override void Attacking(Unit unit)
    {
        base.Attacking(unit);

        // **����ǵ�һ�ι����ɹ����޸Ĳ���**
        if (!hasAttacked)
        {
            hasAttacked = true;

           
            movingSpeed = afterAttackSpeed;

            
            animator.SetFloat("speed", 0.0f);

            Debug.Log($"{gameObject.name} ��һ�ι����ɹ����ƶ��ٶȱ�Ϊ {afterAttackSpeed}������ speed ��Ϊ 0");
        }
    }
}
