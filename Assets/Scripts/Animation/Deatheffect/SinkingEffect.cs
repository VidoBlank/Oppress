using System.Collections;
using UnityEngine;

public class SinkingEffect : MonoBehaviour
{
    [Header("�³�����")]
    public float sinkStartDelay = 2.0f; // �³���ʼ���ӳ�ʱ��
    public float sinkingDuration = 3.0f; // �³��ĳ���ʱ��
    public float sinkSpeed = 0.5f; // �³��ٶ�

    [Header("ѹ������")]
    public bool enableYCompression = false; // �Ƿ����� Y ��ѹ��
    public float compressionDelay = 1.5f; // ѹ����ʼ���ӳ�ʱ��
    public float compressionDuration = 1.5f; // ѹ���ĳ���ʱ��

    private Vector3 initialScale; // ��¼��ʼ����
    private bool isCompressing = false; // �Ƿ�����ѹ����־

    private void Start()
    {
        initialScale = transform.localScale; // ��¼��ʼ����
    }

    // ��¶�������ⲿ����
    public void StartSinkingEffect()
    {
        StartCoroutine(SinkAndCompress());
    }

    private IEnumerator SinkAndCompress()
    {
        // �ȴ��³���ʼ���ӳ�
        yield return new WaitForSeconds(sinkStartDelay);

        float elapsedTime = 0f;

        // ��ʼ�³�
        while (elapsedTime < sinkingDuration)
        {
            transform.position += Vector3.down * sinkSpeed * Time.deltaTime;
            elapsedTime += Time.deltaTime;

            // ��������� Y ��ѹ���Ҵﵽѹ���ӳ�ʱ�䣬��ʼѹ��
            if (enableYCompression && elapsedTime >= compressionDelay && !isCompressing)
            {
                isCompressing = true;
                StartCoroutine(CompressYScale());
            }

            yield return null;
        }

        // �³���ɺ����ٶ���
        Destroy(gameObject);
    }

    private IEnumerator CompressYScale()
    {
        float compressElapsed = 0f;
        float targetYScale = initialScale.y * 0.25f; // Ŀ�� Y ������ֵ��ʹ���ƽ��

        // ��ѹ�� Y �� scale
        while (compressElapsed < compressionDuration)
        {
            float newYScale = Mathf.Lerp(initialScale.y, targetYScale, compressElapsed / compressionDuration);
            transform.localScale = new Vector3(initialScale.x, newYScale, initialScale.z);
            compressElapsed += Time.deltaTime;
            yield return null;
        }

        // ȷ�� Y ����ȫѹ����Ŀ��ֵ
        transform.localScale = new Vector3(initialScale.x, targetYScale, initialScale.z);
    }
}
