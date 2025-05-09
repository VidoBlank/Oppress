using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTest : MonoBehaviour
{
    [Header("ÉúÃüÖµ")]
    public float Hp = 10f;
    
    //¿ÛÑª
    public void ReduceHp(float num)
    {
        Hp -= num;
        if(Hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}
