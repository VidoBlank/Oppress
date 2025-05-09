using System.Collections;
using UnityEngine;

public class StairZSyncEnemy : MonoBehaviour
{
    private Enemy enemy; // 替换 PlayerController 为 Enemy
    private Transform stairParent; // 记录触发的 StairTrigger 的父物体
    private bool isSyncing = false; // 防止重复操作

    [Header("需要同步Z值的目标物体")]
    public Transform syncTarget; // 你想要同步Z值的物体

    [Header("stairMoving 结束后恢复的 Z 值")]
    public float resetZValue = 0f; // 在 stairMoving 结束后恢复的固定 Z 值

    [Header("stairMoving 期间的缩放大小")]
    public Vector3 enlargedScale = new Vector3(1.2f, 1.2f, 1.2f); // 放大后的大小

    public Vector3 originalScale; // 记录原始缩放值

    private void Start()
    {
        enemy = GetComponent<Enemy>(); // 获取 Enemy 组件

        if (syncTarget == null)
        {
            Debug.LogError("请在 Inspector 中指定需要同步Z值的目标物体！");
        }
        else
        {
            originalScale = syncTarget.localScale; // 记录原始缩放值
        }
    }

    private void Update()
    {
        if (enemy == null || syncTarget == null)
            return;

        if (enemy.stairMoving && stairParent != null)
        {
            // 在 stairMoving 期间同步 Z 轴值
            syncTarget.position = new Vector3(syncTarget.position.x, syncTarget.position.y, stairParent.position.z);

            // 放大目标物体
            syncTarget.localScale = enlargedScale;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("StairTrigger"))
        {
            StairTrigger stairTrigger = collision.GetComponent<StairTrigger>();
            if (stairTrigger != null && stairTrigger.transform.parent != null)
            {
                stairParent = stairTrigger.transform.parent;
            }

            // 立即更新 Z 轴和缩放（即使 stairMoving 还未触发）
            if (enemy.stairMoving)
            {
                syncTarget.position = new Vector3(syncTarget.position.x, syncTarget.position.y, stairParent.position.z);
                syncTarget.localScale = enlargedScale;
                isSyncing = true;
                StartCoroutine(MonitorStairMoving());
            }
        }
    }


    private IEnumerator MonitorStairMoving()
    {
        // 持续监测 stairMoving 是否结束
        while (enemy.stairMoving)
        {
            yield return null; // 每帧等待
        }

        // 当 stairMoving 结束，恢复为指定的 resetZValue
        syncTarget.position = new Vector3(syncTarget.position.x, syncTarget.position.y, resetZValue);

        // 恢复原始缩放
        syncTarget.localScale = originalScale;

        isSyncing = false; // 允许下一次进入楼梯时同步 Z 值
    }
}
