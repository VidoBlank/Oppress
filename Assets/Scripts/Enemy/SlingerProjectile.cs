using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlingerProjectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector3 targetPosition;
    private int damage;
    private float flightTime;
    private float explosionRadius;

    [Tooltip("�ӵ����ʱ�䣬������ʱ����Զ�������ըЧ��")]
    public float lifetime = 5f; // �ɸ�����Ҫ�� Inspector �е���

    [Tooltip("��ը��ЧԤ���壬�ڴ�����ըʱ����")]
    public GameObject explosionEffectPrefab;

    [Tooltip("��ը��Ч���ų���ʱ�䣬��������Զ�������Ч����")]
    public float explosionEffectDuration = 2f; // ��λ����

    // ���������ã��������ӵ�����ʱ֪ͨ�����߼����ӵ�����
    private Slinger shooter;

    // ��ֹ�ظ����� Explode
    private bool hasExploded = false;

    void Awake()
    {
        // ��ȡ Rigidbody2D ����������������Զ����
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 1;
    }

    /// <summary>
    /// ��ʼ���ӵ����������������䣬ͬʱ�����ӵ����ʱ��
    /// </summary>
    /// <param name="target">Ŀ��λ��</param>
    /// <param name="damage">�����˺�</param>
    /// <param name="flightTime">����ʱ��</param>
    /// <param name="explosionRadius">��ը�뾶</param>
    /// <param name="shooter">����������</param>
    public void Initialize(Vector3 target, int damage, float flightTime, float explosionRadius, Slinger shooter)
    {
        this.targetPosition = target;
        this.damage = damage;
        this.flightTime = flightTime;
        this.explosionRadius = explosionRadius;
        this.shooter = shooter;
        LaunchProjectile();

        // �����ӵ����ʱ�䣬���� lifetime ���Զ����� Explode����Ŀ��ʱ���ӵ�λ��Ϊ���ģ�
        Invoke("Explode", lifetime);
    }

    /// <summary>
    /// ���㲢�����ӵ��ĳ�ʼ�ٶȣ�ʹ���� flightTime �ڵ���Ŀ��λ��
    /// </summary>
    void LaunchProjectile()
    {
        Vector3 startPosition = transform.position;
        Vector3 displacement = targetPosition - startPosition;

        // ʹ�� Physics2D.gravity ��������ֵ
        float g = Mathf.Abs(Physics2D.gravity.y);

        // ����ˮƽ�ʹ�ֱ����ĳ�ʼ�ٶȣ�
        // vx = dx / t
        // vy = (dy + 0.5 * g * t^2) / t
        float vx = displacement.x / flightTime;
        float vy = (displacement.y + 0.5f * g * flightTime * flightTime) / flightTime;

        rb.velocity = new Vector2(vx, vy);
    }

    void Update()
    {
        // ���ݵ�ǰ�ٶȵ����ӵ���ת����
        if (rb.velocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            float angleOffset = 0f; // ������Ҫ����ƫ�ƽǶȣ��� -90f
            transform.rotation = Quaternion.Euler(0, 0, angle + angleOffset);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // �����ײ����ǩΪ "Object" �����壬����ԣ������󴥷���
        if (collision.CompareTag("Object"))
        {
            return;
        }
        // ֻ�����һ��ϰ���
        if (collision.CompareTag("Player") || collision.CompareTag("Obstacle"))
        {
            CancelInvoke("Explode");
            Explode(collision);
        }
    }

    /// <summary>
    /// ������ը������ʹ����ײĿ���������Ϊ��ը���ģ�
    /// ����ײ�� Ground ʱ�������ӵ�λ��Ϊ��ը���ģ����˴������Ѻ��� Ground��ʹ���޷��赲�ӵ�����
    /// </summary>
    /// <param name="collisionTarget">��ײĿ��� Collider2D</param>
    void Explode(Collider2D collisionTarget)
    {
        if (hasExploded)
            return;
        hasExploded = true;

        Vector3 explosionCenter;
        // �����ײ������ Ground�������ӵ�λ��Ϊ��ը����
        if (collisionTarget.CompareTag("Ground"))
        {
            explosionCenter = transform.position;
        }
        else
        {
            explosionCenter = collisionTarget.bounds.center;
        }

        DoExplode(explosionCenter);
    }

    /// <summary>
    /// ������ը����������ײĿ��ʱ���ӵ�λ��Ϊ��ը����
    /// </summary>
    void Explode()
    {
        if (hasExploded)
            return;
        hasExploded = true;
        DoExplode(transform.position);
    }

    /// <summary>
    /// ����ը�߼����Է�Χ��Ŀ������˺������ű�ը��Ч��Ȼ�������ӵ�
    /// </summary>
    /// <param name="explosionCenter">��ը�����ĵ�</param>
    private void DoExplode(Vector3 explosionCenter)
    {
        // �Ա�ը���ķ�Χ�ڵ�Ŀ������˺�
        Collider2D[] hits = Physics2D.OverlapCircleAll(explosionCenter, explosionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerController player = hit.GetComponent<PlayerController>();
                if (player != null && !player.isDead && !player.isKnockedDown)
                {
                    player.TakeDamage(damage);
                }
            }
            else if (hit.CompareTag("Obstacle"))
            {
                Obstacle obstacle = hit.GetComponent<Obstacle>();
                if (obstacle != null)
                {
                    obstacle.TakeDamage(damage, null, true);
                }
            }
        }

        // ������ը���ĵ� Z ���꣨���磺��Ϊ 0.25f������������Ը��������޸�
        explosionCenter.z = 0.25f;

        // �ڱ�ը�������ɱ�ը��ЧԤ���壬������תΪ X �� 90�㣨���� position.z ����Ϊָ��ֵ��
        if (explosionEffectPrefab != null)
        {
            GameObject effectInstance = Instantiate(explosionEffectPrefab, explosionCenter, Quaternion.Euler(90f, 0f, 0f));
            Destroy(effectInstance, explosionEffectDuration);
        }

        // ֪ͨ�����߼����ӵ�����
        if (shooter != null)
        {
            shooter.ProjectileDestroyed();
            shooter = null;
        }
        Destroy(gameObject);
    }

    // ��ѡ���� Scene ��ͼ����ʾ��ը��Χ
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
