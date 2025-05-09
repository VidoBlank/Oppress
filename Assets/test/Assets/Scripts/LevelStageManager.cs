using System.Collections.Generic;
using UnityEngine;

[System.Serializable]


public class LevelStageManager : MonoBehaviour
{
  

    [Tooltip("当前关卡阶段，从 1 开始计数")]
    public int currentStage = 1;

    private const string CurrentStageKey = "CurrentStage";

    void Start()
    {
        // 从存档加载当前阶段，若无则默认为 1
        currentStage = PlayerPrefs.GetInt(CurrentStageKey, 1);
       
    }

    /// <summary>
    /// 进入下一阶段：先禁用当前阶段的开启对象、开启当前阶段的关闭对象，
    /// 阶段数加1并保存，再激活新阶段的 objectsToActivate 并关闭 objectsToDeactivate。
    /// </summary>
    public void NextStage()
    {
       

        // 阶段数加1并保存到存档
        currentStage++;
        PlayerPrefs.SetInt(CurrentStageKey, currentStage);
        PlayerPrefs.Save();

        
    }

  
   
}
