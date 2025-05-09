using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SkillRange : MonoBehaviour
{
    public float radius = 5f; // 技能范围半径
    private int segments = 50; // 圆形的细节程度
    public Material targetMaterial; // 目标材质
    private LineRenderer lineRenderer;
    private LineRenderer rayLineRenderer; // 用于显示射线的 LineRenderer
    private LineRenderer parabolicLineRenderer; // 用于显示抛物线的 LineRenderer
    private float skillRangeZ = -0.75f; // SkillRange物体的z轴位置

    private Color currentColor; // 当前的颜色
    private Color targetColor; // 目标颜色
    public float colorTransitionSpeed = 5f; // 颜色过渡的速度
    private Vector3 lastMousePosition; // 保存上一次鼠标位置

    private Image crosshairImage; // 准星图片
    public float fadeOutAlpha = 0.1f; // 离开范围时的透明度

    [Header("发射射线功能开关")]
    public bool enableRaycast = false;

    [Header("抛物线功能开关")]
    public bool enableParabolicPath = false;

    [Header("抛物线参数")]
    public float arcHeight = 0.5f; // 抛物线高度系数
    public int parabolicSegments = 20; // 抛物线分段数
    public Color parabolicColor = new Color(1f, 0.5f, 0f, 0.8f); // 抛物线颜色
    private Vector3 pathStartPosition; // 抛物线起点
    private Vector3 pathEndPosition; // 抛物线终点

    [Header("射线起点对象")]
    public GameObject rayOrigin; // 射线起点对象
    [Header("射线颜色和透明度")]
    public Color rayColor = new Color(1f, 0f, 0f, 0.5f);

    private bool isInitialized = false;

    void Awake()
    {
        InitializeComponents();
    }

    // 确保组件初始化
    void InitializeComponents()
    {
        if (isInitialized) return;

        // 创建或获取LineRenderer(圆形范围)
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            lineRenderer.positionCount = segments + 1;
            lineRenderer.useWorldSpace = false;
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;

            if (targetMaterial != null)
                lineRenderer.material = targetMaterial;
            else
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

            lineRenderer.sortingLayerName = "UI";
            lineRenderer.sortingOrder = 50;
        }

        // 创建或获取rayLineRenderer(射线)
        if (rayLineRenderer == null)
        {
            GameObject rayLineObj = transform.Find("RayLineRenderer")?.gameObject;
            if (rayLineObj == null)
            {
                rayLineObj = new GameObject("RayLineRenderer");
                rayLineObj.transform.SetParent(transform);
                rayLineObj.transform.localPosition = Vector3.zero;
            }

            rayLineRenderer = rayLineObj.GetComponent<LineRenderer>();
            if (rayLineRenderer == null)
            {
                rayLineRenderer = rayLineObj.AddComponent<LineRenderer>();
            }

            rayLineRenderer.startWidth = 0.05f;
            rayLineRenderer.endWidth = 0.05f;
            rayLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            rayLineRenderer.material.color = rayColor;
            rayLineRenderer.positionCount = 2;
        }

        // 创建或获取parabolicLineRenderer(抛物线)
        if (parabolicLineRenderer == null)
        {
            GameObject parabolicLineObj = transform.Find("ParabolicLineRenderer")?.gameObject;
            if (parabolicLineObj == null)
            {
                parabolicLineObj = new GameObject("ParabolicLineRenderer");
                parabolicLineObj.transform.SetParent(transform);
                parabolicLineObj.transform.localPosition = Vector3.zero;
            }

            parabolicLineRenderer = parabolicLineObj.GetComponent<LineRenderer>();
            if (parabolicLineRenderer == null)
            {
                parabolicLineRenderer = parabolicLineObj.AddComponent<LineRenderer>();
            }

            parabolicLineRenderer.startWidth = 0.04f;
            parabolicLineRenderer.endWidth = 0.04f;
            parabolicLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            parabolicLineRenderer.material.color = parabolicColor;
            parabolicLineRenderer.positionCount = parabolicSegments;
            parabolicLineRenderer.useWorldSpace = true;
        }

        currentColor = targetColor = Color.white;
        if (lineRenderer.material != null)
        {
            currentColor = targetColor = lineRenderer.material.color;
        }

        // 创建圆形
        CreateCircle();

        // 默认禁用射线和抛物线
        if (rayLineRenderer != null) rayLineRenderer.enabled = false;
        if (parabolicLineRenderer != null) parabolicLineRenderer.enabled = false;

        isInitialized = true;
    }

    void OnEnable()
    {
        // 确保组件已初始化
        InitializeComponents();

        // 寻找并设置准星图片
        if (crosshairImage == null)
        {
            GameObject crosshairCanvas = GameObject.Find("Crosshair");
            if (crosshairCanvas != null)
            {
                crosshairImage = crosshairCanvas.GetComponentInChildren<Image>();
                if (crosshairImage != null)
                {
                    crosshairImage.enabled = true; // 显示准星
                }
            }
        }
        else
        {
            crosshairImage.enabled = true;
        }
    }

    void OnDisable()
    {
        if (crosshairImage != null)
        {
            crosshairImage.enabled = false; // 隐藏准星
        }
    }

    void Update()
    {
        // 确保所有组件都已初始化
        if (!isInitialized)
        {
            InitializeComponents();
            return;
        }

        // 检查鼠标位置变化
        if (Input.mousePosition != lastMousePosition)
        {
            CheckMousePosition();
            lastMousePosition = Input.mousePosition;
        }

        // 更新颜色过渡
        if (lineRenderer != null && lineRenderer.material != null)
        {
            currentColor = Color.Lerp(currentColor, targetColor, colorTransitionSpeed * Time.deltaTime);
            lineRenderer.material.color = currentColor;
        }

        // 处理射线
        if (enableRaycast && !enableParabolicPath) // 如果启用抛物线，则禁用射线
        {
            HandleRaycast();
        }
        else if (rayLineRenderer != null)
        {
            rayLineRenderer.enabled = false;
        }

        // 处理抛物线
        if (enableParabolicPath)
        {
            HandleParabolicPath();
        }
        else if (parabolicLineRenderer != null)
        {
            parabolicLineRenderer.enabled = false;
        }
    }

    private void HandleRaycast()
    {
        if (rayLineRenderer == null || Camera.main == null) return;

        rayLineRenderer.enabled = true;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector3 rangeCenter = rayOrigin != null ? rayOrigin.transform.position : transform.position;

        // 发射一条从技能中心到鼠标光标的射线
        RaycastHit2D hit = Physics2D.Raycast(rangeCenter, (worldMousePos - rangeCenter).normalized, radius);

        // 设置射线可视化
        rayLineRenderer.SetPosition(0, rangeCenter);
        rayLineRenderer.SetPosition(1, worldMousePos);

        if (hit.collider != null)
        {
            Debug.Log($"射线碰到目标: {hit.collider.name}");
        }
    }

    private void HandleParabolicPath()
    {
        if (parabolicLineRenderer == null || Camera.main == null) return;

        parabolicLineRenderer.enabled = true;

        // 获取起点位置 (从指定的rayOrigin或使用技能范围的位置)
        Vector3 startPos = rayOrigin != null ? rayOrigin.transform.position : transform.position;

        // 获取鼠标位置作为终点
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 endPos = Camera.main.ScreenToWorldPoint(mousePos);

        // 限制终点在技能范围内
        Vector2 direction = new Vector2(endPos.x - startPos.x, endPos.y - startPos.y);
        float distance = direction.magnitude;

        if (distance > radius)
        {
            direction = direction.normalized;
            endPos = new Vector3(
                startPos.x + direction.x * radius,
                startPos.y + direction.y * radius,
                endPos.z
            );
        }

        // 更新抛物线路径
        UpdateParabolicPath(startPos, endPos);

        // 保存当前路径点
        pathStartPosition = startPos;
        pathEndPosition = endPos;
    }

    private void UpdateParabolicPath(Vector3 start, Vector3 end)
    {
        // 确保线渲染器已设置
        if (parabolicLineRenderer == null) return;

        Vector3[] positions = new Vector3[parabolicSegments];

        // 计算路径点
        for (int i = 0; i < parabolicSegments; i++)
        {
            float t = i / (float)(parabolicSegments - 1);

            // 线性插值得到X和Z坐标
            float x = Mathf.Lerp(start.x, end.x, t);
            float z = Mathf.Lerp(start.z, end.z, t);

            // 根据抛物线方程计算Y坐标
            float y = Mathf.Lerp(start.y, end.y, t) + arcHeight * Mathf.Sin(t * Mathf.PI);

            positions[i] = new Vector3(x, y, z);
        }

        // 设置线渲染器路径
        parabolicLineRenderer.positionCount = parabolicSegments;
        parabolicLineRenderer.SetPositions(positions);
    }

    void CreateCircle()
    {
        if (lineRenderer == null)
        {
            Debug.LogError("CreateCircle: LineRenderer为空");
            return;
        }

        try
        {
            float angleStep = 360f / segments;
            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.Deg2Rad * i * angleStep;
                float x = Mathf.Sin(angle) * radius;
                float z = Mathf.Cos(angle) * radius;

                lineRenderer.SetPosition(i, new Vector3(x, 0, z));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"创建技能范围圆形时出错: {e.Message}\n{e.StackTrace}");
        }
    }

    void CheckMousePosition()
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane skillPlane = new Plane(Vector3.forward, new Vector3(0, 0, skillRangeZ));

        if (skillPlane.Raycast(ray, out float distance))
        {
            Vector3 worldMousePos = ray.GetPoint(distance);
            float distanceToSkillCenter = Vector3.Distance(new Vector3(worldMousePos.x, worldMousePos.y, 0), new Vector3(transform.position.x, transform.position.y, 0));

            bool isWithinRange = distanceToSkillCenter <= radius;

            if (crosshairImage != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldMousePos);
                crosshairImage.rectTransform.position = screenPos;

                Color crosshairColor = crosshairImage.color;
                crosshairColor.a = isWithinRange ? 1f : fadeOutAlpha;
                crosshairImage.color = crosshairColor;
            }

            targetColor = isWithinRange
                ? new Color(0.83f, 0.6f, 0.1f, 0.85f)
                : new Color(0.65f, 0.7f, 0.7f, 0.1f);
        }
    }

    public void SetRadius(float num)
    {
        radius = num;

        // 确保组件已初始化
        if (!isInitialized)
        {
            InitializeComponents();
        }

        // 额外检查
        if (lineRenderer == null)
        {
            Debug.LogError("SetRadius: LineRenderer为空");
            return;
        }

        CreateCircle();
    }

    public void EnableRaycast(bool enable)
    {
        enableRaycast = enable;
        enableParabolicPath = false; // 禁用抛物线功能

        if (rayLineRenderer == null) return;

        if (!enable)
        {
            rayLineRenderer.enabled = false;
        }
        else
        {
            rayLineRenderer.enabled = true;
        }
    }

    public void EnableParabolicPath(bool enable, float arcHeightValue = 0.5f)
    {
        enableParabolicPath = enable;
        enableRaycast = false; // 禁用射线功能
        arcHeight = arcHeightValue;

        if (parabolicLineRenderer == null) return;

        if (!enable)
        {
            parabolicLineRenderer.enabled = false;
        }
        else
        {
            parabolicLineRenderer.enabled = true;
        }
    }

    // 设置抛物线起点
    public void SetPathOrigin(Transform origin)
    {
        if (origin != null)
        {
            rayOrigin = origin.gameObject;
        }
    }

    // 更新抛物线终点
    public void UpdatePathEndPoint(Vector3 endPoint)
    {
        if (parabolicLineRenderer != null && rayOrigin != null)
        {
            pathEndPosition = endPoint;
            UpdateParabolicPath(rayOrigin.transform.position, endPoint);
        }
    }

    // 检查抛物线功能是否启用
    public bool IsParabolicPathEnabled()
    {
        return enableParabolicPath;
    }

    // 强制刷新抛物线
    public void ForceUpdatePath()
    {
        if (parabolicLineRenderer != null && rayOrigin != null && enableParabolicPath)
        {
            Vector3 startPos = rayOrigin.transform.position;
            UpdateParabolicPath(startPos, pathEndPosition);
        }
    }
}