using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stair : MonoBehaviour
{
    [Header("下端触发器")]
    public StairTrigger BelowTrigger;

    [Header("上端触发器")]
    public StairTrigger AboveTrigger;

    [Header("楼梯路径点（Waypoint）")]
    public Transform startWaypoint;
    public Transform endWaypoint;

    void Start()
    {
        if (BelowTrigger != null)
            BelowTrigger.SetStair(this);
        if (AboveTrigger != null)
            AboveTrigger.SetStair(this);
    }

    // 提供起点 Waypoint
    public Vector3 GetStartWaypoint()
    {
        return startWaypoint.position;
    }

    // 提供终点 Waypoint
    public Vector3 GetEndWaypoint()
    {
        return endWaypoint.position;
    }
}
