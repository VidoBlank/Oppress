using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf_01 : Enemy
{
    [Header("�״ι������޸ĵĹ�����")]
    [Tooltip("��һ�ι������޸ĵĹ�����")]
    public int newAttackDamage = 15;

    private bool hasAttacked = false; // ��¼�Ƿ��Ѿ����й���һ�ι���

    /// <summary>
    /// ��д������������һ�ι����ɹ����޸Ĺ�����
    /// </summary>
    public override void Attacking(Unit unit)
    {
        base.Attacking(unit);

        // **����ǵ�һ�ι����ɹ����޸Ĺ�����**
        if (!hasAttacked)
        {
            hasAttacked = true;
            attackDamage = newAttackDamage;
            Debug.Log($"{gameObject.name} ��һ�ι����󣬹������޸�Ϊ {newAttackDamage}");
        }
    }
}
