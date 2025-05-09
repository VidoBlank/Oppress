using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class AdjustRawImageToResolution : MonoBehaviour
{
    private RawImage rawImage;

    void Start()
    {
        rawImage = GetComponent<RawImage>();

        // 调整 RawImage 的宽高
        AdjustToResolution();
    }

    /// <summary>
    /// 根据当前场景分辨率调整 RawImage 的宽高
    /// </summary>
    void AdjustToResolution()
    {
        // 获取当前屏幕分辨率
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        // 获取 RectTransform
        RectTransform rectTransform = rawImage.rectTransform;

        // 设置宽高为屏幕分辨率
        rectTransform.sizeDelta = new Vector2(screenWidth, screenHeight);

        Debug.Log($"Adjusted RawImage size to: {screenWidth}x{screenHeight}");
    }

    void Update()
    {
        // 如果分辨率可能发生变化，可在 Update 中检查并动态调整
        if (Screen.width != rawImage.rectTransform.sizeDelta.x || Screen.height != rawImage.rectTransform.sizeDelta.y)
        {
            AdjustToResolution();
        }
    }
}
