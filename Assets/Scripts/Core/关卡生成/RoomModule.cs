using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ConnectionType
{
    Top,
    Bottom,
    Left,
    Right
}

public class RoomModule : MonoBehaviour
{
    // 连接点（预制体中放置在房间边缘，枢轴建议放在中心）
    public Transform topPoint;
    public Transform bottomPoint;
    public Transform leftPoint;
    public Transform rightPoint;

    // 新增：各面墙体引用（在 Inspector 中拖入对应子物体）
    public GameObject topWall;
    public GameObject bottomWall;
    public GameObject leftWall;
    public GameObject rightWall;

    // 新增：格子宽高，供 LevelGenerator 中读取
    [Tooltip("此房间在网格中的格子宽度（连接点到连接点距离）")]
    public float gridWidth = 12f;
    [Tooltip("此房间在网格中的格子高度（连接点到连接点距离）")]
    public float gridHeight = 6f;

    // 标记此房间是否为楼梯房
    public bool isStair = false;
    public int floorNumber = 0;

    /// <summary>
    /// 获取指定方向的连接点
    /// </summary>
    public Transform GetConnectionPoint(ConnectionType type)
    {
        switch (type)
        {
            case ConnectionType.Top:
                return topPoint;
            case ConnectionType.Bottom:
                return bottomPoint;
            case ConnectionType.Left:
                return leftPoint;
            case ConnectionType.Right:
                return rightPoint;
            default:
                return null;
        }
    }

    /// <summary>
    /// 禁用指定方向的墙体
    /// </summary>
    public void DisableWall(ConnectionType type)
    {
        GameObject wall = null;
        switch (type)
        {
            case ConnectionType.Top:
                wall = topWall;
                break;
            case ConnectionType.Bottom:
                wall = bottomWall;
                break;
            case ConnectionType.Left:
                wall = leftWall;
                break;
            case ConnectionType.Right:
                wall = rightWall;
                break;
        }
        if (wall != null)
            wall.SetActive(false);
    }

    /// <summary>
    /// 检查指定方向的墙体是否完好（未被禁用）
    /// </summary>
    public bool IsWallIntact(ConnectionType dir)
    {
        switch (dir)
        {
            case ConnectionType.Left: return leftWall == null || leftWall.activeSelf;
            case ConnectionType.Right: return rightWall == null || rightWall.activeSelf;
            case ConnectionType.Top: return topWall == null || topWall.activeSelf;
            case ConnectionType.Bottom: return bottomWall == null || bottomWall.activeSelf;
            default: return true;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DrawConnectionGizmo(topPoint, Color.cyan, "Top");
        DrawConnectionGizmo(bottomPoint, Color.magenta, "Bottom");
        DrawConnectionGizmo(leftPoint, Color.green, "Left");
        DrawConnectionGizmo(rightPoint, Color.red, "Right");

        // 可视化格子范围
        if (leftPoint != null && rightPoint != null)
        {
            float w = gridWidth;
            Vector3 center = (leftPoint.position + rightPoint.position) * 0.5f;
            Handles.color = Color.yellow;
            Handles.DrawWireCube(center, new Vector3(w, gridHeight, 0));
        }
    }

    private void DrawConnectionGizmo(Transform point, Color color, string label)
    {
        if (point == null) return;
        Gizmos.color = color;
        Gizmos.DrawSphere(point.position, 0.15f);
        Handles.Label(point.position + Vector3.up * 0.3f, $"<size=10><b>{label}</b></size>");
    }
#endif
}