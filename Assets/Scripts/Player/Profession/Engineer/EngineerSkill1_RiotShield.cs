using UnityEngine;

public class EngineerSkill1_RiotShield : IPlayerSkill
{
    public string SkillID => "engineer_skill_1";
    public string SkillName => "防爆护盾";

    public bool IsSustained => false;
    public bool IsActive => false;
    public bool IsInstantCast => true;

    private PlayerController player;
    private Engineer engineer;

    public void Init(PlayerController player)
    {
        this.player = player;
        this.engineer = player as Engineer;

        if (engineer == null)
            Debug.LogError("⚠️ EngineerSkill1_RiotShield 初始化失败：player 不是 Engineer！");
    }

    public float NeedEnergy => 0f;  // 暂不消耗能量
    public float Cooldown => 5f;    // 假设冷却5秒，可调整

    public void Prepare(PlayerController player) { }

    public void HandleMouseInput(PlayerController player) { }

    public void OnSkillStart(PlayerController player)
    {
        Debug.Log("使用防爆护盾技能");

        Shield existingShield = engineer.GetComponentInChildren<Shield>();
        if (existingShield)
        {
            existingShield.RestoreHealth(engineer.shieldHealth);
            player.EndSkill();
            return;
        }

        GameObject shieldInstance = GameObject.Instantiate(engineer.shieldPrefab, engineer.shieldPosition);
        Shield shieldScript = shieldInstance.GetComponent<Shield>();
        shieldScript.Init(engineer.shieldHealth, engineer.shieldDuration);

        engineer.animator.SetTrigger("skill1start");
        player.EndSkill();
    }

    public void OnSkillEnd(PlayerController player) { }

    public void Cancel(PlayerController player) { }

    public void Toggle(PlayerController player) { }
}
