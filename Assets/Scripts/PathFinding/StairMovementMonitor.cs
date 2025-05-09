using System.Collections;
using UnityEngine;

public class StairMovementMonitor : MonoBehaviour
{
    private Rigidbody2D rb;
    private Unit unit;
    private Coroutine monitorCoroutine;
    private float lastYPosition;

    [Header("监控参数")]
    public float checkInterval = 0.1f; // 检测间隔
    public float maxNoChangeTime = 0.5f; // 允许 Y 坐标不变的最长时间
    public float positionThreshold = 0.05f; // 允许的最小 Y 坐标变化

    private void Start()
    {
        // 获取 Rigidbody2D 组件
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning($"{gameObject.name} 没有 Rigidbody2D，正在添加...");
            rb = gameObject.AddComponent<Rigidbody2D>();
            
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // 获取 Unit 组件
        unit = GetComponent<Unit>();
        if (unit == null)
        {
            Debug.LogError($"{gameObject.name} 没有 Unit 组件，监控器无法工作！");
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (unit.stairMoving && monitorCoroutine == null)
        {
            StartStairMovementMonitor();
        }
    }

    /// <summary>
    /// 启动楼梯运动监控协程
    /// </summary>
    public void StartStairMovementMonitor()
    {
        if (monitorCoroutine != null)
            StopCoroutine(monitorCoroutine);

        lastYPosition = rb.position.y; // 记录初始 Y 位置
        Debug.Log($"🔍 监控开始: 初始 Y = {lastYPosition}");

        monitorCoroutine = StartCoroutine(MonitorStairMovement());
    }

    /// <summary>
    /// 监控单位的 Y 轴变化，如果在一定时间内未变化，则重置 stairMoving
    /// </summary>
    private IEnumerator MonitorStairMovement()
    {
        float noChangeDuration = 0f;

        while (unit.stairMoving)
        {
            yield return new WaitForSeconds(checkInterval);

            float currentY = rb.position.y;
            Debug.Log($"🟡 监控中: 当前 Y = {currentY}, 之前的 Y = {lastYPosition}, 变化量 = {Mathf.Abs(currentY - lastYPosition)}");

            // Y 轴变化小于 positionThreshold，认为单位未移动
            if (Mathf.Abs(currentY - lastYPosition) < positionThreshold)
            {
                noChangeDuration += checkInterval;
                if (noChangeDuration >= maxNoChangeTime)
                {
                    Debug.Log("⚠️ Y 轴未变化，可能卡住，重置 stairMoving 状态");
                    unit.stairMoving = false;
                    unit.MovingToTarget(); // 重新寻路
                    break;
                }
            }
            else
            {
                noChangeDuration = 0f; // Y 轴有变化，重置计时
            }

            lastYPosition = currentY;
        }

        monitorCoroutine = null;
    }
}
