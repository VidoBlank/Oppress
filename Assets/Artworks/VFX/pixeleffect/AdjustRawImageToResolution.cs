using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class AdjustRawImageToResolution : MonoBehaviour
{
    private RawImage rawImage;

    void Start()
    {
        rawImage = GetComponent<RawImage>();

        // ���� RawImage �Ŀ��
        AdjustToResolution();
    }

    /// <summary>
    /// ���ݵ�ǰ�����ֱ��ʵ��� RawImage �Ŀ��
    /// </summary>
    void AdjustToResolution()
    {
        // ��ȡ��ǰ��Ļ�ֱ���
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        // ��ȡ RectTransform
        RectTransform rectTransform = rawImage.rectTransform;

        // ���ÿ��Ϊ��Ļ�ֱ���
        rectTransform.sizeDelta = new Vector2(screenWidth, screenHeight);

        Debug.Log($"Adjusted RawImage size to: {screenWidth}x{screenHeight}");
    }

    void Update()
    {
        // ����ֱ��ʿ��ܷ����仯������ Update �м�鲢��̬����
        if (Screen.width != rawImage.rectTransform.sizeDelta.x || Screen.height != rawImage.rectTransform.sizeDelta.y)
        {
            AdjustToResolution();
        }
    }
}
