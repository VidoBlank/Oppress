using System.Collections;
using UnityEngine;

public class StairMovingHandler : MonoBehaviour
{
    [Header("目标物体")]
    public GameObject targetObject; // 指定的目标物体，检测其 stairMoving 状态

    [Header("需要修改位置的物体")]
    public GameObject affectedObject; // 需要改变 position.z 的物体

    [Header("Z轴值设置")]
    public float stairMovingZValue = 4.48f; // stairMoving 时的 Z 轴值

    private float originalZValue; // 记录原始的 Z 轴值
    private Unit targetUnit; // 用于获取目标的 Unit 组件

    private void Start()
    {
        if (targetObject == null || affectedObject == null)
        {
            Debug.LogError("目标物体或需要修改的物体未设置！");
            enabled = false;
            return;
        }

        targetUnit = targetObject.GetComponent<Unit>();
        if (targetUnit == null)
        {
            Debug.LogError("目标物体未找到 Unit 组件！");
            enabled = false;
            return;
        }

        originalZValue = affectedObject.transform.position.z;
    }

    private void Update()
    {
        if (targetUnit.stairMoving)
        {
            // 修改 Z 轴值为指定值
            Vector3 newPosition = affectedObject.transform.position;
            newPosition.z = stairMovingZValue;
            affectedObject.transform.position = newPosition;
        }
        else
        {
            // 恢复 Z 轴的原始值
            Vector3 originalPosition = affectedObject.transform.position;
            originalPosition.z = originalZValue;
            affectedObject.transform.position = originalPosition;
        }
    }
}
