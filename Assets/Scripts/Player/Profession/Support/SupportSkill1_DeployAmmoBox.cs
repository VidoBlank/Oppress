using UnityEngine;

public class SupportSkill1_DeployAmmoBox : IPlayerSkill
{
    public string SkillID => "support_skill_1";
    public string SkillName => "部署弹药箱";

    public bool IsSustained => false;
    public bool IsActive => false;
    public bool IsInstantCast => true;

    private Support support;

    public void Init(PlayerController player)
    {
        support = player as Support;
        if (support == null)
            Debug.LogError("⚠️ SupportSkill1_DeployAmmoBox 初始化失败！");
    }

    public float NeedEnergy => support.ammoBoxEnergyCost;
    public float Cooldown => support.ammoBoxCooldown;

    public void Prepare(PlayerController player) { }

    public void HandleMouseInput(PlayerController player) { }

    public void OnSkillStart(PlayerController player)
    {
        Debug.Log("部署弹药箱！");
        GameObject box = GameObject.Instantiate(support.ammoBoxPrefab, support.deployPoint.position, Quaternion.identity);
        GameObject.Destroy(box, support.ammoBoxDuration);
        player.EndSkill();
    }

    public void OnSkillEnd(PlayerController player) { }

    public void Cancel(PlayerController player) { }

    public void Toggle(PlayerController player) { }
}
