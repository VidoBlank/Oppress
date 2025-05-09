using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Trigger : MonoBehaviour
{
    [Header("�Ƿ�ѭ��")]
    public bool isLoop;

    [Header("�Ƿ���")]
    public bool isOpen;

    [Header("�����б�")]
    public List<Condition> conditions;

    [Header("Ч���б�")]
    public List<Effect> effects;

    [HideInInspector]
    public bool canDoEffects;   //�Ƿ�����ִ��Ч��

    private TriggerManager manager;

    private void Awake()
    {
        if(!isOpen)
        {
            gameObject.SetActive(false);
        }
        manager = transform.parent.GetComponent<TriggerManager>();
        foreach (var condition in conditions)
        {
            condition.Initialize();
        }
        foreach (var effect in effects)
        {
            effect.Initialize();
        }
    }

    private void OnEnable()
    {
        canDoEffects = true;
        ResetConditions();
        manager.openTriggers.Add(this);
    }

    private void OnDisable()
    {
        if(manager.openTriggers.Contains(this))
            manager.openTriggers.Remove(this);
    }

    /// <summary>
    /// �ж��Ƿ���������
    /// </summary>
    public bool JudgeConditions()
    {
        bool result = true;
        for (int i = 0; i<conditions.Count;i++)
        {
            if (conditions[i].conditionType == Condition.ConditionType.Separator)
            {
                result = result || conditions[i + 1].IsSatisfied();
                i++; 
            }
            else
            {
                result = result && conditions[i].IsSatisfied();
            }
        }
        return result;
    }

    /// <summary>
    /// ����Ч��
    /// </summary>
    public void TriggerEffects()
    {
        canDoEffects = false;
        StartCoroutine(TriggerEffect());
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void ResetConditions()
    {
        foreach (var condition in conditions)
        {
            condition.ResetCondition();
        }
    }

    private IEnumerator TriggerEffect()
    {
        foreach(Effect effect in effects)
        {
            if(effect.effectType == Effect.EffectType.Interval)
            {
                yield return new WaitForSeconds(effect.timeLength);
            }
            else
            {
                effect.TriggerEffect();
            }
        }
        if (isLoop)
        {
            canDoEffects = true;
            ResetConditions();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
