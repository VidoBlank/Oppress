using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class Condition1_ReachTargetRegion : BaseCondition
{
    [Header("目标区域")]
    public Collider2D targetRegion;

    [Header("目标物体")]
    public GameObject unit;

    public Condition1_ReachTargetRegion(Collider2D targetRegion, GameObject unit)
    {
        this.targetRegion = targetRegion;
        this.unit = unit;
    }

    public override void TriggerCondition()
    {
        if(targetRegion.GetComponent<ReachingRegionCheck>() == null) 
            targetRegion.AddComponent<ReachingRegionCheck>().condition = this;
    }
}
