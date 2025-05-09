using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect2_OpenTrigger : BaseEffect
{
    [Header("Ä¿±ê´¥·¢Æ÷")]
    public Trigger trigger;

    public Effect2_OpenTrigger(Trigger trigger)
    {
        this.trigger = trigger;
    }

    public override void TriggerEffect()
    {
        trigger.gameObject.SetActive(true);
    }

}
