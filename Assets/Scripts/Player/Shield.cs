using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [Header("护盾设置")]
    [SerializeField] private float HP = 100f; // 生命值
    [SerializeField] public float duration = 15f; // 持续时间

    [Header("护盾状态")]
    [SerializeField] private float remainingTime; // 剩余时间
    [SerializeField] private bool isActive = true;
    public float CurrentHP
    {
        get { return HP; }
    }
    private float elapsedTime = 0f;

    // 初始化护盾的生命值和持续时间
    public void Init(float health, float shieldDuration)
    {
        HP = health;
        duration = shieldDuration;
        elapsedTime = 0f; // 重置计时
        isActive = true; // 护盾重新激活
        UpdateRemainingTime(); // 更新剩余时间显示
    }

    public void GetDamage(float damage)
    {
        if (!isActive) return; // 如果护盾已销毁，不处理伤害
        HP -= damage;
        if (HP <= 0)
        {
            DestroySelf();
        }
    }

    public void RestoreHealth(float health, float extraDuration = 0f)
    {
        if (!isActive) return; 
        HP = health; 
        duration += extraDuration; 
        elapsedTime = 0f; 
        UpdateRemainingTime(); // 更新剩余时间显示
    }

    public void DestroySelf()
    {
        if (!isActive) return; // 防止多次销毁
        isActive = false;
        Destroy(gameObject);
    }

    void Update()
    {
        if (!isActive) return; // 如果护盾已销毁，停止计时
        elapsedTime += Time.deltaTime;

        UpdateRemainingTime(); // 实时更新剩余时间

        if (elapsedTime >= duration)
        {
            DestroySelf();
        }
    }

    private void UpdateRemainingTime()
    {
        remainingTime = Mathf.Max(duration - elapsedTime, 0f); // 确保剩余时间不为负
    }
}
