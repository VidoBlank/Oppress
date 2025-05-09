using UnityEngine;

public class LevelManagerTest : MonoBehaviour
{
    private LevelManager levelManager;

    void Start()
    {
        // 自动查找当前场景中的 LevelManager 组件
        levelManager = FindObjectOfType<LevelManager>();
        if (levelManager == null)
        {
            Debug.LogError("未找到 LevelManager，请确保关卡场景中存在 LevelManager 组件！");
        }
        else
        {
            // 自动设置返回场景名称为 "Map"
            levelManager.returnSceneName = "Map";
        }
    }

    void OnGUI()
    {
        // 绘制测试按钮，位置和尺寸可根据需要调整
        if (GUI.Button(new Rect(10, 10, 200, 50), "触发关卡完成"))
        {
            if (levelManager != null)
            {
                levelManager.OnLevelComplete();
            }
        }

        if (GUI.Button(new Rect(10, 70, 200, 50), "触发关卡失败"))
        {
            if (levelManager != null)
            {
                levelManager.OnLevelFailed();
            }
        }
    }
}
