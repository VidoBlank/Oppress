using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTest : MonoBehaviour
{
    [Header("����ֵ")]
    public float Hp = 10f;
    
    //��Ѫ
    public void ReduceHp(float num)
    {
        Hp -= num;
        if(Hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}
