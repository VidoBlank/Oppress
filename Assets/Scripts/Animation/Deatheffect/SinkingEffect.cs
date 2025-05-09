using System.Collections;
using UnityEngine;

public class SinkingEffect : MonoBehaviour
{
    [Header("下沉参数")]
    public float sinkStartDelay = 2.0f; // 下沉开始的延迟时间
    public float sinkingDuration = 3.0f; // 下沉的持续时间
    public float sinkSpeed = 0.5f; // 下沉速度

    [Header("压缩参数")]
    public bool enableYCompression = false; // 是否启用 Y 轴压缩
    public float compressionDelay = 1.5f; // 压缩开始的延迟时间
    public float compressionDuration = 1.5f; // 压缩的持续时间

    private Vector3 initialScale; // 记录初始缩放
    private bool isCompressing = false; // 是否正在压缩标志

    private void Start()
    {
        initialScale = transform.localScale; // 记录初始缩放
    }

    // 暴露方法供外部调用
    public void StartSinkingEffect()
    {
        StartCoroutine(SinkAndCompress());
    }

    private IEnumerator SinkAndCompress()
    {
        // 等待下沉开始的延迟
        yield return new WaitForSeconds(sinkStartDelay);

        float elapsedTime = 0f;

        // 开始下沉
        while (elapsedTime < sinkingDuration)
        {
            transform.position += Vector3.down * sinkSpeed * Time.deltaTime;
            elapsedTime += Time.deltaTime;

            // 如果启用了 Y 轴压缩且达到压缩延迟时间，则开始压缩
            if (enableYCompression && elapsedTime >= compressionDelay && !isCompressing)
            {
                isCompressing = true;
                StartCoroutine(CompressYScale());
            }

            yield return null;
        }

        // 下沉完成后销毁对象
        Destroy(gameObject);
    }

    private IEnumerator CompressYScale()
    {
        float compressElapsed = 0f;
        float targetYScale = initialScale.y * 0.25f; // 目标 Y 轴缩放值，使其扁平化

        // 逐渐压缩 Y 轴 scale
        while (compressElapsed < compressionDuration)
        {
            float newYScale = Mathf.Lerp(initialScale.y, targetYScale, compressElapsed / compressionDuration);
            transform.localScale = new Vector3(initialScale.x, newYScale, initialScale.z);
            compressElapsed += Time.deltaTime;
            yield return null;
        }

        // 确保 Y 轴完全压缩到目标值
        transform.localScale = new Vector3(initialScale.x, targetYScale, initialScale.z);
    }
}
