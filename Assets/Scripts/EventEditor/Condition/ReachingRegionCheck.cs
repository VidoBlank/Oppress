using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReachingRegionCheck : MonoBehaviour
{
    public Condition1_ReachTargetRegion condition;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (condition.unit == collision.gameObject)
        {
            condition.isSatisfied = true;
        }
    }
}
