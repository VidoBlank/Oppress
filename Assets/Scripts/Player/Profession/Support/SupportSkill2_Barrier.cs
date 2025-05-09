using UnityEngine;

public class SupportSkill2_Barrier : IPlayerSkill
{
    public string SkillID => "support_skill_2";
    public string SkillName => "强化掩体";

    public bool IsSustained => false;
    public bool IsActive => false;
    public bool IsInstantCast => false;

    private Support support;

    public void Init(PlayerController player)
    {
        support = player as Support;
        if (support == null)
            Debug.LogError("⚠️ SupportSkill2_Barrier 初始化失败！");
    }

    public float NeedEnergy => support.barrierEnergyCost;
    public float Cooldown => support.barrierCooldown;

    public void Prepare(PlayerController player)
    {
        Debug.Log("准备放置掩体...");
        player.skillRange.SetActive(true);
    }

    public void HandleMouseInput(PlayerController player)
    {
        if (Input.GetMouseButtonDown(1))  // 右键释放
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float distance = Vector3.Distance(player.transform.position, mouseWorldPos);
            if (distance <= support.barrierDeployRange)
            {
                OnSkillStart(player);
            }
            else
            {
                Debug.Log("放置距离过远，取消");
                Cancel(player);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Cancel(player);
        }
    }

    public void OnSkillStart(PlayerController player)
    {
        Debug.Log("放置掩体！");
        GameObject barrier = GameObject.Instantiate(support.barrierPrefab, support.deployPoint.position, Quaternion.identity);
        GameObject.Destroy(barrier, support.barrierDuration);
        player.skillRange.SetActive(false);
        player.EndSkill();
    }

    public void OnSkillEnd(PlayerController player) { }

    public void Cancel(PlayerController player)
    {
        Debug.Log("取消掩体部署");
        player.skillRange.SetActive(false);
        player.isPreparingSkill = false;
    }

    public void Toggle(PlayerController player) { }
}
