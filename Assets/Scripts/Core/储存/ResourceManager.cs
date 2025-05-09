using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using System.IO;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    public int Coins { get; private set; } // 当前金币量

    [Header("UI 显示")]
    public TextMeshProUGUI resourceText; // 用于显示资源量的 TMP 文本
    public string resourcePrefix = "Coins："; // 文本前缀

    [Header("DOTween 动画")]
    public float animationDuration = 0.5f; // UI 更新动画时长

    private const string SaveFilePath = "ResourcesData.json"; // 数据保存路径

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadResources(); // 确保在对象初始化时加载资源数据
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateResourceUI(); // 更新 UI
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 场景加载完成时重新查找 UI
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"场景加载完成：{scene.name}");

        // 查找新的资源文本对象
        resourceText = GameObject.Find("ResourceText")?.GetComponent<TextMeshProUGUI>();
        if (resourceText == null)
        {
            Debug.LogWarning("未找到 ResourceText，请检查场景中的 UI 设置！");
        }
        else
        {
            UpdateResourceUI(); // 更新 UI 显示
        }
    }

    /// <summary>
    /// 初始化资源
    /// </summary>
    public void InitializeResources()
    {
        if (Coins > 0)
        {
            Debug.Log("检测到现有资源数据，跳过初始化！");
            return;
        }

        Coins = 0; // 默认金币量
        SaveResources(); // 保存初始数据
        UpdateResourceUI(); // 更新 UI
        Debug.Log($"资源初始化完成，当前金币数量：{Coins}");
    }

    public void ResetResources()
    {
        Coins = 0; // 默认金币量
        SaveResources(); // 保存初始数据
        UpdateResourceUI(); // 更新 UI
        Debug.Log($"资源初始化完成，当前金币数量：{Coins}");
    }

    /// <summary>
    /// 消耗资源
    /// </summary>
    /// <param name="amount">需要消耗的资源量</param>
    /// <returns>是否成功消耗资源</returns>
    public bool SpendResources(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("消耗资源的数量不能为负数！");
            return false;
        }

        if (Coins >= amount)
        {
            Coins -= amount;
            SaveResources(); // 保存资源数据
            UpdateResourceUI(); // 更新 UI
            Debug.Log($"消耗资源：{amount}，剩余：{Coins}");
            return true;
        }
        Debug.Log("资源不足！");
        return false;
    }

    /// <summary>
    /// 增加资源
    /// </summary>
    /// <param name="amount">增加的资源量</param>
    public void AddResources(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("增加资源的数量不能为负数！");
            return;
        }

        Coins += amount;
        SaveResources(); // 保存资源数据
        UpdateResourceUI(); // 更新 UI
        Debug.Log($"获得资源：{amount}，当前总量：{Coins}");
    }

    /// <summary>
    /// 更新资源 UI
    /// </summary>
    private void UpdateResourceUI()
    {
        if (resourceText != null)
        {
            // 动画更新 UI 文本，并添加前缀
            DOTween.To(() => Coins, x =>
            {
                resourceText.text = $"{resourcePrefix}{x}";
            }, Coins, animationDuration)
            .OnComplete(() => Debug.Log("资源 UI 更新完成！"));
        }
        else
        {
            Debug.LogWarning("Resource Text 未设置！");
        }
    }

    /// <summary>
    /// 保存资源数据到 JSON 文件
    /// </summary>
    private void SaveResources()
    {
        try
        {
            string json = JsonUtility.ToJson(new ResourceData { Coins = this.Coins }, true);
            string fullPath = Path.Combine(Application.persistentDataPath, SaveFilePath);

            File.WriteAllText(fullPath, json);
            Debug.Log($"资源数据已保存到 {fullPath}: {json}");
        }
        catch (IOException ex)
        {
            Debug.LogError($"保存资源数据时发生错误：{ex.Message}");
        }
    }

    private void LoadResources()
    {
        try
        {
            string fullPath = Path.Combine(Application.persistentDataPath, SaveFilePath);

            if (File.Exists(fullPath))
            {
                string json = File.ReadAllText(fullPath);
                ResourceData data = JsonUtility.FromJson<ResourceData>(json);
                this.Coins = data.Coins;
                Debug.Log($"资源数据已加载，当前金币数量：{Coins} 来自文件：{fullPath}");
            }
            else
            {
                Debug.LogWarning($"没有找到保存文件：{fullPath}。请确认是否需要初始化资源。");
            }
        }
        catch (IOException ex)
        {
            Debug.LogError($"加载资源数据时发生错误：{ex.Message}");
        }
    }

    public void UpdateCoinsInSave()
    {
        // Implement saving coin data from other parts if needed
    }

    public void LoadResourcesFromSave(int savedCoins)
    {
        Coins = savedCoins; // 使用存档中的资源量
        UpdateResourceUI();       // 更新 UI 显示
    }

    public void SetResources(int amount)
    {
        Coins = amount;
        SaveResources(); // 保存数据
        UpdateResourceUI(); // 更新 UI
        Debug.Log($"资源已设置为：{Coins}");
    }

    [System.Serializable]
    private class ResourceData
    {
        public int Coins;
    }
}
