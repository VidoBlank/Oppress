using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//交互事件，当目标物体被交互时，事件结束
public class InteractionEvent : BaseEvent
{
    [Header("需要交互的目标物体")]
    public Interactable targetObject;

    public override void EnableEvent()
    {
        Debug.Log("开始交互事件");
    }

    private void Update()
    {
        if(isEnable)
        {
            //当目标物体被交互时结束事件
            if(targetObject.isInteracted)
            {
                EndEvent();
            }
        }
    }
}
