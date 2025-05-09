using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using Cinemachine;
using FronkonGames.Glitches.Interferences;



public class GlitchEffect : MonoBehaviour
{
    //����
    public Volume globalVolume; // ���� Volume
    public TextMeshProUGUI glitchText; // ������ʾ�Լ��ı��� TextMeshPro ����
    public CinemachineVirtualCamera virtualCamera; // Cinemachine Virtual Camera
    public float minFlashFrequency = 0.01f; // �����˸Ƶ��
    public float maxFlashFrequency = 0.1f; // ������˸Ƶ��
    public float startExposure = -10f; // ��ʼ�ع�ֵ
    public float endExposure = 1f; // �����ع�

    private CinemachineBasicMultiChannelPerlin noise; // Cinemachine �������
    private float initialBloomThreshold = 1f; // Bloom ��ʼ��ֵ
    private bool isGlitching = false;


    //��������
    public Interferences interferences;
    private bool isInterferencesActive = false;


    //�Լ������
    public Slider progressSlider; // ������
    public TextMeshProUGUI progressText; // ���Ȱٷֱ��ı�
    public CanvasGroup progressCanvasGroup; // ���ƽ�����͸���ȵ� CanvasGroup



    //�Լ��ı�
    [SerializeField]
    private List<string> checkItems = new List<string>
    {
        "���ڳ�ʼ����ʾ�ӿڡ���",
        "��ʾ��ʼ����ϡ�",
        "������Ӧϵͳ���:������Ӧ������ȶ��ԡ���",
        "������Դ��������:����ʣ�๩��ʱ�䡭��",
        "��Դ������ԣ����ȷ���ؼ�ģ�鹩�硣",
        "�Զ�����ϵͳ��飺<color=yellow>�쳣����Ҫ�˹���Ԥ��</color>",
        "����<color=yellow>ALPHA-B2Ӧ��Э��</color>�����ڵ�����Դ����",
        "���������ɨ��:��������",
        "�������Լ�⣺����Ũ�ȣ��ϵ͡�",
        "��������ϵͳ������������",
        "����Ũ�ȼ�⣺ˮƽ�ѹ��˰�ȫ��Χ�ڡ�",
        "������̼����Ч�ʣ�����֤��",
        "����ģ��װ��У׼��ȷ���ڲ�����ƽ�⡭��",
        "�¿�ϵͳ��⣺ģ������������",
        "�������ƣ��Զ������¶�����ȫˮƽ����",
        "��ʩ������ɨ�裺���ڷ������˳̶ȡ���",
        "��������Լ�飺��⵽��΢����",
        "Ӧ���ܷ�Э�飺ȷ����Ч����",
        "��Ҫ֧�Žṹ�������ȶ���",
        "�ڲ�ͨ��ɨ�裺ȷ�Ͽ�ͨ��·������",
        "��������״̬��⣺����������������",
        "���Ĵ���������״̬:�����߼����ġ���",
        "�洢ϵͳɨ�裺����δ����",
        "�ⲿ̽����״̬���£��ָ�����ɨ�衭��",
        "������ظ���:ȷ���쳣��ȵ㡭��",
        "ϵͳ�����ɡ����йؼ���������������"
    };

