using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [Header("��������")]
    [SerializeField] private float HP = 100f; // ����ֵ
    [SerializeField] public float duration = 15f; // ����ʱ��

    [Header("����״̬")]
    [SerializeField] private float remainingTime; // ʣ��ʱ��
    [SerializeField] private bool isActive = true;
    public float CurrentHP
    {
        get { return HP; }
    }
    private float elapsedTime = 0f;

    // ��ʼ�����ܵ�����ֵ�ͳ���ʱ��
    public void Init(float health, float shieldDuration)
    {
        HP = health;
        duration = shieldDuration;
        elapsedTime = 0f; // ���ü�ʱ
        isActive = true; // �������¼���
        UpdateRemainingTime(); // ����ʣ��ʱ����ʾ
    }

    public void GetDamage(float damage)
    {
        if (!isActive) return; // ������������٣��������˺�
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
        UpdateRemainingTime(); // ����ʣ��ʱ����ʾ
    }

    public void DestroySelf()
    {
        if (!isActive) return; // ��ֹ�������
        isActive = false;
        Destroy(gameObject);
    }

    void Update()
    {
        if (!isActive) return; // ������������٣�ֹͣ��ʱ
        elapsedTime += Time.deltaTime;

        UpdateRemainingTime(); // ʵʱ����ʣ��ʱ��

        if (elapsedTime >= duration)
        {
            DestroySelf();
        }
    }

    private void UpdateRemainingTime()
    {
        remainingTime = Mathf.Max(duration - elapsedTime, 0f); // ȷ��ʣ��ʱ�䲻Ϊ��
    }
}
