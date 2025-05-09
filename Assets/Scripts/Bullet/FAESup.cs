using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FAESup : MonoBehaviour
{

    public List<Enemy> enemies;
    public CircleCollider2D BombCollider;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<Enemy>())
        enemies.Add(collision.GetComponent<Enemy>());
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (enemies.Contains(collision.GetComponent<Enemy>()))
            enemies.Remove(collision.GetComponent<Enemy>());
    }

    public void SetRadius(float riadius)
    {
        BombCollider.radius = riadius;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
