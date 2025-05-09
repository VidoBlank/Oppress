using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class ManualAttackVisualizer : MonoBehaviour
{
    private PlayerController playerController;
    private LineRenderer lineRenderer;
    private GameObject crosshairInstance;  // 每个角色独立的准星实例
    private Image crosshairImage;

    public GameObject crosshairPrefab;  // 场景中设置的准星预制体
    public Material lineMaterial;
    public float fadeOutAlpha = 0.1f;
    private GameObject lastHighlightedObject; // 上一个被高亮的物体
    public Dictionary<Outline, Color> originalOutlineColors = new Dictionary<Outline, Color>();
    // 全局跟踪被高亮的对象及其状态
    private static HashSet<Outline> globallyHighlightedOutlines = new HashSet<Outline>();
    // 全局跟踪高亮对象及其引用计数
    private static Dictionary<Outline, int> highlightedOutlineReferenceCounts = new Dictionary<Outline, int>();


    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("未找到 PlayerController，确保该脚本挂载在含有 PlayerController 的对象上。");
            return;
        }

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 50;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.useWorldSpace = false;


        if (lineMaterial != null)
        {
            lineRenderer.material = lineMaterial;
        }

        lineRenderer.sortingLayerName = "UI";
        lineRenderer.sortingOrder = 50;
    }
    private void HandleHighlight()
    {
        if (!playerController.isManualAttacking)
        {
            ResetLastHighlight();
            return;
        }

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;

        Vector3 rangeCenter = transform.position;
        float radius = playerController.attribute.attackRange.radius;
        float distanceToCenter = Vector3.Distance(worldPos, rangeCenter);

        if (distanceToCenter <= radius)
        {
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("Enemy", "Obstacle"));
            Debug.DrawRay(worldPos, Vector3.forward * 10f, Color.green, 0.1f);

            if (hit.collider != null)
            {
                if (lastHighlightedObject != hit.collider.gameObject)
                {
                    ResetLastHighlight();

                    lastHighlightedObject = hit.collider.gameObject;
                    Outline[] outlines = lastHighlightedObject.GetComponentsInChildren<Outline>();
                    foreach (var outline in outlines)
                    {
                        // 增加引用计数
                        if (highlightedOutlineReferenceCounts.ContainsKey(outline))
                        {
                            highlightedOutlineReferenceCounts[outline]++;
                        }
                        else
                        {
                            highlightedOutlineReferenceCounts[outline] = 1;

                            // 记录原始颜色
                            if (!originalOutlineColors.ContainsKey(outline))
                            {
                                originalOutlineColors[outline] = outline.OutlineColor;
                            }

                            // 设置为高亮
                            outline.OutlineColor = new Color(1f, 1f, 1f, 1f);
                            outline.enabled = true;
                        }
                    }
                }
            }
            else
            {
                ResetLastHighlight();
            }
        }
        else
        {
            ResetLastHighlight();
        }
    }







    public void ResetLastHighlight()
    {
        if (lastHighlightedObject != null)
        {
            Outline[] outlines = lastHighlightedObject.GetComponentsInChildren<Outline>();
            foreach (var outline in outlines)
            {
                if (highlightedOutlineReferenceCounts.ContainsKey(outline))
                {
                    highlightedOutlineReferenceCounts[outline]--;

                    // 只有当引用计数为零时重置高亮
                    if (highlightedOutlineReferenceCounts[outline] <= 0)
                    {
                        highlightedOutlineReferenceCounts.Remove(outline);

                        if (originalOutlineColors.ContainsKey(outline))
                        {
                            // 恢复原始颜色
                            outline.OutlineColor = originalOutlineColors[outline];
                        }
                        else
                        {
                            // 如果没有记录，设置为透明
                            outline.OutlineColor = new Color(1f, 1f, 1f, 0f);
                        }
                    }
                }
            }

            // 清空记录
            originalOutlineColors.Clear();
            lastHighlightedObject = null;
        }
    }


    private void Update()
    {
        if (playerController.isManualAttacking)
        {
            if (crosshairInstance == null && crosshairPrefab != null)
            {
                crosshairInstance = Instantiate(crosshairPrefab, transform);
                crosshairImage = crosshairInstance.GetComponentInChildren<Image>();
            }

            if (crosshairImage != null)
            {
                crosshairImage.enabled = true;
            }

            UpdateAttackRangeVisualization();
            UpdateCrosshairPosition();
            HandleHighlight();
        }
        else
        {
            if (crosshairImage != null)
            {
                crosshairImage.enabled = false;
            }

            if (crosshairInstance != null)
            {
                Destroy(crosshairInstance);
            }

            lineRenderer.enabled = false;

            if (playerController.isMoving || playerController.isUsingSkill || playerController.isInteracting || playerController.isKnockedDown)
            {
                ResetLastHighlight();
            }
        }
    }


    private void UpdateAttackRangeVisualization()
    {
        float radius = playerController.attribute.attackRange.radius;
        int numPoints = 100;
        lineRenderer.positionCount = numPoints + 1;
        float angleStep = 360f / numPoints;

        for (int i = 0; i < numPoints; i++)
        {
            float angle = Mathf.Deg2Rad * i * angleStep;
            float x = Mathf.Sin(angle) * radius;
            float y = Mathf.Cos(angle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, y, -0.75f));
        }

        lineRenderer.SetPosition(numPoints, lineRenderer.GetPosition(0));
        lineRenderer.enabled = true;
    }

    private void UpdateCrosshairPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane skillPlane = new Plane(Vector3.forward, Vector3.zero);

        if (skillPlane.Raycast(ray, out float distance))
        {
            Vector3 worldMousePos = ray.GetPoint(distance);
            crosshairImage.transform.position = Camera.main.WorldToScreenPoint(worldMousePos);

            float distanceToCenter = Vector3.Distance(worldMousePos, playerController.attribute.attackRange.transform.position);
            float alpha = (distanceToCenter <= playerController.attribute.attackRange.radius) ? 1f : fadeOutAlpha;
            Color crosshairColor = crosshairImage.color;
            crosshairColor.a = alpha;
            crosshairImage.color = crosshairColor;
        }
    }

    private void OnDestroy()
    {
        if (crosshairInstance != null)
        {
            Destroy(crosshairInstance);
        }
    }


    private Coroutine resetCoroutine;

    public void DelayedResetLastHighlight(float delay)
    {
        // 如果已有一个重置协程在运行，先停止它
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }

        // 启动新的延迟重置协程
        resetCoroutine = StartCoroutine(ResetHighlightAfterDelay(delay));
    }

    private IEnumerator ResetHighlightAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        ResetLastHighlight();

        // 重置完成后清除协程引用
        resetCoroutine = null;
    }
















}
