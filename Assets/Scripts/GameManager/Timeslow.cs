using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timeslow : MonoBehaviour
{
    public PlayerManager playerManager; // 引用 PlayerManager 组件

    [Header("子弹时间设置")]
    public bool isBulletTimeEnabled = true; // 是否启用子弹时间功能
    public float bulletTimeSpeed = 0.1f; // 子弹时间的速度（缩放时间）

    public bool isBulletTimeActive = false; // 标记子弹时间是否开启

    private void Start()
    {
        if (playerManager == null)
        {
            playerManager = FindObjectOfType<PlayerManager>();
        }
    }

    private void Update()
    {
        if (isBulletTimeEnabled) // 仅在子弹时间功能启用时，才检查
        {
            CheckForBulletTime();
        }
    }

    // 检测是否有玩家处于准备技能状态
    private void CheckForBulletTime()
    {
        bool anyPlayerPreparingSkill = false;

        // 遍历所有玩家，检查是否有玩家在准备技能状态
        foreach (var player in playerManager.players)
        {
            if (player.isPreparingSkill)
            {
                anyPlayerPreparingSkill = true;
                break;
            }
        }

        // 如果有玩家处于准备技能状态且当前没有激活子弹时间，启动子弹时间
        if (anyPlayerPreparingSkill && !isBulletTimeActive)
        {
            ActivateBulletTime();
        }
        // 如果没有玩家处于准备技能状态且当前处于子弹时间，取消子弹时间
        else if (!anyPlayerPreparingSkill && isBulletTimeActive)
        {
            DeactivateBulletTime();
        }
    }

    // 激活子弹时间
    private void ActivateBulletTime()
    {
        isBulletTimeActive = true;
        Time.timeScale = bulletTimeSpeed; // 设置时间缩放为子弹时间的速度
        Debug.Log("子弹时间已启动");
    }

    // 取消子弹时间
    private void DeactivateBulletTime()
    {
        isBulletTimeActive = false;
        Time.timeScale = 1f; // 恢复正常时间流逝
        Debug.Log("子弹时间已取消");
    }
}
