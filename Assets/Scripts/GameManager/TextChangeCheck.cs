using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextChangeCheck : MonoBehaviour
{
    public int index = 0;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            TextManager textManager = transform.parent.GetComponent<TextManager>();
            textManager.ShowDialog(index);
            Destroy(gameObject);
        }
    }
}
