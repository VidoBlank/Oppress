using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class TalentTree_Level0 : MonoBehaviour
{
    [Header("选中图标视觉效果")]
    public GameObject selectionFramePrefab; // 预制体：高亮框（Image）
    private GameObject currentSelectionFrame;

    [Header("音效")]
    public AudioClip successClip;
    public AudioClip failClip;
    private AudioSource audioSource;

    [Header("升级方向图标列表")]
    public List<TalentIcon> talentIcons;

    [Header("晋升UI按钮")]
    public Button promotionButton;

    [Header("消耗金币显示文本")]
    public TextMeshProUGUI coinCostText;

    // ★ 新增：提示“金币不足”的文本 (TMP)
    [Header("金币不足提示文本(默认隐藏)")]
    public TextMeshProUGUI insufficientCoinText;

    // 当前被选择的升级方向
    public TalentIcon selectedTalentIcon;

    // 当前待升级的干员
    public GameObject currentUnit;

    [Header("PlayerTeamManager 引用")]
    public PlayerTeamManager playerTeamManager;
    [Header("干员描述文本")]
    public TextMeshProUGUI descriptionText; // 显示干员介绍的文本组件

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // 如果未手动指定，则通过单例赋值
        if (playerTeamManager == null)
        {
            playerTeamManager = PlayerTeamManager.Instance;
            Debug.Log("TalentTree_Level0: 自动赋值 PlayerTeamManager 实例");
        }

        // 隐藏晋升按钮
        if (promotionButton != null)
        {
            promotionButton.gameObject.SetActive(false);
            Debug.Log("TalentTree_Level0: 隐藏晋升按钮");
        }

        // ★ 在开始时，隐藏金币不足提示
        if (insufficientCoinText != null)
        {
            insufficientCoinText.gameObject.SetActive(false);
        }

        // 为每个升级方向图标绑定点击事件
        if (talentIcons != null)
        {
            foreach (TalentIcon icon in talentIcons)
            {
                TalentIcon currentIcon = icon;
                if (currentIcon.button != null)
                {
                    currentIcon.button.onClick.AddListener(() => OnTalentIconClicked(currentIcon));
                    Debug.Log($"TalentTree_Level0: 绑定升级图标按钮事件，金币要求: {currentIcon.coinCost}");
                }
            }
        }

        // 绑定晋升按钮点击事件
        if (promotionButton != null)
        {
            promotionButton.onClick.AddListener(OnPromotionButtonClicked);
            Debug.Log("TalentTree_Level0: 绑定晋升按钮点击事件");
        }
        GameData.Instance.OnRPChanged += OnRPChanged;
    }
    void OnDestroy()
    {
        if (GameData.Instance != null)
            GameData.Instance.OnRPChanged -= OnRPChanged;
    }
    private void OnRPChanged(int newRP)
    {
        // 如果显示了“资源点不足”提示，再次检查是否满足条件，隐藏提示
        if (selectedTalentIcon != null && insufficientCoinText != null && insufficientCoinText.gameObject.activeSelf)
        {
            if (GameData.Instance.RP >= selectedTalentIcon.coinCost)
            {
                insufficientCoinText.gameObject.SetActive(false);
            }
        }

        // 可选：刷新消耗文本（比如实时更新“所需资源点：10/当前11”）
        if (selectedTalentIcon != null && coinCostText != null)
        {
            coinCostText.text = $"所需资源点: <color=#00FFFF><b>{selectedTalentIcon.coinCost}</b></color>";

        }
    }

    /// <summary>
    /// 外部调用，设置当前干员对象
    /// </summary>
    public void Setup(GameObject unit)
    {
        currentUnit = unit;
        Debug.Log($"TalentTree_Level0: 设置当前干员对象: {currentUnit.name}");
    }

    /// <summary>
    /// 当点击某个升级方向图标时触发
    /// </summary>
    void OnTalentIconClicked(TalentIcon icon)
    {
        selectedTalentIcon = icon;
        Debug.Log($"TalentTree_Level0: 点击升级方向图标，金币要求: {icon.coinCost}");

        if (coinCostText != null)
        {
            coinCostText.text = $"所需资源点: <color=#00FFFF><b>{icon.coinCost}</b></color>";
        }

        if (promotionButton != null)
        {
            promotionButton.gameObject.SetActive(true);
        }

        // 显示描述
        BootSequenceTyper typer = descriptionText.GetComponent<BootSequenceTyper>();
        if (typer != null)
        {
            typer.ShowBootAndDescription(icon.unitDescription);
        }
        else
        {
            descriptionText.text = icon.unitDescription;
        }

        // 隐藏金币不足提示
        if (insufficientCoinText != null)
        {
            insufficientCoinText.gameObject.SetActive(false);
        }

        // ✅ 创建或更新选中框
        CreateOrMoveSelectionFrame(icon.button.gameObject);
    }

    void CreateOrMoveSelectionFrame(GameObject target)
    {
        if (selectionFramePrefab == null || target == null) return;

        // 获取 Canvas 或 UI 根节点作为父物体
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        Transform root = rootCanvas != null ? rootCanvas.transform : target.transform.root;

        if (currentSelectionFrame == null)
        {
            currentSelectionFrame = Instantiate(selectionFramePrefab, root, false);
        }

        // 设置为最下层（在图标后）
        currentSelectionFrame.transform.SetSiblingIndex(1);


        // 获取 RectTransform
        RectTransform rt = currentSelectionFrame.GetComponent<RectTransform>();
        RectTransform targetRT = target.GetComponent<RectTransform>();
        if (rt != null && targetRT != null)
        {
            // 对齐位置
            Vector2 anchoredPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                root as RectTransform,
                RectTransformUtility.WorldToScreenPoint(null, targetRT.position),
                null,
                out anchoredPos
            );

            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = targetRT.sizeDelta * 1f;
            rt.localScale = new Vector3(0.3f, 0.3f, 1f); // 初始缩小
        }

        currentSelectionFrame.SetActive(true);

        // ✅ 播放弹出动画
        rt.DOScale(new Vector3(1.08f, 1.08f, 1f), 0.3f)
          .SetEase(Ease.OutBack, 0.5f);
    }


    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// 当点击晋升按钮时触发升级操作
    /// </summary>
    void OnPromotionButtonClicked()
    {
        if (selectedTalentIcon != null && currentUnit != null)
        {
            if (GameData.Instance.RP >= selectedTalentIcon.coinCost)
            {
                GameData.Instance.RP -= selectedTalentIcon.coinCost;
                MainUI.Instance.UpdateRPDisplay();

                if (playerTeamManager != null)
                {
                    playerTeamManager.DeleteUnit(currentUnit);
                    playerTeamManager.AddNewUnit(selectedTalentIcon.upgradedUnitPrefab.name);
                    RoleUpgradeManager.Instance.RefreshAvatarListUI();

                    // ✅ 使用 UI 音效管理器播放成功音效
                    UIAudioManager.Instance?.PlaySuccess();

                    Destroy(this.gameObject); // 正常销毁
                }
            }
            else
            {
                // ✅ 使用 UI 音效管理器播放失败音效
                UIAudioManager.Instance?.PlayFail();

                if (insufficientCoinText != null)
                {
                    insufficientCoinText.gameObject.SetActive(true);
                    insufficientCoinText.text = "指令中止：资源点不足，无法完成改造流程。";
                }
            }
        }
    }


}


[System.Serializable]

public class TalentIcon
{
    [Header("图标按钮")]
    public Button button;

    [Header("所需金币")]
    public int coinCost;

    [Header("升级后干员预制体")]
    public GameObject upgradedUnitPrefab;

    [Header("干员介绍文本")]
    [TextArea(2, 5)]
    public string unitDescription; // 干员描述文本
}

