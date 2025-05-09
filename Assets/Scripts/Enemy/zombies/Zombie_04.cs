using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie_04 : Enemy
{
    [Header("�¼���ϵ��")]
    [Tooltip("��Hp����ʱ�����趨�ļ��ٱ��������� 0.3 ��ʾ30% �ٶȣ�")]
    public float newSlowFactor = 0.3f;

    [Header("���ƶ��ٶ�")]
    [Tooltip("��Hp����ʱ�����趨���ƶ��ٶ�")]
    public float newMovingSpeed = 2.0f;

    [Header("��������ʱ��")]
    [Tooltip("���� Blend Tree `speed` �����ı仯ʱ��")]
    public float animationTransitionTime = 1.0f;

    private float originalMovingSpeed;
    private Coroutine animationTransitionCoroutine;
    private Coroutine restoreSpeedCoroutine;

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        originalMovingSpeed = movingSpeed;
    }

    /// <summary>
    /// ����ʱ�����������޸��ƶ��ٶ� & slowFactor��ƽ������ Blend Tree `speed` ����
    /// </summary>
    public override void ReduceHp(float num)
    {
        base.ReduceHp(num);

        // **����** �޸��ƶ��ٶ� & slowFactor
        slowFactor = newSlowFactor;
        movingSpeed = newMovingSpeed;

        // **ƽ��** ���� Blend Tree `speed` ����
        if (animationTransitionCoroutine != null)
        {
            StopCoroutine(animationTransitionCoroutine);
        }
        animationTransitionCoroutine = StartCoroutine(SmoothAnimationSpeed(1.0f));

        // **�����ָ��ٶȵ�Э��**���ȴ� slowDuration ��ָ���
        if (restoreSpeedCoroutine != null)
        {
            StopCoroutine(restoreSpeedCoroutine);
        }
        restoreSpeedCoroutine = StartCoroutine(RestoreSpeed());
    }

    /// <summary>
    /// ƽ���޸� Blend Tree �� `speed` ����
    /// </summary>
    private IEnumerator SmoothAnimationSpeed(float targetBlendSpeed)
    {
        float elapsedTime = 0f;
        float startBlendSpeed = animator.GetFloat("speed");

        while (elapsedTime < animationTransitionTime)
        {
            float blendValue = elapsedTime / animationTransitionTime;

            animator.SetFloat("speed", Mathf.Lerp(startBlendSpeed, targetBlendSpeed, blendValue));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        animator.SetFloat("speed", targetBlendSpeed);
    }

    /// <summary>
    /// �� slowDuration ��������ָ��ƶ��ٶȣ����� `speed` ƽ���� 0
    /// </summary>
    private IEnumerator RestoreSpeed()
    {
        yield return new WaitForSeconds(slowDuration);

        // **����** �ָ��ƶ��ٶ�
        movingSpeed = originalMovingSpeed;

        // **ƽ��** ���� Blend Tree `speed` ����
        if (animationTransitionCoroutine != null)
        {
            StopCoroutine(animationTransitionCoroutine);
        }
        animationTransitionCoroutine = StartCoroutine(SmoothAnimationSpeed(0f)); // 
    }
}
