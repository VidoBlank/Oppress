using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DrawBoxCollider2D : MonoBehaviour
{
    private new BoxCollider2D collider;
    private GameObject borderObject;

    private void Awake()
    {
        collider = GetComponent<BoxCollider2D>();

        // ����һ����������Ϸ����������߿�
        borderObject = new GameObject("BoxCollider2DBorder");
        borderObject.transform.SetParent(transform); // ���ø�����
        borderObject.transform.localPosition = Vector3.zero;

        // ���һ�� LineRenderer ��������Ʊ߿���
        LineRenderer lineRenderer = borderObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 5;
        lineRenderer.useWorldSpace = false;

        // �����������rotation����Ϊ0
        borderObject.transform.localRotation = Quaternion.identity;
    }

    private void Update()
    {
        if (collider != null && borderObject != null)
        {
            // ��ȡBoxCollider2D�ı�������ϵ���ĸ��ǵ�
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

            // ���ñ߿������localScale
            borderObject.transform.localScale = Vector3.one;
        }
    }
}