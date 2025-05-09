using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // 引入场景管理命名空间

public class AmmoWarningUI : MonoBehaviour
{
    public GameObject canvasPrefab;              // 用于在世界空间显示 UI 的 Canvas 预制体
    public TextMeshProUGUI ammoWarningPrefab;      // 弹药警告文本的 TextMeshPro 预制体
    private TextMeshProUGUI ammoWarningInstance;   // 弹药警告文本实例
    private GameObject canvasInstance;             // Canvas 实例
    public Transform targetCharacter;              // 目标角色
    public PlayerController playerController;      // 目标角色的 PlayerController

    void OnEnable()
    {
        // 监听场景加载事件，确保每次场景加载时都重新创建 Canvas
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // 取消监听场景加载事件
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 每次场景加载时销毁现有的 Canvas，并重新创建
        if (canvasInstance != null)
        {
            Destroy(canvasInstance);
        }

        // 重新创建 Canvas
        canvasInstance = Instantiate(canvasPrefab, targetCharacter.position, Quaternion.identity);
        Canvas canvas = canvasInstance.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
        }
    }

    void Start()
    {
        // 初始场景加载时创建 Canvas
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void Update()
    {
        // 确保 Canvas 跟随目标角色
        if (canvasInstance != null)
        {
            // 更新 Canvas 位置，使其始终显示在目标角色上方
            canvasInstance.transform.position = targetCharacter.position + new Vector3(0, 2, 0); // 调整 UI 显示位置
            canvasInstance.transform.rotation = Quaternion.Euler(0, 0, 0); // 重置旋转
        }

        // 检查目标角色的弹药状态（耗尽或低于20%）
        if (playerController != null)
        {
            if (playerController.energy <= 0)
            {
                // 弹药耗尽
                if (ammoWarningInstance == null)
                {
                    CreateAmmoWarning();
                }
                ammoWarningInstance.text = "弹药耗尽";
            }
            else if (playerController.energy < playerController.attribute.maxEnergy * 0.2f)
            {
                // 弹药低于20%
                if (ammoWarningInstance == null)
                {
                    CreateAmmoWarning();
                }
                ammoWarningInstance.text = "弹药低";
            }
            else
            {
                // 弹药充足，隐藏警告
                if (ammoWarningInstance != null)
                {
                    DestroyAmmoWarning();
                }
            }
        }
    }

    private void CreateAmmoWarning()
    {
        // 在目标角色上方实例化弹药警告文本
        ammoWarningInstance = Instantiate(
            ammoWarningPrefab,
            targetCharacter.position + new Vector3(0, 1.75f, 0),
            Quaternion.identity,
            canvasInstance.transform
        );

        // 根据当前弹药状态设置初始文本
        if (playerController.energy <= 0)
        {
            ammoWarningInstance.text = "弹药耗尽";
        }
        else
        {
            ammoWarningInstance.text = "弹药低";
        }

        ammoWarningInstance.alignment = TextAlignmentOptions.Center;
        // 防止 UI 旋转失真
        ammoWarningInstance.transform.localRotation = Quaternion.Euler(0, 0, 0);
        // 可调整 UI 相对于角色的位置
        canvasInstance.transform.position = targetCharacter.position + new Vector3(0, 2, 10);

        // 调整 Canvas 的排序顺序，确保 UI 可见
        Canvas canvas = canvasInstance.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = -1;
        }
    }

    private void DestroyAmmoWarning()
    {
        if (ammoWarningInstance != null)
        {
            Destroy(ammoWarningInstance.gameObject);
        }
    }
}
