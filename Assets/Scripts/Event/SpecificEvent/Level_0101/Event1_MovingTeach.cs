using System.Collections;
using UnityEngine;

public class Event1_MovingTeach : BaseEvent
{
    private CameraController cameraController;
    public TypewriterColorJitterEffect typewriterEffect; // ��ѧ�ı���ʾЧ��
    public string[] tutorialTexts; // ��ѧ�ı�����
    public float textDisplayDelay = 2f; // ÿ���ı���ʾ֮��ļ��

    public GameObject objectToEnable; // Ҫ���õ�����
    public GameObject uiObject; //׼�����õ�UI
    public GameObject objectToMove; // Ҫ�ƶ�������
    public AudioSource audioSource;
    public AudioSource audioSource2;
    public float moveDistance = 1.93f; // �����ƶ��ľ���
    public float moveDuration = 1.5f; // �ƶ�����ʱ��
    public PolygonCollider2D tutorialCameraBounds; // �µ�����߽�

    public float newMinFOV = 10f; // ���������С FOV
    public float newMaxFOV = 30f; // ���������� FOV

    private Outline[] outlines; // �洢����1���������Outline���
    private bool isObjectMoved = false; // �ж������Ƿ��ƶ����


    private void Start()
    {
        typewriterEffect = FindObjectOfType<TypewriterColorJitterEffect>();
        cameraController = FindObjectOfType<CameraController>();

        if (cameraController == null)
        {
            Debug.LogError("CameraController δ�ҵ�����ȷ���������������������");
        }
    }

    public override void EnableEvent()
    {
        Debug.Log("��ʼ�ƶ���ѧ");

        // ������ѧ����
        StartCoroutine(MovingTeachProcess());
    }

    private IEnumerator MovingTeachProcess()
    {
        // ��ʾϵͳ��ʾ�ı�
        yield return DisplayText("[ϵͳ��ʾ] ���ڽ��п���ģ��У׼��");

        // ����ָ�����岢�������������Outline
        if (objectToEnable != null)
        {
            objectToEnable.SetActive(true);
            outlines = objectToEnable.GetComponentsInChildren<Outline>();
            foreach (var outline in outlines)
            {
                outline.enabled = false;
            }
        }

        // ��ʼ�ƶ���һ������
        if (objectToMove != null)
        {
            yield return StartCoroutine(MoveObject());
        }
    }

    private IEnumerator MoveObject()
    {
        Vector3 startPosition = objectToMove.transform.position;
        Vector3 targetPosition = startPosition + Vector3.up * moveDistance;
        audioSource.Play();
        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            objectToMove.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
            yield return null;
        }

        objectToMove.transform.position = targetPosition;
        isObjectMoved = true; // ��������ƶ����

        // ��������1��Outline���
        if (outlines != null)
        {
            foreach (var outline in outlines)
            {
                outline.enabled = true;
            }
        }

        

        // ��ʾ�ƶ���ѧ�ı�
        yield return StartCoroutine(ShowMovingTutorial());

    }

    private void SetTutorialCameraBoundsAndFOV()
    {
        if (cameraController != null)
        {
            // ��������߽�
            if (tutorialCameraBounds != null)
            {
                cameraController.SetCameraBounds(tutorialCameraBounds);
                Debug.Log("����߽��Ѹ��¡�");
            }

            // ������� FOV ��Χ
            cameraController.minFOV = newMinFOV;
            cameraController.maxFOV = newMaxFOV;
            Debug.Log($"���FOV��Χ�Ѹ���: ��С {newMinFOV}, ��� {newMaxFOV}");
        }
    }

    private IEnumerator ShowMovingTutorial()
    {
        // ���� GameManager �µ� Player Input ���
        GameObject gameManager = GameObject.Find("Gamemanager");
        if (gameManager != null)
        {
            PlayerInput playerInput = gameManager.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = true;
                Debug.Log("Player Input ���������");
            }
        }
        // ��ʾ��ʾ�ı��������ѡ���ѡ��ɫ��
        yield return DisplayText(tutorialTexts[0]);
        // �޸�����߽��FOV
        SetTutorialCameraBoundsAndFOV();
        // �ȴ����ѡ���ɫ
        yield return new WaitUntil(() => IsPlayerSelected());
        audioSource2.Play();

        Debug.Log("��⵽��ɫ�ѱ�ѡ��");
        if (uiObject != null)
        {
            uiObject.SetActive(true);
            Debug.Log("������������: " + uiObject.name);
        }
        else
        {
            Debug.LogError("������δ���䣬���������Ƿ���ȷ");
        }

        // ��ʾ��ʾ�ı������Ҽ�����������ƽ�ɫ�ƶ���
        yield return DisplayText(tutorialTexts[1]);

        // �ȴ�����ƶ���ɫ
        yield return new WaitUntil(() => IsPlayerMoved());

        Debug.Log("��⵽��ɫ���ƶ�");
        audioSource2.Play();
        // ��ʾ����ģ��У׼���

        yield return DisplayText("���Ƹ�Ա����¥���ƶ����·�����");

        // ��ѧ����
        EndEvent();
    }

    private bool IsPlayerSelected()
    {
        //return PlayerInput.instance != null && PlayerInput.instance.selectedPlayers.Count > 0;
        return PlayerManager.instance != null && PlayerManager.instance.selectedPlayers.Count > 0;
    }

    private bool IsPlayerMoved()
    {
        if (PlayerManager.instance != null)
        {
            foreach (var player in PlayerManager.instance.selectedPlayers)
            {
                if (player.isMoving)
                {
                    return true;
                }
            }
        }
        return false;
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

            // �ȴ�һ��ʱ������ʾ��һ����ʾ
            yield return new WaitForSeconds(textDisplayDelay);
        }
        else
        {
            Debug.LogWarning("TypewriterEffect δ�ҵ���");
        }
    }
}
