using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;

public class RoleUpgradeDetailUI : MonoBehaviour
{
    [Header("UI 组件")]
    public Image avatarImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    [Header("技能选中背景图")]
    public Image skill1Highlight;
    public Image skill2Highlight;
    public Image skill3Highlight;

    [Header("角色统计显示区域")]
    public TextMeshProUGUI statsText;

    [Header("技能UI")]
    public GameObject skill1Obj;
    public GameObject skill2Obj;
    public GameObject skill3Obj;
    public TextMeshProUGUI skill1NameText;
    public TextMeshProUGUI skill2NameText;
    public TextMeshProUGUI skill3NameText;
    public Image skill1Icon;
    public Image skill2Icon;
    public Image skill3Icon;
    public TextMeshProUGUI skillDescText;
    public RectTransform skillDescBG;
    public AudioClip hoverSfx;
    public AudioSource audioSource;

  

    private PlayerController pc;

    private void Start()
    {
        skillDescText.gameObject.SetActive(false);
        skillDescBG.gameObject.SetActive(false);
    }

    public void Setup(GameObject unit)
    {
        pc = unit.GetComponent<PlayerController>();
        if (pc == null)
        {
            Debug.LogError($"角色 {unit.name} 未找到 PlayerController！");
            return;
        }

        pc.RefreshUnlockedSkills(); // ✅ 强制刷新技能列表，防止引用的是旧数据

        if (avatarImage != null && pc.symbol.playerImage != null)
            avatarImage.sprite = pc.symbol.playerImage;

        pc.OnStatsChanged += RefreshUI;
        pc.OnLevelChanged += RefreshUIWithLevel;

        ForceRefresh();
    }


    private void OnDisable()
    {
        if (pc != null)
        {
            pc.OnStatsChanged -= RefreshUI;
            pc.OnLevelChanged -= RefreshUIWithLevel;
        }
    }

    private void RefreshUIWithLevel(int _) => RefreshUI();

    private void RefreshUI()
    {
        if (pc == null) return;

        SetWithTyping(nameText, pc.symbol != null ? pc.symbol.Name : pc.name);
        SetWithTyping(levelText, GetFormattedLevelText(pc.level));
        SetWithTyping(statsText, GenerateStatsString());

        UpdateSkillUI(pc.level, pc.symbol.unitName);
    }

    private void UpdateSkillUI(int level, string unitName)
    {
        var skillGroup = SkillGroupManager.Instance.GetSkillGroupFor(unitName);  // ✅ 改用统一数据源

        if (skillGroup == null)
        {
            skill1Obj.SetActive(false);
            skill2Obj.SetActive(false);
            skill3Obj.SetActive(false);
            return;
        }

        skill1Obj.SetActive(level >= 1);
        skill1NameText.gameObject.SetActive(level >= 1);
        skill2Obj.SetActive(level >= 2);
        skill2NameText.gameObject.SetActive(level >= 2);
        skill3Obj.SetActive(level >= 3);
        skill3NameText.gameObject.SetActive(level >= 3);

        if (level >= 1 && skillGroup.skill1 != null)
        {
            skill1NameText.text = skillGroup.skill1.name;
            skill1Icon.sprite = skillGroup.skill1.icon;
            AddHoverEvent(skill1Icon.gameObject, skillGroup.skill1.desc);
            AddClickEquipEvent(skill1Icon.gameObject, 0);
        }

        if (level >= 2 && skillGroup.skill2 != null)
        {
            skill2NameText.text = skillGroup.skill2.name;
            skill2Icon.sprite = skillGroup.skill2.icon;
            AddHoverEvent(skill2Icon.gameObject, skillGroup.skill2.desc);
            AddClickEquipEvent(skill2Icon.gameObject, 1);
        }

        if (level >= 3 && skillGroup.skill3 != null)
        {
            skill3NameText.text = skillGroup.skill3.name;
            skill3Icon.sprite = skillGroup.skill3.icon;
            AddHoverEvent(skill3Icon.gameObject, skillGroup.skill3.desc);
            AddClickEquipEvent(skill3Icon.gameObject, 2);
        }
        // ✅ 根据当前装备技能恢复选中视觉
        HideAllSkillHighlights();
        string equippedID = pc.equippedSkill?.SkillID;

        if (!string.IsNullOrEmpty(equippedID))
        {
            if (skillGroup.skill1 != null && equippedID == skillGroup.skill1.id)
                ShowSelectedHighlight(skill1Highlight, false);
            else if (skillGroup.skill2 != null && equippedID == skillGroup.skill2.id)
                ShowSelectedHighlight(skill2Highlight, false);
            else if (skillGroup.skill3 != null && equippedID == skillGroup.skill3.id)
                ShowSelectedHighlight(skill3Highlight, false);
        }

    }


