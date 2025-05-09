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

    [Tooltip("子弹存活时间，超过该时间后自动触发爆炸效果")]
    public float lifetime = 5f; // 可根据需要在 Inspector 中调整

    [Tooltip("爆炸特效预制体，在触发爆炸时播放")]
    public GameObject explosionEffectPrefab;

    [Tooltip("爆炸特效播放持续时间，播放完后自动销毁特效对象")]
    public float explosionEffectDuration = 2f; // 单位：秒

    // 发射者引用，用于在子弹销毁时通知发射者减少子弹计数
    private Slinger shooter;

    // 防止重复调用 Explode
    private bool hasExploded = false;

    void Awake()
    {
        // 获取 Rigidbody2D 组件，若不存在则自动添加
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 1;
    }

    /// <summary>
    /// 初始化子弹参数，并启动发射，同时设置子弹存活时间
    /// </summary>
    /// <param name="target">目标位置</param>
    /// <param name="damage">攻击伤害</param>
    /// <param name="flightTime">飞行时间</param>
    /// <param name="explosionRadius">爆炸半径</param>
    /// <param name="shooter">发射者引用</param>
    public void Initialize(Vector3 target, int damage, float flightTime, float explosionRadius, Slinger shooter)
    {
        this.targetPosition = target;
        this.damage = damage;
        this.flightTime = flightTime;
        this.explosionRadius = explosionRadius;
        this.shooter = shooter;
        LaunchProjectile();

        // 设置子弹存活时间，超过 lifetime 后自动调用 Explode（无目标时以子弹位置为中心）
        Invoke("Explode", lifetime);
    }

    /// <summary>
    /// 计算并设置子弹的初始速度，使其在 flightTime 内到达目标位置
    /// </summary>
    void LaunchProjectile()
    {
        Vector3 startPosition = transform.position;
        Vector3 displacement = targetPosition - startPosition;

        // 使用 Physics2D.gravity 计算重力值
        float g = Mathf.Abs(Physics2D.gravity.y);

        // 计算水平和垂直方向的初始速度：
        // vx = dx / t
        // vy = (dy + 0.5 * g * t^2) / t
        float vx = displacement.x / flightTime;
        float vy = (displacement.y + 0.5f * g * flightTime * flightTime) / flightTime;

        rb.velocity = new Vector2(vx, vy);
    }

    void Update()
    {
        // 根据当前速度调整子弹旋转方向
        if (rb.velocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            float angleOffset = 0f; // 根据需要调整偏移角度，如 -90f
            transform.rotation = Quaternion.Euler(0, 0, angle + angleOffset);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 如果碰撞到标签为 "Object" 的物体，则忽略（避免误触发）
        if (collision.CompareTag("Object"))
        {
            return;
        }
        // 只检测玩家或障碍物
        if (collision.CompareTag("Player") || collision.CompareTag("Obstacle"))
        {
            CancelInvoke("Explode");
            Explode(collision);
        }
    }

    /// <summary>
    /// 公共爆炸方法：使用碰撞目标的中心作为爆炸中心；
    /// 当碰撞到 Ground 时，则以子弹位置为爆炸中心（但此处我们已忽略 Ground，使其无法阻挡子弹）。
    /// </summary>
    /// <param name="collisionTarget">碰撞目标的 Collider2D</param>
    void Explode(Collider2D collisionTarget)
    {
        if (hasExploded)
            return;
        hasExploded = true;

        Vector3 explosionCenter;
        // 如果碰撞到的是 Ground，则以子弹位置为爆炸中心
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
    /// 公共爆炸方法：无碰撞目标时以子弹位置为爆炸中心
    /// </summary>
    void Explode()
    {
        if (hasExploded)
            return;
        hasExploded = true;
        DoExplode(transform.position);
    }

    /// <summary>
    /// 处理爆炸逻辑：对范围内目标造成伤害，播放爆炸特效，然后销毁子弹
    /// </summary>
    /// <param name="explosionCenter">爆炸的中心点</param>
    private void DoExplode(Vector3 explosionCenter)
    {
        // 对爆炸中心范围内的目标造成伤害
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

        // 调整爆炸中心的 Z 坐标（例如：设为 0.25f），这里你可以根据需求修改
        explosionCenter.z = 0.25f;

        // 在爆炸中心生成爆炸特效预制体，设置旋转为 X 轴 90°（并将 position.z 保持为指定值）
        if (explosionEffectPrefab != null)
        {
            GameObject effectInstance = Instantiate(explosionEffectPrefab, explosionCenter, Quaternion.Euler(90f, 0f, 0f));
            Destroy(effectInstance, explosionEffectDuration);
        }

        // 通知发射者减少子弹计数
        if (shooter != null)
        {
            shooter.ProjectileDestroyed();
            shooter = null;
        }
        Destroy(gameObject);
    }

    // 可选：在 Scene 视图中显示爆炸范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
