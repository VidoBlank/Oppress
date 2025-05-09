using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoleUpgradeButtonController : MonoBehaviour
{
    [Header("UI 引用")]
    public Button upgradeButton;
    public TextMeshProUGUI upgradeInfoText;
    private BootSequenceTyper typer;

    [Header("升级设置")]
    public int[] upgradeCosts = new int[] { 2, 3 };
    [TextArea(2, 4)] public string upgradeSuccessMessage = "> LEVEL SYNC 完成\n> 干员模块强化已应用";
    [TextArea(2, 4)] public string upgradeFailMessage = "> 同步失败：资源点不足";
    public AudioClip successClip;
    public AudioClip failClip;

    [Header("附加UI控制")]
    public Image extraImageToHide;  // ✅ 新增：0级或满级时隐藏的 UI 元素

    public PlayerController player;
    public AudioSource audioSource;
    public string lastShownMessage = "";
    public BootSequenceTyper Typer => typer;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);

        RoleUpgradeDetailUI detailUI = GetComponent<RoleUpgradeDetailUI>();
        if (detailUI != null)
        {
            player = detailUI.GetPlayerController();
        }
        else
        {
            Debug.LogError("找不到 RoleUpgradeDetailUI 组件！");
        }

        typer = upgradeInfoText.GetComponent<BootSequenceTyper>();
        if (typer == null)
        {
            Debug.LogError("BootSequenceTyper 未挂载在 upgradeInfoText 上！");
        }

        GameData.Instance.OnRPChanged += OnRPChanged;
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (GameData.Instance != null)
            GameData.Instance.OnRPChanged -= OnRPChanged;
    }

    private void OnRPChanged(int newRP)
    {
        RefreshUI();
    }

    public void Setup(GameObject unit)
    {
        if (unit == null) return;

        player = unit.GetComponent<PlayerController>();
        RefreshUI();
    }

    private int GetCurrentUpgradeCost()
    {
        if (player == null) return 0;
        int index = Mathf.Clamp(player.level - 1, 0, upgradeCosts.Length - 1);
        return upgradeCosts[index];
    }

    public void RefreshUI()
    {
        lastShownMessage = "";

        if (player == null || typer == null)
        {
            upgradeButton.gameObject.SetActive(false);
            upgradeInfoText.gameObject.SetActive(false);
            return;
        }

        upgradeInfoText.gameObject.SetActive(true);

        if (player.level < 1)
        {
            upgradeButton.gameObject.SetActive(false);
            ShowMessage(
                $"> 干员状态：基本构型\n" +
                $"> 当前构型不支持 LEVEL SYNC 晋升操作", true);

            if (extraImageToHide != null)
                extraImageToHide.gameObject.SetActive(false);  // ✅ 隐藏

            return;
        }

        if (player.level >= 3)
        {
            upgradeButton.gameObject.SetActive(false);
            ShowMessage(
                $"> 等级上限已达成\n" +
                $"> 干员处于最优性能状态，无需进一步同步", true);

            if (extraImageToHide != null)
                extraImageToHide.gameObject.SetActive(false);  // ✅ 隐藏

            return;
        }

        int cost = GetCurrentUpgradeCost();
        upgradeButton.gameObject.SetActive(true);
        upgradeButton.interactable = true;

        ShowMessage(
            $"> 晋升权限受限\n" +
            $"> 所需资源点：<b><color=#00FFFF>{cost}</color></b>\n" +
            $"> 执行 LEVEL SYNC 指令以强化干员性能并解锁插槽扩展", false);

        // ✅ 若等级在 1 或 2，则显示附加UI
        if (extraImageToHide != null)
            extraImageToHide.gameObject.SetActive(true);
    }

    private void OnUpgradeButtonClicked()
    {
        if (player == null || typer == null || player.level < 1 || player.level >= 3)
            return;

        int cost = GetCurrentUpgradeCost();

        if (GameData.Instance.RP >= cost)
        {
            GameData.Instance.RP -= cost;
            player.SetLevel(player.level + 1);

            PlaySound(successClip);  // ✅ 成功播放成功音效
            ShowMessage(upgradeSuccessMessage, true);

            RoleUpgradeDetailUI detailUI = GetComponent<RoleUpgradeDetailUI>();
            if (detailUI != null)
            {
                detailUI.ForceRefresh();
            }

            Invoke(nameof(RefreshUI), 1.5f);
        }
        else
        {
            string failMsg =
                $"> 晋升权限受限\n" +
                $"> 所需资源点：<b><color=#00FFFF>{cost}</color></b>\n" +
                $"> 执行 LEVEL SYNC 指令以强化干员性能并解锁插槽扩展\n" +
                $"→ {upgradeFailMessage}";

            ShowMessage(failMsg, true);

            PlaySound(failClip);  // ✅ 失败播放失败音效
        }
    }


    private void ShowMessage(string message, bool instant)
    {
        if (lastShownMessage == message) return;

        lastShownMessage = message;
        if (instant)
            typer.ShowInstant(message);
        else
            typer.ShowBootAndDescription(message);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}
