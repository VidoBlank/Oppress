using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReachingTargetCheck : BaseEvent
{
    [Header("Ŀ���")]
    public Transform targetPosition;

    [Header("��ʾ�ı����")]
    public TypewriterColorJitterEffect typewriterEffect;

    [Header("��Ϣ����")]
    public string[] messages;
    public int messageCount = 1;
    public float messageDisplayInterval = 3f; // �����õ��ı���ʾ���

    [Header("�������ѡ��")]
    public bool updateCameraBounds = false;
    public PolygonCollider2D newCameraBounds;

    public bool updateCameraFOV = false;
    public float newMinFOV = 15f;
    public float newMaxFOV = 60f;

    [Header("�����������")]
    public GameObject[] objectsToEnable; // ָ����Ҫ������GameObject����
    public GameObject[] objectsToDisable; // ָ����Ҫ�رյ�GameObject����

    [Header("��Ҽ������")]
    public int requiredPlayerCount = 1; // ��Ҫ�ﵽ���������

    private BoxCollider2D triggerCollider;
    private HashSet<GameObject> playersInside = new HashSet<GameObject>(); // ���ڼ�¼���봥���������
    private CameraController cameraController;

    public override void EnableEvent()
    {
        Debug.Log("��ʼ����Ƿ񵽴�Ŀ��");

        if (targetPosition != null)
        {
            transform.position = targetPosition.position;

            // ��Ӵ���������̬������С
            triggerCollider = gameObject.AddComponent<BoxCollider2D>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector2(2f, 2f); // ����ʵ�����������������С
            Debug.Log($"����������ӣ���СΪ��{triggerCollider.size}");
        }
        else
        {
            Debug.LogError("Ŀ���δ���ã�����Ŀ����Ƿ��ѷ��䡣");
        }

        // ���� CameraController
        cameraController = FindObjectOfType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogWarning("δ�ҵ� CameraController�������ع��ܿ����޷�����������");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playersInside.Add(collision.gameObject); // ��ӽ��봥���������
            Debug.Log($"��ǰ���������{playersInside.Count}/{requiredPlayerCount}");

            if (playersInside.Count >= requiredPlayerCount)
            {
                TriggerEvent();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playersInside.Remove(collision.gameObject); // �Ƴ��뿪�����������
            Debug.Log($"��ǰ���������{playersInside.Count}/{requiredPlayerCount}");
        }
    }

    private void TriggerEvent()
    {
        Debug.Log("��⵽�㹻����ҵ���Ŀ���");

        if (updateCameraBounds && newCameraBounds != null && cameraController != null)
        {
            cameraController.SetCameraBounds(newCameraBounds);
            Debug.Log("����߽��Ѹ��¡�");
        }

        if (updateCameraFOV && cameraController != null)
        {
            cameraController.minFOV = newMinFOV;
            cameraController.maxFOV = newMaxFOV;
            Debug.Log($"���FOV��Χ�Ѹ��£�Min = {newMinFOV}, Max = {newMaxFOV}");
        }

        // ��������GameObject
        if (objectsToEnable != null && objectsToEnable.Length > 0)
        {
            foreach (var obj in objectsToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    Debug.Log($"{obj.name} �ѱ����á�");
                }
            }
        }

        // �����ر�GameObject
        if (objectsToDisable != null && objectsToDisable.Length > 0)
        {
            foreach (var obj in objectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    Debug.Log($"{obj.name} �ѱ����á�");
                }
            }
        }

        StartCoroutine(DisplayMultipleMessages());
        EndEvent();
    }

    private IEnumerator DisplayMultipleMessages()
    {
        if (messages == null || messages.Length == 0)
        {
            Debug.LogWarning("��Ϣ����Ϊ�գ��޷���ʾ��Ϣ��");
            yield break;
        }

        int count = Mathf.Min(messageCount, messages.Length);
        for (int i = 0; i < count; i++)
        {
            yield return DisplayText(messages[i], useTypewriter: true);
        }
    }

    private IEnumerator DisplayText(string text, bool useTypewriter = false)
    {
        if (useTypewriter && typewriterEffect != null)
        {
            typewriterEffect.SetText(text);
            while (typewriterEffect.isTyping)
            {
                yield return null;
            }
        }
        else
        {
            Debug.Log(text);
        }

        yield return new WaitForSeconds(messageDisplayInterval);
    }
}
