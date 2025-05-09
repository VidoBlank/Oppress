using UnityEngine;
using System.Collections;

public class Marker : MonoBehaviour
{
    [Header("特效 Prefabs")]
    public GameObject enemyEffectPrefab;
    public GameObject objectEffectPrefab;
    public GameObject obstacleEffectPrefab;
    public GameObject groundEffectPrefab;

    [Header("特效位置偏移")]
    public Vector3 effectOffset = new Vector3(0f, -0.5f, 0f); // 默认偏移量

    [Header("Layer Settings")]
    public LayerMask interactableLayers; // 设置检测的图层
    public LayerMask groundLayer; // 设置 Ground 层的图层

    [Header("射线设置")]
    public float yAxisSpacing = 0.5f; // 每条射线在 Y 轴上的间距
    public int maxRayCount = 10; // 最大射线数量
    public float maxRayLength = 10f; // 射线的最大长度

    // 用来判断是否在视野内
    public bool isInSight = false;

    private void Update()
    {
        // 检测鼠标右键点击
        if (Input.GetMouseButtonDown(1))
        {
            DetectClickAndPlayEffect();
        }
    }

    private void DetectClickAndPlayEffect()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(Camera.main.transform.position.z); // 计算相机深度
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition); // 将鼠标位置转换为世界坐标
        worldPosition.z = 0; // 在 2D 空间中将 Z 轴设为 0

        // 发射射线检测鼠标位置是否穿过任何 visionCollider
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        // 计算鼠标世界位置
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        if (!plane.Raycast(ray, out float dist)) return;
        Vector3 mouseWorldPos = ray.GetPoint(dist);

        // ✅ 是否在任意 FogOfWarRevealer3D 视野中（包括 soften distance）
        bool isMouseInVision = false;

        var revealers = FindObjectsOfType<FOW.FogOfWarRevealer3D>();
        foreach (var revealer in revealers)
        {
            Vector3 eyePos = revealer.GetEyePosition();
            float radius = revealer.ViewRadius;

            // 如果启用了 soften distance，则扩大范围
            if (FOW.FogOfWarWorld.instance.UsingSoftening)
            {
                radius += revealer.RevealHiderInFadeOutZonePercentage * revealer.SoftenDistance;
            }

            if (Vector3.Distance(mouseWorldPos, eyePos) <= radius)
            {
                isMouseInVision = true;
                break;
            }
        }

        isInSight = isMouseInVision;


        // 发射第一条射线，检测与目标的交互
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, Mathf.Infinity, interactableLayers);
        Debug.DrawRay(worldPosition, Vector3.forward * 10, Color.red, 0.5f);

        if (hit.collider != null)
        {
            // 如果检测到物体，根据目标类型播放特效
            Debug.Log($"Hit object: {hit.collider.gameObject.name}, Tag: {hit.collider.tag}");
            PlayEffectBasedOnTarget(hit.collider.gameObject, worldPosition);
        }
        else
        {
            // 沿 Y 轴方向发射后续射线阵列
            FireRayArrayAlongYAxis(worldPosition);
        }

        // 打印是否在视野内
        Debug.Log($"Is in sight: {isInSight}");
    }


    private bool IsMouseRayPassingThroughVisionCollider(Vector3 mousePosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        // 获取所有 PlayerController
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in players)
        {
            if (player.visionCollider != null)
            {
                // 获取 visionCollider 的边界框
                Bounds visionBounds = player.visionCollider.bounds;

                // 检测射线是否与 visionCollider 发生交叉
                if (visionBounds.IntersectRay(ray))
                {
                    return true; // 只要射线穿过任意一个 visionCollider，则返回 true
                }
            }
        }

        return false; // 没有穿过任何 visionCollider
    }

    private void FireRayArrayAlongYAxis(Vector3 startPosition)
    {
        for (int i = 1; i <= maxRayCount; i++)
        {
            // 计算每条射线的起点（沿 Y 轴递减）
            Vector3 rayStartPosition = startPosition + new Vector3(0, -i * yAxisSpacing, 0);

            // 发射射线检测 Ground 层
            RaycastHit2D groundHit = Physics2D.Raycast(rayStartPosition, Vector2.zero, maxRayLength, groundLayer);

            // 绘制射线可视化
            Debug.DrawRay(rayStartPosition, Vector2.zero * maxRayLength, Color.green, 0.5f);

            if (groundHit.collider != null)
            {
                // 如果检测到 Ground 层，播放特效并停止后续射线
                Debug.Log($"Ground detected at {groundHit.point}");
                PlayEffect(groundHit.point, groundEffectPrefab);
                break;
            }
        }
    }

    public void PlayEffectBasedOnTarget(GameObject target, Vector3 mousePosition)
    {
        // 如果不在视野内，只触发射线阵列
        if (!isInSight)
        {
            FireRayArrayAlongYAxis(mousePosition);
            return; // 直接返回，不执行后续逻辑
        }

        // 正常逻辑：当在视野内时，播放特效
        if (target.CompareTag("Enemy"))
        {
            Bounds bounds = target.GetComponent<Collider2D>().bounds;
            Vector3 bottomCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            PlayEffect(bottomCenter + effectOffset, enemyEffectPrefab);
        }
        else if (target.CompareTag("Object"))
        {
            Bounds bounds = target.GetComponent<Collider2D>().bounds;
            Vector3 bottomCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            PlayEffect(bottomCenter + effectOffset, objectEffectPrefab);
        }
        else if (target.CompareTag("Obstacle"))
        {
            Bounds bounds = target.GetComponent<Collider2D>().bounds;
            Vector3 bottomCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            PlayEffect(bottomCenter + effectOffset, obstacleEffectPrefab);
        }
    }


    public void PlayEffect(Vector3 position, GameObject effectPrefab)
    {
        if (effectPrefab != null)
        {
            // 创建新的特效对象（不销毁当前的 effectObject）
            GameObject newEffectObject = Instantiate(effectPrefab, position + effectOffset, Quaternion.identity);

            // 检查是否包含粒子系统
            ParticleSystem particleSystem = newEffectObject.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                StartCoroutine(DestroyEffectAfterParticle(newEffectObject, particleSystem));
            }
            else
            {
                StartCoroutine(DestroyEffectAfterTime(newEffectObject, 2f)); // 默认播放时间
            }
        }
    }

    private IEnumerator DestroyEffectAfterTime(GameObject effectObject, float time)
    {
        yield return new WaitForSeconds(time);

        // 销毁特效对象
        if (effectObject != null)
        {
            Destroy(effectObject);
        }
    }

    private IEnumerator DestroyEffectAfterParticle(GameObject effectObject, ParticleSystem particleSystem)
    {
        // 等待粒子系统播放完成
        yield return new WaitUntil(() => !particleSystem.IsAlive(true));

        // 销毁特效对象
        if (effectObject != null)
        {
            Destroy(effectObject);
        }
    }
}
