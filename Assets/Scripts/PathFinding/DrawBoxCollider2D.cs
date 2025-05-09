using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DrawBoxCollider2D : MonoBehaviour
{
    private new BoxCollider2D collider;
    private GameObject borderObject;

    private void Awake()
    {
        collider = GetComponent<BoxCollider2D>();

        // 创建一个辅助的游戏对象来代表边框
        borderObject = new GameObject("BoxCollider2DBorder");
        borderObject.transform.SetParent(transform); // 设置父物体
        borderObject.transform.localPosition = Vector3.zero;

        // 添加一个 LineRenderer 组件来绘制边框线
        LineRenderer lineRenderer = borderObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 5;
        lineRenderer.useWorldSpace = false;

        // 将物体自身的rotation设置为0
        borderObject.transform.localRotation = Quaternion.identity;
    }

    private void Update()
    {
        if (collider != null && borderObject != null)
        {
            // 获取BoxCollider2D的本地坐标系的四个角点
            Vector2 colliderSize = collider.size;
            Vector2 position = collider.offset;

            Vector3[] borderPoints = new Vector3[5];
            borderPoints[0] = new Vector3(-colliderSize.x / 2, colliderSize.y / 2) + (Vector3)position;
            borderPoints[1] = new Vector3(colliderSize.x / 2, colliderSize.y / 2) + (Vector3)position;
            borderPoints[2] = new Vector3(colliderSize.x / 2, -colliderSize.y / 2) + (Vector3)position;
            borderPoints[3] = new Vector3(-colliderSize.x / 2, -colliderSize.y / 2) + (Vector3)position;
            borderPoints[4] = borderPoints[0];

            LineRenderer lineRenderer = borderObject.GetComponent<LineRenderer>();
            lineRenderer.SetPositions(borderPoints);

            // 重置边框物体的localScale
            borderObject.transform.localScale = Vector3.one;
        }
    }
}