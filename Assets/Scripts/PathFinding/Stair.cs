using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stair : MonoBehaviour
{
    [Header("�¶˴�����")]
    public StairTrigger BelowTrigger;

    [Header("�϶˴�����")]
    public StairTrigger AboveTrigger;

    [Header("¥��·���㣨Waypoint��")]
    public Transform startWaypoint;
    public Transform endWaypoint;

    void Start()
    {
        if (BelowTrigger != null)
            BelowTrigger.SetStair(this);
        if (AboveTrigger != null)
            AboveTrigger.SetStair(this);
    }

    // �ṩ��� Waypoint
    public Vector3 GetStartWaypoint()
    {
        return startWaypoint.position;
    }

    // �ṩ�յ� Waypoint
    public Vector3 GetEndWaypoint()
    {
        return endWaypoint.position;
    }
}
