using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

public class PlayerUIManager : MonoBehaviour
{
    #region 字段声明

    [Header("头像框预制体")]
    public GameObject playerFramePrefab;
    [Header("UI容器")]
    public Transform frameParent;
    [Header("布局组件")]
    public HorizontalLayoutGroup layoutGroup;

    private PlayerManager playerManager;
    private List<PlayerController> players = new List<PlayerController>();

    // 存储每个玩家对应的头像框
    public Dictionary<PlayerController, GameObject> playerFrames = new Dictionary<PlayerController, GameObject>();

    // 记录 Content 的原始位置和缩放，用于高亮动画还原
    private Dictionary<RectTransform, (Vector2 originalPosition, Vector3 originalScale)> originalStates = new Dictionary<RectTransform, (Vector2, Vector3)>();

    private int playerFrameCounter = 0; // 头像框计数

    #endregion

    #region 生命周期方法

    private void Awake()
    {
        DOTween.SetTweensCapacity(2000, 100);
        playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager == null)
        {
            Debug.LogError("PlayerManager 未找到，请确保场景中有一个带有 PlayerManager 脚本的对象。");
        }
    }

    private void Start()
    {
        // 根据 PlayerManager 中的玩家列表创建头像框
        foreach (var player in playerManager.players)
        {
            if (!playerFrames.ContainsKey(player))
            {
                AddPlayerUI(player);
            }
        }
    }

    private void Update()
    {
        // 更新所有玩家头像框的状态
        foreach (var player in players)
        {
            UpdatePlayerFrameUI(player);
        }
    }

    private void OnEnable()
    {
        PlayerController.OnPlayerAdded += AddPlayerUI;
        PlayerController.OnPlayerRemoved += RemovePlayerUI;

        // 订阅场景中所有玩家的死亡和受击事件
        foreach (var player in FindObjectsOfType<PlayerController>())
        {
            player.OnPlayerDeath += HandlePlayerDeath;
            player.OnPlayerHit += HandlePlayerHit;
        }
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerAdded -= AddPlayerUI;
        PlayerController.OnPlayerRemoved -= RemovePlayerUI;

        foreach (var player in FindObjectsOfType<PlayerController>())
        {
            player.OnPlayerDeath -= HandlePlayerDeath;
            player.OnPlayerHit -= HandlePlayerHit;
        }
    }

    #endregion

    #region UI 更新方法

    /// <summary>
    /// 更新指定玩家头像框的 UI 信息
    /// </summary>
    private void UpdatePlayerFrameUI(PlayerController player)
    {
        if (!playerFrames.TryGetValue(player, out GameObject frame))
        {
            Debug.LogWarning($"未找到玩家 {player.name} 的UI头像框。");
            return;
        }

        // 缓存 Content 及其下子对象
        Transform content = frame.transform.Find("Content");
        if (content == null)
        {
            Debug.LogError("PlayerFrame 中未找到 Content 子对象。");
            return;
        }
        Image backImageComponent = GetChildComponent<Image>(content, "BackImage");
        Image playerImageComponent = GetChildComponent<Image>(content, "PlayerImage");
        Slider healthSlider = GetChildComponent<Slider>(content, "HealthSlider");
        Slider ammoSlider = GetChildComponent<Slider>(content, "AmmoSlider");
        TextMeshProUGUI statusText = GetChildComponent<TextMeshProUGUI>(content, "StatusText");
        TextMeshProUGUI levelText = GetChildComponent<TextMeshProUGUI>(content, "LevelText");
        if (levelText != null)
        {
            levelText.text = GetColoredLevelText(player.level);
        }

        // 更新头像图片
        if (player.symbol != null)
        {
            playerImageComponent.sprite = player.symbol.playerImage;
        }

        // 更新血量和弹药百分比，使用 Tween 平滑过渡
        float healthPercentage = player.health / player.TotalMaxHealth;
        float ammoPercentage = player.energy / player.TotalMaxEnergy;

        if (healthSlider != null)
        {
            healthSlider.DOValue(healthPercentage, 0.5f).SetEase(Ease.OutQuad);
            UpdateSliderColor(healthSlider, healthPercentage);
        }
        if (ammoSlider != null)
        {
            ammoSlider.DOValue(ammoPercentage, 0.5f).SetEase(Ease.OutQuad);
            UpdateSliderColor(ammoSlider, ammoPercentage);
        }

        // 设置状态文本
        if (statusText != null)
        {
            statusText.text = GetPlayerStatusText(player);
        }

        // 当玩家倒地时，头像淡入暗红色，否则恢复为白色
        if (player.isKnockedDown)
        {
            playerImageComponent.DOKill();
            playerImageComponent.DOColor(new Color(0.5f, 0, 0, 1f), 0.5f);
        }
        else if (!player.isDead)
        {
            float healthPercent = player.health / player.attribute.maxhealth;

            if (healthPercent <= 0.3f)
            {
                if (!DOTween.IsTweening(playerImageComponent))
                {
                    playerImageComponent.DOKill();
                    playerImageComponent.color = Color.white;
                    playerImageComponent.DOColor(new Color(1f, 0.3f, 0.3f), 0.4f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);
                }
            }
            else
            {
                if (DOTween.IsTweening(playerImageComponent))
                {
                    playerImageComponent.DOKill();
                    playerImageComponent.DOColor(Color.white, 0.2f);
                }
            }
        }


        // 更新 BackImage 透明度：选中时显示，否则透明
        if (backImageComponent != null && !playerManager.selectedPlayers.Contains(player))
        {
            Color col = backImageComponent.color;
            backImageComponent.color = new Color(col.r, col.g, col.b, 0);
        }
    }

    /// <summary>
    /// 更新 Slider 内部填充和数值图片的颜色
    /// </summary>
    private void UpdateSliderColor(Slider slider, float percentage)
    {
        Color normalColor = Color.white;
        Color lowValueColor = Color.red;
        Image fillImage = slider.fillRect?.GetComponent<Image>();
        if (fillImage != null)
        {
            fillImage.color = percentage <= 0.35f ? lowValueColor : normalColor;
        }
        Image valueImage = GetChildComponent<Image>(slider.transform, "ValueImage");
        if (valueImage != null)
        {
            // 若弹药为 0，则直接设为低色
            valueImage.color = Mathf.Approximately(percentage, 0) ? lowValueColor : (percentage <= 0.35f ? lowValueColor : normalColor);
        }
    }

    /// <summary>
    /// 根据玩家状态生成状态文本
    /// </summary>
    private string GetPlayerStatusText(PlayerController player)
    {
        if (player.isDead)
            return "死亡";
        if (player.isKnockedDown)
        {
            int remainingTime = Mathf.CeilToInt(player.attribute.knockdownHealth);
            if (player.rescueTarget != null)
            {
                return $"<color=red>倒地</color> - 正在被救援";
            }
            return $"<color=red>受伤倒地</color>，距离死亡还有<color=red>{remainingTime}</color>秒";
        }
        if (player.isInteracting)
            return player.currentInteractionTarget != null ? $"与 {player.currentInteractionTarget.name} 交互中" : "交互中";
        if (player.isCarrying)
            return player.currentCarriedItem != null ? $"携带 {player.currentCarriedItem.name}" : "携带物品";
        if (player.isPreparingSkill)
            return "准备技能";
        if (player.isUsingSkill)
            return "使用技能";
        if (player.isMoving)
            return "移动中";
      
        if (player.isObstaclecanhit)
        {
            string targetName = player.manualTarget != null ? player.manualTarget.name : "未知目标";
            return $"正在主动攻击 - {targetName}";
        }
        if (player.animator.GetBool("isShooting"))
            return "战斗中";
        if (Mathf.Approximately(player.energy, 0))
            return "<color=red>弹药耗尽</color>";
        return "待命中";
    }

    /// <summary>
    /// 辅助方法：在指定 parent 下查找子对象并获取其组件
    /// </summary>
    private T GetChildComponent<T>(Transform parent, string childName) where T : Component
    {
        Transform child = parent.Find(childName);
        if (child != null)
            return child.GetComponent<T>();
        return null;
    }

    #endregion

    #region 玩家头像框管理

    /// <summary>
    /// 创建并添加玩家的头像框 UI
    /// </summary>
    private void AddPlayerUI(PlayerController player)
    {
        if (players.Contains(player) || playerFrames.ContainsKey(player))
            return;

        players.Add(player);
        // 按玩家的 ID 排序
        players.Sort((a, b) => a.symbol.ID.CompareTo(b.symbol.ID));

        GameObject frame = Instantiate(playerFramePrefab, frameParent);
        frame.name = $"PlayerFrame_{player.symbol.ID}";
        playerFrames[player] = frame;

        // 初始化头像框内按钮
        Transform content = frame.transform.Find("Content");
        if (content != null)
        {
            Button button = content.GetComponent<Button>();
            if (button != null)
            {
                PlayerButtonHandler buttonHandler = button.GetComponent<PlayerButtonHandler>();
                if (buttonHandler == null)
                {
                    buttonHandler = button.gameObject.AddComponent<PlayerButtonHandler>();
                }
                buttonHandler.Initialize(playerManager, player);
                Debug.Log($"为 {frame.name} 设置 PlayerButtonHandler（玩家索引：{players.IndexOf(player)}）");
            }
            else
            {
                Debug.LogError($"未在 {frame.name} 的 Content 上找到 Button 组件。");
            }
        }
        else
        {
            Debug.LogError($"未在 {frame.name} 中找到 Content 子对象。");
        }

        UpdateSpacing();
    }

    /// <summary>
    /// 移除指定玩家的头像框 UI
    /// </summary>
    private void RemovePlayerUI(PlayerController player)
    {
        if (!players.Contains(player))
            return;

        players.Remove(player);
        players.Sort((a, b) => a.symbol.ID.CompareTo(b.symbol.ID));

        if (playerFrames.TryGetValue(player, out GameObject frame))
        {
            Destroy(frame);
            playerFrames.Remove(player);
            UpdateSpacing();
        }
    }

    /// <summary>
    /// 根据当前玩家数量更新头像框间隔
    /// </summary>
    private void UpdateSpacing()
    {
        if (layoutGroup != null)
        {
            int playerCount = players.Count;
            layoutGroup.spacing = CalculateSpacing(playerCount);
        }
    }

    /// <summary>
    /// 根据玩家数量计算间隔公式
    /// </summary>
    private float CalculateSpacing(int playerCount)
    {
        return -4.896f * Mathf.Pow(playerCount, 2) + 113.202f * playerCount - 575f;
    }

    #endregion

    #region 选择与高亮相关

    public void SelectPlayer(PlayerController player)
    {
        ShowCheckSymbol(player);
    }

    public void UnselectPlayer(PlayerController player)
    {
        HideCheckSymbol(player);
    }

    public void HightlightPlayer(PlayerController player)
    {
        ShowSkillBar(player);
    }

    public void UnhightlightPlayer(PlayerController player)
    {
        HideSkillBar(player);
    }

    public void SelectActionTarget(GameObject target)
    {
        Outline outline = target.GetComponentInChildren<Outline>();
        if (outline != null)
        {
            outline.enabled = true;
            outline.OutlineWidth = 1.5f;
            Color newColor = outline.OutlineColor;
            newColor.a = 0.65f;
            outline.OutlineColor = newColor;
        }
    }

    public void UnselectActionTarget(GameObject target)
    {
        Outline outline = target.GetComponentInChildren<Outline>();
        if (outline != null)
        {
            outline.OutlineWidth = 1f;
            Color newColor = outline.OutlineColor;
            newColor.a = 0.35f;
            outline.OutlineColor = newColor;
        }
    }

    public void SelectTargetPosition(Vector3 position)
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.blue;
            lineRenderer.endColor = Color.blue;
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.useWorldSpace = true;
        }
        lineRenderer.positionCount = 5;
        Vector2 colliderSize = new Vector2(1.0f, 1.0f);
        Vector3[] borderPoints = new Vector3[5];
        borderPoints[0] = new Vector3(-colliderSize.x / 2, colliderSize.y / 2, 0) + position;
        borderPoints[1] = new Vector3(colliderSize.x / 2, colliderSize.y / 2, 0) + position;
        borderPoints[2] = new Vector3(colliderSize.x / 2, -colliderSize.y / 2, 0) + position;
        borderPoints[3] = new Vector3(-colliderSize.x / 2, -colliderSize.y / 2, 0) + position;
        borderPoints[4] = borderPoints[0];
        lineRenderer.SetPositions(borderPoints);
    }

    public void UnselectTargetPosition(Vector3 position)
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
    }

    public void HideCheckSymbol(PlayerController player)
    {
        Transform checkSymbol = player.transform.Find("CheckSymbol");
        if (checkSymbol != null)
        {
            checkSymbol.gameObject.SetActive(false);
        }
    }

    public void ShowCheckSymbol(PlayerController player)
    {
        Transform checkSymbol = player.transform.Find("CheckSymbol");
        if (checkSymbol != null)
        {
            checkSymbol.gameObject.SetActive(true);
        }
    }

    public void HideSkillBar(PlayerController player)
    {
        var skillBar = player.transform.Find("SkillBar");
        if (skillBar != null)
        {
            // 关闭 UI 渲染，但保持 GameObject 活跃
            var canvas = skillBar.GetComponent<Canvas>();
            if (canvas != null)
                canvas.enabled = false;
            else if (skillBar.TryGetComponent<CanvasGroup>(out var cg))
                cg.alpha = 0f;
        }
    }

    public void ShowSkillBar(PlayerController player)
    {
        var skillBar = player.transform.Find("SkillBar");
        if (skillBar != null)
        {
            // 恢复 UI 渲染
            var canvas = skillBar.GetComponent<Canvas>();
            if (canvas != null)
                canvas.enabled = true;
            else if (skillBar.TryGetComponent<CanvasGroup>(out var cg))
                cg.alpha = 1f;

            // 同步冷却状态到滑条
            if (player.skillCooldownManager != null)
                player.skillCooldownManager.SyncCooldown();
        }
    }


    public void ClearAllHighlights()
    {
        foreach (var kvp in playerFrames)
        {
            Transform checkSymbol = kvp.Key.transform.Find("CheckSymbol");
            if (checkSymbol != null)
            {
                checkSymbol.gameObject.SetActive(false);
            }

            Transform backImage = kvp.Value.transform.Find("Content/BackImage");
            if (backImage != null)
            {
                Image backImageComponent = backImage.GetComponent<Image>();
                backImageComponent.DOKill();
                backImageComponent.DOFade(0, 0.2f);
            }
        }
    }

    public void HighlightSelectedPlayers(List<PlayerController> selectedPlayers, int currentSkillBarIndex)
    {
        // 遍历所有玩家头像框，更新 BackImage 和 CheckSymbol
        foreach (var kvp in playerFrames)
        {
            RectTransform contentRect = kvp.Value.transform.Find("Content").GetComponent<RectTransform>();
            Transform backImage = contentRect.Find("BackImage");
            Transform checkSymbol = kvp.Key.transform.Find("CheckSymbol");

            // 记录原始状态
            if (!originalStates.ContainsKey(contentRect))
            {
                originalStates[contentRect] = (contentRect.anchoredPosition, contentRect.localScale);
            }

            contentRect.DOKill();
            contentRect.DOScale(originalStates[contentRect].originalScale, 0.2f).SetEase(Ease.OutQuad);
            contentRect.DOAnchorPos(originalStates[contentRect].originalPosition, 0.2f).SetEase(Ease.OutQuad);

            if (backImage != null)
            {
                Image backImageComponent = backImage.GetComponent<Image>();
                backImageComponent.DOKill();
                if (selectedPlayers.Contains(kvp.Key))
                {
                    backImageComponent.DOFade(1, 0.2f);
                }
                else
                {
                    backImageComponent.DOFade(0, 0.2f);
                }
            }
            if (checkSymbol != null)
            {
                checkSymbol.gameObject.SetActive(selectedPlayers.IndexOf(kvp.Key) == currentSkillBarIndex);
            }
        }
        // 对选中的玩家头像框执行缩放和位置调整
        foreach (PlayerController player in selectedPlayers)
        {
            if (playerFrames.TryGetValue(player, out GameObject frame))
            {
                RectTransform contentRect = frame.transform.Find("Content").GetComponent<RectTransform>();
                contentRect.DOKill();
                if (selectedPlayers.Count == 1)
                {
                    contentRect.DOScale(1.1f, 0.2f).SetEase(Ease.OutQuad);
                    contentRect.DOAnchorPos(new Vector2(-5, 20), 0.2f).SetEase(Ease.OutQuad);
                }
                else
                {
                    if (selectedPlayers.IndexOf(player) == currentSkillBarIndex)
                    {
                        contentRect.DOScale(1.1f, 0.2f).SetEase(Ease.OutQuad);
                        contentRect.DOAnchorPos(new Vector2(-5, 35), 0.1f).SetEase(Ease.OutQuad);
                    }
                    else
                    {
                        contentRect.DOScale(1.1f, 0.2f).SetEase(Ease.OutQuad);
                        contentRect.DOAnchorPos(new Vector2(-5, 20), 0.2f).SetEase(Ease.OutQuad);
                    }
                }
            }
        }
    }

    #endregion

    #region 玩家受击与死亡反馈

    /// <summary>
    /// 处理玩家死亡后的头像框动画
    /// </summary>
    private void HandlePlayerDeath(PlayerController player)
    {
        if (playerFrames.TryGetValue(player, out GameObject frame))
        {
            Image playerImage = GetChildComponent<Image>(frame.transform.Find("Content"), "PlayerImage");
            CanvasGroup canvasGroup = frame.GetComponent<CanvasGroup>() ?? frame.AddComponent<CanvasGroup>();

            playerImage.DOColor(Color.gray, 0.2f);
            frame.transform.DOShakePosition(1f, new Vector3(10, 10, 0), 15, 120)
                .OnComplete(() =>
                {
                    canvasGroup.DOFade(0, 1f).OnComplete(() =>
                    {
                        playerFrames.Remove(player);
                        Destroy(frame);
                    });
                });
        }
        else
        {
            Debug.LogWarning($"移除玩家 {player.name} 的UI时未找到对应头像框。");
        }
    }

    /// <summary>
    /// 处理玩家受击反馈：头像框闪红、抖动，头像颜色闪红后恢复
    /// </summary>
    private void HandlePlayerHit(PlayerController player)
    {
        if (playerFrames.TryGetValue(player, out GameObject frame))
        {
            Transform content = frame.transform.Find("Content");
            if (content != null)
            {
                // 可在此处添加 BackImage 的高亮闪烁动画

                // 玩家头像颜色闪红后恢复
                Image playerImage = GetChildComponent<Image>(content, "PlayerImage");
                playerImage.DOKill();
                playerImage.color = new Color(1f, 0f, 0f, 1f);
                playerImage.DOColor(Color.white, 0.5f).SetEase(Ease.OutQuad);

                // Content 抖动效果
                RectTransform contentRect = content.GetComponent<RectTransform>();
                if (contentRect != null)
                {
                    contentRect.DOKill();
                    contentRect.DOShakeAnchorPos(0.5f, new Vector2(5, 5), 10, 90, false);
                }
            }
        }
    }
    private string GetColoredLevelText(int level)
    {
        string colorHex = GetColorHexForLevel(level);
        return $"等级<color={colorHex}>{level}</color>";
    }

    private string GetColorHexForLevel(int level)
    {
        switch (level)
        {
            case 0: return "#FFFFFF";   // 灰 - 新兵
            case 1: return "#00AAFF";   // 蓝 - 正式干员
            case 2: return "#FFD700";   // 金 - 高级干员
            case 3: return "#FF6C00";   // 橙 - 精英干员
            default: return "#FFFFFF";  // 默认白
        }
    }

    #endregion
}
