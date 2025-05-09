using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 单例模式

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 跨场景保持
        }
        else
        {
            Destroy(gameObject); // 防止重复创建
        }
    }
}
