using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TalentSlotUI : MonoBehaviour
{
    // ==========�����ֶ�==========
    public Button slotButton;
    public Button upgradeButton;
    public Image selectedIndicator;   // ѡ�к���ʾ�ı�ʶ
    public GameObject introductionPanel;
    public TextMeshProUGUI introductionText;
    [TextArea] public string introductionContent;
    public SlotEffect slotEffect;
    public string slotID;
    public int rowID; // �����б�ţ����� 1��2��3��
    [Header("��Ч")]
    public AudioClip insufficientRPClip; // RP������Ч
    private AudioSource audioSource;

    // �� �������������ĵ� RP ��
    // Ĭ��ֵ�ɸ����м������ã����� 1��=1, 2��=2, 3��=3����Ҳ���� Inspector �е���
    public int upgradeRPCost = 1;

    // �� ������RP������ʾ�ı������� Inspector �а󶨶�Ӧ�� UI �ı���
    public TextMeshProUGUI insufficientRPText;

    // �ص���������ť���ʱ�ش����۶���
    public System.Action<TalentSlotUI> OnUpgradeConfirmed;

    // ==========˽���ֶ�==========
    private bool isOpen = false;     // �Ƿ���չ��
    private bool isUpgraded = false; // �Ƿ��Ѿ������ɹ�

    // ���� DoTween ����
    private RectTransform selectedIndicatorRect;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // �󶨰�ť
        if (slotButton != null)
            slotButton.onClick.AddListener(OnSlotClicked);
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);

        // ��ʼ�� RectTransform ����
        if (selectedIndicator != null)
            selectedIndicatorRect = selectedIndicator.GetComponent<RectTransform>();

        // ���ݵ�ǰ����״̬��ʼ�� UI
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
            Debug.Log($"[{name}] ������״̬�������Ч");
            return;
        }

        // �ر�ͬһ���������Ѵ򿪵Ĳ� UI
        HideOtherSlotUIsInSameRow();

        // �л���ǰ��չ��״̬
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
    /// �ر�ͬһ��������չ���Ĳ���ϸ��Ϣ���
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
            Debug.Log("RP���㣬�޷�������");
            if (insufficientRPText != null)
            {
                insufficientRPText.gameObject.SetActive(true);
                insufficientRPText.text = "��Դ�㲻�㣬�޷�������";
            }

            // ���� RP ������Ч
            if (audioSource != null && insufficientRPClip != null)
            {
                audioSource.PlayOneShot(insufficientRPClip, 0.6f); // �ɵ�������
            }

            return;
        }


        // �� �۳���������� RP
        GameData.Instance.RP -= upgradeRPCost;
        MainUI.Instance.UpdateRPDisplay();
        if (insufficientRPText != null)
            insufficientRPText.gameObject.SetActive(false);

        // ֪ͨ�ⲿ���������߼�
        OnUpgradeConfirmed?.Invoke(this);

        // ���Ϊ������
        isUpgraded = true;

        // ���ؽ�������������ť
        if (introductionPanel != null)
            introductionPanel.SetActive(false);
        if (upgradeButton != null)
            upgradeButton.gameObject.SetActive(false);

        // �ö�����ʾѡ��ָʾ��
        ShowSelectedIndicator(true);

        // ���ò۰�ť����
        if (slotButton != null)
            slotButton.interactable = false;

        Debug.Log($"[{name}] ������������ {upgradeRPCost} RP����ǰ������ȷ��");
    }

    // �ⲿ���ã����ò�Ϊ����ѡ��/������״̬
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
            Debug.Log($"[{name}] SetAsUpgraded => ������״̬");
        }
        else
        {
            HideSelectedIndicator(false);
            Debug.Log($"[{name}] SetAsUpgraded => δ����״̬");
        }
    }

    // ========== չʾ/���� UI ==========
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

        // ��ʾ����������Ϣ
        if (insufficientRPText != null)
        {
            insufficientRPText.gameObject.SetActive(true);
            insufficientRPText.text = $"������Դ�㣺<b><color=#00FFFF>{upgradeRPCost}</color></b>";

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

    // ========== selectedIndicator ���� ==========
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
            Debug.LogWarning($"[{name}] selectedIndicator δ���ã�");
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
