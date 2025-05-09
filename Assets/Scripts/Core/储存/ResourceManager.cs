using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using System.IO;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    public int Coins { get; private set; } // ��ǰ�����

    [Header("UI ��ʾ")]
    public TextMeshProUGUI resourceText; // ������ʾ��Դ���� TMP �ı�
    public string resourcePrefix = "Coins��"; // �ı�ǰ׺

    [Header("DOTween ����")]
    public float animationDuration = 0.5f; // UI ���¶���ʱ��

    private const string SaveFilePath = "ResourcesData.json"; // ���ݱ���·��

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadResources(); // ȷ���ڶ����ʼ��ʱ������Դ����
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateResourceUI(); // ���� UI
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
    /// �����������ʱ���²��� UI
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"����������ɣ�{scene.name}");

        // �����µ���Դ�ı�����
        resourceText = GameObject.Find("ResourceText")?.GetComponent<TextMeshProUGUI>();
        if (resourceText == null)
        {
            Debug.LogWarning("δ�ҵ� ResourceText�����鳡���е� UI ���ã�");
        }
        else
        {
            UpdateResourceUI(); // ���� UI ��ʾ
        }
    }

    /// <summary>
    /// ��ʼ����Դ
    /// </summary>
    public void InitializeResources()
    {
        if (Coins > 0)
        {
            Debug.Log("��⵽������Դ���ݣ�������ʼ����");
            return;
        }

        Coins = 0; // Ĭ�Ͻ����
        SaveResources(); // �����ʼ����
        UpdateResourceUI(); // ���� UI
        Debug.Log($"��Դ��ʼ����ɣ���ǰ���������{Coins}");
    }

    public void ResetResources()
    {
        Coins = 0; // Ĭ�Ͻ����
        SaveResources(); // �����ʼ����
        UpdateResourceUI(); // ���� UI
        Debug.Log($"��Դ��ʼ����ɣ���ǰ���������{Coins}");
    }

    /// <summary>
    /// ������Դ
    /// </summary>
    /// <param name="amount">��Ҫ���ĵ���Դ��</param>
    /// <returns>�Ƿ�ɹ�������Դ</returns>
    public bool SpendResources(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("������Դ����������Ϊ������");
            return false;
        }

        if (Coins >= amount)
        {
            Coins -= amount;
            SaveResources(); // ������Դ����
            UpdateResourceUI(); // ���� UI
            Debug.Log($"������Դ��{amount}��ʣ�ࣺ{Coins}");
            return true;
        }
        Debug.Log("��Դ���㣡");
        return false;
    }

    /// <summary>
    /// ������Դ
    /// </summary>
    /// <param name="amount">���ӵ���Դ��</param>
    public void AddResources(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("������Դ����������Ϊ������");
            return;
        }

        Coins += amount;
        SaveResources(); // ������Դ����
        UpdateResourceUI(); // ���� UI
        Debug.Log($"�����Դ��{amount}����ǰ������{Coins}");
    }

    /// <summary>
    /// ������Դ UI
    /// </summary>
    private void UpdateResourceUI()
    {
        if (resourceText != null)
        {
            // �������� UI �ı��������ǰ׺
            DOTween.To(() => Coins, x =>
            {
                resourceText.text = $"{resourcePrefix}{x}";
            }, Coins, animationDuration)
            .OnComplete(() => Debug.Log("��Դ UI ������ɣ�"));
        }
        else
        {
            Debug.LogWarning("Resource Text δ���ã�");
        }
    }

    /// <summary>
    /// ������Դ���ݵ� JSON �ļ�
    /// </summary>
    private void SaveResources()
    {
        try
        {
            string json = JsonUtility.ToJson(new ResourceData { Coins = this.Coins }, true);
            string fullPath = Path.Combine(Application.persistentDataPath, SaveFilePath);

            File.WriteAllText(fullPath, json);
            Debug.Log($"��Դ�����ѱ��浽 {fullPath}: {json}");
        }
        catch (IOException ex)
        {
            Debug.LogError($"������Դ����ʱ��������{ex.Message}");
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
                Debug.Log($"��Դ�����Ѽ��أ���ǰ���������{Coins} �����ļ���{fullPath}");
            }
            else
            {
                Debug.LogWarning($"û���ҵ������ļ���{fullPath}����ȷ���Ƿ���Ҫ��ʼ����Դ��");
            }
        }
        catch (IOException ex)
        {
            Debug.LogError($"������Դ����ʱ��������{ex.Message}");
        }
    }

    public void UpdateCoinsInSave()
    {
        // Implement saving coin data from other parts if needed
    }

    public void LoadResourcesFromSave(int savedCoins)
    {
        Coins = savedCoins; // ʹ�ô浵�е���Դ��
        UpdateResourceUI();       // ���� UI ��ʾ
    }

    public void SetResources(int amount)
    {
        Coins = amount;
        SaveResources(); // ��������
        UpdateResourceUI(); // ���� UI
        Debug.Log($"��Դ������Ϊ��{Coins}");
    }

    [System.Serializable]
    private class ResourceData
    {
        public int Coins;
    }
}