    private void Awake()
    {
        if (checkItems == null || checkItems.Count == 0)
        {
            InitializeDefaultCheckItems();
        }

        if (virtualCamera != null)
        {
            noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise == null)
            {
               
            }
        }
    }

   

    private void InitializeDefaultCheckItems()
    {
        checkItems = new List<string>
        {
            "���ڳ�ʼ����ʾ�ӿڡ���",
            "��ʾ��ʼ����ϡ�",
            "������Ӧϵͳ���:������Ӧ������ȶ��ԡ���",
            "������Դ��������:����ʣ�๩��ʱ�䡭��",
            "��Դ������ԣ����ȷ���ؼ�ģ�鹩�硣",
            "�Զ�����ϵͳ��飺<color=yellow>�쳣����Ҫ�˹���Ԥ��</color>",
            "����<color=yellow>ALPHA-B2Ӧ��Э��</color>�����ڵ�����Դ����",
            "���������ɨ��:��������",
            "�������Լ�⣺����Ũ�ȣ��ϵ͡�",
            "��������ϵͳ������������",
            "����Ũ�ȼ�⣺ˮƽ�ѹ��˰�ȫ��Χ�ڡ�",
            "������̼����Ч�ʣ�����֤��",
            "����ģ��װ��У׼��ȷ���ڲ�����ƽ�⡭��",
            "�¿�ϵͳ��⣺ģ������������",
            "�������ƣ��Զ������¶�����ȫˮƽ����",
            "��ʩ������ɨ�裺���ڷ������˳̶ȡ���",
            "��������Լ�飺��⵽��΢����",
            "Ӧ���ܷ�Э�飺ȷ����Ч����",
            "��Ҫ֧�Žṹ�������ȶ���",
            "�ڲ�ͨ��ɨ�裺ȷ�Ͽ�ͨ��·������",
            "��������״̬��⣺����������������",
            "���Ĵ���������״̬:�����߼����ġ���",
            "�洢ϵͳɨ�裺����δ����",
            "�ⲿ̽����״̬���£��ָ�����ɨ�衭��",
            "������ظ���:ȷ���쳣��ȵ㡭��",
            "ϵͳ�����ɡ����йؼ���������������"
        };
    }

    public IEnumerator StartGlitchEffect()
    {
        if (isGlitching || globalVolume == null)
            yield break;

        isGlitching = true;
        globalVolume.weight = 1f;

        // ������ɫ����Ч��
        if (globalVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
            colorAdjustments.active = true;

        if (globalVolume.profile.TryGet(out Bloom bloom))
        {
            initialBloomThreshold = bloom.threshold.value;
            bloom.active = true;
        }

        // ��ʼ��������
        if (progressSlider != null)
        {
            progressSlider.value = 0f;
            progressCanvasGroup.alpha = 0f; // ����Ϊ͸��
            StartCoroutine(FadeCanvasGroup(progressCanvasGroup, 1f, 0.5f)); // ����
        }

        float minIntensity = 0.5f;
        float maxIntensity = 0.85f;

        int totalItems = checkItems.Count; // ���Լ���Ŀ��

        for (int i = 0; i < totalItems; i++)
        {
            float currentFlashFrequency = i < totalItems / 2
                ? Random.Range(0.5f, 1.25f)
                : Random.Range(0.25f, 0.03f);

            if (globalVolume.profile.TryGet(out ColorAdjustments colorAdjustmentsEffect))
            {
                colorAdjustmentsEffect.postExposure.value = Random.Range(-5f, 5f);
            }

            // ���½�����
            if (progressSlider != null)
            {
                float progress = (i + 1) / (float)totalItems;
                progressSlider.value = progress;

                if (progressText != null)
                    progressText.text = Mathf.RoundToInt(progress * 100) + "%";
            }

            // ��ʾ�Լ���Ŀ
            string currentText = checkItems[i];
            glitchText.text = currentText;

            // ��������Ƿ����� Interferences
            if (Random.value > 0.35f)
            {
                float randomDuration = Random.Range(0.25f, 0.5f);
                StartCoroutine(EnableInterferencesTemporarily(randomDuration));
            }

            // ��̬������� Interferences ǿ��11
            if (isInterferencesActive && interferences.settings != null)
            {
                interferences.settings.intensity = Random.Range(minIntensity, maxIntensity);
            }

            yield return new WaitForSeconds(currentFlashFrequency);
        }

       

        if (globalVolume.profile.TryGet(out ColorAdjustments colorAdjustmentsFinal))
        {
            colorAdjustmentsFinal.postExposure.value = endExposure;
        }

        if (interferences != null)
        {
            interferences.SetActive(false);
            isInterferencesActive = false;
        }

        glitchText.text = "ϵͳ������ɣ�Tau-Primeվ��״̬�Ѹ��¡�";
        yield return new WaitForSeconds(3f);
        glitchText.text = "";
        // �Լ���ɺ����ؽ�����
        if (progressSlider != null)
        {
            StartCoroutine(FadeCanvasGroup(progressCanvasGroup, 0f, 0.75f)); // ����
            yield return new WaitForSeconds(0.75f); // �ȴ��������
        }
        yield return StartCoroutine(FadeOutVolumeWeight());
        isGlitching = false;
    }









    ///////////////////////Э��////////////////////////

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }



    private IEnumerator EnableInterferencesTemporarily(float duration)
    {
        if (interferences != null)
        {
            interferences.SetActive(true); // ���� Interferences
            isInterferencesActive = true; // ����״̬
            yield return new WaitForSeconds(duration);
            interferences.SetActive(false); // �ر� Interferences
            isInterferencesActive = false; // ����״̬
        }
    }



    private IEnumerator FadeOutVolumeWeight()
    {
        float fadeDuration = 0.35f; // ����ʱ��
        float startWeight = globalVolume.weight;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            globalVolume.weight = Mathf.Lerp(startWeight, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        globalVolume.weight = 0f; // ȷ������ֵΪ 0
    }

    /// <summary>
    /// ///////////////////////////////////////
    /// </summary>



    //����Ч��
    public void ResetPostProcessingEffects()
    {
        if (globalVolume == null || globalVolume.profile == null)
            return;

        if (globalVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            colorAdjustments.postExposure.value = 0f;
            colorAdjustments.active = false;
        }

        if (globalVolume.profile.TryGet(out Bloom bloom))
        {
            bloom.threshold.value = 0.85f;
            bloom.intensity.value = 1f;
            bloom.active = false;
        }

        globalVolume.weight = 0f;
    }
}
