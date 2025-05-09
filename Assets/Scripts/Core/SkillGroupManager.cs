using System.Collections.Generic;
using UnityEngine;

public class SkillGroupManager : MonoBehaviour
{
    public static SkillGroupManager Instance { get; private set; }

    [Header("所有职业技能组")]
    public List<RoleUpgradeDetailUI.SkillGroup> allSkillGroups;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 可选：保留跨场景
    }

    /// <summary>
    /// 查找对应角色的 SkillGroup
    /// </summary>
    public RoleUpgradeDetailUI.SkillGroup GetSkillGroupFor(string unitName)
    {
        return allSkillGroups.Find(g => g.unitName == unitName);
    }

}
