using System.Collections.Generic;
using UnityEngine;

[System.Serializable]


public class LevelStageManager : MonoBehaviour
{
  

    [Tooltip("��ǰ�ؿ��׶Σ��� 1 ��ʼ����")]
    public int currentStage = 1;

    private const string CurrentStageKey = "CurrentStage";

    void Start()
    {
        // �Ӵ浵���ص�ǰ�׶Σ�������Ĭ��Ϊ 1
        currentStage = PlayerPrefs.GetInt(CurrentStageKey, 1);
       
    }

    /// <summary>
    /// ������һ�׶Σ��Ƚ��õ�ǰ�׶εĿ������󡢿�����ǰ�׶εĹرն���
    /// �׶�����1�����棬�ټ����½׶ε� objectsToActivate ���ر� objectsToDeactivate��
    /// </summary>
    public void NextStage()
    {
       

        // �׶�����1�����浽�浵
        currentStage++;
        PlayerPrefs.SetInt(CurrentStageKey, currentStage);
        PlayerPrefs.Save();

        
    }

  
   
}
