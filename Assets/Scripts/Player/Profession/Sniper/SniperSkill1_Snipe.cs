using UnityEngine;

public class SniperSkill1_Snipe : IPlayerSkill
{
    public string SkillID => "sniper_skill_1";
    public string SkillName => "狙击";

    public bool IsSustained => false;
    public bool IsActive => false;
    public bool IsInstantCast => false;

    private PlayerController player;
    private Sniper sniper;

    public void Init(PlayerController player)
    {
        this.player = player;
        this.sniper = player as Sniper;

        if (sniper == null)
            Debug.LogError("⚠️ SniperSkill1_Snipe 初始化失败：player 不是 Sniper！");
    }

    public float NeedEnergy => sniper != null ? sniper.snipeEnergyCost : 0f;
    public float Cooldown => sniper != null ? sniper.snipeCooldown : 0f;

    public void Prepare(PlayerController player)
    {
        // TODO: 蓄力准备逻辑
    }

    public void HandleMouseInput(PlayerController player)
    {
        // TODO: 处理瞄准、释放输入
    }

    public void OnSkillStart(PlayerController player)
    {
        // TODO: 触发狙击逻辑
    }

    public void OnSkillEnd(PlayerController player) { }

    public void Cancel(PlayerController player) { }

    public void Toggle(PlayerController player) { }
}
