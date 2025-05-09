using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCondition
{
    [Header("是否满足条件"),HideInInspector]
    public bool isSatisfied = false;    //是否满足条件

    /// <summary>
    /// 触发条件判断
    /// </summary>
    public virtual void TriggerCondition()
    {

    }
}
