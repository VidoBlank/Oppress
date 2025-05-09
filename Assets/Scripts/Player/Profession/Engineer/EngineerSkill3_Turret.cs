using UnityEngine;

public class EngineerSkill3_Turret : IPlayerSkill
{
    public string SkillID => "engineer_skill_3";
    public string SkillName => "工程炮台";

    public bool IsSustained => false;
    public bool IsActive => false;
    public bool IsInstantCast => false;

    private PlayerController player;
    private Engineer engineer;

    public void Init(PlayerController player)
    {
        this.player = player;
        this.engineer = player as Engineer;

        if (engineer == null)
            Debug.LogError("⚠️ EngineerSkill3_Turret 初始化失败：player 不是 Engineer！");
    }

    public float NeedEnergy => engineer != null ? engineer.turretEnergyCost : 0f;
    public float Cooldown => engineer != null ? engineer.turretCooldown : 0f;

    public void Prepare(PlayerController player)
    {
        // TODO: 进入放置准备状态，例如显示炮台预览
        Debug.Log("进入炮台布置准备状态");
    }

    public void HandleMouseInput(PlayerController player)
    {
        // TODO: 根据鼠标位置放置炮台
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
            Camera.main.WorldToScreenPoint(player.transform.position).z));

        GameObject turret = GameObject.Instantiate(engineer.turretPrefab, mouseWorldPos, Quaternion.identity);
        // TODO: 初始化炮台脚本，如设置持续时间等

        player.EndSkill();
    }

    public void OnSkillStart(PlayerController player)
    {
        // 此处可选：准备状态结束时进入执行
    }

    public void OnSkillEnd(PlayerController player) { }

    public void Cancel(PlayerController player)
    {
        Debug.Log("取消炮台布置");
    }

    public void Toggle(PlayerController player) { }
}
