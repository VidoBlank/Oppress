using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InspectorEx;



public class Condition : MonoBehaviour
{

    public enum ConditionType
    {
        [InspectorName("条件分隔符")]
        Separator = 0,
        [InspectorName("到达目标区域")]
        ReachTargetRegion = 1,
        [InspectorName("计时器")]
        Timer = 2,
    }

    [Header("条件类型")]
    public ConditionType conditionType;

    private BaseCondition condition;

    //条件0_条件分隔符相关

    //条件1_到达目标区域相关
    [PropertyActive("conditionType", CompareType.Equal, ConditionType.ReachTargetRegion, "目标区域")]
    public Collider2D targetRegion;

    [PropertyActive("conditionType", CompareType.Equal, ConditionType.ReachTargetRegion, "目标物体")]
    public GameObject unit;

    //条件2_计时器相关
    [PropertyActive("conditionType", CompareType.Equal, ConditionType.Timer, "时间长度")]
    public float timeLength;

    /// <summary>
    /// 初始化
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
    /// 判断是否满足条件
    /// </summary>
    public bool IsSatisfied()
    {
        return condition.isSatisfied;
    }

    /// <summary>
    /// 重置条件
    /// </summary>
    public void ResetCondition()
    {
        condition.isSatisfied = false;
        condition.TriggerCondition();
    }
}

