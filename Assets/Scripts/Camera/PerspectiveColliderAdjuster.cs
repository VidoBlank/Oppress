using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class ColliderHorizontalExtender : MonoBehaviour
{
    public float maxExtend = 1f; // ����ӳ�����
    public float centerThreshold = 0.2f; // ��Ļ������ֵ��Χ��0 �� 1����Ĭ�� 20%
    public LayerMask colliderLayerMask; // ��Ҫ��������ײ��㼶

    private Camera mainCamera;

    // ����ÿ����ײ���ԭʼ��С��ƫ��
    private Dictionary<BoxCollider2D, (Vector2 size, Vector2 offset)> originalValues = new Dictionary<BoxCollider2D, (Vector2 size, Vector2 offset)>();

    void Start()
    {
        mainCamera = GetComponent<Camera>();

        // ��ʼ��ʱ�������� BoxCollider2D ��ԭʼ��С��ƫ��
        foreach (var collider in FindObjectsOfType<BoxCollider2D>())
        {
            if (!originalValues.ContainsKey(collider))
            {
                originalValues[collider] = (collider.size, collider.offset);
            }
        }
    }

    void Update()
    {
        ExtendColliders();
    }

    private void ExtendColliders()
    {
        // ��ȡ���������� BoxCollider2D
        BoxCollider2D[] colliders = FindObjectsOfType<BoxCollider2D>();

        foreach (var collider in colliders)
        {
            // ȷ����ײ����ָ���㼶��
            if (((1 << collider.gameObject.layer) & colliderLayerMask) == 0)
                continue;

            // ��ȡ��ײ�������λ��
            Vector3 worldPosition = collider.transform.position;

            // ת��Ϊ��Ļ����
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

            // �ж��Ƿ�����Ļ��Χ��
            if (screenPosition.x < 0 || screenPosition.x > Screen.width ||
                screenPosition.y < 0 || screenPosition.y > Screen.height)
            {
                // ��Ļ������壬�ָ�ԭʼ��С��ƫ��
                ResetColliderSize(collider);
                continue;
            }

            // ��һ����Ļ���� (-1 �� 1)�����ڼ����ӳ�����
            float normalizedScreenX = (screenPosition.x / Screen.width) * 2 - 1;

            // �����������ֵ��Χ�ڣ���ָ�ԭʼ��С��ƫ��
            if (Mathf.Abs(normalizedScreenX) <= centerThreshold)
            {
                ResetColliderSize(collider);
                continue;
            }

            // �����ӳ�ֵ�����ݳ�����ֵ�ľ����������ӣ�
            float effectiveScreenX = Mathf.Abs(normalizedScreenX) - centerThreshold;
            float extendAmount = Mathf.Clamp01(effectiveScreenX / (1 - centerThreshold)) * maxExtend;

            // ��̬������ײ��Ĵ�С��ƫ��
            Vector2 originalSize = originalValues[collider].size;
            Vector2 originalOffset = originalValues[collider].offset;

            if (normalizedScreenX < 0) // ��Ļ��ߣ������ӳ�
            {
                collider.size = new Vector2(originalSize.x + extendAmount, originalSize.y);
                collider.offset = new Vector2(originalOffset.x + extendAmount / 2, originalOffset.y);
            }
            else if (normalizedScreenX > 0) // ��Ļ�ұߣ������ӳ�
            {
                collider.size = new Vector2(originalSize.x + extendAmount, originalSize.y);
                collider.offset = new Vector2(originalOffset.x - extendAmount / 2, originalOffset.y);
            }
        }
    }

    private void ResetColliderSize(BoxCollider2D collider)
    {
        // �ָ���ײ���ԭʼ��С��ƫ��
        if (originalValues.ContainsKey(collider))
        {
            var original = originalValues[collider];
            collider.size = original.size;
            collider.offset = original.offset;
        }
    }
}
