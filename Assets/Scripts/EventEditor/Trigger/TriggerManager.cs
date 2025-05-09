using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerManager : MonoBehaviour
{
    [Header("�������б�")]
    public List<Trigger> triggers;

    [HideInInspector]
    public List<Trigger> openTriggers;

    private void Awake()
    {

    }


    void Update()
    {
        foreach(var trigger in openTriggers)
        {
            if(trigger.canDoEffects)
            {
                if(trigger.JudgeConditions())
                {
                    trigger.TriggerEffects();
                }
            }
        }
    }
}
