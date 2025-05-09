using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class CharacterSlot : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform animatedContainer;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public Image iconImage;
    public Slider healthSlider;
    public Slider energySlider;
    public Button removeButton;
    public RectTransform content;
    public Image playerImage;

    private GameObject unitPrefab;
    private PlayerController playerController;
    private Vector2 originalPosition;
    private bool isSelected = false;
    private bool isAnimating = false;
    private bool hasCapturedOriginalPos = false;

    private readonly Color normalHealthColor = Color.white;
    private readonly Color normalEnergyColor = Color.white;

    private const float LowThreshold = 0.3f;
    private const float JumpHeight = 20f;
    private const float JumpDuration = 0.3f;
    private readonly Color injuredColor = new Color(1f, 0.3f, 0.3f);
    private Tween iconPulseTween;
   
    private Dictionary<Graphic, Color> cachedOriginalColors = null;

    private void Start()
    {
        StartCoroutine(DelayInitialize());
        SetRemoveButtonActive(false);
    }

    private IEnumerator DelayInitialize()
    {
        yield return null;
        originalPosition = Vector2.zero;
        hasCapturedOriginalPos = true;

        LayoutElement le = animatedContainer.GetComponent<LayoutElement>();
        if (le == null)
        {
            le = animatedContainer.gameObject.AddComponent<LayoutElement>();
        }
        le.ignoreLayout = true;
    }

    public GameObject UnitPrefab => unitPrefab;

    public void Setup(GameObject unitInstance)
    {
        unitPrefab = unitInstance;
        playerController = unitInstance.GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError($"❌ PlayerController not found on {unitInstance.name}!");
            return;
        }

        if (!hasCapturedOriginalPos)
        {
            originalPosition = animatedContainer.anchoredPosition;
            hasCapturedOriginalPos = true;
        }

        nameText.text = playerController.symbol != null ? playerController.symbol.Name : playerController.name;
        iconImage.sprite = playerController.symbol.playerImage;

        if (levelText != null)
        {
            levelText.text = GetColoredLevelText(playerController.level);
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = playerController.TotalMaxHealth;
            healthSlider.value = playerController.health;
        }
        if (energySlider != null)
        {
            energySlider.maxValue = playerController.TotalMaxEnergy;
            energySlider.value = playerController.energy;
        }

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnSlotClicked);
        }

        bool isAlreadySelected = PlayerTeamManager.Instance.selectedUnitPrefabs.Contains(unitPrefab);
        SetRemoveButtonActive(isAlreadySelected);

        animatedContainer.anchoredPosition = isAlreadySelected
            ? originalPosition + new Vector2(0, JumpHeight)
            : originalPosition;

        isSelected = isAlreadySelected;
        playerController.OnNameChanged += RefreshNameDisplay;
        RefreshNameDisplay();

    }
    private void OnDestroy()
    {
        if (iconPulseTween != null && iconPulseTween.IsActive()) iconPulseTween.Kill();

        // 清除所有绑定到该 GameObject 的 tween（可选）
        DOTween.Kill(this, true);

        if (playerController != null)
        {
            playerController.OnNameChanged -= RefreshNameDisplay;
        }
    }

    private void Update()
    {
        if (playerController == null) return;

        if (!isAnimating)
        {
            UpdateSlider(healthSlider, playerController.health, playerController.TotalMaxHealth, normalHealthColor);
            UpdateSlider(energySlider, playerController.energy, playerController.TotalMaxEnergy, normalEnergyColor);
        }

        if (levelText != null)
        {
            levelText.text = GetColoredLevelText(playerController.level);
        }

        float healthPercent = playerController.health / playerController.TotalMaxHealth;
        if (healthPercent < LowThreshold)
        {
            if (iconPulseTween == null || !iconPulseTween.IsActive())
            {
                iconImage.color = Color.white;
                iconPulseTween = iconImage.DOColor(injuredColor, 0.4f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }
        else
        {
            if (iconPulseTween != null && iconPulseTween.IsActive())
            {
                iconPulseTween.Kill();
                iconPulseTween = null;
                iconImage.color = Color.white;
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
        return level switch
        {
            0 => "#FFFFFF",
            1 => "#00AAFF",
            2 => "#FFD700",
            3 => "#FF6C00",
            _ => "#FFFFFF"
        };
    }

    private void UpdateSlider(Slider slider, float currentValue, float maxValue, Color normalColor)
    {
        if (slider == null) return;

        slider.maxValue = maxValue;
        slider.value = currentValue;

        Image fillImage = slider.fillRect?.GetComponent<Image>();
        if (fillImage != null)
        {
            float percentage = currentValue / maxValue;
            fillImage.color = percentage < LowThreshold ? Color.red : normalColor;
        }
    }

    public void OnSlotClicked()
    {
        if (unitPrefab == null)
        {
            Debug.LogError("❌ unitPrefab is null, cannot process selection!");
            return;
        }

        var teamManager = PlayerTeamManager.Instance;
        if (teamManager.selectedUnitPrefabs.Contains(unitPrefab))
        {
            teamManager.DeselectCharacter(unitPrefab);
            DeselectAnimation();
            SetRemoveButtonActive(false);
        }
        else
        {
            if (teamManager.selectedUnitPrefabs.Count >= 4)
            {
                Debug.LogWarning("⚠️ Cannot select more than 4 characters!");
                return;
            }

            int prevCount = teamManager.selectedUnitPrefabs.Count;
            teamManager.SelectCharacter(unitPrefab);

            // 判断是否成功添加（数量变化 && 现在在列表中）
            if (teamManager.selectedUnitPrefabs.Count > prevCount &&
                teamManager.selectedUnitPrefabs.Contains(unitPrefab))
            {
                SelectAnimation();
                SetRemoveButtonActive(true);
            }

        }
    }

    public void SelectAnimation()
    {
        if (!isSelected)
        {
            isSelected = true;
            animatedContainer.DOAnchorPos(originalPosition + new Vector2(0, JumpHeight), 0.2f).SetEase(Ease.OutQuad);
        }
    }

    private void DeselectAnimation()
    {
        if (isSelected)
        {
            isSelected = false;
            animatedContainer.DOAnchorPos(originalPosition, 0.2f).SetEase(Ease.OutQuad);
        }
    }

    public void SetRemoveButtonActive(bool active)
    {
        if (removeButton == null) return;

        removeButton.gameObject.SetActive(active);
        if (active)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(() =>
            {
                if (ConfirmDialog.Instance != null)
                {
                    int rpReward = Mathf.Clamp(playerController.level, 1, 3);
                    string content = $"是否回收干员：<color=#00FFFF>{playerController.symbol.Name}</color>\n\n" +
                                     $"回收后将返还 <color=#00FF00>{rpReward} RP</color>";
                    ConfirmDialog.Instance.Show(content, RecycleUnitAndRemove);
                }
            });
        }
    }

    private void RecycleUnitAndRemove()
    {
        if (PlayerTeamManager.Instance == null || unitPrefab == null) return;

        int rpReward = Mathf.Clamp(playerController.level, 1, 3);
        GameData.Instance.RP += rpReward;
        MainUI.Instance.UpdateRPDisplay();
        UIAudioManager.Instance?.PlayRemoveUnit();

        PlayRemoveEffect(() =>
        {
            PlayerTeamManager.Instance.DeleteUnit(unitPrefab);
        });
    }

    public void PlayRecoveryEffect()
    {
        isAnimating = true;
        // 先取消低血量闪红动画（如果存在）
        if (iconPulseTween != null && iconPulseTween.IsActive())
        {
            iconPulseTween.Kill();
            iconPulseTween = null;
        }
        iconImage.color = Color.white; // 恢复正常颜色
        if (healthSlider != null)
        {
            Image healthFill = healthSlider.fillRect?.GetComponent<Image>();
            if (healthFill != null) healthFill.DOColor(Color.green, 0.2f);
        }
        if (energySlider != null)
        {
            Image energyFill = energySlider.fillRect?.GetComponent<Image>();
            if (energyFill != null) energyFill.DOColor(Color.green, 0.2f);
        }

        AnimateJump(content);
        if (healthSlider != null) AnimateJump(healthSlider.GetComponent<RectTransform>());
        if (energySlider != null) AnimateJump(energySlider.GetComponent<RectTransform>());

        if (content != null)
        {
            Image[] images = content.GetComponentsInChildren<Image>(false);
            foreach (Image img in images)
            {
                Color origColor = img.color;
                img.DOColor(Color.green * 1.8f, 0.2f)
                   .SetLoops(2, LoopType.Yoyo)
                   .OnComplete(() => img.color = origColor);
            }
        }

        if (healthSlider != null && playerController != null)
        {
            float targetHealth = playerController.health;
            healthSlider.maxValue = playerController.TotalMaxHealth;
            healthSlider.DOValue(targetHealth, 0.6f).SetEase(Ease.OutQuad);
        }
        if (energySlider != null && playerController != null)
        {
            float targetEnergy = playerController.energy;
            energySlider.maxValue = playerController.TotalMaxEnergy;
            energySlider.DOValue(targetEnergy, 0.6f).SetEase(Ease.OutQuad);
        }

        Invoke(nameof(ResetAnimationFlag), 0.5f);
    }

    private void AnimateJump(RectTransform rt)
    {
        if (rt == null) return;

        Vector2 orig = rt.anchoredPosition;
        Vector2 jumpPos = orig + new Vector2(0, JumpHeight);
        rt.DOAnchorPos(jumpPos, JumpDuration / 2)
          .SetEase(Ease.OutQuad)
          .OnComplete(() => rt.DOAnchorPos(orig, JumpDuration / 2).SetEase(Ease.InQuad));
    }

    private void ResetAnimationFlag()
    {
        isAnimating = false;
    }

    public void PlayRemoveEffect(System.Action onComplete = null)
    {
        if (isAnimating) return;
        isAnimating = true;

        RectTransform rt = GetComponent<RectTransform>();
        CanvasGroup cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        float shakeDuration = 0.25f;
        float fadeDuration = 0.4f;
        float colorDuration = 0.4f;

        if (healthSlider != null) healthSlider.gameObject.SetActive(false);
        if (energySlider != null) energySlider.gameObject.SetActive(false);
        if (levelText != null) levelText.gameObject.SetActive(false);
        if (nameText != null) nameText.gameObject.SetActive(false);

        Image[] images = GetComponentsInChildren<Image>();
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>();

        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOShakeAnchorPos(shakeDuration, new Vector2(25f, 0f), 30, 90).SetEase(Ease.Linear));

        foreach (var img in images) seq.Join(img.DOColor(Color.black, colorDuration));
        foreach (var txt in texts) seq.Join(txt.DOColor(Color.black, colorDuration));

        seq.Join(cg.DOFade(0f, fadeDuration).SetEase(Ease.InOutSine).SetDelay(colorDuration * 0.4f));
        seq.OnComplete(() =>
        {
            onComplete?.Invoke();
            Destroy(gameObject);
        });
    }



    public void RefreshNameDisplay()
    {
        if (playerController != null && nameText != null)
        {
            nameText.text = playerController.symbol != null ? playerController.symbol.Name : playerController.name;
        }
    }
    public void PlaySelectionFailedEffect()
    {
        DOTween.Kill(animatedContainer);
        DOTween.Kill(this);

        isAnimating = true;

        Image[] allImages = GetComponentsInChildren<Image>(true);
        TMP_Text[] allTexts = GetComponentsInChildren<TMP_Text>(true);

        // ✅ 仅在第一次备份颜色
        if (cachedOriginalColors == null)
        {
            cachedOriginalColors = new Dictionary<Graphic, Color>();
            foreach (var g in allImages) cachedOriginalColors[g] = g.color;
            foreach (var t in allTexts) cachedOriginalColors[t] = t.color;
        }

        // ⚡ 瞬间变灰
        Color failColor = Color.gray;
        foreach (var g in allImages) g.color = failColor;
        foreach (var t in allTexts) t.color = failColor;

        float peak = JumpHeight * 0.6f;
        float upTime = 0.08f;
        float downTime = 0.16f;
        Vector2 peakPos = originalPosition + new Vector2(0, peak);

        Sequence seq = DOTween.Sequence();
        seq.Append(animatedContainer.DOAnchorPos(peakPos, upTime).SetEase(Ease.OutCubic));
        seq.Append(animatedContainer.DOAnchorPos(originalPosition, downTime).SetEase(Ease.InBack));

        // ✅ 恢复颜色动画
        seq.AppendCallback(() =>
        {
            foreach (var g in allImages)
                if (cachedOriginalColors.TryGetValue(g, out var color))
                    g.DOColor(color, 0.12f).SetTarget(this);

            foreach (var t in allTexts)
                if (cachedOriginalColors.TryGetValue(t, out var color))
                    t.DOColor(color, 0.12f).SetTarget(this);
        });

        seq.OnComplete(() => isAnimating = false);

        UIAudioManager.Instance?.PlayFail();
    }








}
