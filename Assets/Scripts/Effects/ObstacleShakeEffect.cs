using UnityEngine;
using System.Collections;

public class ObstacleImpactVibration : MonoBehaviour
{
    [Header("��Ŀ��")]
    public GameObject vibrationObject; // Ҫ�񶯵Ķ�����δָ����Ĭ��ʹ�ñ�����

    [Header("�񶯲���")]
    public float vibrationDuration = 0.3f;    // �񶯳���ʱ��
    public float vibrationIntensity = 0.05f;  // ���X��λ��
    public float frequency = 20f;             // ��Ƶ��

    private Obstacle obstacle;   // �����ϰ������
    private float lastHealth;    // ��һ�ε�����ֵ��¼
    private bool isVibrating = false; // ����Ƿ�������

    private void Start()
    {
        // ��ȡͬһ�����ϵ� Obstacle ���
        obstacle = GetComponent<Obstacle>();
        if (obstacle == null)
        {
            Debug.LogError("ObstacleImpactVibration ��Ҫ�����ں��� Obstacle ����� GameObject �ϣ�");
            enabled = false;
            return;
        }

        // ��δָ���񶯶�����Ĭ��ʹ�õ�ǰ����
        if (vibrationObject == null)
        {
            vibrationObject = gameObject;
        }

        // ��ʼ������ֵ��¼
        lastHealth = obstacle.currentHealth;
    }

    private void Update()
    {
        // �������ֵ�Ƿ񽵵ͣ��ܵ��˺���
        if (obstacle.currentHealth < lastHealth)
        {
            if (!isVibrating)
            {
                StartCoroutine(Vibrate());
            }
        }
        lastHealth = obstacle.currentHealth;
    }

    private IEnumerator Vibrate()
    {
        isVibrating = true;
        // ��¼��ʼ�ֲ�λ�ã�ʹ�� localPosition �ɱ����ܵ�������Ӱ��
        Vector3 originalPos = vibrationObject.transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < vibrationDuration)
        {
            // ʹ��һ����ʱ��˥����ϵ����ʹ���𽥼���
            float damping = 1 - (elapsedTime / vibrationDuration);
            // ���������񶯵�X��ƫ�ƣ�2��Ƶ��ת����
            float xOffset = Mathf.Sin(elapsedTime * frequency * Mathf.PI * 2) * vibrationIntensity * damping;
            vibrationObject.transform.localPosition = originalPos + new Vector3(xOffset, 0, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �񶯽�����λ
        vibrationObject.transform.localPosition = originalPos;
        isVibrating = false;
    }
}
