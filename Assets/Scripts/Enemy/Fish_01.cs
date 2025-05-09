using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish_01 : Enemy
{
    [Header("首次攻击后修改的参数")]
    [Tooltip("首次攻击后移动速度")]
    public float afterAttackSpeed = 1.1f;

    private bool hasAttacked = false; // 记录是否已经进行过第一次攻击

    /// <summary>
    /// 重写攻击方法，第一次攻击成功后，直接修改 Blend Tree speed 并降低移动速度
    /// </summary>
    public override void Attacking(Unit unit)
    {
        base.Attacking(unit);

        // **如果是第一次攻击成功，修改参数**
        if (!hasAttacked)
        {
            hasAttacked = true;

           
            movingSpeed = afterAttackSpeed;

            
            animator.SetFloat("speed", 0.0f);

            Debug.Log($"{gameObject.name} 第一次攻击成功！移动速度变为 {afterAttackSpeed}，动画 speed 设为 0");
        }
    }
}
