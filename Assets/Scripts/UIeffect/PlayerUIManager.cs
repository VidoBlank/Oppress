using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

public class PlayerUIManager : MonoBehaviour
{
    #region �ֶ�����

    [Header("ͷ���Ԥ����")]
    public GameObject playerFramePrefab;
    [Header("UI����")]
    public Transform frameParent;
    [Header("�������")]
    public HorizontalLayoutGroup layoutGroup;

    private PlayerManager playerManager;
    private List<PlayerController> players = new List<PlayerController>();

    // �洢ÿ����Ҷ�Ӧ��ͷ���
    public Dictionary<PlayerController, GameObject> playerFrames = new Dictionary<PlayerController, GameObject>();

    // ��¼ Content ��ԭʼλ�ú����ţ����ڸ���������ԭ
    private Dictionary<RectTransform, (Vector2 originalPosition, Vector3 originalScale)> originalStates = new Dictionary<RectTransform, (Vector2, Vector3)>();

    private int playerFrameCounter = 0; // ͷ������

    #endregion

    #region �������ڷ���

    private void Awake()
    {
        DOTween.SetTweensCapacity(2000, 100);
        playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager == null)
        {
            Debug.LogError("PlayerManager δ�ҵ�����ȷ����������һ������ PlayerManager �ű��Ķ���");
        }
    }

    private void Start()
    {
        // ���� PlayerManager �е�����б���ͷ���
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
        // �����������ͷ����״̬
        foreach (var player in players)
        {
            UpdatePlayerFrameUI(player);
        }
    }

    private void OnEnable()
    {
        PlayerController.OnPlayerAdded += AddPlayerUI;
        PlayerController.OnPlayerRemoved += RemovePlayerUI;

        // ���ĳ�����������ҵ��������ܻ��¼�
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

    #region UI ���·���

    /// <summary>
    /// ����ָ�����ͷ���� UI ��Ϣ
    /// </summary>
    private void UpdatePlayerFrameUI(PlayerController player)
    {
        if (!playerFrames.TryGetValue(player, out GameObject frame))
        {
            Debug.LogWarning($"δ�ҵ���� {player.name} ��UIͷ���");
            return;
        }

        // ���� Content �������Ӷ���
        Transform content = frame.transform.Find("Content");
        if (content == null)
        {
            Debug.LogError("PlayerFrame ��δ�ҵ� Content �Ӷ���");
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

        // ����ͷ��ͼƬ
        if (player.symbol != null)
        {
            playerImageComponent.sprite = player.symbol.playerImage;
        }

        // ����Ѫ���͵�ҩ�ٷֱȣ�ʹ�� Tween ƽ������
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

        // ����״̬�ı�
        if (statusText != null)
        {
            statusText.text = GetPlayerStatusText(player);
        }

        // ����ҵ���ʱ��ͷ���밵��ɫ������ָ�Ϊ��ɫ
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


        // ���� BackImage ͸���ȣ�ѡ��ʱ��ʾ������͸��
        if (backImageComponent != null && !playerManager.selectedPlayers.Contains(player))
        {
            Color col = backImageComponent.color;
            backImageComponent.color = new Color(col.r, col.g, col.b, 0);
        }
    }

    /// <summary>
    /// ���� Slider �ڲ�������ֵͼƬ����ɫ
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
            // ����ҩΪ 0����ֱ����Ϊ��ɫ
            valueImage.color = Mathf.Approximately(percentage, 0) ? lowValueColor : (percentage <= 0.35f ? lowValueColor : normalColor);
        }
    }

    /// <summary>
    /// �������״̬����״̬�ı�
    /// </summary>
    private string GetPlayerStatusText(PlayerController player)
    {
        if (player.isDead)
            return "����";
        if (player.isKnockedDown)
        {
            int remainingTime = Mathf.CeilToInt(player.attribute.knockdownHealth);
            if (player.rescueTarget != null)
            {
                return $"<color=red>����</color> - ���ڱ���Ԯ";
            }
            return $"<color=red>���˵���</color>��������������<color=red>{remainingTime}</color>��";
        }
        if (player.isInteracting)
            return player.currentInteractionTarget != null ? $"�� {player.currentInteractionTarget.name} ������" : "������";
        if (player.isCarrying)
            return player.currentCarriedItem != null ? $"Я�� {player.currentCarriedItem.name}" : "Я����Ʒ";
        if (player.isPreparingSkill)
            return "׼������";
        if (player.isUsingSkill)
            return "ʹ�ü���";
        if (player.isMoving)
            return "�ƶ���";
      
        if (player.isObstaclecanhit)
        {
            string targetName = player.manualTarget != null ? player.manualTarget.name : "δ֪Ŀ��";
            return $"������������ - {targetName}";
        }
        if (player.animator.GetBool("isShooting"))
            return "ս����";
        if (Mathf.Approximately(player.energy, 0))
            return "<color=red>��ҩ�ľ�</color>";
        return "������";
    }

    /// <summary>
    /// ������������ָ�� parent �²����Ӷ��󲢻�ȡ�����
    /// </summary>
    private T GetChildComponent<T>(Transform parent, string childName) where T : Component
    {
        Transform child = parent.Find(childName);
        if (child != null)
            return child.GetComponent<T>();
        return null;
    }

    #endregion

    #region ���ͷ������

    /// <summary>
    /// �����������ҵ�ͷ��� UI
    /// </summary>
    private void AddPlayerUI(PlayerController player)
    {
        if (players.Contains(player) || playerFrames.ContainsKey(player))
            return;

        players.Add(player);
        // ����ҵ� ID ����
        players.Sort((a, b) => a.symbol.ID.CompareTo(b.symbol.ID));

        GameObject frame = Instantiate(playerFramePrefab, frameParent);
        frame.name = $"PlayerFrame_{player.symbol.ID}";
        playerFrames[player] = frame;

        // ��ʼ��ͷ����ڰ�ť
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
                Debug.Log($"Ϊ {frame.name} ���� PlayerButtonHandler�����������{players.IndexOf(player)}��");
            }
            else
            {
                Debug.LogError($"δ�� {frame.name} �� Content ���ҵ� Button �����");
            }
        }
        else
        {
            Debug.LogError($"δ�� {frame.name} ���ҵ� Content �Ӷ���");
        }

        UpdateSpacing();
    }

    /// <summary>
    /// �Ƴ�ָ����ҵ�ͷ��� UI
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
    /// ���ݵ�ǰ�����������ͷ�����
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
    /// �������������������ʽ
    /// </summary>
    private float CalculateSpacing(int playerCount)
    {
        return -4.896f * Mathf.Pow(playerCount, 2) + 113.202f * playerCount - 575f;
    }

    #endregion

    #region ѡ����������

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
            // �ر� UI ��Ⱦ�������� GameObject ��Ծ
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
            // �ָ� UI ��Ⱦ
            var canvas = skillBar.GetComponent<Canvas>();
            if (canvas != null)
                canvas.enabled = true;
            else if (skillBar.TryGetComponent<CanvasGroup>(out var cg))
                cg.alpha = 1f;

            // ͬ����ȴ״̬������
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
        // �����������ͷ��򣬸��� BackImage �� CheckSymbol
        foreach (var kvp in playerFrames)
        {
            RectTransform contentRect = kvp.Value.transform.Find("Content").GetComponent<RectTransform>();
            Transform backImage = contentRect.Find("BackImage");
            Transform checkSymbol = kvp.Key.transform.Find("CheckSymbol");

            // ��¼ԭʼ״̬
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
        // ��ѡ�е����ͷ���ִ�����ź�λ�õ���
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

    #region ����ܻ�����������

    /// <summary>
    /// ��������������ͷ��򶯻�
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
            Debug.LogWarning($"�Ƴ���� {player.name} ��UIʱδ�ҵ���Ӧͷ���");
        }
    }

    /// <summary>
    /// ��������ܻ�������ͷ������졢������ͷ����ɫ�����ָ�
    /// </summary>
    private void HandlePlayerHit(PlayerController player)
    {
        if (playerFrames.TryGetValue(player, out GameObject frame))
        {
            Transform content = frame.transform.Find("Content");
            if (content != null)
            {
                // ���ڴ˴���� BackImage �ĸ�����˸����

                // ���ͷ����ɫ�����ָ�
                Image playerImage = GetChildComponent<Image>(content, "PlayerImage");
                playerImage.DOKill();
                playerImage.color = new Color(1f, 0f, 0f, 1f);
                playerImage.DOColor(Color.white, 0.5f).SetEase(Ease.OutQuad);

                // Content ����Ч��
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
        return $"�ȼ�<color={colorHex}>{level}</color>";
    }

    private string GetColorHexForLevel(int level)
    {
        switch (level)
        {
            case 0: return "#FFFFFF";   // �� - �±�
            case 1: return "#00AAFF";   // �� - ��ʽ��Ա
            case 2: return "#FFD700";   // �� - �߼���Ա
            case 3: return "#FF6C00";   // �� - ��Ӣ��Ա
            default: return "#FFFFFF";  // Ĭ�ϰ�
        }
    }

    #endregion
}
