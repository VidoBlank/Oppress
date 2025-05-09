using UnityEngine;

public class MedicSkill3_HealingField : IPlayerSkill
{
    public string SkillID => "medic_skill_3";
    public string SkillName => "急救力场";

    public bool IsSustained => false;
    public bool IsActive => false;
    public bool IsInstantCast => true;

    private Medic medic;

    public void Init(PlayerController player)
    {
        medic = player as Medic;
        if (medic == null)
            Debug.LogError("⚠️ MedicSkill3_HealingField 初始化失败！");
    }

    public float NeedEnergy => medic.fieldEnergyCost;
    public float Cooldown => medic.fieldCooldown;

    public void Prepare(PlayerController player) { }

    public void HandleMouseInput(PlayerController player) { }

    public void OnSkillStart(PlayerController player)
    {
        Debug.Log("释放急救力场！");
        GameObject field = GameObject.Instantiate(medic.healingFieldPrefab, player.transform.position, Quaternion.identity);
        // TODO：初始化力场治疗与减伤逻辑
        GameObject.Destroy(field, medic.fieldDuration);
        player.EndSkill();
    }

    public void OnSkillEnd(PlayerController player) { }

    public void Cancel(PlayerController player) { }

    public void Toggle(PlayerController player) { }
}
