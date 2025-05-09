using UnityEngine;

public class MedicSkill2_DeployDrone : IPlayerSkill
{
    public string SkillID => "medic_skill_2";
    public string SkillName => "医疗无人机";

    public bool IsSustained => false;
    public bool IsActive => false;
    public bool IsInstantCast => true;

    private Medic medic;

    public void Init(PlayerController player)
    {
        medic = player as Medic;
        if (medic == null)
            Debug.LogError("⚠️ MedicSkill2_DeployDrone 初始化失败！");
    }

    public float NeedEnergy => medic.droneEnergyCost;
    public float Cooldown => medic.droneCooldown;

    public void Prepare(PlayerController player) { }

    public void HandleMouseInput(PlayerController player) { }

    public void OnSkillStart(PlayerController player)
    {
        Debug.Log("部署医疗无人机！");
        GameObject drone = GameObject.Instantiate(medic.dronePrefab, player.transform.position, Quaternion.identity);
        // TODO：初始化无人机参数
        GameObject.Destroy(drone, medic.droneDuration);
        player.EndSkill();
    }

    public void OnSkillEnd(PlayerController player) { }

    public void Cancel(PlayerController player) { }

    public void Toggle(PlayerController player) { }
}