    private void AddClickEquipEvent(GameObject obj, int skillIndex)
    {
        Button btn = obj.GetComponent<Button>() ?? obj.AddComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            if (pc == null)
            {
                Debug.LogWarning("⚠️ PlayerController 未绑定！");
                return;
            }

            if (skillIndex < 0 || skillIndex >= pc.unlockedSkills.Count)
            {
                Debug.LogWarning($"⚠️ 无效的技能索引：{skillIndex}");
                return;
            }

            var skill = pc.unlockedSkills[skillIndex];
            if (skill == null)
            {
                Debug.LogWarning("⚠️ 找不到技能！");
                return;
            }

            pc.EquipSkill(skill);
            Debug.Log($"✅ 已切换技能：{skill.SkillName} 已装备！");

            // ✅ 查找当前 SkillGroup → 获取当前技能 → 获取 overrideUnitName
            var skillGroup = SkillGroupManager.Instance.GetSkillGroupFor(pc.symbol.unitName);
            string newName = null;
            switch (skillIndex)
            {
                case 0: newName = skillGroup?.skill1?.overrideUnitName; break;
                case 1: newName = skillGroup?.skill2?.overrideUnitName; break;
                case 2: newName = skillGroup?.skill3?.overrideUnitName; break;
            }

            // ✅ 设置干员名字并刷新 nameText
            if (!string.IsNullOrEmpty(newName))
            {
                pc.NameOverride = newName;          // ✅ 设置并自动触发事件
                SetWithTyping(nameText, newName);  // ✅ 本地面板立即刷新

            }


            // ✅ 自动刷新 SkillButtonDisplay（无需手动传 SkillGroup）
            var display = pc.GetComponentInChildren<SkillButtonDisplay>();
            if (display != null)
            {
                display.Init(pc);
            }

            // ✅ 视觉选中效果处理
            HideAllSkillHighlights(); // 先隐藏所有高亮
            switch (skillIndex)
            {
                case 0: ShowSelectedHighlight(skill1Highlight, true); break;
                case 1: ShowSelectedHighlight(skill2Highlight, true); break;
                case 2: ShowSelectedHighlight(skill3Highlight, true); break;
            }
        });
    }





    private void AddHoverEvent(GameObject obj, string desc)
    {
        var trigger = obj.GetComponent<EventTrigger>() ?? obj.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        Vector3 originalScale = obj.transform.localScale;
        float hoverScale = 1.05f;

        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener((_) =>
        {
            skillDescText.gameObject.SetActive(true);
            skillDescBG.gameObject.SetActive(true);
            skillDescText.text = desc;
            LayoutRebuilder.ForceRebuildLayoutImmediate(skillDescText.rectTransform);
            skillDescBG.sizeDelta = skillDescText.rectTransform.sizeDelta + new Vector2(40, 20);
            obj.transform.DOScale(originalScale * hoverScale, 0.15f).SetEase(Ease.OutBack);
            if (hoverSfx && audioSource) audioSource.PlayOneShot(hoverSfx);
        });
        trigger.triggers.Add(enter);

        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener((_) =>
        {
            skillDescText.gameObject.SetActive(false);
            skillDescBG.gameObject.SetActive(false);
            obj.transform.DOScale(originalScale, 0.15f).SetEase(Ease.InOutSine);
        });
        trigger.triggers.Add(exit);
    }

    #region 数据结构
    [System.Serializable]
    public class SkillInfo
    {
        public string overrideUnitName; // ✅ 新增：技能对应的干员名称（比如“突击兵”）
        public string id;   // ✅ 和技能类中的 SkillID 一致
        public string name; // 显示用的名称
        [TextArea(2, 4)] public string desc; // 技能说明
        public Sprite icon; // 图标资源
    }


    [System.Serializable]
    public class SkillGroup
    {
        public string unitName;
        public SkillInfo skill1;
        public SkillInfo skill2;
        public SkillInfo skill3;
    }
    #endregion

    #region UI展示相关
    private void SetWithTyping(TextMeshProUGUI target, string content)
    {
        if (target == null) return;
        var typer = target.GetComponent<BootSequenceTyper>();
        if (typer != null)
        {
            typer.StopTyping();
            typer.UpdateCachedDescription(content);
            typer.ShowBootAndDescription(content);
        }
        else
        {
            target.text = content;
        }
    }

    private string GetFormattedLevelText(int level)
    {
        string color = GetRankColorHex(level);
        string title = GetRankTitle(level);
        return $"等级：<color={color}>{title}</color> [{level}]";
    }

    private string GetRankTitle(int level)
    {
        return level switch
        {
            0 => "新兵",
            1 => "正式干员",
            2 => "高级干员",
            3 => "精英干员",
            _ => "未知",
        };
    }

    private string GetRankColorHex(int level)
    {
        return level switch
        {
            0 => "#BBBBBB",
            1 => "#00AAFF",
            2 => "#FFD700",
            3 => "#FF6C00",
            _ => "#FFFFFF",
        };
    }

    private string GenerateStatsString()
    {
        if (pc == null) return "";
        return
     $"生命值: {pc.health}/{pc.TotalMaxHealth}\n" +
     $"弹药量: {pc.energy}/{pc.TotalMaxEnergy}\n" +
     $"子弹伤害: {pc.TotalDamage}\n" +
     $"攻击间隔: {pc.TotalAttackDelay:F2}s\n" +
     $"移动速度: {pc.TotalMovingSpeed:F1}\n" +
     $"子弹速度: {pc.TotalBulletSpeed:F1}\n" +
     $"角色视野: {pc.TotalVisionRadius:F1}\n" +
     $"交互速度: {pc.TotalInteractSpeed:F2}s / {pc.TotalRecoverSpeed:F2}s";

    }
    #endregion

    public void ForceRefresh()
    {
        if (pc == null) return;
        var buttonController = GetComponent<RoleUpgradeButtonController>();
        if (buttonController != null)
        {
            buttonController.Typer?.StopTyping();
            buttonController.lastShownMessage = "";
        }
        RefreshUI();
    }

    public PlayerController GetPlayerController() => pc;




    private void ShowSelectedHighlight(Image target, bool doTween)
    {
        if (target == null) return;
        target.gameObject.SetActive(true);

        if (!doTween)
        {
            target.rectTransform.localScale = new Vector3(1.08f, 1.08f, 1f);
            return;
        }

        target.rectTransform.localScale = new Vector3(0.3f, 0.3f, 1f);
        target.rectTransform.DOScale(new Vector3(1.08f, 1.08f, 1f), 0.3f).SetEase(Ease.OutBack);
    }
    private void HideAllSkillHighlights()
    {
        if (skill1Highlight != null) skill1Highlight.gameObject.SetActive(false);
        if (skill2Highlight != null) skill2Highlight.gameObject.SetActive(false);
        if (skill3Highlight != null) skill3Highlight.gameObject.SetActive(false);
    }







}
