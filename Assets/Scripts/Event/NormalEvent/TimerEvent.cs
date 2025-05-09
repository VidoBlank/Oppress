using System.Collections;
using UnityEngine;
using TMPro; // ����TextMeshPro�����ռ�

// ����ʱ�¼���������ʱ�������¼�����
public class TimerEvent : BaseEvent
{
    [Header("����ʱʱ��")]
    public float time = 0f; // �ܵ���ʱʱ��

    [Header("�ı�����")]
    public TMP_Text timerText; // ������ʾʣ��ʱ���TextMeshPro���
    public string timerPrefixText = "ʣ��ʱ�䣺"; // ����ʱ���ı�ǰ׺

    [Header("��;����������")]
    public GameObject halfwayObject; // �ڵ���ʱһ��ʱҪ����������
    public GameObject endwayObject; // �ڵ���ʱ����ʱҪ����������

    [Header("�����󴥷�����������")]
    public GameObject elevatorTarget; // ���ڻ�ȡ������������Ŀ������

    public TypewriterColorJitterEffect typewriterEffect; // ������ʾ���������ʾ�ı�
    public string startText = "��ֵ�������ݵִ"; // ����ʱ��ʼʱ����ʾ�ı�
    public string middleText = "С��<color=#FF0000>�Ϸ�</color>���µĹ���þѻ��ִ�����Щ���ˡ�"; // ����ʱ���������ʾ�ı�
    public string endText = "����ʱ�����������и�Ա�ƶ���<color=#00FF00>��������</color>�Գ��롣"; // ����ʱ���������ʾ�ı�

    private float timer = 0f;        // ��ʱ��
    private bool isTiming = false;  // �Ƿ����ڼ�ʱ
    private bool isHalfwayTriggered = false; // ����Ƿ񴥷���һ���¼�
    public Animator elevatorAnimator; // ���ݵ�Animator
    public AudioSource audioSource;
    public AudioSource audioSource2;


    public override void EnableEvent()
    {
        Debug.Log("��ʼ��ʱ���¼�");
        audioSource.Play();
        timer = 0f;
        isTiming = true;
        isHalfwayTriggered = false;

        // ��ȡ���ݵ�Animator���
        if (elevatorTarget != null)
        {
            elevatorAnimator = elevatorTarget.GetComponent<Animator>();
            if (elevatorAnimator == null)
            {
                Debug.LogWarning($"{elevatorTarget.name} ��δ�ҵ� Animator �����");
            }
        }

        // ���¿�ʼ��ʾ�ı�
        if (typewriterEffect != null)
        {
            typewriterEffect.SetText(startText);
        }

        // ��ʼ���ı���ʾ
        UpdateTimerText(time);
    }

    private void Update()
    {
        if (isTiming)
        {
            timer += Time.deltaTime;

            // ʣ��ʱ��
            float remainingTime = Mathf.Max(time - timer, 0f);

            // �����ı���ʾ
            UpdateTimerText(remainingTime);

            // ����Ƿ񵽴�һ��ʱ�䣬������;�¼�
            if (!isHalfwayTriggered && timer >= time / 2)
            {
                isHalfwayTriggered = true;
                typewriterEffect.SetText(middleText);
                if (halfwayObject != null)
                {
                    halfwayObject.SetActive(true);
                    Debug.Log($"{halfwayObject.name} ���ڵ���ʱһ��ʱ������");
                }
            }
            if (remainingTime <= 15f)
            {

                endwayObject.SetActive(true);
            }

            // ��⵹��ʱ�Ƿ����
            if (timer >= time)
            {
                isTiming = false;
                Debug.Log("��ʱ�����������¼�������");
                HandleTimerEnd();
                EndEvent();
            }
        }
    }

    private void UpdateTimerText(float remainingTime)
    {
        if (timerText != null)
        {
            string timeText;

            // ��ʣ��ʱ�� <= 20 ��ʱ���ı�������ɫΪ��ɫ
            if (remainingTime <= 20f)
            {
                timeText = $"{timerPrefixText}<color=red>{remainingTime:F1}��</color>";
            }
            else
            {
                timeText = $"{timerPrefixText}{remainingTime:F1}��";
            }

            timerText.text = timeText; // ����TextMeshPro�ı�����
        }
    }

    private void HandleTimerEnd()
    {
        // ���½�����ʾ�ı�
        if (typewriterEffect != null)
        {
            typewriterEffect.SetText(endText);
        }
        
        audioSource2.Play();
        // �������ݶ���
        if (elevatorAnimator != null)
        {
            elevatorAnimator.SetTrigger("elevatorstart");
            Debug.Log($"���� {elevatorTarget.name} �� elevatorStart ������������");
        }
    }
}
