using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetrolSup : MonoBehaviour
{
    public List<Enemy> enemies;
    public CircleCollider2D BombCollider;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Enemy>())
            enemies.Add(collision.GetComponent<Enemy>());
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (enemies.Contains(collision.GetComponent<Enemy>()))
            enemies.Remove(collision.GetComponent<Enemy>());
    }

    public void SetRadiusAndTime(float riadius,float time)
    {
        BombCollider.radius = riadius;
        lifeTime = time;
    }

    public void Buring(float buringDamage)
    {
        burn = true;
        damage = buringDamage;
    }

    [Header("剩余持续时间")]
    public float lifeTime;

    [Header("燃烧")]
    public bool burn;

    private float damage;

    // Update is called once per frame
    void Update()
    {
        if (burn == true && lifeTime > 0)
        {
            foreach (Enemy enemy in enemies)
            {
                enemy.ReduceHp(damage*Time.deltaTime);
            }
            lifeTime -= Time.deltaTime;
        }
        if(lifeTime<0)
        {
            Destroy(gameObject);
        }
    }
}
