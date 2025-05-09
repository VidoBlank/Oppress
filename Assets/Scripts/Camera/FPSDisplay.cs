using UnityEngine;
using TMPro;  // ����TextMeshPro�������ռ�
using UnityEngine.SceneManagement; // ���볡�����������ռ�

public class FPSDisplay : MonoBehaviour
{
    public TextMeshProUGUI fpsText; // ��TextMeshProUGUI����ʾ֡��
    private float deltaTime = 0.0f;

    [Header("֡����ʾ����")]
    public bool showFPS = true;  // ��Inspector�пɹ�ѡ�������Ƿ���ʾ֡��

    private void Start()
    {
        // ����VSync
        QualitySettings.vSyncCount = 1;

        // ����֡�����ޣ�����120
        Application.targetFrameRate = 60;

        // ��������Collider
        DisableColliders();
    }

    private void OnEnable()
    {
        // ���ĳ��������¼�
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // ȡ�����ĳ��������¼�
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (showFPS) // �����ѡ����ʾ֡��
        {
            // ����deltaTime����֡�ʼ���
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;

            // ����TextMeshProUGUI�ı�����ʾ֡��
            if (fpsText != null)
            {
                fpsText.text = string.Format("{0:0.} FPS", fps);
            }
        }
        else
        {
            // ���δ��ѡ��ʾ֡�ʣ�����FPS�ı�
            if (fpsText != null)
            {
                fpsText.text = "";
            }
        }
    }

    /// <summary>
    /// �����������ʱ�����²���FPS�ı�
    /// </summary>
    /// <param name="scene">���صĳ���</param>
    /// <param name="mode">����ģʽ</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"����������ɣ�{scene.name}");

        // ���²���FPS�ı�����
        fpsText = GameObject.Find("FPSText")?.GetComponent<TextMeshProUGUI>();
        if (fpsText == null)
        {
            Debug.LogWarning("δ�ҵ� FPSText�����鳡���е� UI ���ã�");
        }
    }

    /// <summary>
    /// ���ó����е�����Collider
    /// </summary>
    private void DisableColliders()
    {
        // �ҵ����д���Collider�Ķ��󲢽�������
        MeshCollider[] meshColliders = FindObjectsOfType<MeshCollider>();
        foreach (MeshCollider meshCol in meshColliders)
        {
            meshCol.enabled = false;
        }

    
    }
}
