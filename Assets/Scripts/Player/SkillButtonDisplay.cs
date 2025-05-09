using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillButtonDisplay : MonoBehaviour
{
    [Header("UI组件")]
    public Image skillIcon;
    public TextMeshProUGUI skillNameText;

    private PlayerController pc;
    private RoleUpgradeDetailUI.SkillGroup skillGroup;

    public void Init(PlayerController player)
    {
        pc = player;
        skillGroup = SkillGroupManager.Instance.GetSkillGroupFor(pc.symbol.unitName);

        pc.OnSkillEquipped -= UpdateSkillDisplay;
        pc.OnSkillEquipped += UpdateSkillDisplay;

        UpdateSkillDisplay(pc.equippedSkill);
        // 🔥 同步冷却UI
        if (pc.skillCooldownManager != null)
            pc.skillCooldownManager.SyncCooldown();
    }


    public void UpdateSkillDisplay(IPlayerSkill skill)
    {
        if (skill == null || skillGroup == null) return;

        RoleUpgradeDetailUI.SkillInfo info = null;

        if (skill.SkillID == skillGroup.skill1?.id) info = skillGroup.skill1;
        else if (skill.SkillID == skillGroup.skill2?.id) info = skillGroup.skill2;
        else if (skill.SkillID == skillGroup.skill3?.id) info = skillGroup.skill3;

        if (info != null)
        {
            skillIcon.sprite = info.icon;               // ✅ 图标稳定来源
            skillNameText.text = info.name;             // ✅ 名称来源
        }
        else
        {
            Debug.LogWarning($"⚠️ 找不到技能ID匹配的图标和文本：{skill.SkillID}");
        }
    }

}
