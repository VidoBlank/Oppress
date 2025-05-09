using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UpgradeAvatarSlot : MonoBehaviour
{
    public Image avatarImage;
    public TextMeshProUGUI nameText;
    // ��ʾ��ɫ�ȼ����ı����
    public TextMeshProUGUI levelText;

    private GameObject unitInstance;
    private Button button;

    // ���ʱ�Ļص������ݵ�ǰ�󶨵Ľ�ɫʵ���������� null ʱ��ʾ�ر���ϸҳ��
    private System.Action<GameObject> onClickedCallback;

    // ���Ų���
    public float selectedScale = 1.2f;    // ѡ��ʱ�ķŴ���
    public float deselectedScale = 1.0f;  // Ĭ������
    public float scaleDuration = 0.2f;    // ��������ʱ��

    // ��̬���������ڼ�¼��ǰѡ�еĲۣ�ȷ����ѡЧ��
    private static UpgradeAvatarSlot currentSelectedSlot = null;

    // ����Ƿ���ѡ��״̬
    public bool isSelected = false;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            Debug.Log("Button ����ҵ����� OnClick �¼���");
            button.onClick.AddListener(OnClick);
        }
        else
        {
            Debug.LogWarning("UpgradeAvatarSlot δ�ҵ� Button ���");
        }
    }

    /// <summary>
    /// �󶨽�ɫ���ݺ͵���ص�
    /// </summary>
    public void Setup(GameObject unit, System.Action<GameObject> onClicked)
    {
        unitInstance = unit;
        onClickedCallback = onClicked;

        // �� PlayerController �л�ȡͷ�����ƺ͵ȼ�����
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
            Debug.LogError($"��ɫ {unit.name} δ�ҵ� PlayerController �����");
        }
    }

    /// <summary>
    /// �������Ҫ����ص���Ҳ��ֱ�ӵ����������
    /// </summary>
    public void Setup(GameObject unit)
    {
        Setup(unit, null);
    }

    /// <summary>
    /// ��ť���ʱ���õķ���
    /// </summary>
    private void OnClick()
    {
        Debug.Log("UpgradeAvatarSlot clicked for unit: " + (unitInstance != null ? unitInstance.name : "null"));
        if (isSelected)
        {
            // �Ѿ�ѡ�У��ٴε����ȡ��ѡ�У���֪ͨ�ϲ�ر���ϸҳ��
            Deselect();
            onClickedCallback?.Invoke(null);
        }
        else
        {
            // ��ǰδѡ�У���ѡ�У�ͬʱȡ������ѡ�еĲ�
            Select();
            onClickedCallback?.Invoke(unitInstance);
        }
    }

    /// <summary>
    /// ѡ�е�ǰ�ۣ����Ŵ���ʾ������Ч������ͬʱȡ�������۵�ѡ��״̬
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
    /// ȡ����ǰ�۵�ѡ��״̬���ָ�ԭʼ����
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
        return $"�ȼ�<color={color}>{level}</color> ";
    }

    private string GetRankTitle(int level)
    {
        switch (level)
        {
            case 0: return "�±�";
            case 1: return "��ʽ��Ա";
            case 2: return "�߼���Ա";
            case 3: return "��Ӣ��Ա";
            default: return "δ֪";
        }
    }

    private string GetRankColorHex(int level)
    {
        switch (level)
        {
            case 0: return "#FFFFFF"; // ��ɫ
            case 1: return "#00AAFF"; // ��ɫ
            case 2: return "#FFD700"; // ��ɫ
            case 3: return "#FF6C00"; // ��ɫ
            default: return "#FFFFFF";
        }
    }
    /// <summary>
    /// ÿ֡���½�ɫ�ȼ���ʾ
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
