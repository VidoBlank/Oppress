using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect1_ShowTip : BaseEffect
{
    [Header("��ʾ��Ϣ")]
    public string tip;

    public Effect1_ShowTip(string tip)
    {
        this.tip = tip;
    }

    public override void TriggerEffect()
    {
        TypewriterColorJitterEffect typewriterEffect = GameObject.Find("������/Gamemanager").GetComponent<TypewriterColorJitterEffect>();
        typewriterEffect.SetText(tip);
    }
}
