using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf_01 : Enemy
{
    [Header("首次攻击后修改的攻击力")]
    [Tooltip("第一次攻击后修改的攻击力")]
    public int newAttackDamage = 15;

    private bool hasAttacked = false; // 记录是否已经进行过第一次攻击

    /// <summary>
    /// 重写攻击方法，第一次攻击成功后，修改攻击力
    /// </summary>
    public override void Attacking(Unit unit)
    {
        base.Attacking(unit);

        // **如果是第一次攻击成功，修改攻击力**
        if (!hasAttacked)
        {
            hasAttacked = true;
            attackDamage = newAttackDamage;
            Debug.Log($"{gameObject.name} 第一次攻击后，攻击力修改为 {newAttackDamage}");
        }
    }
}
