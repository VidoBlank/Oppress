using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "JackOfAll", menuName = "SlotEffects/JackOfAll")]
public class JackOfAll : SlotEffect
{
    public override void ApplyEffect(PlayerController player)
    {
        // ���������츳����ʱ�����ñ��Ϊ true
        player.hasVersatileTalent = true;
        Debug.Log("�������֡�Ч���������������һ��1����һ��2���츳��");
    }

    public override void RemoveEffect(PlayerController player)
    {
        // ��Ч���Ƴ�ʱ��ȡ�������ֱ��
        player.hasVersatileTalent = false;
        Debug.Log("�������֡�Ч���Ƴ���ȡ�������츳�ۡ�");
    }
}
