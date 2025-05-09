using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf_02 : Enemy
{
    [Header("��̹�������")]
    [Tooltip("����ٶ�")]
    public float dashSpeed = 10f;

    [Tooltip("��̳���ʱ��")]
    public float dashDuration = 0.5f;

    [Tooltip("���ڼ��Ŀ���Ƿ�����̷�Χ�� Collider2D�������� IsTrigger��")]
    public Collider2D dashCollider;

    [Header("�״ι������޸ĵĹ�����")]
    [Tooltip("��һ�ι������޸ĵĹ�����")]
    public int newAttackDamage = 15;

    // ����Ƿ��Ѿ������˳�̹�������һ�ι�����
    private bool hasDashed = false;
    // ��ǵ�ǰ�Ƿ��ڳ��״̬
    private bool isDashing = false;
    // ��¼���Ŀ�꣨����Ŀ�� Tag Ϊ "Player"��
    private Transform dashTarget;

    private Rigidbody2D rb;

    /// <summary>
    /// ������ Start() �е��� base.Start()������ʼ�� Rigidbody2D
    /// </summary>
    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        if(rb == null)
        {
            Debug.LogError($"{gameObject.name} ȱ�� Rigidbody2D �����");
        }

        if(dashCollider == null)
        {
            Debug.LogError($"{gameObject.name} ȱ�� dashCollider������ Inspector ��ָ����");
        }
    }

    /// <summary>
    /// ��Ŀ����� dashCollider ��Χʱ������̹���
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ����Ŀ��� Tag Ϊ "Player"
        if (!hasDashed && !isDashing && collision.CompareTag("Player"))
        {
            dashTarget = collision.transform;
            StartCoroutine(DashAttack());
        }
    }

    /// <summary>
    /// ��̹���Э�̣��ڳ���ڼ䣬��״̬����Ϊ�� Moving������ Attack������̽�����ָ�״̬Ϊ Moving
    /// </summary>
    private IEnumerator DashAttack()
    {
        isDashing = true;
        hasDashed = true;
        animator.SetFloat("speed", 1f);
        // �ڳ���ڼ䣬��״̬����Ϊ Attack����ͣ�����ƶ��߼�
        state = EnemyState.Attack;
        Debug.Log($"{gameObject.name} ��ʼ��̹�����");

        // �����̷��򣨹�һ����
        Vector2 dashDir = (dashTarget.position - transform.position).normalized;

        float timer = 0f;
        while (timer < dashDuration)
        {
            // ʹ��ֱ���޸� transform.position ���г�̣�Ҳ���� rb.MovePosition ���棩
            transform.position += (Vector3)(dashDir * dashSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // ��̽����󣬽�״̬�ָ�Ϊ Moving��ʹ���� Update() �ָ����� Moving() ����
        state = EnemyState.Moving;
        isDashing = false;
        Debug.Log($"{gameObject.name} ��̹����������ָ������ƶ���");
    }

    /// <summary>
    /// ��д��ͨ������������һ�ι�������̹����󣩳ɹ��󣬽��������޸�Ϊָ��ֵ
    /// </summary>
    public override void Attacking(Unit unit)
    {
        base.Attacking(unit);

        // ����Ѿ���ɳ�̹�������һ�ι����������޸Ĺ�����
        if (hasDashed)
        {
            attackDamage = newAttackDamage;
            Debug.Log($"{gameObject.name} ��һ�ι����󣬹������޸�Ϊ {newAttackDamage}");
            animator.SetFloat("speed", 0f);
        }
    }
}
