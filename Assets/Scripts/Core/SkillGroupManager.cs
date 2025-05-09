using System.Collections.Generic;
using UnityEngine;

public class SkillGroupManager : MonoBehaviour
{
    public static SkillGroupManager Instance { get; private set; }

    [Header("����ְҵ������")]
    public List<RoleUpgradeDetailUI.SkillGroup> allSkillGroups;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // ��ѡ�������糡��
    }

    /// <summary>
    /// ���Ҷ�Ӧ��ɫ�� SkillGroup
    /// </summary>
    public RoleUpgradeDetailUI.SkillGroup GetSkillGroupFor(string unitName)
    {
        return allSkillGroups.Find(g => g.unitName == unitName);
    }

}
