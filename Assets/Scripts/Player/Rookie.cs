using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rookie : PlayerController
{
    private RookieLevelSystem rookieLevelSystem;

    void Awake()
    {
        base.Awake();
        // 获取 RookieLevelSystem 组件
        rookieLevelSystem = GetComponent<RookieLevelSystem>();
        if (rookieLevelSystem == null)
        {
            Debug.LogError("Rookie 未找到 RookieLevelSystem 组件！");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        level = Mathf.Clamp(level, 0, 0); // Rookie 等级固定为 0
        rookieLevelSystem?.SetRookieAttributes(this); // 设置 0 级属性
    }

    public override void SetLevel(int newLevel)
    {
        if (newLevel > 0)
        {
            // 当升级到 1 级时，转换为其他职业单位
            TransformToClass(newLevel);
        }
        else
        {
            Debug.LogWarning("Rookie 只能升级为 1 级以上的职业单位！");
        }
    }

    private void TransformToClass(int targetLevel)
    {
        GameObject newClassPrefab = null;

        // 根据目标职业单位，加载对应的预制体
        switch (targetLevel)
        {
            case 1:
                newClassPrefab = Resources.Load<GameObject>("Prefabs/Rifleman");
                break;
            case 2:
                newClassPrefab = Resources.Load<GameObject>("Prefabs/Engineer");
                break;
            case 3:
                newClassPrefab = Resources.Load<GameObject>("Prefabs/Sniper");
                break;
            case 4:
                newClassPrefab = Resources.Load<GameObject>("Prefabs/Support");
                break;
            default:
                Debug.LogError("未知的职业单位目标等级！");
                return;
        }

        if (newClassPrefab != null)
        {
            // 替换当前对象为目标职业单位
            GameObject newClassInstance = Instantiate(newClassPrefab, transform.position, transform.rotation);
            Destroy(gameObject); // 销毁当前 Rookie 对象
            Debug.Log($"Rookie 升级为 {newClassPrefab.name} 职业单位！");
        }
        else
        {
            Debug.LogError("未找到目标职业单位的预制体！");
        }
    }
}
