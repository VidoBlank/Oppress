using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect1_ShowTip : BaseEffect
{
    [Header("提示信息")]
    public string tip;

    public Effect1_ShowTip(string tip)
    {
        this.tip = tip;
    }

    public override void TriggerEffect()
    {
        TypewriterColorJitterEffect typewriterEffect = GameObject.Find("管理器/Gamemanager").GetComponent<TypewriterColorJitterEffect>();
        typewriterEffect.SetText(tip);
    }
}
