using InspectorEx;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Condition;

public class Effect : MonoBehaviour
{
    public enum EffectType
    {
        [InspectorName("�ȴ����")]
        Interval = 0,
        [InspectorName("��ʾ��ʾ")]
        ShowTip = 1,
        [InspectorName("�򿪴�����")]
        OpenTrigger = 2,
    }

    [Header("��������")]
    public EffectType effectType;

    private BaseEffect effect;

    //Ч��0_�ȴ�������
    [PropertyActive("effectType", CompareType.Equal, EffectType.Interval, "ʱ�䳤��")]
    public float timeLength;

    //Ч��1_��ʾ��ʾ���
    [PropertyActive("effectType", CompareType.Equal, EffectType.ShowTip, "��ʾ��Ϣ")]
    public string tip;

    //Ч��2_�򿪴�����
    [PropertyActive("effectType", CompareType.Equal, EffectType.OpenTrigger, "Ŀ�괥����")]
    public Trigger trigger;


    /// <summary>
    /// ��ʼ��
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
    /// ����Ч��
    /// </summary>
    public void TriggerEffect()
    {
        effect.TriggerEffect();
    }
}
