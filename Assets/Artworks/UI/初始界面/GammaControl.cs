using UnityEngine;
using UnityEngine.UI;

public class GammaControl : MonoBehaviour
{
    [Header("Ŀ��ͼƬ")]
    public Image targetImage; // ��Ҫ�������ȵ�ͼƬ

    [Header("٤�����������")]
    public Slider gammaSlider; // UI �����������ڿ���٤��ֵ

    [Header("٤��Χ")]
    public float minGamma = 1.0f; // ٤��ֵ��Сֵ
    public float maxGamma = 2.2f; // ٤��ֵ���ֵ

    [Header("������ɫ")]
    public Color baseColor = Color.white; // ͼƬ��ԭʼ��ɫ

    private void Start()
    {
        // ȷ�� Slider ����
        if (gammaSlider == null)
        {
            Debug.LogError("Gamma Slider δ���ã�");
            return;
        }

        // ��ʼ�� Slider ��ֵ���¼�����
        gammaSlider.value = 0.5f; // Ĭ��ֵ���м�
        gammaSlider.onValueChanged.AddListener(UpdateGamma);

        // ����Ӧ��٤��ֵ
        UpdateGamma(gammaSlider.value);
    }

    /// <summary>
    /// ���� Slider ��ֵ��̬����٤��ֵ
    /// </summary>
    /// <param name="sliderValue">��������ǰֵ</param>
    public void UpdateGamma(float sliderValue)
    {
        if (targetImage == null)
        {
            Debug.LogError("Ŀ��ͼƬδ���ã�");
            return;
        }

        // ���㵱ǰ٤��ֵ
        float gammaValue = Mathf.Lerp(minGamma, maxGamma, sliderValue);

        // ������ɫ����
        float r = Mathf.Pow(baseColor.r, 1f / gammaValue);
        float g = Mathf.Pow(baseColor.g, 1f / gammaValue);
        float b = Mathf.Pow(baseColor.b, 1f / gammaValue);

        // ����ͼƬ��ɫ
        targetImage.color = new Color(r, g, b, baseColor.a);
        Debug.Log($"Gamma: {gammaValue}, Brightness Updated.");
    }
}
