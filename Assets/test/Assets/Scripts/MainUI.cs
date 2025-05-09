using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    public static MainUI Instance { get; private set; }

    public Button Button_AddCoin;
    public Button Button_RemoveCoin;
    public TextMeshProUGUI Text_Coin;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Button_AddCoin.onClick.AddListener(AddRP);
        Button_RemoveCoin.onClick.AddListener(RemoveRP);

        // ✅ 注册 RP 改变事件
        GameData.Instance.OnRPChanged += HandleRPChanged;

        // ✅ 初始刷新
        HandleRPChanged(GameData.Instance.RP);
    }

    void OnDestroy()
    {
        // ✅ 注销事件监听，避免内存泄露
        if (GameData.Instance != null)
        {
            GameData.Instance.OnRPChanged -= HandleRPChanged;
        }
    }

    void AddRP()
    {
        GameData.Instance.RP++;
        // ❌ 不需要手动调用 UpdateRPDisplay()
    }

    void RemoveRP()
    {
        GameData.Instance.RP--;
    }

    // ✅ 响应 RP 改变
    private void HandleRPChanged(int newValue)
    {
        UpdateRPDisplay();
    }

    public void UpdateRPDisplay()
    {
        Text_Coin.text = $"资源点(RP): {GameData.Instance.RP}";
    }
}
