using UnityEngine;

public class ZSync : MonoBehaviour
{
    [Header("目标物体（提供 Z 轴参考）")]
    public Transform target; // 目标物体，提供 Z 轴基准

    private void Update()
    {
        if (target == null)
        {
            Debug.LogError("请在 Inspector 中指定目标物体（target）！");
            return;
        }

        // 同步 Z 轴值
        transform.position = new Vector3(transform.position.x, transform.position.y, target.position.z);
    }
}
