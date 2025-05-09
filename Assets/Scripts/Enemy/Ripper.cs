using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ripper : Enemy
{
    [Header("攻击调整参数")]
    [Tooltip("修改后的攻击力")]
    public int newAttackDamage = 25;

    [Tooltip("修改后的攻击间隔")]
    public float newAttackInterval = 1.5f;

    [Tooltip("触发攻击调整所需的攻击次数")]
    public int attackThreshold = 3;

    [Tooltip("speed = 0 的延迟时间")]
    public float speedChangeDelay = 0.5f; // 默认 0.5s 延迟

    private int attackCount = 0; // 记录攻击次数

    /// <summary>
    /// 重写攻击方法，前三次攻击后修改攻击力与攻击间隔，并在延迟后将速度设为0
    /// </summary>
    public override void Attacking(Unit unit)
    {
        base.Attacking(unit);

        attackCount++;

        if (attackCount >= attackThreshold)
        {
            // **立即修改攻击力与攻击间隔**
            attackDamage = newAttackDamage;
            attackInterval = newAttackInterval;
            Debug.Log($"{gameObject.name} 第 {attackCount} 次攻击后，攻击力修改为 {newAttackDamage}，攻击间隔修改为 {newAttackInterval}");

            // **延迟修改 speed = 0**
            StartCoroutine(DelayedSetSpeed());
        }
    }

    /// <summary>
    /// **延迟一定时间后将动画 Blend Tree `speed = 0`**
    /// </summary>
    private IEnumerator DelayedSetSpeed()
    {
        yield return new WaitForSeconds(speedChangeDelay);
        animator.SetFloat("speed", 0f);
        Debug.Log($"{gameObject.name} 在 {speedChangeDelay} 秒后将动画 speed 设为 0");
    }
}
