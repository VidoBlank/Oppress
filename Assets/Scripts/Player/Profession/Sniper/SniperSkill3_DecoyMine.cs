using UnityEngine;

public class SniperSkill3_DecoyMine : IPlayerSkill
{
    public string SkillID => "sniper_skill_3";
    public string SkillName => "诱饵诡雷";

    public bool IsSustained => false;
    public bool IsActive => false;
    public bool IsInstantCast => true;

    private PlayerController player;
    private Sniper sniper;

    public void Init(PlayerController player)
    {
        this.player = player;
        this.sniper = player as Sniper;

        if (sniper == null)
            Debug.LogError("⚠️ SniperSkill3_DecoyMine 初始化失败：player 不是 Sniper！");
    }

    public float NeedEnergy => sniper != null ? sniper.decoyEnergyCost : 0f;
    public float Cooldown => sniper != null ? sniper.decoyCooldown : 0f;

    public void Prepare(PlayerController player) { }

    public void HandleMouseInput(PlayerController player) { }

    public void OnSkillStart(PlayerController player)
    {
        // TODO: 实现诡雷投放逻辑
        // 示例：GameObject.Instantiate(sniper.decoyMinePrefab, 位置, 旋转);
    }

    public void OnSkillEnd(PlayerController player) { }

    public void Cancel(PlayerController player) { }

    public void Toggle(PlayerController player) { }
}
