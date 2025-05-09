using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InspectorEx;



public class Condition : MonoBehaviour
{

    public enum ConditionType
    {
        [InspectorName("�����ָ���")]
        Separator = 0,
        [InspectorName("����Ŀ������")]
        ReachTargetRegion = 1,
        [InspectorName("��ʱ��")]
        Timer = 2,
    }

    [Header("��������")]
    public ConditionType conditionType;

    private BaseCondition condition;

    //����0_�����ָ������

    //����1_����Ŀ���������
    [PropertyActive("conditionType", CompareType.Equal, ConditionType.ReachTargetRegion, "Ŀ������")]
    public Collider2D targetRegion;

    [PropertyActive("conditionType", CompareType.Equal, ConditionType.ReachTargetRegion, "Ŀ������")]
    public GameObject unit;

    //����2_��ʱ�����
    [PropertyActive("conditionType", CompareType.Equal, ConditionType.Timer, "ʱ�䳤��")]
    public float timeLength;

    /// <summary>
    /// ��ʼ��
    /// </summary>
    public void Initialize()
    {
        switch (conditionType)
        {
            case ConditionType.Separator:
                condition = new Condition0_Separator();
                break;
            case ConditionType.ReachTargetRegion:
                condition = new Condition1_ReachTargetRegion(targetRegion,unit);
                break;
            case ConditionType.Timer:
                condition = new Condition2_Timer(timeLength);
                break;
        }        
    }

    /// <summary>
    /// �ж��Ƿ���������
    /// </summary>
    public bool IsSatisfied()
    {
        return condition.isSatisfied;
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void ResetCondition()
    {
        condition.isSatisfied = false;
        condition.TriggerCondition();
    }
}

