using UnityEngine;
using UnityEngine.UI;

public class GammaControl : MonoBehaviour
{
    [Header("目标图片")]
    public Image targetImage; // 需要调整亮度的图片

    [Header("伽马调整滑动条")]
    public Slider gammaSlider; // UI 滑动条，用于控制伽马值

    [Header("伽马范围")]
    public float minGamma = 1.0f; // 伽马值最小值
    public float maxGamma = 2.2f; // 伽马值最大值

    [Header("基础颜色")]
    public Color baseColor = Color.white; // 图片的原始颜色

    private void Start()
    {
        // 确保 Slider 存在
        if (gammaSlider == null)
        {
            Debug.LogError("Gamma Slider 未设置！");
            return;
        }

        // 初始化 Slider 的值和事件监听
        gammaSlider.value = 0.5f; // 默认值在中间
        gammaSlider.onValueChanged.AddListener(UpdateGamma);

        // 初次应用伽马值
        UpdateGamma(gammaSlider.value);
    }

    /// <summary>
    /// 根据 Slider 的值动态更新伽马值
    /// </summary>
    /// <param name="sliderValue">滑动条当前值</param>
    public void UpdateGamma(float sliderValue)
    {
        if (targetImage == null)
        {
            Debug.LogError("目标图片未设置！");
            return;
        }

        // 计算当前伽马值
        float gammaValue = Mathf.Lerp(minGamma, maxGamma, sliderValue);

        // 调整颜色亮度
        float r = Mathf.Pow(baseColor.r, 1f / gammaValue);
        float g = Mathf.Pow(baseColor.g, 1f / gammaValue);
        float b = Mathf.Pow(baseColor.b, 1f / gammaValue);

        // 更新图片颜色
        targetImage.color = new Color(r, g, b, baseColor.a);
        Debug.Log($"Gamma: {gammaValue}, Brightness Updated.");
    }
}
