using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class UIVisualizationManager : MonoBehaviour
{
    public PlayerManager playerManager;

    [Header("Mouse Detection Toggle")]
    public bool enableMouseDetection = true; // 是否启用鼠标检测

    [Header("Layer Settings")]
    public LayerMask interactableLayers; // 设置检测的图层，例如 "Enemy", "Obstacle"

    [Header("Cursor Textures")]
    public Texture2D defaultCursor;     // 默认光标
    public Texture2D attackCursor;
    public Texture2D interactCursor;   // 物品交互光标
    public Texture2D rescueCursor;     // 救援光标

    [Header("Outline Settings")]
    public Color enemyOutlineColor = Color.red;        // 敌人高亮轮廓颜色
    public Color objectOutlineColor = Color.green;     // 物品高亮轮廓颜色
    public float outlineWidth = 3f;                    // 高亮轮廓宽度

    [Header("Dimmed Outline Settings")]
    public Color dimmedEnemyColor = new Color(1f, 0.5f, 0.5f, 0.3f);  // 敌人暗淡颜色
    public Color dimmedObjectColor = new Color(0.5f, 1f, 0.5f, 0.3f); // 物体暗淡颜色
    public float dimmedOutlineWidth = 1f;                             // 暗淡轮廓宽度

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f; // 从高亮到暗淡的过渡时间

    private List<GameObject> highlightedObjects = new List<GameObject>(); // 当前所有高亮的对象
    private Outline currentOutline;           // 当前的 Outline 组件
   

    private void Start()
    {
        if (playerManager == null)
        {
            playerManager = FindObjectOfType<PlayerManager>();
        }
    }

    private void Update()
    {
        if (!enableMouseDetection || IsAnyPlayerPreparingSkill())
        {
            // 如果在技能准备状态，隐藏光标
            ResetAllHighlights();
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // 隐藏光标
            return;
        }

        // 检查选中玩家是否大于 0
        if (playerManager != null && playerManager.selectedPlayers.Count > 0)
        {
            HandleMouseHighlight(); // 更新高亮效果
        }
        else
        {
            // 如果没有选中玩家，重置高亮效果和光标
            ResetAllHighlights();
            SetDefaultCursor();
        }
    }

    private bool IsAnyPlayerPreparingSkill()
    {
        // 遍历已选玩家，检查是否有玩家正在准备技能
        foreach (var player in playerManager.selectedPlayers)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null && playerController.isPreparingSkill)
            {
                return true; // 如果有任何玩家处于技能准备状态，返回 true
            }
        }
        return false;
    }
    private bool IsMouseInAnyRevealerVision(Ray ray)
    {
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        if (!plane.Raycast(ray, out float distance)) return false;

        Vector3 mouseWorldPos = ray.GetPoint(distance);
        FOW.FogOfWarRevealer3D[] revealers = FindObjectsOfType<FOW.FogOfWarRevealer3D>();

        bool isInside = false;

        foreach (var revealer in revealers)
        {
            Vector3 eyePos = revealer.GetEyePosition();

            // 🔧 计算实际可视半径：包含 soften distance
            float radius = revealer.ViewRadius;
            if (FOW.FogOfWarWorld.instance.UsingSoftening)
            {
                radius += revealer.RevealHiderInFadeOutZonePercentage * revealer.SoftenDistance;
            }

            // ✅ 画圆形 debug 范围
            DebugDrawCircle(eyePos, radius, Color.yellow);

            if (Vector3.Distance(mouseWorldPos, eyePos) <= radius)
            {
                Debug.DrawLine(eyePos, mouseWorldPos, Color.green); // 可见连线
                Debug.Log($"[Revealer Debug] 鼠标在 {revealer.name} 的【实际】视野范围内");
                isInside = true;
            }
        }

        if (!isInside)
        {
          
        }

        return isInside;
    }

    private void DebugDrawCircle(Vector3 center, float radius, Color color, int segments = 40)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(Mathf.Cos(0), Mathf.Sin(0), 0) * radius;
        for (int i = 1; i <= segments; i++)
        {
            float angle = Mathf.Deg2Rad * (i * angleStep);
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            Debug.DrawLine(prevPoint, nextPoint, color);
            prevPoint = nextPoint;
        }
    }


    private void HandleMouseHighlight()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        // 定义 2D 平面用于转换世界位置
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        Vector3 worldPos = Vector3.zero;
        if (plane.Raycast(ray, out float distance))
        {
            worldPos = ray.GetPoint(distance);
        }

        // --------------------------
        // 👇 1. 先尝试 2D 射线检测
        RaycastHit2D hit2D = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, interactableLayers);
        GameObject targetObject = null;

        if (hit2D.collider != null)
        {
            targetObject = hit2D.collider.gameObject;
        }
        else
        {
            // 👇 2. 再尝试 3D 射线检测
            int layerMask3D = LayerMask.GetMask("Ground", "Enemy", "Obstacle", "Object");
            if (Physics.Raycast(ray, out RaycastHit hit3D, 100f, layerMask3D))
            {
                GameObject rawHit = hit3D.collider.gameObject;
                Transform current = rawHit.transform;
                while (current != null)
                {
                    if (current.GetComponent<Outline>() != null ||
                        current.CompareTag("Enemy") ||
                        current.CompareTag("Obstacle") ||
                        current.CompareTag("Object") ||
                        current.CompareTag("Player"))
                    {
                        targetObject = current.gameObject;
                        break;
                    }
                    current = current.parent;
                }
            }
        }

        if (!IsTargetOrRayVisible(ray, targetObject) && !IsMouseInAnyRevealerVision(ray))
        {
            ResetAllHighlights();
            SetDefaultCursor();
            return;
        }

        // 👇 高亮处理
        if (targetObject != null)
        {
            if (!highlightedObjects.Contains(targetObject))
            {
                if (!targetObject.CompareTag("Player"))
                {
                    HighlightObject(targetObject);
                }
            }

            UpdateCursor(targetObject);
        }
        else
        {
            ResetAllHighlights();
            SetDefaultCursor();
        }

        CleanupDestroyedHighlights();
    }


    private bool IsTargetOrRayVisible(Ray ray, GameObject target)
    {
        // ✅ 1. 目标拥有 FogOfWarRevealer3D 且鼠标落在它的视野中
        if (target != null)
        {
            var revealer = target.GetComponent<FOW.FogOfWarRevealer3D>();
            if (revealer != null && IsMouseInRevealerVision(revealer, ray))
            {
                return true;
            }
        }

        // ✅ 2. 射线穿过任意角色的视野碰撞体
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in players)
        {
            if (player.visionCollider != null)
            {
                Bounds visionBounds = player.visionCollider.bounds;
                if (visionBounds.IntersectRay(ray))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsMouseInRevealerVision(FOW.FogOfWarRevealer3D revealer, Ray ray)
    {
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(distance);
            Vector3 eyePos = revealer.GetEyePosition();

            float radius = revealer.ViewRadius;

            // ✅ 如果开启了软化距离，扩大视野范围
            if (FOW.FogOfWarWorld.instance.UsingSoftening)
            {
                radius += revealer.RevealHiderInFadeOutZonePercentage * revealer.SoftenDistance;
            }

            return Vector3.Distance(mouseWorldPos, eyePos) <= radius;
        }
        return false;
    }






    /// <summary>
    /// 使用多条线段近似绘制一个圆形，用于可视化调试。
    /// </summary>
    private void DrawCircle(Vector3 center, float radius, Color color, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(Mathf.Cos(0), Mathf.Sin(0), 0) * radius;
        for (int i = 1; i <= segments; i++)
        {
            float angle = Mathf.Deg2Rad * (i * angleStep);
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            Debug.DrawLine(prevPoint, nextPoint, color);
            prevPoint = nextPoint;
        }
    }
    private void CleanupDestroyedHighlights()
    {
        // 检查高亮对象列表，移除已销毁的对象
        for (int i = highlightedObjects.Count - 1; i >= 0; i--)
        {
            if (highlightedObjects[i] == null)
            {
                highlightedObjects.RemoveAt(i);
            }
        }
    }


    private void HighlightObject(GameObject target)
    {
        Outline outline = target.GetComponent<Outline>();
        if (outline == null)
        {
            outline = target.AddComponent<Outline>();
        }

        // 设置高亮轮廓
        outline.enabled = true;
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineWidth = outlineWidth;

        // 根据目标类型设置高亮颜色
        if (target.CompareTag("Enemy"))
        {
            outline.OutlineColor = enemyOutlineColor;
        }
        else
        {
            outline.OutlineColor = objectOutlineColor;
        }

        // 将该对象添加到高亮列表中
        highlightedObjects.Add(target);
    }

    private void ResetAllHighlights()
    {
        // 遍历并重置所有高亮对象
        foreach (var highlightedObject in highlightedObjects)
        {
            RestoreHighlight(highlightedObject);
        }
        // 清空高亮列表
        highlightedObjects.Clear();
    }

    private void RestoreHighlight(GameObject target)
    {
        Outline outline = target.GetComponent<Outline>();
        if (outline != null)
        {
            outline.OutlineColor = (target.CompareTag("Enemy")) ? dimmedEnemyColor : dimmedObjectColor;
            outline.OutlineWidth = dimmedOutlineWidth;
        }
    }



    private void UpdateCursor(GameObject target)
    {
        // 确保目标对象仍然存在
        if (target == null)
        {
            SetDefaultCursor();
            return;
        }

        // 根据目标类型切换鼠标光标
        if (target.CompareTag("Enemy") || target.CompareTag("Obstacle"))
        {
            // 如果是敌人或障碍物，使用攻击光标
            Cursor.SetCursor(attackCursor, new Vector2(attackCursor.width / 2, attackCursor.height / 2), CursorMode.Auto);
        }
        else if (target.CompareTag("Player")) // 检测到角色
        {
            PlayerController playerController = target.GetComponent<PlayerController>();

            if (playerController != null && playerController.isKnockedDown) // 仅在倒地时切换光标
            {
                // 如果玩家倒地，使用救援光标
                Cursor.SetCursor(rescueCursor, new Vector2(rescueCursor.width / 2, rescueCursor.height / 2), CursorMode.Auto);
            }
        }
        else
        {
            // 默认情况：使用交互光标
            Cursor.SetCursor(interactCursor, new Vector2(interactCursor.width / 2, interactCursor.height / 2), CursorMode.Auto);
        }
    }



    private void SetDefaultCursor()
    {
        Cursor.SetCursor(defaultCursor, new Vector2(defaultCursor.width / 2, defaultCursor.height / 2), CursorMode.Auto);
    }
}