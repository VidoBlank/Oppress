using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cinemachine;
using FronkonGames.Glitches.Interferences;

public class Event_10_0_AlarmSound : BaseEvent
{
    [Header("Cinemachine����")]
    public CinemachineVirtualCamera virtualCamera; // ��Ҫ���������������
    public float shakeDuration = 2f; // ��������ʱ��
    public float shakeAmplitude = 1f; // ��������
    public float shakeFrequency = 1f; // ����Ƶ��

    [Header("Interferences����")]
    public Interferences interferences; // Interferences ����
    public float interferencesIntensity = 0.8f; // Interferences ǿ��

    [Header("��Ч����")]
    public AudioClip alarmSound; // ������Ч
    public AudioSource audioSource; // ��Ч����Դ

    [Header("�ı�����")]
    public TMP_Text eventText; // ������ʾ�¼��ı���TextMeshPro���
    public List<string> textSegments; // �ı���������
    public float textDisplayInterval = 3f; // ÿ���ı���ʾ�ļ��ʱ��

    [Header("�¼���������")]
    public float eventStartDelay = 5f; // �ӳ�ʱ�䣬��λ��
    public bool enableTestMode = false; // �Ƿ����ò���ģʽ

    private bool lastTestModeState = false; // �ϴβ���ģʽ��״̬
    private CinemachineBasicMultiChannelPerlin noiseComponent; // ������������
    private bool isInterferencesActive = false; // Interferences �Ƿ�����

    private void Update()
    {
        // ������ģʽ�Ƿ�����
        if (enableTestMode && !lastTestModeState)
        {
            lastTestModeState = true;
            StartCoroutine(TestAllEffects());
        }
        else if (!enableTestMode && lastTestModeState)
        {
            lastTestModeState = false;
        }
    }

    public override void EnableEvent()
    {
        StartCoroutine(DelayedEventStart());
    }

    private IEnumerator DelayedEventStart()
    {
        // �ȴ�ָ�����ӳ�ʱ��
        yield return new WaitForSeconds(eventStartDelay);

        // �����¼����߼�
        StartCoroutine(EventSequence());
    }

    private IEnumerator EventSequence()
    {
        // ��ȡ Cinemachine ���������
        if (virtualCamera != null)
        {
            noiseComponent = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        // ��������������� Interferences Ч��
        if (noiseComponent != null && interferences != null)
        {
            // �����������������
            noiseComponent.m_AmplitudeGain = shakeAmplitude;
            noiseComponent.m_FrequencyGain = shakeFrequency;

            // ���� Interferences
            interferences.settings.intensity = interferencesIntensity;
            interferences.SetActive(true);
            isInterferencesActive = true;

            // ���ž�����Ч
            if (audioSource != null && alarmSound != null)
            {
                audioSource.clip = alarmSound;
                audioSource.Play();

                // ����������Ч�߼�
                StartCoroutine(FadeOutAudioAfterDelay(20f));
            }

            // ��������
            yield return new WaitForSeconds(shakeDuration);

            // ֹͣ���������
            noiseComponent.m_AmplitudeGain = 0f;
            noiseComponent.m_FrequencyGain = 0f;

            // ֹͣ Interferences Ч��
            interferences.SetActive(false);
            isInterferencesActive = false;
        }

        // ��ʾ����ı�
        if (eventText != null && textSegments != null && textSegments.Count > 0)
        {
            foreach (string segment in textSegments)
            {
                eventText.text = segment;
                yield return new WaitForSeconds(textDisplayInterval);
            }
        }

        // ���һ���ı���ʾ�󣬽����¼�
        EndEvent();
    }

    private IEnumerator FadeOutAudioAfterDelay(float delay)
    {
        // �ȴ�ָ���ӳ�ʱ��
        yield return new WaitForSeconds(delay);

        if (audioSource != null)
        {
            float initialVolume = audioSource.volume; // ��¼��ǰ����
            float fadeDuration = 2f; // ����ʱ��
            float elapsedTime = 0f;

            // �𽥽�������
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(initialVolume, 0f, elapsedTime / fadeDuration);
                yield return null;
            }

            // ��ɺ�ֹͣ������Ч
            audioSource.volume = 0f;
            audioSource.Stop();
            Debug.Log("��Ч�����Ѵӵ�ǰֵ������ 0����ֹͣ���š�");
        }
        else
        {
            Debug.LogWarning("δ�ҵ� AudioSource ������޷����е���������");
        }
    }

    private IEnumerator TestAllEffects()
    {
        Debug.Log("��ʼ��������Ч��");

        // ��������������� Interferences Ч��
        if (virtualCamera != null)
        {
            noiseComponent = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noiseComponent != null)
            {
                noiseComponent.m_AmplitudeGain = shakeAmplitude;
                noiseComponent.m_FrequencyGain = shakeFrequency;

                // ���� Interferences
                if (interferences != null)
                {
                    interferences.settings.intensity = interferencesIntensity;
                    interferences.SetActive(true);
                    isInterferencesActive = true;
                }

                // ���ž�����Ч
                if (audioSource != null && alarmSound != null)
                {
                    audioSource.clip = alarmSound;
                    audioSource.Play();

                    // ����������Ч�߼�
                    StartCoroutine(FadeOutAudioAfterDelay(20f));
                }

                yield return new WaitForSeconds(shakeDuration);

                // ֹͣ���������
                noiseComponent.m_AmplitudeGain = 0f;
                noiseComponent.m_FrequencyGain = 0f;

                // ֹͣ Interferences Ч��
                if (interferences != null)
                {
                    interferences.SetActive(false);
                    isInterferencesActive = false;
                }
            }
        }

        // ��ʾ�����ı�
        if (eventText != null && textSegments != null && textSegments.Count > 0)
        {
            foreach (string segment in textSegments)
            {
                eventText.text = segment;
                yield return new WaitForSeconds(textDisplayInterval);
            }
        }

        Debug.Log("���Խ���");
    }
}
