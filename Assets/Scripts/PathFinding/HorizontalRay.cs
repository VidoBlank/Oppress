using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalRay : MonoBehaviour
{

    private void View()
    {
        Vector3 rightDirection = Quaternion.Euler(0f, 0f, 0f) * Vector3.right;
        Vector2 pointPosition = new Vector2(transform.position.x + GetComponent<Collider2D>().offset.x, transform.position.y + GetComponent<Collider2D>().offset.y);
        Ray2D rightRay = new Ray2D(pointPosition, rightDirection);

        Vector3 leftDirection = Quaternion.Euler(0f, 0f, 0f) * Vector3.right;
         pointPosition = new Vector2(transform.position.x + GetComponent<Collider2D>().offset.x, transform.position.y + GetComponent<Collider2D>().offset.y);
        Ray2D leftRay = new Ray2D(pointPosition, leftDirection);

        LayerMask targetLayerMask = (1 << 10)|(1 << 11); //对楼梯与目标进行检测

        RaycastHit2D[] hits;
        hits = Physics2D.RaycastAll(rightRay.origin, rightRay.direction,50f, targetLayerMask);

    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
