using UnityEngine;
using TMPro;  // 引入TextMeshPro的命名空间
using UnityEngine.SceneManagement; // 引入场景管理命名空间

public class FPSDisplay : MonoBehaviour
{
    public TextMeshProUGUI fpsText; // 用TextMeshProUGUI来显示帧率
    private float deltaTime = 0.0f;

    [Header("帧率显示设置")]
    public bool showFPS = true;  // 在Inspector中可勾选，控制是否显示帧率

    private void Start()
    {
        // 禁用VSync
        QualitySettings.vSyncCount = 1;

        // 设置帧率上限，例如120
        Application.targetFrameRate = 60;

        // 禁用所有Collider
        DisableColliders();
    }

    private void OnEnable()
    {
        // 订阅场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 取消订阅场景加载事件
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (showFPS) // 如果勾选了显示帧率
        {
            // 计算deltaTime用于帧率计算
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;

            // 更新TextMeshProUGUI文本，显示帧率
            if (fpsText != null)
            {
                fpsText.text = string.Format("{0:0.} FPS", fps);
            }
        }
        else
        {
            // 如果未勾选显示帧率，隐藏FPS文本
            if (fpsText != null)
            {
                fpsText.text = "";
            }
        }
    }

    /// <summary>
    /// 场景加载完成时，重新查找FPS文本
    /// </summary>
    /// <param name="scene">加载的场景</param>
    /// <param name="mode">加载模式</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"场景加载完成：{scene.name}");

        // 重新查找FPS文本对象
        fpsText = GameObject.Find("FPSText")?.GetComponent<TextMeshProUGUI>();
        if (fpsText == null)
        {
            Debug.LogWarning("未找到 FPSText，请检查场景中的 UI 设置！");
        }
    }

    /// <summary>
    /// 禁用场景中的所有Collider
    /// </summary>
    private void DisableColliders()
    {
        // 找到所有带有Collider的对象并禁用它们
        MeshCollider[] meshColliders = FindObjectsOfType<MeshCollider>();
        foreach (MeshCollider meshCol in meshColliders)
        {
            meshCol.enabled = false;
        }

    
    }
}
