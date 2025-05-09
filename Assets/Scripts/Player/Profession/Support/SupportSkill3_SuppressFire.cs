using UnityEngine;
using System.Collections;

public class SupportSkill3_SuppressFire : IPlayerSkill
{
    public string SkillID => "support_skill_3";
    public string SkillName => "火力压制";

    public bool IsSustained => true;
    public bool IsActive { get; private set; } = false;
    public bool IsInstantCast => false;

    private Support support;
    private Coroutine suppressCoroutine;

    public void Init(PlayerController player)
    {
        support = player as Support;
        if (support == null)
            Debug.LogError("⚠️ SupportSkill3_SuppressFire 初始化失败！");
    }

    public float NeedEnergy => 0f;  // 启动不消耗，持续期间按秒扣能量
    public float Cooldown => support.suppressCooldown;

    public void Prepare(PlayerController player) { }

    public void HandleMouseInput(PlayerController player)
    {
        // 持续模式下鼠标控制开火方向
    }

    public void OnSkillStart(PlayerController player)
    {
        if (IsActive) return;

        IsActive = true;
        Debug.Log("进入火力压制模式！");
        player.isMoving = false;  // 禁止移动

        suppressCoroutine = player.StartCoroutine(SuppressFireRoutine(player));
    }

    public void OnSkillEnd(PlayerController player)
    {
        if (!IsActive) return;

        IsActive = false;
        Debug.Log("退出火力压制模式！");
        player.isMoving = true;

        if (suppressCoroutine != null)
            player.StopCoroutine(suppressCoroutine);
    }

    public void Cancel(PlayerController player)
    {
        OnSkillEnd(player);
    }

    public void Toggle(PlayerController player)
    {
        if (IsActive)
            OnSkillEnd(player);
        else
            OnSkillStart(player);
    }

    private IEnumerator SuppressFireRoutine(PlayerController player)
    {
        while (IsActive)
        {
            if (player.energy < support.suppressEnergyCostPerSec)
            {
                Debug.Log("能量不足，自动退出火力压制！");
                OnSkillEnd(player);
                yield break;
            }

            player.energy -= support.suppressEnergyCostPerSec;
            Debug.Log("机枪扫射中...");

            // TODO：实现机枪射击逻辑
            yield return new WaitForSeconds(support.suppressFireRate);
        }
    }
}
