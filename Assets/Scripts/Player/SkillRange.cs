using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SkillRange : MonoBehaviour
{
    public float radius = 5f; // ���ܷ�Χ�뾶
    private int segments = 50; // Բ�ε�ϸ�ڳ̶�
    public Material targetMaterial; // Ŀ�����
    private LineRenderer lineRenderer;
    private LineRenderer rayLineRenderer; // ������ʾ���ߵ� LineRenderer
    private LineRenderer parabolicLineRenderer; // ������ʾ�����ߵ� LineRenderer
    private float skillRangeZ = -0.75f; // SkillRange�����z��λ��

    private Color currentColor; // ��ǰ����ɫ
    private Color targetColor; // Ŀ����ɫ
    public float colorTransitionSpeed = 5f; // ��ɫ���ɵ��ٶ�
    private Vector3 lastMousePosition; // ������һ�����λ��

    private Image crosshairImage; // ׼��ͼƬ
    public float fadeOutAlpha = 0.1f; // �뿪��Χʱ��͸����

    [Header("�������߹��ܿ���")]
    public bool enableRaycast = false;

    [Header("�����߹��ܿ���")]
    public bool enableParabolicPath = false;

    [Header("�����߲���")]
    public float arcHeight = 0.5f; // �����߸߶�ϵ��
    public int parabolicSegments = 20; // �����߷ֶ���
    public Color parabolicColor = new Color(1f, 0.5f, 0f, 0.8f); // ��������ɫ
    private Vector3 pathStartPosition; // ���������
    private Vector3 pathEndPosition; // �������յ�

    [Header("����������")]
    public GameObject rayOrigin; // ����������
    [Header("������ɫ��͸����")]
    public Color rayColor = new Color(1f, 0f, 0f, 0.5f);

    private bool isInitialized = false;

    void Awake()
    {
        InitializeComponents();
    }

    // ȷ�������ʼ��
    void InitializeComponents()
    {
        if (isInitialized) return;

        // �������ȡLineRenderer(Բ�η�Χ)
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

        // �������ȡrayLineRenderer(����)
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

        // �������ȡparabolicLineRenderer(������)
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

        // ����Բ��
        CreateCircle();

        // Ĭ�Ͻ������ߺ�������
        if (rayLineRenderer != null) rayLineRenderer.enabled = false;
        if (parabolicLineRenderer != null) parabolicLineRenderer.enabled = false;

        isInitialized = true;
    }

    void OnEnable()
    {
        // ȷ������ѳ�ʼ��
        InitializeComponents();

        // Ѱ�Ҳ�����׼��ͼƬ
        if (crosshairImage == null)
        {
            GameObject crosshairCanvas = GameObject.Find("Crosshair");
            if (crosshairCanvas != null)
            {
                crosshairImage = crosshairCanvas.GetComponentInChildren<Image>();
                if (crosshairImage != null)
                {
                    crosshairImage.enabled = true; // ��ʾ׼��
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
            crosshairImage.enabled = false; // ����׼��
        }
    }

    void Update()
    {
        // ȷ������������ѳ�ʼ��
        if (!isInitialized)
        {
            InitializeComponents();
            return;
        }

        // ������λ�ñ仯
        if (Input.mousePosition != lastMousePosition)
        {
            CheckMousePosition();
            lastMousePosition = Input.mousePosition;
        }

        // ������ɫ����
        if (lineRenderer != null && lineRenderer.material != null)
        {
            currentColor = Color.Lerp(currentColor, targetColor, colorTransitionSpeed * Time.deltaTime);
            lineRenderer.material.color = currentColor;
        }

        // ��������
        if (enableRaycast && !enableParabolicPath) // ������������ߣ����������
        {
            HandleRaycast();
        }
        else if (rayLineRenderer != null)
        {
            rayLineRenderer.enabled = false;
        }

        // ����������
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

        // ����һ���Ӽ������ĵ�����������
        RaycastHit2D hit = Physics2D.Raycast(rangeCenter, (worldMousePos - rangeCenter).normalized, radius);

        // �������߿��ӻ�
        rayLineRenderer.SetPosition(0, rangeCenter);
        rayLineRenderer.SetPosition(1, worldMousePos);

        if (hit.collider != null)
        {
            Debug.Log($"��������Ŀ��: {hit.collider.name}");
        }
    }

    private void HandleParabolicPath()
    {
        if (parabolicLineRenderer == null || Camera.main == null) return;

        parabolicLineRenderer.enabled = true;

        // ��ȡ���λ�� (��ָ����rayOrigin��ʹ�ü��ܷ�Χ��λ��)
        Vector3 startPos = rayOrigin != null ? rayOrigin.transform.position : transform.position;

        // ��ȡ���λ����Ϊ�յ�
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 endPos = Camera.main.ScreenToWorldPoint(mousePos);

        // �����յ��ڼ��ܷ�Χ��
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

        // ����������·��
        UpdateParabolicPath(startPos, endPos);

        // ���浱ǰ·����
        pathStartPosition = startPos;
        pathEndPosition = endPos;
    }

    private void UpdateParabolicPath(Vector3 start, Vector3 end)
    {
        // ȷ������Ⱦ��������
        if (parabolicLineRenderer == null) return;

        Vector3[] positions = new Vector3[parabolicSegments];

        // ����·����
        for (int i = 0; i < parabolicSegments; i++)
        {
            float t = i / (float)(parabolicSegments - 1);

            // ���Բ�ֵ�õ�X��Z����
            float x = Mathf.Lerp(start.x, end.x, t);
            float z = Mathf.Lerp(start.z, end.z, t);

            // ���������߷��̼���Y����
            float y = Mathf.Lerp(start.y, end.y, t) + arcHeight * Mathf.Sin(t * Mathf.PI);

            positions[i] = new Vector3(x, y, z);
        }

        // ��������Ⱦ��·��
        parabolicLineRenderer.positionCount = parabolicSegments;
        parabolicLineRenderer.SetPositions(positions);
    }

    void CreateCircle()
    {
        if (lineRenderer == null)
        {
            Debug.LogError("CreateCircle: LineRendererΪ��");
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
            Debug.LogError($"�������ܷ�ΧԲ��ʱ����: {e.Message}\n{e.StackTrace}");
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

        // ȷ������ѳ�ʼ��
        if (!isInitialized)
        {
            InitializeComponents();
        }

        // ������
        if (lineRenderer == null)
        {
            Debug.LogError("SetRadius: LineRendererΪ��");
            return;
        }

        CreateCircle();
    }

    public void EnableRaycast(bool enable)
    {
        enableRaycast = enable;
        enableParabolicPath = false; // ���������߹���

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
        enableRaycast = false; // �������߹���
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

    // �������������
    public void SetPathOrigin(Transform origin)
    {
        if (origin != null)
        {
            rayOrigin = origin.gameObject;
        }
    }

    // �����������յ�
    public void UpdatePathEndPoint(Vector3 endPoint)
    {
        if (parabolicLineRenderer != null && rayOrigin != null)
        {
            pathEndPosition = endPoint;
            UpdateParabolicPath(rayOrigin.transform.position, endPoint);
        }
    }

    // ��������߹����Ƿ�����
    public bool IsParabolicPathEnabled()
    {
        return enableParabolicPath;
    }

    // ǿ��ˢ��������
    public void ForceUpdatePath()
    {
        if (parabolicLineRenderer != null && rayOrigin != null && enableParabolicPath)
        {
            Vector3 startPos = rayOrigin.transform.position;
            UpdateParabolicPath(startPos, pathEndPosition);
        }
    }
}