using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class DynamicRenderTextureScaler : MonoBehaviour
{
    public CameraController cameraController; // ���� CameraController
    public RawImage rawImage; // Ŀ�� RawImage
    public Camera cameraToUpdate; // ������Ҫ���µ� Camera

    private RenderTexture renderTexture;

    void Start()
    {
        if (rawImage == null)
        {
            rawImage = GetComponent<RawImage>();
        }

        if (cameraController == null)
        {
            Debug.LogError("CameraController δ���䣡");
            return;
        }

        if (cameraToUpdate == null)
        {
            Debug.LogError("δָ�� Camera ���ã�");
            return;
        }

        // ������ʼ�� RenderTexture
        CreateRenderTexture();
    }

    void Update()
    {
        // �������������� RawImage δ��ʼ������������
        if (cameraController == null || rawImage == null || cameraToUpdate == null) return;

        // ���� RenderTexture
        UpdateRenderTexture();
    }

    void CreateRenderTexture()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();  // �ͷ����е� RenderTexture
        }

        // ��ȡ RawImage ��ǰ�ĳߴ磨��Ⱥ͸߶ȣ�
        int width = Mathf.RoundToInt(rawImage.rectTransform.rect.width);
        int height = Mathf.RoundToInt(rawImage.rectTransform.rect.height);

        // ������ʼ�� RenderTexture��������Ϊ R8G8B8A8_UNorm ��ʽ
        renderTexture = new RenderTexture(width, height, 24, GraphicsFormat.R8G8B8A8_UNorm);
        renderTexture.filterMode = FilterMode.Point;  // ����Ϊ Point ģʽ
        renderTexture.Create();

        // �� RenderTexture Ӧ�õ� RawImage
        if (rawImage != null)
        {
            rawImage.texture = renderTexture;
        }

        // ���� Camera �� targetTexture Ϊ�µ� RenderTexture
        if (cameraToUpdate != null)
        {
            cameraToUpdate.targetTexture = renderTexture;
        }
    }

    void UpdateRenderTexture()
    {
        // ��ȡ RawImage ��ǰ�ĳߴ磨��Ⱥ͸߶ȣ�
        int width = Mathf.RoundToInt(rawImage.rectTransform.rect.width);
        int height = Mathf.RoundToInt(rawImage.rectTransform.rect.height);

        // ��ȡ FOV ��Χ�͵�ǰ FOV
        float minFOV = cameraController.minFOV;
        float maxFOV = cameraController.maxFOV;
        float currentFOV = cameraController.GetCurrentFov();

        // ���� FOV �仯����
        float ratio = Mathf.InverseLerp(minFOV, maxFOV, currentFOV);

        // ���� FOV ������̬���� RenderTexture �Ŀ��
        int dynamicWidth = Mathf.RoundToInt(Mathf.Lerp(width * 0.625f, width, ratio));
        int dynamicHeight = Mathf.RoundToInt(Mathf.Lerp(height * 0.625f, height, ratio));

        // ����ֱ���û�б仯���򲻸���
        if (renderTexture != null && renderTexture.width == dynamicWidth && renderTexture.height == dynamicHeight)
            return;

        // ���� RenderTexture ��С
        if (renderTexture != null)
        {
            renderTexture.Release(); // �ͷ�֮ǰ�� RenderTexture
        }

        renderTexture = new RenderTexture(dynamicWidth, dynamicHeight, 24, GraphicsFormat.R8G8B8A8_UNorm);
        renderTexture.filterMode = FilterMode.Point;  // ����Ϊ Point ģʽ
        renderTexture.Create();

        // ���� RawImage ������
        if (rawImage != null)
        {
            rawImage.texture = renderTexture;
        }

        // ���� Camera �� targetTexture
        if (cameraToUpdate != null)
        {
            cameraToUpdate.targetTexture = renderTexture;
        }
    }
}
