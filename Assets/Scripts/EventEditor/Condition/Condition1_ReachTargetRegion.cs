using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class Condition1_ReachTargetRegion : BaseCondition
{
    [Header("Ŀ������")]
    public Collider2D targetRegion;

    [Header("Ŀ������")]
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
