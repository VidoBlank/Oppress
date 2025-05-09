using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ProcessManager : MonoBehaviour
{
    [Header("������")]
    public List<Process> processGroup = new List<Process>();

    //��������
    [Serializable]
    public class Process
    {
        [Header("���̴������¼�")]
        public List<BaseEvent> events = new List<BaseEvent>();

        [Header("���̽���������")]
        public List<BaseEvent> endConditions = new List<BaseEvent>();
    }

    [Header("��ǰ�������")]
    public int processIndex = -1;

    //��ʼ������
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

    //��ʼ��һ������
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

    //��ǰ�����Ƿ����
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
