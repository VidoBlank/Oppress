using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//怪物事件，启动时激活怪物，当怪物被全部消灭时，事件结束
public class MonsterEvent : BaseEvent
{
    [Header("怪物组")]
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
        Debug.Log("开始怪物事件");
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
