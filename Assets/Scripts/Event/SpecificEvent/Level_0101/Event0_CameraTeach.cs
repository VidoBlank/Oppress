using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using UnityEngine.UI;

public class Event0_CameraTeach : BaseEvent
{
    private CameraController cameraController;
    public TypewriterColorJitterEffect typewriterEffect;
    public string[] narrativeTexts; // �����ı�����
    public string[] tutorialTexts; // ��ѧ�ı�����
    public float textDisplayDelay = 2f; // ÿ���ı���ʾ֮��ļ��

    private int currentTextIndex = 0; // ��ǰ��ʾ����ʾ����
    public bool cameraUnlocked = false; // ����Ƿ��Ѿ�����

    public CanvasGroup blackScreen; // �ڳ��� CanvasGroup
    public float fadeDuration = 1f; // �ڳ����뵭����ʱ��
    public GlitchEffect glitchEffect; 
    public PolygonCollider2D tutorialCameraBounds; // ��ѧ�����е�����߽�

    private bool glitchTriggered = false;
    private Vector3 lastCameraPosition;
    private float lastCameraFOV;


    public AudioSource audioSource;
    public GameObject audioFadeInObject;
    public GameObject Effect;


    private void Start()
    {
        Transform[] allTransforms = FindObjectsOfType<Transform>(true);
        foreach (Transform t in allTransforms)
        {
            if (t.name == "PlayerFrame_2")
            {
                t.gameObject.SetActive(false);
                break;
            }
        }

        // ��ʼ������״̬
        if (blackScreen != null)
        {
            blackScreen.alpha = 1f; 
            blackScreen.blocksRaycasts = true; 
        }

        typewriterEffect = FindObjectOfType<TypewriterColorJitterEffect>();
        glitchEffect = FindObjectOfType<GlitchEffect>(); 

        cameraController = FindObjectOfType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("CameraController δ�ҵ�����ȷ���������������������");
        }
    }

    public override void EnableEvent()
    {
        Debug.Log("��ʼ�����ѧ");

        // �������
        if (cameraController != null)
        {
            cameraController.isCameraLocked = true;
            Debug.Log("���������");
        }
        // ���� AudioFadeIn ����
        if (audioFadeInObject != null)
        {
            AudioFadeIn audioFadeIn = audioFadeInObject.GetComponent<AudioFadeIn>();
            if (audioFadeIn != null)
            {
                audioFadeIn.enableEffect = true; // ���� AudioFadeIn ��Ч��
                Debug.Log("AudioFadeIn �����á�");
            }
            else
            {
                Debug.LogWarning("AudioFadeIn �ű�δ�ҵ���");
            }
        }
        else
        {
            Debug.LogWarning("audioFadeInObject δָ����");
        }
        // ��ʼ��ѧ����
        StartCoroutine(StartCameraTeach());
    }

    private IEnumerator StartCameraTeach()
    {
        // �׶� 1���ڳ��������
        
        yield return StartCoroutine(FadeBlackScreen(1, 0)); // ��ȫ�ڵ�����͸��

        // �׶� 2���ڳ�����ʾ�����ı�
        yield return StartCoroutine(ShowNarrativeTexts());
       


        // �׶� 3����������߽�
        SetTutorialCameraBounds();
        

        // �׶� 4����ѧ��ʼ
        yield return StartCoroutine(CameraTutorial());
    }


    private IEnumerator FadeBlackScreen(float from, float to)
    {
        float elapsedTime = 0f;

        // ȷ����ȡ�� Color Adjustments
        ColorAdjustments colorAdjustments = null;
        if (glitchEffect.globalVolume.profile.TryGet(out colorAdjustments))
        {
            colorAdjustments.active = true;
        }
        else
        {
            Debug.LogWarning("Color Adjustments not found in Volume Profile.");
        }

        // ��һ�� Lerp�������ع�ֵ
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            // ��� Color Adjustments ���ڣ����� postExposure
            if (colorAdjustments != null)
            {
                colorAdjustments.postExposure.value = Mathf.Lerp(-10f, -7.5f, elapsedTime / fadeDuration);
            }

            yield return null;
        }

        // ȷ�������ع�ֵ
        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.value = -7.5f; // �����ع�ֵ
        }

        // ���� elapsedTime �Ա㿪ʼ�ڶ��� Lerp
        elapsedTime = 0f;

        // �ڶ��� Lerp����������͸����
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            // Lerp the black screen alpha
            blackScreen.alpha = Mathf.Lerp(from, to, elapsedTime / fadeDuration);

            yield return null;
        }

        // ȷ������͸����ֵ��ȷ
        blackScreen.alpha = to;
        blackScreen.blocksRaycasts = to > 0; // ����ڳ���ȫ͸����ȡ����������赲
    }



    private IEnumerator ShowNarrativeTexts()
    {
        for (int i = 0; i < narrativeTexts.Length; i++)
        {
            yield return DisplayText(narrativeTexts[i]);

            
            if (i == 3 && !glitchTriggered && glitchEffect != null)
            {
                glitchTriggered = true; 
                yield return StartCoroutine(glitchEffect.StartGlitchEffect());
            }

        }
    }

    private void SetTutorialCameraBounds()
    {
        if (cameraController != null && tutorialCameraBounds != null)
        {
            cameraController.SetCameraBounds(tutorialCameraBounds);
            Effect.SetActive(true);
            Debug.Log("����߽��Ѹ��¡�");

        }
       
    }

    private IEnumerator CameraTutorial()
    {
        if (typewriterEffect == null)
        {
            typewriterEffect = FindObjectOfType<TypewriterColorJitterEffect>();
        }

        // ��ʾ��ҽ������
        yield return DisplayText("����[Z]���Խ���/������������");
        yield return new WaitUntil(() => !cameraController.isCameraLocked);

        Debug.Log("����ѳɹ�������");
        audioSource.Play();
        cameraUnlocked = true;

        // ��ʼ�����λ�ú�����״̬
        if (Camera.main != null)
        {
            lastCameraPosition = Camera.main.transform.position;
            lastCameraFOV = Camera.main.fieldOfView;
        }

        // ��ʾ����ƶ����
        yield return DisplayText("�ƶ���굽��Ļ��Ե���ƶ���������");
        yield return new WaitUntil(() => CameraMoved());
        audioSource.Play();
        Debug.Log("��⵽����ƶ���");

        // ��ʾ����������
        yield return DisplayText("ʹ�������ֵ������������š�");
        yield return new WaitUntil(() => CameraZoomed());
        audioSource.Play();
        Debug.Log("��⵽������š�");

        // ��ѧ����
        yield return DisplayText("��ʾģ��У׼��ɡ����ڲ���һ��ְ��Ϊ<color=#FFFF00>��ǹ��</color>��Synaptic��Ա��");
        Debug.Log("�����ѧ������");
        EndEvent(); // ��ѧ����
    }

    private bool CameraMoved()
    {
        // ������λ�ñ仯
        if (Camera.main != null)
        {
            Vector3 currentCameraPosition = Camera.main.transform.position;
            if (currentCameraPosition != lastCameraPosition)
            {
                lastCameraPosition = currentCameraPosition; // ������һ�μ�¼��λ��
                return true;
            }
        }
        return false;
    }

    private bool CameraZoomed()
    {
        // ������ FOV (Field of View) ���ű仯
        if (Camera.main != null)
        {
            float currentFOV = Camera.main.fieldOfView;
            if (Mathf.Abs(currentFOV - lastCameraFOV) > 0.1f) // ������ű仯����
            {
                lastCameraFOV = currentFOV; // ������һ�μ�¼������ֵ
                return true;
            }
        }
        return false;
    }

















    private IEnumerator DisplayText(string text)
    {
        // �����µ���ʾ�ı�
        typewriterEffect.SetText(text);

        // �ȴ��ı�����Ч�����
        while (typewriterEffect.isTyping)
        {
            yield return null;
        }

        // �ȴ�һ��ʱ������ʾ��һ����ʾ
        yield return new WaitForSeconds(textDisplayDelay);
    }
}
