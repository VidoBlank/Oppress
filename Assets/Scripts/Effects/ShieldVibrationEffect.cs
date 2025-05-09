using System.Collections;
using UnityEngine;

public class ShieldVibrationEffect : MonoBehaviour
{
    [Header("��Ŀ��")]
    public GameObject vibrationObject; // Ҫ�񶯵Ķ�����δָ����Ĭ��ʹ�ñ�����

    [Header("�񶯲���")]
    public float vibrationDuration = 0.3f;    // �񶯳���ʱ��
    public float vibrationIntensity = 0.05f;  // ���X��λ��
    public float frequency = 20f;             // ��Ƶ��

    private Shield shield;         // ���� Shield ���
    private float lastHP;          // ��һ֡�Ļ���Ѫ��
    private bool isVibrating = false;

    private void Start()
    {
        // ��ȡͬһ�����ϵ� Shield ���
        shield = GetComponent<Shield>();
        if (shield == null)
        {
            Debug.LogError("ShieldVibrationEffect ��Ҫ�����ں��� Shield ����� GameObject �ϣ�");
            enabled = false;
            return;
        }

        // ��δָ���񶯶�����Ĭ��ʹ�õ�ǰ����
        if (vibrationObject == null)
        {
            vibrationObject = gameObject;
        }

        // ��ʼ��Ѫ����¼
        lastHP = shield.CurrentHP;
    }

    private void Update()
    {
        // ��⻤��Ѫ���Ƿ񽵵ͣ����ܵ��˺���
        if (shield.CurrentHP < lastHP)
        {
            if (!isVibrating)
            {
                StartCoroutine(Vibrate());
            }
        }
        lastHP = shield.CurrentHP;
    }

    private IEnumerator Vibrate()
    {
        isVibrating = true;
        // ��¼��ʼ�ֲ�λ�ã�ʹ�� localPosition �����ܸ�����Ӱ��
        Vector3 originalPos = vibrationObject.transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < vibrationDuration)
        {
            // ˥��ϵ���������𽥼���
            float damping = 1 - (elapsedTime / vibrationDuration);
            // ���������񶯵�X��ƫ�ƣ�Ƶ��ת��Ϊ 2�� * frequency��
            float xOffset = Mathf.Sin(elapsedTime * frequency * Mathf.PI * 2) * vibrationIntensity * damping;
            vibrationObject.transform.localPosition = originalPos + new Vector3(xOffset, 0, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �𶯽�����λ
        vibrationObject.transform.localPosition = originalPos;
        isVibrating = false;
    }
}
