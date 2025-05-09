using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UpgradeAvatarSlot : MonoBehaviour
{
    public Image avatarImage;
    public TextMeshProUGUI nameText;
    // 显示角色等级的文本组件
    public TextMeshProUGUI levelText;

    private GameObject unitInstance;
    private Button button;

    // 点击时的回调，传递当前绑定的角色实例；当传递 null 时表示关闭详细页面
    private System.Action<GameObject> onClickedCallback;

    // 缩放参数
    public float selectedScale = 1.2f;    // 选中时的放大倍数
    public float deselectedScale = 1.0f;  // 默认缩放
    public float scaleDuration = 0.2f;    // 动画持续时间

    // 静态变量，用于记录当前选中的槽，确保单选效果
    private static UpgradeAvatarSlot currentSelectedSlot = null;

    // 标记是否处于选中状态
    public bool isSelected = false;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            Debug.Log("Button 组件找到，绑定 OnClick 事件。");
            button.onClick.AddListener(OnClick);
        }
        else
        {
            Debug.LogWarning("UpgradeAvatarSlot 未找到 Button 组件");
        }
    }

    /// <summary>
    /// 绑定角色数据和点击回调
    /// </summary>
    public void Setup(GameObject unit, System.Action<GameObject> onClicked)
    {
        unitInstance = unit;
        onClickedCallback = onClicked;

        // 从 PlayerController 中获取头像、名称和等级数据
        PlayerController pc = unit.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.OnNameChanged += RefreshNameDisplay;
            if (avatarImage != null && pc.symbol != null)
            {
                avatarImage.sprite = pc.symbol.playerImage;
            }
            if (nameText != null)
            {
                nameText.text = pc.symbol != null ? pc.symbol.Name : unit.name;
            }
            if (levelText != null)
            {
                levelText.text = GetFormattedLevelText(pc.level);
            }

        }
        else
        {
            Debug.LogError($"角色 {unit.name} 未找到 PlayerController 组件！");
        }
    }

    /// <summary>
    /// 如果不需要传入回调，也可直接调用这个重载
    /// </summary>
    public void Setup(GameObject unit)
    {
        Setup(unit, null);
    }

    /// <summary>
    /// 按钮点击时调用的方法
    /// </summary>
    private void OnClick()
    {
        Debug.Log("UpgradeAvatarSlot clicked for unit: " + (unitInstance != null ? unitInstance.name : "null"));
        if (isSelected)
        {
            // 已经选中，再次点击则取消选中，并通知上层关闭详细页面
            Deselect();
            onClickedCallback?.Invoke(null);
        }
        else
        {
            // 当前未选中，则选中，同时取消其他选中的槽
            Select();
            onClickedCallback?.Invoke(unitInstance);
        }
    }

    /// <summary>
    /// 选中当前槽，并放大显示（卡牌效果），同时取消其他槽的选中状态
    /// </summary>
    public void Select()
    {
        if (currentSelectedSlot != null && currentSelectedSlot != this)
        {
            currentSelectedSlot.Deselect();
        }
        isSelected = true;
        currentSelectedSlot = this;
        transform.DOScale(Vector3.one * selectedScale, scaleDuration);
    }

    /// <summary>
    /// 取消当前槽的选中状态，恢复原始缩放
    /// </summary>
    public void Deselect()
    {
        isSelected = false;
        if (currentSelectedSlot == this)
            currentSelectedSlot = null;
        transform.DOScale(Vector3.one * deselectedScale, scaleDuration);
    }
    private string GetFormattedLevelText(int level)
    {
        string color = GetRankColorHex(level);
        string title = GetRankTitle(level);
        return $"等级<color={color}>{level}</color> ";
    }

    private string GetRankTitle(int level)
    {
        switch (level)
        {
            case 0: return "新兵";
            case 1: return "正式干员";
            case 2: return "高级干员";
            case 3: return "精英干员";
            default: return "未知";
        }
    }

    private string GetRankColorHex(int level)
    {
        switch (level)
        {
            case 0: return "#FFFFFF"; // 灰色
            case 1: return "#00AAFF"; // 蓝色
            case 2: return "#FFD700"; // 金色
            case 3: return "#FF6C00"; // 橙色
            default: return "#FFFFFF";
        }
    }
    /// <summary>
    /// 每帧更新角色等级显示
    /// </summary>
    void Update()
    {
        if (unitInstance != null && levelText != null)
        {
            PlayerController pc = unitInstance.GetComponent<PlayerController>();
            if (pc != null)
            {
                levelText.text = GetFormattedLevelText(pc.level);
            }

        }
    }


    private void OnDestroy()
    {
        if (unitInstance != null)
        {
            var pc = unitInstance.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.OnNameChanged -= RefreshNameDisplay;
            }
        }
    }



    public void RefreshNameDisplay()
    {
        if (unitInstance == null || nameText == null) return;

        PlayerController pc = unitInstance.GetComponent<PlayerController>();
        if (pc != null && pc.symbol != null)
        {
            nameText.text = pc.symbol.Name;
        }
    }

}
