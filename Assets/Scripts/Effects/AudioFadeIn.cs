using System.Collections;
using UnityEngine;

public class AudioFadeIn : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource; // Ŀ����ƵԴ
    public float delay = 5f; // �ӳ�ʱ�䣨�룩
    public float fadeDuration = 3f; // �����������ʱ�䣨�룩

    [Header("Options")]
    public bool enableEffect = true; // ��ѡ��Ƿ���Ч��
    private bool effectTriggered = false; // ���Ч���Ƿ��Ѵ���

    private void Update()
    {
        // ��� enableEffect �Ƿ��δ�������״̬
        if (enableEffect && !effectTriggered)
        {
            effectTriggered = true; // ��ֹ�ظ�����
            if (audioSource == null)
            {
                Debug.LogError("AudioSource is not assigned!");
                return;
            }

            // ȷ��������ʼΪ0
            audioSource.volume = 0f;
            audioSource.playOnAwake = false;

            // ����������ƵЭ��
            StartCoroutine(PlayAudioWithFadeIn());
        }
    }

    private IEnumerator PlayAudioWithFadeIn()
    {
        // �ȴ�ָ���ӳ�ʱ��
        yield return new WaitForSeconds(delay);

        // ��ʼ������Ƶ
        audioSource.Play();

        // ����������
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Clamp01(elapsed / fadeDuration) * 0.5f;
            yield return null;
        }

        // ȷ������Ϊ0.5
        audioSource.volume = 0.5f;
    }
}
