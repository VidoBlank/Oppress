using System.Collections;
using UnityEngine;

public class Event2_DestroyBarrier : BaseEvent
{
    public GameObject barrier;
    public PolygonCollider2D updatedCameraBounds; // �µ�����߽�


    public string messageAfterDestroy = "ʿ�����Զ�����������������Χ�ڵĵ��ˡ�";

    // �µ�FOV����
    public float newMinFOV = 10f;
    public float newMaxFOV = 30f;

    private CameraController cameraController;
    public TypewriterColorJitterEffect typewriterEffect;

    private void Start()
    {
        cameraController = FindObjectOfType<CameraController>();
        typewriterEffect = FindObjectOfType<TypewriterColorJitterEffect>();

        if (cameraController == null)
        {
            Debug.LogError("CameraController δ�ҵ�����ȷ���������������������");
        }
        if (typewriterEffect == null)
        {
            Debug.LogWarning("TypewriterColorJitterEffect δ�ҵ����ı���ʾ���޷�ʹ�ô��ֻ�Ч����");
        }
    }

    public override void EnableEvent()
    {
        Debug.Log("��ʼ�ݻ��ϰ��¼�");
    }

    private void Update()
    {
        // ���ϰ��ﲻ����ʱ����ʾ�ѱ��ݻ٣�
        if (!barrier)
        {
            // ��������߽�
            if (cameraController != null && updatedCameraBounds != null)
            {
                cameraController.SetCameraBounds(updatedCameraBounds);
                Debug.Log("����߽��Ѹ��¡�");
            }

            // �������FOV��Χ
            if (cameraController != null)
            {
                cameraController.minFOV = newMinFOV;
                cameraController.maxFOV = newMaxFOV;
                Debug.Log($"���FOV��Χ�Ѹ���: ��С {newMinFOV}, ��� {newMaxFOV}");
            }

            // ��ʾ�ı���ʾ
            StartCoroutine(DisplayText(messageAfterDestroy));
            EndEvent();
        }
    }

    private IEnumerator DisplayText(string text)
    {
        if (typewriterEffect != null)
        {
            typewriterEffect.SetText(text);

            // �ȴ��ı�����Ч�����
            while (typewriterEffect.isTyping)
            {
                yield return null;
            }

        }
    }
}
