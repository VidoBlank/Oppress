using UnityEngine;
using DG.Tweening;

public class DeployElevatorController : MonoBehaviour
{
    [Header("��������")]
    public GameObject elevator;       // ָ���ĵ��� GameObject
    public float moveDuration = 2f;   // �����ƶ�����ʱ�䣨�룩
    public float moveDistance = 5f;   // ���������ƶ��ľ��루��λ��

    [Header("����������")]
    public MonoBehaviour[] managerComponents;  // ָ���Ĺ������ű��������

    [Header("��Ƶ����")]
    public AudioSource targetAudioSource;        // Ŀ�� AudioSource�������滻���֡�pitch ���ɺ� volume ������
    public AudioClip replacementClip;            // ָ��������Ƭ�Σ������滻Ŀ�� AudioSource �� clip
    public float targetPitch = 1.2f;               // Ŀ�� pitch ֵ
    public float pitchTransitionDuration = 0.5f;   // pitch ����ʱ�䣨�룩
    public float volumeFadeDuration = 1.0f;        // volume ��������ʱ�䣨�룩

    public AudioSource secondaryAudioSource;       // ����ӵ� AudioSource�������� volume ����

    /// <summary>
    /// ����ť�������ô˷���
    /// </summary>
    public void OnDeployButtonClicked()
    {
        if (elevator == null)
        {
            Debug.LogError("δָ������ GameObject��");
            return;
        }

        // �滻Ŀ�� AudioSource �� clip Ϊָ�������֣�������
        if (targetAudioSource != null && replacementClip != null)
        {
            targetAudioSource.clip = replacementClip;
            targetAudioSource.Play();
        }

        // ��������ʱ�����ٽ�Ŀ�� AudioSource �� pitch ���ɵ�ָ��ֵ
        if (targetAudioSource != null)
        {
            DOTween.To(() => targetAudioSource.pitch, x => targetAudioSource.pitch = x, targetPitch, pitchTransitionDuration);
        }

        // ���ŵ����ϵ� AudioSource
        AudioSource elevatorAudio = elevator.GetComponent<AudioSource>();
        if (elevatorAudio != null)
        {
            elevatorAudio.Play();
            secondaryAudioSource.Play();
        }

        // �������Ŀ��λ�ã������ƶ� moveDistance ����λ��
        Vector3 targetPos = elevator.transform.position - new Vector3(0, moveDistance, 0);

        // ʹ�� DOTween ִ�е����ƶ�������ʹ�� Ease.OutCubic ʵ���ȿ�����Ļ���Ч��
        elevator.transform.DOMoveY(targetPos.y, moveDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                // �����ƶ���ɺ󣬿�������ָ���Ĺ������ű����
                if (managerComponents != null)
                {
                    foreach (var comp in managerComponents)
                    {
                        if (comp != null)
                        {
                            comp.enabled = true;
                        }
                    }
                }

                // �ڵ����ƶ���ɺ󣬽�Ŀ�� AudioSource �� volume �� volumeFadeDuration ���ڹ��ɻ� 0
                if (targetAudioSource != null)
                {
                    DOTween.To(() => targetAudioSource.volume, x => targetAudioSource.volume = x, 0f, volumeFadeDuration);
                }

                // ������ӵ� secondaryAudioSource ִ�� volume ����Ч��
                if (secondaryAudioSource != null)
                {
                    DOTween.To(() => secondaryAudioSource.volume, x => secondaryAudioSource.volume = x, 0f, volumeFadeDuration);
                }
            });
    }
}
