using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�����¼�������ʱ�����������ﱻȫ������ʱ���¼�����
public class MonsterEvent : BaseEvent
{
    [Header("������")]
    public List<GameObject> monsters = new List<GameObject>();

    public override void InitEvent()
    {
        foreach (GameObject monster in monsters)
        {
            monster.SetActive(false);
        }
    }

    public override void EnableEvent()
    {
        Debug.Log("��ʼ�����¼�");
        foreach (GameObject monster in monsters)
        {
            monster.SetActive(true);
        }
    }

    private void Update()
    {
        foreach (GameObject monster in monsters)
        {
            if(monster)
            {
                return;
            }
        }
        EndEvent();
    }
}
