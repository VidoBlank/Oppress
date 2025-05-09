using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "JackOfAll", menuName = "SlotEffects/JackOfAll")]
public class JackOfAll : SlotEffect
{
    public override void ApplyEffect(PlayerController player)
    {
        // 当多面手天赋激活时，设置标记为 true
        player.hasVersatileTalent = true;
        Debug.Log("【多面手】效果激活：允许额外解锁一个1级与一个2级天赋。");
    }

    public override void RemoveEffect(PlayerController player)
    {
        // 当效果移除时，取消多面手标记
        player.hasVersatileTalent = false;
        Debug.Log("【多面手】效果移除：取消额外天赋槽。");
    }
}
