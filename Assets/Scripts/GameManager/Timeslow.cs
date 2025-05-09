using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timeslow : MonoBehaviour
{
    public PlayerManager playerManager; // ���� PlayerManager ���

    [Header("�ӵ�ʱ������")]
    public bool isBulletTimeEnabled = true; // �Ƿ������ӵ�ʱ�书��
    public float bulletTimeSpeed = 0.1f; // �ӵ�ʱ����ٶȣ�����ʱ�䣩

    public bool isBulletTimeActive = false; // ����ӵ�ʱ���Ƿ���

    private void Start()
    {
        if (playerManager == null)
        {
            playerManager = FindObjectOfType<PlayerManager>();
        }
    }

    private void Update()
    {
        if (isBulletTimeEnabled) // �����ӵ�ʱ�书������ʱ���ż��
        {
            CheckForBulletTime();
        }
    }

    // ����Ƿ�����Ҵ���׼������״̬
    private void CheckForBulletTime()
    {
        bool anyPlayerPreparingSkill = false;

        // ����������ң�����Ƿ��������׼������״̬
        foreach (var player in playerManager.players)
        {
            if (player.isPreparingSkill)
            {
                anyPlayerPreparingSkill = true;
                break;
            }
        }

        // �������Ҵ���׼������״̬�ҵ�ǰû�м����ӵ�ʱ�䣬�����ӵ�ʱ��
        if (anyPlayerPreparingSkill && !isBulletTimeActive)
        {
            ActivateBulletTime();
        }
        // ���û����Ҵ���׼������״̬�ҵ�ǰ�����ӵ�ʱ�䣬ȡ���ӵ�ʱ��
        else if (!anyPlayerPreparingSkill && isBulletTimeActive)
        {
            DeactivateBulletTime();
        }
    }

    // �����ӵ�ʱ��
    private void ActivateBulletTime()
    {
        isBulletTimeActive = true;
        Time.timeScale = bulletTimeSpeed; // ����ʱ������Ϊ�ӵ�ʱ����ٶ�
        Debug.Log("�ӵ�ʱ��������");
    }

    // ȡ���ӵ�ʱ��
    private void DeactivateBulletTime()
    {
        isBulletTimeActive = false;
        Time.timeScale = 1f; // �ָ�����ʱ������
        Debug.Log("�ӵ�ʱ����ȡ��");
    }
}
