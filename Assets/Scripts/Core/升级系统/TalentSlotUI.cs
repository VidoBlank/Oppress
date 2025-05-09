using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TalentSlotUI : MonoBehaviour
{
    // ==========公开字段==========
    public Button slotButton;
    public Button upgradeButton;
    public Image selectedIndicator;   // 选中后显示的标识
    public GameObject introductionPanel;
    public TextMeshProUGUI introductionText;
    [TextArea] public string introductionContent;
    public SlotEffect slotEffect;
    public string slotID;
    public int rowID; // 所属行编号（例如 1、2、3）
    [Header("音效")]
    public AudioClip insufficientRPClip; // RP不足音效
    private AudioSource audioSource;

    // ★ 新增：升级消耗的 RP 数
    // 默认值可根据行级别设置（例如 1级=1, 2级=2, 3级=3），也可在 Inspector 中调整
    public int upgradeRPCost = 1;

    // ★ 新增：RP不足提示文本（请在 Inspector 中绑定对应的 UI 文本）
    public TextMeshProUGUI insufficientRPText;

    // 回调：升级按钮点击时回传本槽对象
    public System.Action<TalentSlotUI> OnUpgradeConfirmed;

    // ==========私有字段==========
    private bool isOpen = false;     // 是否已展开
    private bool isUpgraded = false; // 是否已经升级成功

    // 用于 DoTween 动画
    private RectTransform selectedIndicatorRect;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // 绑定按钮
        if (slotButton != null)
            slotButton.onClick.AddListener(OnSlotClicked);
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);

        // 初始化 RectTransform 引用
        if (selectedIndicator != null)
            selectedIndicatorRect = selectedIndicator.GetComponent<RectTransform>();

        // 根据当前升级状态初始化 UI
        if (isUpgraded)
        {
            if (selectedIndicator != null)
                selectedIndicator.gameObject.SetActive(true);
            if (slotButton != null)
                slotButton.interactable = false;
            if (upgradeButton != null)
                upgradeButton.gameObject.SetActive(false);
            if (introductionPanel != null)
                introductionPanel.SetActive(false);
        }
        else
        {
            if (upgradeButton != null) upgradeButton.gameObject.SetActive(false);
            if (selectedIndicator != null) selectedIndicator.gameObject.SetActive(false);
            if (introductionPanel != null) introductionPanel.SetActive(false);
        }
    }

    void OnSlotClicked()
    {
        if (isUpgraded)
        {
            Debug.Log($"[{name}] 已升级状态，点击无效");
            return;
        }

        // 关闭同一行中其他已打开的槽 UI
        HideOtherSlotUIsInSameRow();

        // 切换当前槽展开状态
        isOpen = !isOpen;
        if (isOpen)
        {
            ShowSlotUI();
        }
        else
        {
            HideSlotUI();
        }
    }

    /// <summary>
    /// 关闭同一行中其他展开的槽详细信息面板
    /// </summary>
    void HideOtherSlotUIsInSameRow()
    {
        TalentSlotUI[] allSlots = FindObjectsOfType<TalentSlotUI>();
        foreach (var slot in allSlots)
        {
            if (slot != this && slot.rowID == this.rowID && slot.isOpen)
            {
                slot.HideSlotUI();
                slot.isOpen = false;
            }
        }
    }

    void OnUpgradeButtonClicked()
    {
        if (GameData.Instance.RP < upgradeRPCost)
        {
            Debug.Log("RP不足，无法升级！");
            if (insufficientRPText != null)
            {
                insufficientRPText.gameObject.SetActive(true);
                insufficientRPText.text = "资源点不足，无法升级！";
            }

            // 播放 RP 不足音效
            if (audioSource != null && insufficientRPClip != null)
            {
                audioSource.PlayOneShot(insufficientRPClip, 0.6f); // 可调节音量
            }

            return;
        }


        // ★ 扣除升级所需的 RP
        GameData.Instance.RP -= upgradeRPCost;
        MainUI.Instance.UpdateRPDisplay();
        if (insufficientRPText != null)
            insufficientRPText.gameObject.SetActive(false);

        // 通知外部处理升级逻辑
        OnUpgradeConfirmed?.Invoke(this);

        // 标记为已升级
        isUpgraded = true;

        // 隐藏介绍面板和升级按钮
        if (introductionPanel != null)
            introductionPanel.SetActive(false);
        if (upgradeButton != null)
            upgradeButton.gameObject.SetActive(false);

        // 用动画显示选中指示器
        ShowSelectedIndicator(true);

        // 禁用槽按钮交互
        if (slotButton != null)
            slotButton.interactable = false;

        Debug.Log($"[{name}] 已升级，花费 {upgradeRPCost} RP，当前升级已确认");
    }

    // 外部调用：设置槽为“已选中/升级”状态
    public void SetAsUpgraded(bool upgraded)
    {
        isUpgraded = upgraded;
        if (slotButton != null)
            slotButton.interactable = !upgraded;
        if (upgradeButton != null)
            upgradeButton.gameObject.SetActive(false);

        if (upgraded)
        {
            ShowSelectedIndicator(false);
            selectedIndicator.color = Color.white;
            Debug.Log($"[{name}] SetAsUpgraded => 已升级状态");
        }
        else
        {
            HideSelectedIndicator(false);
            Debug.Log($"[{name}] SetAsUpgraded => 未升级状态");
        }
    }

    // ========== 展示/隐藏 UI ==========
    private void ShowSlotUI()
    {
        if (upgradeButton != null)
            upgradeButton.gameObject.SetActive(true);

        if (introductionPanel != null)
        {
            introductionPanel.SetActive(true);

            if (introductionText != null)
            {
                BootSequenceTyper typer = introductionText.GetComponent<BootSequenceTyper>();
                if (typer != null)
                {
                    typer.ShowBootAndDescription(introductionContent);
                }
                else
                {
                    introductionText.text = introductionContent;
                }
            }
        }

        // 显示升级消耗信息
        if (insufficientRPText != null)
        {
            insufficientRPText.gameObject.SetActive(true);
            insufficientRPText.text = $"所需资源点：<b><color=#00FFFF>{upgradeRPCost}</color></b>";

        }

        ShowSelectedIndicator(true);
    }


    private void HideSlotUI()
    {
        if (upgradeButton != null)
            upgradeButton.gameObject.SetActive(false);
        if (introductionPanel != null)
            introductionPanel.SetActive(false);
        if (!isUpgraded)
        {
            HideSelectedIndicator(false);
        }
    }

    // ========== selectedIndicator 动画 ==========
    private void ShowSelectedIndicator(bool doTween)
    {
        if (selectedIndicator == null) return;
        if (selectedIndicatorRect == null)
            selectedIndicatorRect = selectedIndicator.GetComponent<RectTransform>();

        selectedIndicator.gameObject.SetActive(true);
        if (!doTween)
        {
            selectedIndicatorRect.localScale = new Vector3(1.08f, 1.08f, 1f);
            return;
        }
        selectedIndicatorRect.localScale = new Vector3(0.3f, 0.3f, 1f);
        selectedIndicatorRect.DOScale(new Vector3(1.08f, 1.08f, 1f), 0.3f)
                             .SetEase(Ease.OutBack, 0.5f);
    }

    private void HideSelectedIndicator(bool doTween)
    {
        if (selectedIndicator == null) return;
        if (selectedIndicatorRect == null)
            selectedIndicatorRect = selectedIndicator.GetComponent<RectTransform>();

        if (!doTween)
        {
            selectedIndicator.gameObject.SetActive(false);
            return;
        }
        selectedIndicatorRect.DOScale(Vector3.zero, 0.3f)
                             .SetEase(Ease.InBack, 0.75f)
                             .OnComplete(() =>
                             {
                                 selectedIndicator.gameObject.SetActive(false);
                             });
    }

    public void SetSlotLocked(bool locked)
    {
        if (selectedIndicator == null)
        {
            Debug.LogWarning($"[{name}] selectedIndicator 未设置！");
            return;
        }
        if (locked)
        {
            selectedIndicator.gameObject.SetActive(false);
            selectedIndicator.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        }
        else
        {
            selectedIndicator.gameObject.SetActive(false);
            selectedIndicator.color = Color.white;
        }
    }

    public void DisableInteraction()
    {
        if (slotButton != null)
            slotButton.interactable = false;
    }
}
