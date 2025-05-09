using UnityEngine;

public class LevelManagerTest : MonoBehaviour
{
    private LevelManager levelManager;

    void Start()
    {
        // �Զ����ҵ�ǰ�����е� LevelManager ���
        levelManager = FindObjectOfType<LevelManager>();
        if (levelManager == null)
        {
            Debug.LogError("δ�ҵ� LevelManager����ȷ���ؿ������д��� LevelManager �����");
        }
        else
        {
            // �Զ����÷��س�������Ϊ "Map"
            levelManager.returnSceneName = "Map";
        }
    }

    void OnGUI()
    {
        // ���Ʋ��԰�ť��λ�úͳߴ�ɸ�����Ҫ����
        if (GUI.Button(new Rect(10, 10, 200, 50), "�����ؿ����"))
        {
            if (levelManager != null)
            {
                levelManager.OnLevelComplete();
            }
        }

        if (GUI.Button(new Rect(10, 70, 200, 50), "�����ؿ�ʧ��"))
        {
            if (levelManager != null)
            {
                levelManager.OnLevelFailed();
            }
        }
    }
}
