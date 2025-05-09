using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�����¼�����Ŀ�����屻����ʱ���¼�����
public class InteractionEvent : BaseEvent
{
    [Header("��Ҫ������Ŀ������")]
    public Interactable targetObject;

    public override void EnableEvent()
    {
        Debug.Log("��ʼ�����¼�");
    }

    private void Update()
    {
        if(isEnable)
        {
            //��Ŀ�����屻����ʱ�����¼�
            if(targetObject.isInteracted)
            {
                EndEvent();
            }
        }
    }
}
