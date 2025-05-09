using UnityEngine;

public class EngineerSkill2_Grapeshot : IPlayerSkill
{
    public string SkillID => "engineer_skill_2";
    public string SkillName => "霰弹枪模式";

    public bool IsSustained => true;     // 持续型技能
    public bool IsActive { get; private set; } = false;
    public bool IsInstantCast => false;

    private PlayerController player;
    private Engineer engineer;

    public void Init(PlayerController player)
    {
        this.player = player;
        this.engineer = player as Engineer;

        if (engineer == null)
            Debug.LogError("⚠️ EngineerSkill2_Grapeshot 初始化失败：player 不是 Engineer！");
    }

    public float NeedEnergy => engineer != null ? engineer.grapeshotEnergyCost : 0f;
    public float Cooldown => engineer != null ? engineer.grapeshotCooldown : 0f;

    public void Prepare(PlayerController player)
    {
        // 持续技能无需准备
    }

    public void HandleMouseInput(PlayerController player)
    {
        // 持续技能的鼠标处理（如特殊攻击模式）
    }

    public void OnSkillStart(PlayerController player)
    {
        IsActive = true;
        Debug.Log("进入霰弹枪模式");

        // TODO：切换武器模型、修改攻击逻辑为霰弹枪模式
        // 例如：player.SwitchToGrapeshotMode();
    }

    public void OnSkillEnd(PlayerController player)
    {
        IsActive = false;
        Debug.Log("退出霰弹枪模式");

        // TODO：恢复普通攻击模式
        // 例如：player.SwitchToNormalMode();
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
}
