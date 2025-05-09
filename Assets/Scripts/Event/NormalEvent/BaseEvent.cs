using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BaseEvent : MonoBehaviour
{
    [HideInInspector]
    public bool isInit = false;

    [HideInInspector]
    public bool isEnable = false;

    [HideInInspector]
    public bool isEnd = false;

    private void Awake()
    {

    }

    private void Start()
    {

    }

    public virtual void InitEvent()
    {

    }

    public virtual void EnableEvent()
    {
        Debug.Log("开始事件");
    }

    public void EndEvent()
    {
        isEnd = true;
        enabled = false;

        gameObject.SetActive(false);    //测试用，后面可删除
    }
}
