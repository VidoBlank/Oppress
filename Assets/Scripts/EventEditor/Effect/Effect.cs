using InspectorEx;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Condition;

public class Effect : MonoBehaviour
{
    public enum EffectType
    {
        [InspectorName("等待间隔")]
        Interval = 0,
        [InspectorName("显示提示")]
        ShowTip = 1,
        [InspectorName("打开触发器")]
        OpenTrigger = 2,
    }

    [Header("条件类型")]
    public EffectType effectType;

    private BaseEffect effect;

    //效果0_等待间隔相关
    [PropertyActive("effectType", CompareType.Equal, EffectType.Interval, "时间长度")]
    public float timeLength;

    //效果1_显示提示相关
    [PropertyActive("effectType", CompareType.Equal, EffectType.ShowTip, "提示信息")]
    public string tip;

    //效果2_打开触发器
    [PropertyActive("effectType", CompareType.Equal, EffectType.OpenTrigger, "目标触发器")]
    public Trigger trigger;


    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize()
    {
        switch (effectType)
        {
            case EffectType.Interval:
                effect = new Effect0_Interval(timeLength);
                break;
            case EffectType.ShowTip:
                effect = new Effect1_ShowTip(tip);
                break;
            case EffectType.OpenTrigger:
                effect = new Effect2_OpenTrigger(trigger);
                break;
        }
    }

    /// <summary>
    /// 触发效果
    /// </summary>
    public void TriggerEffect()
    {
        effect.TriggerEffect();
    }
}
