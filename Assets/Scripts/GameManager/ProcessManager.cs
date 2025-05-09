using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ProcessManager : MonoBehaviour
{
    [Header("流程组")]
    public List<Process> processGroup = new List<Process>();

    //流程设置
    [Serializable]
    public class Process
    {
        [Header("流程触发的事件")]
        public List<BaseEvent> events = new List<BaseEvent>();

        [Header("流程结束的条件")]
        public List<BaseEvent> endConditions = new List<BaseEvent>();
    }

    [Header("当前流程序号")]
    public int processIndex = -1;

    //初始化流程
    void InitProcesses()
    {
        foreach (Process process in processGroup)
        {
            foreach (BaseEvent processEvent in process.events)
            {
                processEvent.InitEvent();
                processEvent.isInit = true;
                processEvent.enabled = false;
            }
        }
    }

    //开始下一个流程
    void StartNextProcess()
    {
        processIndex += 1;
        if(processIndex >= processGroup.Count)
        {
            return;
        }
        foreach (BaseEvent processEvent in processGroup[processIndex].events)
        {
            processEvent.enabled = true;
            processEvent.EnableEvent();
            processEvent.isEnable = true;
        }
    }

    //当前流程是否结束
    bool isCurProcessEnd()
    {
        if (processIndex >= processGroup.Count)
        {
            return false;
        }
        foreach (BaseEvent endCondition in processGroup[processIndex].endConditions)
        {
            if(!endCondition.isEnd)
            {
                return false;
            }
        }
        return true;
    }

    private void Start()
    {
        InitProcesses();
        StartNextProcess();
    }

    private void Update()
    {
        if(isCurProcessEnd())
        {
            StartNextProcess();
        }
    }
}
