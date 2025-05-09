using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Event5_SaveSniper : BaseEvent
{
    public GameObject targetSniper; // �ѻ��ֶ���
    public CapsuleCollider2D sniperCollider;
    public Rigidbody2D sniperRigidbody;
    public TypewriterColorJitterEffect typewriterEffect;
    public float textDisplayDelay = 3f; // ÿ���ı���ʾ֮��ļ��

    public TextMeshProUGUI anotherTextMeshPro;

    // �������ã�UI���󣬰���CanvasGroup��test������
    public GameObject uiObject;

    // �������ã�ָ��GameObject
    public GameObject specifiedGameObject;

    // ��������߽��������
    public PolygonCollider2D updatedCameraBounds;

    private CameraController cameraController;

    private void Start()
    {
        if (targetSniper != null)
        {
            sniperCollider = targetSniper.GetComponent<CapsuleCollider2D>();
            sniperRigidbody = targetSniper.GetComponent<Rigidbody2D>();
        }
        else
        {
            Debug.LogError("Ŀ��ѻ��ֶ���δָ����");
        }

        if (typewriterEffect == null)
        {
            typewriterEffect = FindObjectOfType<TypewriterColorJitterEffect>();
            if (typewriterEffect == null)
            {
                Debug.LogWarning("δ�ҵ�TypewriterColorJitterEffect���ı���ʹ��Debug.Log��ʾ��");
            }
        }

        if (anotherTextMeshPro == null)
        {
            Debug.LogWarning("δ����anotherTextMeshPro��������ʾ�ı���");
        }

        cameraController = FindObjectOfType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("δ�ҵ�CameraController���޷���������߽硣");
        }
    }

    public override void EnableEvent()
    {
        Debug.Log("��ʼ���Ⱦѻ����¼�");

        // ������ײ��
        if (sniperCollider != null)
        {
            sniperCollider.enabled = true;
        }

        // ȡ��Rigidbody2Dλ�ö���Լ��
        if (sniperRigidbody != null)
        {
            sniperRigidbody.constraints = RigidbodyConstraints2D.None;
            sniperRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // ��ʼЭ���¼�����
        StartCoroutine(SaveSniperProcess());
    }

    private IEnumerator SaveSniperProcess()
    {
        // ��ȡCanvasGroup����alphaΪ1
        if (uiObject != null)
        {
            CanvasGroup cg = uiObject.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
            }
            else
            {
                Debug.LogWarning("uiObject��δ�ҵ�CanvasGroup��");
            }

            // �ر���Ϊ"test"��������
            Transform testChild = uiObject.transform.Find("test");
            if (testChild != null)
            {
                testChild.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("δ�ҵ���Ϊ 'test' �������塣");
            }
        }
        else
        {
            Debug.LogWarning("uiObjectδ���ã�");
        }

        // ��һ���ı������ı���ɫ��
        string firstMessage = "����һ��<color=#FF0000>������</color>�ĸ�Ա�����Ƹ�Ա�Ҽ����������<color=#00FF00>��Ԯ</color>��Ŀ�ꡣ";
        yield return DisplayText(firstMessage, useTypewriter: true);

        // �ȴ�Ŀ��ѻ��ִӵ���״̬�ָ�
        PlayerController sniperController = targetSniper.GetComponent<PlayerController>();
        if (sniperController == null)
        {
            Debug.LogError("Ŀ��ѻ���ȱ�� PlayerController �ű����޷��жϵ���״̬��");
            EndEvent();
            yield break;
        }

        // �ȴ��ѻ���isKnockedDownΪfalse�Ҳ�Ϊdead
        yield return new WaitUntil(() => !sniperController.isKnockedDown && !sniperController.isDead);
        Transform[] allTransforms = FindObjectsOfType<Transform>(true);
        foreach (Transform sniperframe in allTransforms)
        {
            if (sniperframe.name == "PlayerFrame_2")
            {
                sniperframe.gameObject.SetActive(true);
                break;
            }
        }
        // ��ʾ��Ŀ����������tau��primeվ�����硱�ڶ������ı������
        string rescueMessage = "Ŀ������������������tau��primeվ�����硣ְ�ܣ��ѻ���";
        if (anotherTextMeshPro != null)
        {
            anotherTextMeshPro.text = rescueMessage;
            anotherTextMeshPro.gameObject.SetActive(true);

            // ��ʾһ��ʱ�������
            yield return new WaitForSeconds(textDisplayDelay);
            anotherTextMeshPro.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log(rescueMessage);
        }

        // ��ʾ���ƽ̳��ı�
        string tutorialMessage = "ʹ�ò�ǹ�ָ�Ա�ļ���<color=#FFFF00>������ǹ��[��ݼ�W]</color>��ѡ�в�<color=#00FF00>����</color>Ŀ���Ѿ���";
        // ����ָ����GameObject
        if (specifiedGameObject != null)
        {
            specifiedGameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("ָ����GameObjectδ���ã�");
        }
        yield return DisplayText(tutorialMessage, useTypewriter: true);

        string infoBoxMessage2 = "���<color=#FFFF00>��Աͷ��</color>��ʹ�����ֿ�ݼ�<color=#FFFF00>1��2��3��4</color>���ɿ���ѡ��Ŀ���Ա��";
        yield return DisplayText(infoBoxMessage2, useTypewriter: true);
        // ��������߽�
        UpdateCameraBounds();

        // �¼�����
        EndEvent();
        // �ȴ����ƽ̳����
        yield return WaitForHealing(sniperController);

        // ��ʾ������ɺ����Ϣ����ʾ

           

        // ��������߽�
        UpdateCameraBounds();

        // �¼�����
        EndEvent();
    }

    private void UpdateCameraBounds()
    {
        if (cameraController != null && updatedCameraBounds != null)
        {
            cameraController.SetCameraBounds(updatedCameraBounds);
            Debug.Log("����߽��Ѹ��¡�");
        }
        else
        {
            Debug.LogWarning("�޷���������߽磬δ����CameraController����µı߽硣");
        }
    }

    private IEnumerator WaitForHealing(PlayerController sniperController)
    {
        // �ȴ��ѻ��ֵ��������
        yield return new WaitUntil(() => sniperController.health >= sniperController.attribute.maxhealth);

        // ֹͣ�̳���ز���
        Debug.Log("���ƽ̳���ɣ�");
    }

    private IEnumerator DisplayText(string text, bool useTypewriter = false)
    {
        if (useTypewriter && typewriterEffect != null)
        {
            typewriterEffect.SetText(text);
            // �ȴ��������
            while (typewriterEffect.isTyping)
            {
                yield return null;
            }
        }
        else
        {
            // ���δʹ��typewriter��û��typewriterEffect����ֱ�����
            Debug.Log(text);
        }

        // �ȴ�һ��ʱ���ټ�����һ���ı�
        yield return new WaitForSeconds(textDisplayDelay);
    }
}
