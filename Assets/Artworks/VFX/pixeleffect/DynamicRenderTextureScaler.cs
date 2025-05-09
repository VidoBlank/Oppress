using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class DynamicRenderTextureScaler : MonoBehaviour
{
    public CameraController cameraController; // 引用 CameraController
    public RawImage rawImage; // 目标 RawImage
    public Camera cameraToUpdate; // 引用需要更新的 Camera

    private RenderTexture renderTexture;

    void Start()
    {
        if (rawImage == null)
        {
            rawImage = GetComponent<RawImage>();
        }

        if (cameraController == null)
        {
            Debug.LogError("CameraController 未分配！");
            return;
        }

        if (cameraToUpdate == null)
        {
            Debug.LogError("未指定 Camera 引用！");
            return;
        }

        // 创建初始的 RenderTexture
        CreateRenderTexture();
    }

    void Update()
    {
        // 如果相机控制器或 RawImage 未初始化，跳过更新
        if (cameraController == null || rawImage == null || cameraToUpdate == null) return;

        // 更新 RenderTexture
        UpdateRenderTexture();
    }

    void CreateRenderTexture()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();  // 释放现有的 RenderTexture
        }

        // 获取 RawImage 当前的尺寸（宽度和高度）
        int width = Mathf.RoundToInt(rawImage.rectTransform.rect.width);
        int height = Mathf.RoundToInt(rawImage.rectTransform.rect.height);

        // 创建初始的 RenderTexture，并设置为 R8G8B8A8_UNorm 格式
        renderTexture = new RenderTexture(width, height, 24, GraphicsFormat.R8G8B8A8_UNorm);
        renderTexture.filterMode = FilterMode.Point;  // 设置为 Point 模式
        renderTexture.Create();

        // 将 RenderTexture 应用到 RawImage
        if (rawImage != null)
        {
            rawImage.texture = renderTexture;
        }

        // 设置 Camera 的 targetTexture 为新的 RenderTexture
        if (cameraToUpdate != null)
        {
            cameraToUpdate.targetTexture = renderTexture;
        }
    }

    void UpdateRenderTexture()
    {
        // 获取 RawImage 当前的尺寸（宽度和高度）
        int width = Mathf.RoundToInt(rawImage.rectTransform.rect.width);
        int height = Mathf.RoundToInt(rawImage.rectTransform.rect.height);

        // 获取 FOV 范围和当前 FOV
        float minFOV = cameraController.minFOV;
        float maxFOV = cameraController.maxFOV;
        float currentFOV = cameraController.GetCurrentFov();

        // 计算 FOV 变化比例
        float ratio = Mathf.InverseLerp(minFOV, maxFOV, currentFOV);

        // 根据 FOV 比例动态计算 RenderTexture 的宽高
        int dynamicWidth = Mathf.RoundToInt(Mathf.Lerp(width * 0.625f, width, ratio));
        int dynamicHeight = Mathf.RoundToInt(Mathf.Lerp(height * 0.625f, height, ratio));

        // 如果分辨率没有变化，则不更新
        if (renderTexture != null && renderTexture.width == dynamicWidth && renderTexture.height == dynamicHeight)
            return;

        // 更新 RenderTexture 大小
        if (renderTexture != null)
        {
            renderTexture.Release(); // 释放之前的 RenderTexture
        }

        renderTexture = new RenderTexture(dynamicWidth, dynamicHeight, 24, GraphicsFormat.R8G8B8A8_UNorm);
        renderTexture.filterMode = FilterMode.Point;  // 设置为 Point 模式
        renderTexture.Create();

        // 更新 RawImage 的纹理
        if (rawImage != null)
        {
            rawImage.texture = renderTexture;
        }

        // 更新 Camera 的 targetTexture
        if (cameraToUpdate != null)
        {
            cameraToUpdate.targetTexture = renderTexture;
        }
    }
}
