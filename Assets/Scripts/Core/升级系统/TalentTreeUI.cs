using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TalentTreeUI : MonoBehaviour
{
    #region Inspector Fields

    [Header("Row Containers (Assign in Inspector)")]
    [SerializeField] private Transform row1Container;
    [SerializeField] private Transform row2Container;
    [SerializeField] private Transform row3Container;

    // ★ 新增：重置天赋按钮、RP消耗及提示文本
    [Header("重置天赋功能")]
    public UnityEngine.UI.Button resetButton;
    [Tooltip("重置天赋所需消耗的 RP 数")]
    public int resetRPcost = 5;
    [Tooltip("RP不足重置时的提示文本")]
    public TMPro.TextMeshProUGUI insufficientResetRPText;

    #endregion

    #region Internal State

    private List<TalentSlotUI> row1Slots = new List<TalentSlotUI>();
    private List<TalentSlotUI> row2Slots = new List<TalentSlotUI>();
    private List<TalentSlotUI> row3Slots = new List<TalentSlotUI>();

    private List<TalentSlotUI> row1Selections = new List<TalentSlotUI>();
    private List<TalentSlotUI> row2Selections = new List<TalentSlotUI>();
    private TalentSlotUI row3Selected;

    private PlayerController targetPlayer;
    private bool lastHasVersatileTalent;
    private int lastPlayerLevel;
    #endregion

    #region Unity Callbacks

    void Awake()
    {
        InitializeSlotList(row1Container, row1Slots);
        InitializeSlotList(row2Container, row2Slots);
        InitializeSlotList(row3Container, row3Slots);

        // ★ 初始化重置按钮点击事件
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetTalentTree);
        GameData.Instance.OnRPChanged += OnRPChanged;
    }
    private void OnDestroy()
    {
        if (GameData.Instance != null)
            GameData.Instance.OnRPChanged -= OnRPChanged;
    }
    private void OnRPChanged(int newRP)
    {
        // ★ 如果之前因资源不足而显示提示，现在资源充足了就隐藏
        if (insufficientResetRPText != null && insufficientResetRPText.gameObject.activeSelf)
        {
            if (newRP >= resetRPcost)
            {
                insufficientResetRPText.gameObject.SetActive(false);
            }
        }

        // 可选：可以控制 resetButton 是否可点击
        if (resetButton != null)
        {
            resetButton.interactable = newRP >= resetRPcost;
        }
    }

    #endregion

    #region Initialization Methods

    void Update()
    {
        if (targetPlayer != null && targetPlayer.level != lastPlayerLevel)
        {
            lastPlayerLevel = targetPlayer.level;
            Debug.Log($"[TalentTreeUI] 玩家等级变化：{lastPlayerLevel}");
            SetRowVisibility();
            LockHigherTierSlots();
        }

        if (targetPlayer != null && targetPlayer.hasVersatileTalent != lastHasVersatileTalent)
        {
            lastHasVersatileTalent = targetPlayer.hasVersatileTalent;
            Debug.Log($"[TalentTreeUI] 检测到 hasVersatileTalent 变化，当前值：{lastHasVersatileTalent}");
            LockHigherTierSlots();
        }
    }

    void InitializeSlotList(Transform container, List<TalentSlotUI> slotList)
    {
        if (container == null)
            return;

        slotList.AddRange(container.GetComponentsInChildren<TalentSlotUI>());
        foreach (var slot in slotList)
        {
            slot.OnUpgradeConfirmed = OnSlotUpgradeConfirmed;
        }
    }

    #endregion

    #region Public Setup

    public void SetupForPlayer(PlayerController player)
    {
        targetPlayer = player;
        int level = player.level;

        SetRowVisibility();
        SetRowInteractable(row1Slots, level >= 1);
        SetRowInteractable(row2Slots, level >= 2);
        SetRowInteractable(row3Slots, level >= 3);

        RestoreTalentState();
        LockHigherTierSlots();
    }

    #endregion

    #region UI Interaction Methods

    void SetRowInteractable(List<TalentSlotUI> slotList, bool interactable)
    {
        foreach (var slot in slotList)
        {
            if (slot.slotButton == null)
                continue;

            slot.slotButton.interactable = interactable;
            slot.SetSlotLocked(!interactable);
        }
    }

    void SetRowVisibility()
    {
        if (row1Container != null)
            row1Container.gameObject.SetActive(targetPlayer.level >= 1);

        if (row2Container != null)
            row2Container.gameObject.SetActive(targetPlayer.level >= 2 && row1Selections.Count > 0);

        if (row3Container != null)
            row3Container.gameObject.SetActive(targetPlayer.level >= 3 && row2Selections.Count > 0);
    }

    #endregion

    #region Slot Upgrade Methods

    int GetMaxSelections(int row)
    {
        if (row == 1 || row == 2)
        {
            if (targetPlayer != null && targetPlayer.hasVersatileTalent)
                return 2;
            return 1;
        }
        return 1;
    }

    void OnSlotUpgradeConfirmed(TalentSlotUI slot)
    {
        if (row1Slots.Contains(slot))
        {
            int maxSelections = GetMaxSelections(1);
            if (row1Selections.Contains(slot))
                return;

            if (row1Selections.Count >= maxSelections)
            {
                Debug.LogWarning("已达到1级天赋选择上限！");
                return;
            }

            row1Selections.Add(slot);
            ApplyTalentSlotEffect(slot);
            slot.SetAsUpgraded(true);
            Debug.Log($"[TalentTreeUI] 1级选中槽：{slot.name} (ID={slot.slotID})");

            if (row1Selections.Count == maxSelections)
                DisableNonSelected(row1Slots, row1Selections);
        }
        else if (row2Slots.Contains(slot))
        {
            int maxSelections = GetMaxSelections(2);
            if (row2Selections.Contains(slot))
                return;

            if (row2Selections.Count >= maxSelections)
            {
                Debug.LogWarning("已达到2级天赋选择上限！");
                return;
            }

            row2Selections.Add(slot);
            ApplyTalentSlotEffect(slot);
            slot.SetAsUpgraded(true);
            Debug.Log($"[TalentTreeUI] 2级选中槽：{slot.name} (ID={slot.slotID})");

            if (row2Selections.Count == maxSelections)
                DisableNonSelected(row2Slots, row2Selections);
        }
        else if (row3Slots.Contains(slot))
        {
            if (row3Selected != null)
            {
                Debug.LogWarning("已选择3级天赋，不能再次选择！");
                return;
            }
            row3Selected = slot;
            ApplyTalentSlotEffect(slot);
            slot.SetAsUpgraded(true);
            Debug.Log($"[TalentTreeUI] 3级选中槽：{slot.name} (ID={slot.slotID})");
            DisableNonSelected(row3Slots, new List<TalentSlotUI> { row3Selected });
        }

        if (slot.slotEffect != null && targetPlayer != null &&
            !targetPlayer.slots.Exists(s => s.slotName == slot.slotID))
        {
            Slot newSlot = new Slot(slot.slotID, slot.slotEffect);
            targetPlayer.slots.Add(newSlot);
        }

        SaveTalentState();
        LockHigherTierSlots();
        targetPlayer.TriggerStatsChanged();

        var detailUI = FindObjectOfType<RoleUpgradeDetailUI>();
        if (detailUI != null && detailUI.GetPlayerController() == targetPlayer)
        {
            detailUI.ForceRefresh();



        }
    }

    void ApplyTalentSlotEffect(TalentSlotUI slot)
    {
        if (slot.slotEffect != null && targetPlayer != null)
        {
            slot.slotEffect.ApplyEffect(targetPlayer);
            Debug.Log($"TalentTreeUI: 应用天赋效果 {slot.slotID} 到角色 {targetPlayer.name}");
        }
    }

    void DisableNonSelected(List<TalentSlotUI> slotList, List<TalentSlotUI> selectedSlots)
    {
        foreach (var slot in slotList)
        {
            if (!selectedSlots.Contains(slot))
            {
                slot.DisableInteraction();
                slot.SetSlotLocked(true);
            }
        }
    }

    void UpdateRowSlotState(List<TalentSlotUI> slotList, List<TalentSlotUI> selectedSlots, int maxSelections, int requiredLevel)
    {
        foreach (var slot in slotList)
        {
            if (selectedSlots.Contains(slot))
            {
                slot.SetAsUpgraded(true);
                slot.DisableInteraction();
            }
            else
            {
                bool canInteract = targetPlayer.level >= requiredLevel && selectedSlots.Count < maxSelections;
                slot.slotButton.interactable = canInteract;
                slot.SetSlotLocked(!canInteract);
            }
        }
    }

    void LockHigherTierSlots()
    {
        if (row1Slots != null)
        {
            UpdateRowSlotState(row1Slots, row1Selections, GetMaxSelections(1), 1);
        }

        if (row2Slots != null)
        {
            int maxRow2 = GetMaxSelections(2);
            UpdateRowSlotState(row2Slots, row2Selections, maxRow2, 2);
        }

        if (row3Slots != null)
        {
            if (row3Selected != null)
            {
                foreach (var slot in row3Slots)
                {
                    if (slot == row3Selected)
                        slot.SetAsUpgraded(true);
                    else
                    {
                        slot.DisableInteraction();
                        slot.SetSlotLocked(true);
                    }
                }
            }
            else
            {
                foreach (var slot in row3Slots)
                {
                    bool canInteract = targetPlayer.level >= 3;
                    slot.slotButton.interactable = canInteract;
                    slot.SetSlotLocked(!canInteract);
                }
            }
        }
        SetRowVisibility();
    }

    #endregion

    #region Talent State Management

    void SaveTalentState()
    {
        if (GameData.Instance == null || targetPlayer == null)
        {
            Debug.LogWarning("[TalentTreeUI] 无法保存天赋状态 - GameData 或 targetPlayer 为 null");
            return;
        }

        string characterKey = targetPlayer.symbol.unitID;

        if (!GameData.Instance.talentState.ContainsKey(characterKey))
            GameData.Instance.talentState[characterKey] = new Dictionary<string, string>();

        string row1IDs = row1Selections.Count > 0 ? string.Join(",", row1Selections.Select(s => s.slotID)) : "";
        string row2IDs = row2Selections.Count > 0 ? string.Join(",", row2Selections.Select(s => s.slotID)) : "";
        string row3ID = row3Selected != null ? row3Selected.slotID : "";

        GameData.Instance.talentState[characterKey]["Row1"] = row1IDs;
        GameData.Instance.talentState[characterKey]["Row2"] = row2IDs;
        GameData.Instance.talentState[characterKey]["Row3"] = row3ID;

        GameData.Instance.Save();
        Debug.Log($"[TalentTreeUI] 保存天赋状态 - 角色ID={characterKey}, Row1={row1IDs}, Row2={row2IDs}, Row3={row3ID}");
    }

    void RestoreTalentState()
    {
        if (GameData.Instance == null || targetPlayer == null || GameData.Instance.talentState == null)
        {
            Debug.LogWarning("[TalentTreeUI] 无法恢复天赋状态 - GameData 或 targetPlayer 为 null，或 talentState 为 null");
            return;
        }

        string characterKey = targetPlayer.symbol.unitID;
        if (!GameData.Instance.talentState.ContainsKey(characterKey))
        {
            Debug.LogWarning($"[TalentTreeUI] 角色ID={characterKey} 没有天赋存档");
            return;
        }

        Dictionary<string, string> state = GameData.Instance.talentState[characterKey];
        Debug.Log($"[TalentTreeUI] 恢复天赋状态 - 角色ID={characterKey}, Row1={state.GetValueOrDefault("Row1")}, Row2={state.GetValueOrDefault("Row2")}, Row3={state.GetValueOrDefault("Row3")}");

        // 恢复 1 级
        string row1IDs = state.GetValueOrDefault("Row1");
        if (!string.IsNullOrEmpty(row1IDs))
        {
            foreach (var id in row1IDs.Split(','))
            {
                var selected = row1Slots.Find(slot => slot.slotID == id);
                if (selected != null)
                {
                    row1Selections.Add(selected);
                    selected.SetAsUpgraded(true);
                }
                else
                {
                    Debug.LogWarning($"[TalentTreeUI] 行1 找不到槽 ID={id}");
                }
            }
            UpdateRowSlotState(row1Slots, row1Selections, GetMaxSelections(1), 1);
        }

        // 恢复 2 级
        string row2IDs = state.GetValueOrDefault("Row2");
        if (!string.IsNullOrEmpty(row2IDs))
        {
            foreach (var id in row2IDs.Split(','))
            {
                var selected = row2Slots.Find(slot => slot.slotID == id);
                if (selected != null)
                {
                    row2Selections.Add(selected);
                    selected.SetAsUpgraded(true);
                }
                else
                {
                    Debug.LogWarning($"[TalentTreeUI] 行2 找不到槽 ID={id}");
                }
            }
            UpdateRowSlotState(row2Slots, row2Selections, GetMaxSelections(2), 2);
        }

        // 恢复 3 级
        string row3ID = state.GetValueOrDefault("Row3");
        if (!string.IsNullOrEmpty(row3ID))
        {
            row3Selected = row3Slots.Find(slot => slot.slotID == row3ID);
            if (row3Selected != null)
                row3Selected.SetAsUpgraded(true);
            else
                Debug.LogWarning($"[TalentTreeUI] 行3 找不到槽 ID={row3ID}");
        }

        SetRowVisibility();
        LockHigherTierSlots();
        Debug.Log("[TalentTreeUI] 天赋状态已恢复并应用完成。");
    }

    #endregion

    #region Reset Talent Tree Function

    /// <summary>
    /// 重置天赋树：撤回所有升级、清空存档，并调用角色所有天赋的 RemoveEffect 方法
    /// </summary>
    public void ResetTalentTree()
    {
        // 检查 RP 是否足够用于重置
        if (GameData.Instance.RP < resetRPcost)
        {
            Debug.Log("RP不足，无法重置天赋树！");
            if (insufficientResetRPText != null)
            {
                insufficientResetRPText.gameObject.SetActive(true);
                insufficientResetRPText.text = "RP不足，无法重置天赋树！";
            }
            return;
        }

        // 扣除 RP 并更新显示
        GameData.Instance.RP -= resetRPcost;
        MainUI.Instance.UpdateRPDisplay();
        if (insufficientResetRPText != null)
            insufficientResetRPText.gameObject.SetActive(false);

        // 清空当前角色的天赋存档
        if (targetPlayer != null && GameData.Instance.talentState != null)
        {
            string characterKey = targetPlayer.symbol.unitID;
            if (GameData.Instance.talentState.ContainsKey(characterKey))
            {
                GameData.Instance.talentState[characterKey]["Row1"] = "";
                GameData.Instance.talentState[characterKey]["Row2"] = "";
                GameData.Instance.talentState[characterKey]["Row3"] = "";
                GameData.Instance.Save();
            }
            // 撤销角色上所有已应用的天赋效果
            if (targetPlayer.slots != null)
            {
                foreach (var slot in targetPlayer.slots)
                {
                    if (slot.slotEffect != null)
                    {
                        slot.slotEffect.RemoveEffect(targetPlayer);
                    }
                }
                targetPlayer.slots.Clear();
            }
        }

        // 清空 UI 上的选择记录并重置所有槽状态
        foreach (var slot in row1Slots)
        {
            slot.SetAsUpgraded(false);
            if (slot.slotButton != null)
                slot.slotButton.interactable = true;
        }
        row1Selections.Clear();

        foreach (var slot in row2Slots)
        {
            slot.SetAsUpgraded(false);
            if (slot.slotButton != null)
                slot.slotButton.interactable = true;
        }
        row2Selections.Clear();

        foreach (var slot in row3Slots)
        {
            slot.SetAsUpgraded(false);
            if (slot.slotButton != null)
                slot.slotButton.interactable = true;
        }
        row3Selected = null;

        SetRowInteractable(row1Slots, targetPlayer.level >= 1);
        SetRowInteractable(row2Slots, targetPlayer.level >= 2);
        SetRowInteractable(row3Slots, targetPlayer.level >= 3);
        SetRowVisibility();
        targetPlayer.TriggerStatsChanged();
        Debug.Log("[TalentTreeUI] 天赋树已重置");
    }

    #endregion
}
