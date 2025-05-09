using UnityEngine;

public class MedicSkill1_HealGun : IPlayerSkill
{
    public string SkillID => "medic_skill_1";
    public string SkillName => "治疗枪";

    public bool IsSustained => false;
    public bool IsActive => false;
    public bool IsInstantCast => false;

    private Medic medic;

    public void Init(PlayerController player)
    {
        medic = player as Medic;
        if (medic == null)
            Debug.LogError("⚠️ MedicSkill1_HealGun 初始化失败！");
    }

    public float NeedEnergy => 0f;  // 启动不消耗，持续期间每秒扣能量
    public float Cooldown => medic.healCooldown;

    public void Prepare(PlayerController player)
    {
        Debug.Log("准备发射治疗射线...");
        player.skillRange.SetActive(true);
    }

    public void HandleMouseInput(PlayerController player)
    {
        if (Input.GetMouseButtonDown(1))
        {
            // TODO：检测目标友军并开始治疗
            OnSkillStart(player);
        }

        if (Input.GetMouseButtonDown(0))
        {
            Cancel(player);
        }
    }

    public void OnSkillStart(PlayerController player)
    {
        Debug.Log("开始治疗友军！");
        // TODO：开启治疗协程，持续治疗选中目标
        player.skillRange.SetActive(false);
        player.EndSkill();
    }

    public void OnSkillEnd(PlayerController player) { }

    public void Cancel(PlayerController player)
    {
        Debug.Log("取消治疗枪技能");
        player.skillRange.SetActive(false);
        player.isPreparingSkill = false;
    }

    public void Toggle(PlayerController player) { }
}
