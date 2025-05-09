using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Condition2_Timer : BaseCondition
{
    [Header("时间长度")]
    public float timeLength;

    public Condition2_Timer(float timeLength)
    {
        this.timeLength = timeLength;
    }

    public override void TriggerCondition()
    {

        GameObject MonoStubTemp = GameObject.Find("MonoStubTemp");
        if (MonoStubTemp == null)
        {
            MonoStubTemp = new GameObject();
            MonoStubTemp.name = "MonoStubTemp";
            MonoStubTemp.AddComponent<MonoStub>();
        }
        MonoStubTemp.GetComponent<MonoStub>().StartCoroutine(TimerProcess(timeLength));
    }


    IEnumerator TimerProcess(float delay)
    {
        yield return new WaitForSeconds(delay);
        isSatisfied = true;
    }

}
