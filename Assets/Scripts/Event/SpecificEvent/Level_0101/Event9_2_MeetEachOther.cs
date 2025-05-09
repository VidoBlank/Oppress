using System.Collections;
using UnityEngine;

public class Event9_2_MeetEachOther : BaseEvent
{
    [Header("�����������")]
    public GameObject[] objectsToEnableSequentially; // ���ο���������
    public float[] enableDelays = { 1.5f, 1.5f, 1.5f, 2f }; // ÿ������Ŀ������ʱ��

    [Header("��Ч����")]
    public AudioClip activationSound; // ����ʱ����Ч
    public AudioSource audioSource; // ���ڲ�����Ч�� AudioSource

    public override void EnableEvent()
    {
        base.EnableEvent();
        Debug.Log("Event9_2_MeetEachOther �¼��Ѵ���");

        // ��ʼ������������
        StartCoroutine(EnableObjectsSequentially());
    }

    private IEnumerator EnableObjectsSequentially()
    {
        Debug.Log("��ʼ������������...");
        for (int i = 0; i < objectsToEnableSequentially.Length; i++)
        {
            // �ȴ�ָ����ʱ���������û�����ã�Ĭ�� 1.5 �룩
            yield return new WaitForSeconds(i < enableDelays.Length ? enableDelays[i] : 1.5f);

            // ��������
            if (objectsToEnableSequentially[i] != null)
            {
                objectsToEnableSequentially[i].SetActive(true);
                Debug.Log($"{objectsToEnableSequentially[i].name} �ѱ����á�");

                // ������Ч
                if (audioSource != null && activationSound != null)
                {
                    audioSource.PlayOneShot(activationSound);
                    Debug.Log("������Ч��" + activationSound.name);
                }
            }
        }
        Debug.Log("�����������������á�");
        EndEvent();
    }
}