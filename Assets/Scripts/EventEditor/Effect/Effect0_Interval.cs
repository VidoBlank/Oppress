using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect0_Interval : BaseEffect
{
    [Header("时间长度")]
    public float timeLength;

    public Effect0_Interval(float timeLength)
    {
        this.timeLength = timeLength;
    }
}
