using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCondition
{
    [Header("�Ƿ���������"),HideInInspector]
    public bool isSatisfied = false;    //�Ƿ���������

    /// <summary>
    /// ���������ж�
    /// </summary>
    public virtual void TriggerCondition()
    {

    }
}
