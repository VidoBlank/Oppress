using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class ColliderHorizontalExtender : MonoBehaviour
{
    public float maxExtend = 1f; // 最大延长距离
    public float centerThreshold = 0.2f; // 屏幕中心阈值范围（0 到 1），默认 20%
    public LayerMask colliderLayerMask; // 需要调整的碰撞体层级

    private Camera mainCamera;

    // 缓存每个碰撞体的原始大小和偏移
    private Dictionary<BoxCollider2D, (Vector2 size, Vector2 offset)> originalValues = new Dictionary<BoxCollider2D, (Vector2 size, Vector2 offset)>();

    void Start()
    {
        mainCamera = GetComponent<Camera>();

        // 初始化时缓存所有 BoxCollider2D 的原始大小和偏移
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
        // 获取场景中所有 BoxCollider2D
        BoxCollider2D[] colliders = FindObjectsOfType<BoxCollider2D>();

        foreach (var collider in colliders)
        {
            // 确保碰撞体在指定层级中
            if (((1 << collider.gameObject.layer) & colliderLayerMask) == 0)
                continue;

            // 获取碰撞体的世界位置
            Vector3 worldPosition = collider.transform.position;

            // 转换为屏幕坐标
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

            // 判断是否在屏幕范围内
            if (screenPosition.x < 0 || screenPosition.x > Screen.width ||
                screenPosition.y < 0 || screenPosition.y > Screen.height)
            {
                // 屏幕外的物体，恢复原始大小和偏移
                ResetColliderSize(collider);
                continue;
            }

            // 归一化屏幕坐标 (-1 到 1)，用于计算延长比例
            float normalizedScreenX = (screenPosition.x / Screen.width) * 2 - 1;

            // 如果在中心阈值范围内，则恢复原始大小和偏移
            if (Mathf.Abs(normalizedScreenX) <= centerThreshold)
            {
                ResetColliderSize(collider);
                continue;
            }

            // 计算延长值（根据超出阈值的距离线性增加）
            float effectiveScreenX = Mathf.Abs(normalizedScreenX) - centerThreshold;
            float extendAmount = Mathf.Clamp01(effectiveScreenX / (1 - centerThreshold)) * maxExtend;

            // 动态调整碰撞体的大小和偏移
            Vector2 originalSize = originalValues[collider].size;
            Vector2 originalOffset = originalValues[collider].offset;

            if (normalizedScreenX < 0) // 屏幕左边：向右延长
            {
                collider.size = new Vector2(originalSize.x + extendAmount, originalSize.y);
                collider.offset = new Vector2(originalOffset.x + extendAmount / 2, originalOffset.y);
            }
            else if (normalizedScreenX > 0) // 屏幕右边：向左延长
            {
                collider.size = new Vector2(originalSize.x + extendAmount, originalSize.y);
                collider.offset = new Vector2(originalOffset.x - extendAmount / 2, originalOffset.y);
            }
        }
    }

    private void ResetColliderSize(BoxCollider2D collider)
    {
        // 恢复碰撞体的原始大小和偏移
        if (originalValues.ContainsKey(collider))
        {
            var original = originalValues[collider];
            collider.size = original.size;
            collider.offset = original.offset;
        }
    }
}
