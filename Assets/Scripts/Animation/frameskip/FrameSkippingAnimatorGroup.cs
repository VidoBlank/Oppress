using UnityEngine;
using System.Collections.Generic;

public class FrameSkippingAnimatorGroup : MonoBehaviour
{
    public GameObject[] parentObjects;      // �������������
    public float totalDuration = 1f;        // ��������ʱ�����룩
    public int targetFramesPerSecond = 12;  // Ŀ��֡�ʣ�fps��
    public bool enableFrameSkipping = true; // ��Inspector�����û���ó�֡Ч��

    private float frameDuration;            // ÿ֡��ʱ��
    private float timeAccumulator = 0f;     // �����ۻ�ʱ��
    private List<Animator> animators = new List<Animator>();

    void Start()
    {
        frameDuration = totalDuration / targetFramesPerSecond;
        FindAllAnimators(); // ��������Animator

        // ��ʼ���ã������Զ�����
        SetAnimatorsSpeed(1f);
    }

    void Update()
    {
        if (enableFrameSkipping)
            ApplyFrameSkipping();
        else
            SetAnimatorsSpeed(1f); // �����ٶȲ���
    }

    // ��������Animator
    void FindAllAnimators()
    {
        foreach (GameObject parentObject in parentObjects)
        {
            if (parentObject == null) continue;
            animators.AddRange(parentObject.GetComponentsInChildren<Animator>());
        }
    }

    // Ӧ�ó�֡Ч��
    void ApplyFrameSkipping()
    {
        timeAccumulator += Time.deltaTime;
        if (timeAccumulator >= frameDuration)
        {
            foreach (Animator animator in animators)
            {
                if (animator == null || !animator.gameObject.activeInHierarchy) continue; // ���Animator�Ƿ�Ϊnull�������٣��Ҹ������Ƿ񼤻�

                animator.speed = 1f;
                animator.Update(frameDuration);  // ����һ֡
                animator.speed = 0f;  // ��ͣ
            }
            timeAccumulator %= frameDuration; // ����ʱ��
        }
    }

    // ����Animator�����ٶ�
    void SetAnimatorsSpeed(float speed)
    {
        foreach (Animator animator in animators)
        {
            if (animator == null || !animator.gameObject.activeInHierarchy) continue; // ���Animator�Ƿ�Ϊnull�������٣��Ҹ������Ƿ񼤻�
            animator.speed = speed;
        }
    }

    // Inspector�п�������/���ó�֡Ч��
    public void ToggleFrameSkipping(bool enabled)
    {
        enableFrameSkipping = enabled;
        if (!enabled) SetAnimatorsSpeed(1f); // �ָ������ٶ�
    }
}
