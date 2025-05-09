using System.Collections;
using UnityEngine;

public class Event9_0_ActivatePower : BaseEvent
{
    [Header("ָ����Ŀ������")]
    public GameObject targetObject; // ���ڴ�������������߼���Ŀ������

    [Header("ָ���ķ������")]
    public Material[] emissionMaterials; // ��Ҫ���з���Ч���Ĳ�������

    [Header("������ɲ���")]
    public Color targetEmissionColor = Color.yellow; // Ŀ�귢����ɫ����ɫ��
    public float transitionDuration = 2f;            // �������ʱ��

    [Header("���������")]
    public GameObject objectToActivate;        // Ҫ���õ�����
    public Transform moveTargetPosition;       // �ƶ���Ŀ��λ��

    private Animator targetAnimator;           // Animator ���
    private BoxCollider2D targetCollider;      // BoxCollider2D ���
    private MonoBehaviour ammoBoxComponent;    // ���� AmmoBox �� MonoBehaviour ������
    public Outline targetOutline;              // Outline ���
    public AudioSource audioSource;            // ���ڲ�����Ч����ƵԴ

    public override void EnableEvent()
    {
        Debug.Log("��ʼ���� PowerUp �¼�");

        // ����Ŀ�������߼�
        HandleTargetObject();

        // ��������ʹ����߼�
        if (emissionMaterials != null && emissionMaterials.Length > 0)
        {
            StartCoroutine(TransitionEmission());
        }
        else
        {
            Debug.LogWarning("δָ���κη�����ʣ�");
        }

        // ���������岢�ƶ�
        ActivateAndMoveObject();
    }

    private void HandleTargetObject()
    {
        if (targetObject == null)
        {
            Debug.LogError("δ����Ŀ�����壬���� Inspector ��ָ����");
            return;
        }

        // ��ȡ������ Animator ����
        targetAnimator = targetObject.GetComponent<Animator>();
        if (targetAnimator != null)
        {
            targetAnimator.SetTrigger("powerup");
            StartCoroutine(FadeInAudio());
            Debug.Log("Ŀ������ Animator �Ѵ��� powerup Trigger");
        }

        // ���� Outline ���
        targetOutline = targetObject.GetComponent<Outline>();
        if (targetOutline != null)
        {
            targetOutline.enabled = true; // ���� Outline
            Debug.Log("Ŀ������� Outline ���������");
        }
        else
        {
            Debug.LogWarning("Ŀ��������δ�ҵ� Outline �����");
        }

        // ���� AmmoBox ���
        ammoBoxComponent = targetObject.GetComponent<MonoBehaviour>();
        if (ammoBoxComponent != null)
        {
            ammoBoxComponent.enabled = false;
            Debug.Log("AmmoBox ����ѹر�");
        }

        // ���� BoxCollider2D
        targetCollider = targetObject.GetComponent<BoxCollider2D>();
        if (targetCollider != null)
        {
            targetCollider.enabled = false;
            Debug.Log("BoxCollider2D ����ѹر�");
        }
    }

    private IEnumerator TransitionEmission()
    {
        Debug.Log("��ʼ����Ч������...");

        foreach (Material mat in emissionMaterials)
        {
            if (mat != null && mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION"); // ���÷���
                Color initialColor = mat.GetColor("_EmissionColor"); // ��ʼ��ɫ
                float elapsedTime = 0f;

                while (elapsedTime < transitionDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsedTime / transitionDuration);
                    Color currentColor = Color.Lerp(initialColor, targetEmissionColor, t);
                    mat.SetColor("_EmissionColor", currentColor);
                    yield return null;
                }

                mat.SetColor("_EmissionColor", targetEmissionColor);
                Debug.Log($"���� {mat.name} ������ɫ������ɡ�");
            }
            else
            {
                Debug.LogWarning($"���� {mat?.name} δ�ҵ� _EmissionColor ����");
            }
        }
    }

    private IEnumerator FadeInAudio()
    {
        if (audioSource != null)
        {
            audioSource.volume = 0f;
            audioSource.Play();
            float elapsedTime = 0f;

            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                audioSource.volume = Mathf.Clamp01(elapsedTime / transitionDuration);
                yield return null;
            }

            audioSource.volume = 1f;
            Debug.Log("��Ч�����Ѵ� 0 ���ɵ� 1��");
        }
        else
        {
            Debug.LogWarning("δ�ҵ� AudioSource ������޷�������Ч��");
        }
    }

    private void ActivateAndMoveObject()
    {
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
            Debug.Log($"����������: {objectToActivate.name}");

            // �ƶ����嵽ָ��λ��
            PlayerController playerController = objectToActivate.GetComponent<PlayerController>();
            if (playerController != null && moveTargetPosition != null)
            {
                playerController.MoveToTarget(moveTargetPosition.position);
                Debug.Log($"{objectToActivate.name} �����ƶ���ָ��λ��: {moveTargetPosition.position}");
            }
            else
            {
                Debug.LogWarning("PlayerController �����Ŀ��λ��δ�ҵ����޷��ƶ����壡");
            }
        }
        else
        {
            Debug.LogWarning("δָ��Ҫ���õ����壡");
        }
    }
}
