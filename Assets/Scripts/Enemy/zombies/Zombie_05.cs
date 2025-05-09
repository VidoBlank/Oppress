using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie_05 : Enemy
{
    [Header("��ģʽ����")]
    [Tooltip("�����ģʽ�� HP ��ֵ")]
    public float berserkHpThreshold = 50f;

    [Tooltip("��ǰҡʱ�䣨rage ����ʱ�䣩")]
    public float rageDuration = 2.0f;

    [Tooltip("��ģʽ�Ĺ�����")]
    public int berserkAttackDamage = 50;

    [Tooltip("��ģʽ�Ĺ������")]
    public float berserkAttackInterval = 0.8f;

    [Tooltip("��ģʽ���ƶ��ٶ�")]
    public float berserkMovingSpeed = 3.5f;

    [Tooltip("��ģʽ�� slowFactor�����ͼ���Ч��Ӱ�죩")]
    public float berserkSlowFactor = 0.9f;

    [Tooltip("��ǰҡ�ڼ���˺����������0.5 = �ܵ� 50% �˺���")]
    public float damageReductionFactor = 0.5f;

    [Header("���� & �Ӿ�Ч��")]
    [Tooltip("���� Blend Tree `speed` �����ı仯ʱ��")]
    public float animationTransitionTime = 1.0f;

    [Tooltip("ָ��Ҫ�仯��ģ�Ͷ���")]
    public Transform berserkModelObject;  // **ָ��Ҫ�仯��ģ��**

    [Tooltip("��״̬�µķŴ���������ԭʼ�ߴ磩")]
    public float berserkScaleMultiplier = 1.2f;

    [Tooltip("ģ�ͷŴ����ʱ��")]
    public float scaleTransitionTime = 1.0f;

    private bool isBerserk = false; // ����Ƿ��Ѿ������ģʽ
    private bool isRaging = false; // ����Ƿ��� `rage` ����״̬
    private float originalAttackDamage;
    private float originalAttackInterval;
    private float originalMovingSpeed;
    private float originalSlowFactor;
    private Vector3 originalScale; // ��¼ԭʼģ�ʹ�С
    private Coroutine animationTransitionCoroutine;
    private Coroutine scaleCoroutine;

    private void Start()
    {
        // ��ȡ Animator
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        // **ȷ�� berserkModelObject ��Ϊ�գ�����Ĭ��ʹ�õ�ǰ����**
        if (berserkModelObject == null)
        {
            berserkModelObject = transform;
            Debug.LogWarning($"{gameObject.name}: berserkModelObject Ϊ�գ�Ĭ��ʹ�ñ���");
        }

        // ��¼ԭʼ����
        originalAttackDamage = attackDamage;
        originalAttackInterval = attackInterval;
        originalMovingSpeed = movingSpeed;
        originalSlowFactor = slowFactor;
        originalScale = berserkModelObject.localScale; // ��¼ **ָ��ģ��** ��ԭʼ��С
    }

    /// <summary>
    /// ����ʱ����������Ƿ���Ҫ�����ģʽ
    /// </summary>
    public override void ReduceHp(float num)
    {
        if (isRaging) // �� rage ����״̬�£���ִ����ͨ�ܻ��߼�
        {
            num *= damageReductionFactor; // �˺�����
        }

        base.ReduceHp(num);

        // ����Ƿ�����ģʽ
        if (!isBerserk && !isRaging && HP <= berserkHpThreshold)
        {
            StartCoroutine(RagePhase());
        }
    }

    /// <summary>
    /// �����ǰҡ��rage ״̬�����ڴ��ڼ��޷��ƶ��������ܵ����˺�������ָ��ģ���𽥷Ŵ�
    /// </summary>
    private IEnumerator RagePhase()
    {
        isRaging = true;
        animator.SetTrigger("rage"); // ���� `rage` ����
        Debug.Log($"{gameObject.name} �����ǰҡ״̬���޷��ƶ����˺����� {damageReductionFactor * 100}%");

        // ��ֹ�ƶ�
        float previousSpeed = movingSpeed;
        movingSpeed = 0;

        // **ģ���𽥷Ŵ�**
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(ScaleOverTime(berserkModelObject, originalScale * berserkScaleMultiplier, scaleTransitionTime));

        // �ȴ� `rageDuration` ����
        yield return new WaitForSeconds(rageDuration);

        // �ָ��ƶ��ٶ�
        movingSpeed = previousSpeed;

        // ���������Ŀ�ģʽ
        EnterBerserkMode();
    }

    /// <summary>
    /// �����ģʽ
    /// </summary>
    private void EnterBerserkMode()
    {
        isBerserk = true;
        isRaging = false; // ��� `rage` ״̬

        // �޸�����
        attackDamage = berserkAttackDamage;
        attackInterval = berserkAttackInterval;
        movingSpeed = berserkMovingSpeed;
        slowFactor = berserkSlowFactor;

        Debug.Log($"{gameObject.name} �����ģʽ��������: {attackDamage}, �ƶ��ٶ�: {movingSpeed}, �������: {attackInterval}");

        // **ƽ������ Blend Tree `speed` ����**
        if (animationTransitionCoroutine != null)
        {
            StopCoroutine(animationTransitionCoroutine);
        }
        animationTransitionCoroutine = StartCoroutine(SmoothAnimationSpeed(1.0f));
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
    /// **ƽ������ָ��ģ�͵Ĵ�С**
    /// </summary>
    private IEnumerator ScaleOverTime(Transform target, Vector3 targetScale, float duration)
    {
        if (target == null) yield break; // ȷ������Ϊ��

        float elapsedTime = 0f;
        Vector3 startScale = target.localScale;

        while (elapsedTime < duration)
        {
            target.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        target.localScale = targetScale;
    }
}
