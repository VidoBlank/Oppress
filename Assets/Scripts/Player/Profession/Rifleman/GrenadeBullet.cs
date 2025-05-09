using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 独立的榴弹子弹类，不继承自Bullet
/// </summary>
public class GrenadeBullet : MonoBehaviour
{
    [Header("榴弹属性")]
    public float damage = 30f;
    public float speed = 15f;
    public float maxRange = 10f;
    public float explosionRadius = 2f;
    public float arcHeight = 0.5f;
    public float arcAngle = 45f;  // 发射角度
    public float lifetime = 5f;

    [Header("弹跳设置")]
    public float bounceFactor = 0.3f;    // 弹跳系数 (0-1)
    public float bounceDelay = 0.8f;     // 弹跳后的延迟爆炸时间
    public int maxBounces = 1;           // 最大弹跳次数
    private float bounceDuration;







    public LayerMask groundLayerMask;    // 地面层遮罩
    public float launchHeightOffset = 0.5f;  // 在 Inspector 里也能调节
    [Header("视觉效果")]
    public GameObject explosionEffectPrefab;
    public Color trailColor = new Color(1f, 0.5f, 0f, 0.5f);
    public SpriteRenderer grenadeSprite;

    [Header("物理设置")]
    public LayerMask targetLayerMask;   // 目标层遮罩(Enemy, Obstacle)

    // 轨迹参数
    private Vector3 startPos;
    private Vector3 targetPos;
    private float journeyLength;
    private float startTime;
    private bool hasExploded = false;
    private PlayerController owner;

    // 弹跳相关
    private int bounceCount = 0;
    private bool isInBouncePhase = false;
    private Vector3 bounceDirection;
    private float bounceStartTime;
    private Vector3 bounceStartPos;
    private float bounceDistance;
    private Coroutine delayedExplosionCoroutine;

    // 组件引用
    private TrailRenderer trailRenderer;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private void Awake()
    {

        // 确保有弹体渲染器
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // 添加拖尾效果
        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer == null)
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
            trailRenderer.time = 0.3f;
            trailRenderer.startWidth = 0.1f;
            trailRenderer.endWidth = 0.02f;
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
            trailRenderer.startColor = trailColor;
            trailRenderer.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
        }

        // 添加碰撞体
        if (GetComponent<CircleCollider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.2f;
        }

        // 添加刚体
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;  // 初始不受重力影响
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        // 设置初始层级
        gameObject.layer = LayerMask.NameToLayer("Bullet");

        // 如果未设置目标层级，设置默认值
        if (targetLayerMask.value == 0)
        {
            targetLayerMask = LayerMask.GetMask("Enemy", "Obstacle");
        }

        // 如果未设置地面层级，设置默认值
        if (groundLayerMask.value == 0)
        {
            groundLayerMask = LayerMask.GetMask("Ground");
        }
    }

    /// <summary>
    /// 初始化榴弹并开始飞行
    /// </summary>
    public void Initialize(Vector3 startPosition, Vector3 targetPosition, float damageAmount, float flightSpeed, float explosionSize, PlayerController sender)
    {
        this.startPos = startPosition;
        this.damage = damageAmount;
        this.speed = flightSpeed;
        this.explosionRadius = explosionSize;
        this.owner = sender;
        this.startPos = startPosition + new Vector3(0f, launchHeightOffset, 0f);
        // 1) 射线找地面高度
        Vector3 groundPoint = startPosition;
        RaycastHit2D hit = Physics2D.Raycast(
            new Vector2(targetPosition.x, startPosition.y + 5f),
            Vector2.down,
            10f,
            groundLayerMask
        );
        if (hit.collider != null)
            groundPoint.y = hit.point.y;              // 找到地面
        else
            groundPoint.y = startPosition.y;          // 没找到就同高

        this.targetPos = new Vector3(targetPosition.x, groundPoint.y, targetPosition.z);

        // 2) 计算水平行程和飞行时长
        journeyLength = Vector3.Distance(startPos, targetPos);
        lifetime = journeyLength / speed;               // 严格用距离/速度
        startTime = Time.time;

        // 3) 初始位置和朝向
        transform.position = startPosition;
        Vector3 dir = (targetPos - startPosition).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 4) 子弹自旋效果
        StartCoroutine(RotateOverTime());
    }

    private void Update()
    {
        if (hasExploded)
            return;

     
        // 根据当前状态更新位置
        if (isInBouncePhase)
        {
            UpdateBouncePosition();
        }
        else
        {
            float progress = (Time.time - startTime) / lifetime;
            MoveBullet(progress);
        }
    }

    /// <summary>
    /// 根据进度计算并更新榴弹位置（抛物线轨迹）
    /// </summary>
    private void MoveBullet(float progress)
    {
        if (isInBouncePhase) return;

        // 限制 t 在 [0,1]，保证不会跑过头
        float t = Mathf.Clamp01(progress);

        // 水平插值
        Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

        // 抛物线高度：y = 4 * h * t * (1 - t)
        float heightOffset = 4f * arcHeight * t * (1f - t);
        currentPos.y += heightOffset;
        currentPos.z = 0f;

        transform.position = currentPos;

        // 更新朝向（沿切线方向）
        if (t < 0.95f)
        {
            float nextT = Mathf.Clamp01(t + 0.01f);
            Vector3 nextPos = Vector3.Lerp(startPos, targetPos, nextT);
            nextPos.y += 4f * arcHeight * nextT * (1f - nextT);
            nextPos.z = 0f;
            Vector3 dir = (nextPos - currentPos).normalized;
            float rotAng = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(rotAng, Vector3.forward);
        }
    }
    /// <summary>
    /// 更新弹跳阶段的位置
    /// </summary>
    private void UpdateBouncePosition()
    {
        if (!isInBouncePhase || hasExploded)
            return;

        float bounceElapsedTime = Time.time - bounceStartTime;
        float bounceProgress = Mathf.Clamp01(bounceElapsedTime / bounceDuration);

        // 计算弹跳后的新位置
        Vector3 bounceTarget = bounceStartPos + bounceDirection * bounceDistance;
        bounceTarget.z = 0f; // 确保Z轴为0

        Vector3 currentPos = Vector3.Lerp(bounceStartPos, bounceTarget, bounceProgress);

        // 添加抛物线弹跳效果（在弹跳的过程中，高度先增加后减少）
        float bounceHeight = bounceDistance * bounceFactor * 0.5f;
        float heightOffset = 4f * bounceHeight * bounceProgress * (1f - bounceProgress);
        currentPos.y += heightOffset;
        currentPos.z = 0f; // 确保Z轴为0

        // 更新位置
        transform.position = currentPos;

        // 更新朝向
        if (bounceProgress < 0.95f)
        {
            float nextProgress = Mathf.Min(bounceProgress + 0.01f, 1f);
            Vector3 nextPos = Vector3.Lerp(bounceStartPos, bounceTarget, nextProgress);
            nextPos.y += 4f * bounceHeight * nextProgress * (1f - nextProgress);
            nextPos.z = 0f; // 确保Z轴为0

            Vector3 direction = (nextPos - currentPos).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // 当弹跳结束时爆炸
        if (bounceProgress >= 1.0f && delayedExplosionCoroutine == null)
         {
            delayedExplosionCoroutine = StartCoroutine(DelayedExplosion(bounceDelay));
          }
    }

    /// <summary>
    /// 开始弹跳
    /// </summary>
    private void StartBounce(Vector3 contactPoint, Vector3 normal)
    {
        // 记录弹跳次数
        bounceCount++;

        // 设置为弹跳阶段
        isInBouncePhase = true;

        // 保存弹跳起点
        bounceStartPos = transform.position;
        bounceStartTime = Time.time;

        // 计算弹跳方向（入射角度反射）
        Vector3 inDirection = (transform.position - startPos).normalized;
        bounceDirection = Vector3.Reflect(inDirection, normal).normalized;
        bounceDirection.z = 0f; // 确保Z轴为0

        // 随着弹跳次数增加，弹跳距离减少
        bounceDistance = journeyLength * bounceFactor / bounceCount;
        bounceDuration = bounceDistance / speed;
        // 播放弹跳音效
        PlayBounceSound();

        // 创建弹跳特效
        CreateBounceEffect(contactPoint);

        Debug.Log($"榴弹开始第{bounceCount}次弹跳，方向:{bounceDirection}, 距离:{bounceDistance}");
    }

    /// <summary>
    /// 播放弹跳音效
    /// </summary>
    private void PlayBounceSound()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            // 可以在这里播放弹跳音效
            audioSource.pitch = 1.5f;
            if (audioSource.clip != null)
            {
                audioSource.PlayOneShot(audioSource.clip, 0.3f);
            }
        }
    }

    /// <summary>
    /// 创建弹跳特效
    /// </summary>
    private void CreateBounceEffect(Vector3 contactPoint)
    {
        // 创建简单的弹跳特效（小型粒子爆发）
        GameObject bounceEffect = new GameObject("BounceSpark");
        bounceEffect.transform.position = contactPoint;

        // 添加粒子系统
        ParticleSystem ps = bounceEffect.AddComponent<ParticleSystem>();

        // 配置粒子系统
        var main = ps.main;
        main.startColor = new Color(1f, 0.7f, 0.3f, 0.8f);  // 黄橙色
        main.startSize = 0.3f;
        main.startLifetime = 0.3f;
        main.duration = 0.1f;
        main.loop = false;

        // 设置发射
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 15) });

        // 设置形状
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 30f;

        // 修复错误：正确设置形状的旋转
        // 计算反射角度（2D平面内）
        Vector3 reflectDir = Vector3.Reflect(transform.forward, Vector3.up);
        float reflectAngle = Mathf.Atan2(reflectDir.y, reflectDir.x) * Mathf.Rad2Deg;
        shape.rotation = new Vector3(0, 0, reflectAngle);

        // 设置粒子大小变化
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 0f);

        // 自动销毁
        Destroy(bounceEffect, 1f);
    }

    /// <summary>
    /// 使榴弹绕自身轴旋转的效果
    /// </summary>
    private IEnumerator RotateOverTime()
    {
        while (!hasExploded)
        {
            transform.Rotate(0, 0, 360f * Time.deltaTime);
            yield return null;
        }
    }

    /// <summary>
    /// 碰撞检测
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasExploded)
            return;

        // 检查碰撞的层
        int collisionLayer = collision.gameObject.layer;

        // 只处理地面碰撞，不对敌人或障碍物产生即时爆炸反应
        if (((1 << collisionLayer) & groundLayerMask) != 0)
        {
            // 如果还有弹跳次数，进行弹跳
            if (bounceCount < maxBounces)
            {
                // 计算碰撞点和法线
                Vector3 contactPoint = collision.ClosestPoint(transform.position);
                Vector3 normal = (transform.position - contactPoint).normalized;
                normal.z = 0f; // 确保Z轴为0

                // 开始弹跳
                StartBounce(contactPoint, normal);
            }
            else
            {
                // 没有弹跳次数了，等待短暂时间后爆炸
                if (delayedExplosionCoroutine == null)
                {
                    delayedExplosionCoroutine = StartCoroutine(DelayedExplosion(0.2f));
                }
            }
        }

        // 注意：不再处理敌人和障碍物的碰撞爆炸，榴弹只会在地面弹跳后爆炸
    }

    /// <summary>
    /// 延迟爆炸
    /// </summary>
    private IEnumerator DelayedExplosion(float delay)
    {
        yield return new WaitForSeconds(delay);
        Explode();
    }

    /// <summary>
    /// 爆炸效果
    /// </summary>
    public void Explode()
    {
        if (hasExploded)
            return;

        hasExploded = true;

        // 创建爆炸特效
        CreateExplosionEffect();

        // 应用范围伤害
        ApplyExplosionDamage();

        // 隐藏子弹并销毁
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
        if (trailRenderer != null)
            trailRenderer.enabled = false;

        // 禁用碰撞体
        var collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;

        // 延迟销毁物体
        Destroy(gameObject, 2f);
    }

    /// <summary>
    /// 创建爆炸视觉特效
    /// </summary>
    private void CreateExplosionEffect()
    {
        // 使用预设特效
        if (explosionEffectPrefab != null)
        {
            GameObject explosionObj = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

            // 设置爆炸效果大小
            explosionObj.transform.localScale = Vector3.one * (explosionRadius / 2f);

            // 尝试获取粒子系统
            ParticleSystem ps = explosionObj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                // 计算销毁时间（基于粒子系统持续时间）
                float destroyDelay = ps.main.duration + ps.main.startLifetime.constant;
                Destroy(explosionObj, destroyDelay);
            }
            else
            {
                // 默认2秒后销毁
                Destroy(explosionObj, 2f);
            }
        }
        else
        {
            // 如果没有预设特效，创建简单的粒子效果
            CreateSimpleExplosionEffect();
        }

        // 播放爆炸音效
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }

    /// <summary>
    /// 创建简单的粒子爆炸效果
    /// </summary>
    private void CreateSimpleExplosionEffect()
    {
        GameObject explosionObj = new GameObject("GrenadeExplosion");
        explosionObj.transform.position = transform.position;

        // 添加粒子系统
        ParticleSystem ps = explosionObj.AddComponent<ParticleSystem>();

        // 配置主模块
        var main = ps.main;
        main.startColor = new Color(1f, 0.5f, 0f, 0.8f);  // 橙色
        main.startSize = explosionRadius;
        main.startLifetime = 0.5f;
        main.duration = 0.3f;
        main.loop = false;

        // 配置发射模块
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

        // 配置形状模块
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        // 添加颜色渐变
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.5f, 0f), 0.0f),  // 开始橙色
                new GradientColorKey(new Color(0.8f, 0.3f, 0f), 0.3f), // 中间红橙色
                new GradientColorKey(new Color(0.5f, 0.5f, 0.5f), 1.0f)  // 结束灰色
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.8f, 0.0f),
                new GradientAlphaKey(0.5f, 0.3f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colorOverLifetime.color = gradient;

        // 设置大小变化
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeOverLifetimeCurve = new AnimationCurve();
        sizeOverLifetimeCurve.AddKey(0f, 0.5f);
        sizeOverLifetimeCurve.AddKey(0.1f, 1f);
        sizeOverLifetimeCurve.AddKey(1f, 0.1f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeOverLifetimeCurve);

        // 销毁
        Destroy(explosionObj, 2f);
    }

    /// <summary>
    /// 应用爆炸范围伤害
    /// </summary>
    private void ApplyExplosionDamage()
    {
        // 获取范围内的所有碰撞体
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, targetLayerMask);

        // 处理每个受影响的对象
        foreach (Collider2D hitCollider in hitColliders)
        {
            // 固定伤害，不再进行距离判断与衰减
            float actualDamage = damage;

            if (hitCollider.CompareTag("Enemy"))
            {
                Enemy enemy = hitCollider.GetComponent<Enemy>();
                if (enemy != null && !enemy.isDead)
                {
                    enemy.ReduceHp(actualDamage);
                    ApplyKnockback(enemy, Vector2.Distance(transform.position, hitCollider.transform.position));

                    if (owner != null)
                        owner.TriggerBulletHitEnemy(enemy, actualDamage);

                    Debug.Log($"榴弹爆炸对敌人 {enemy.name} 造成 {actualDamage:F1} 点固定伤害");
                }
            }
            else if (hitCollider.CompareTag("Obstacle"))
            {
                Obstacle obstacle = hitCollider.GetComponent<Obstacle>();
                if (obstacle != null && owner != null)
                {
                    obstacle.ApplyDamageFromPlayer(owner);
                    Debug.Log($"榴弹爆炸对障碍物 {obstacle.name} 造成伤害");
                }
            }
        }
    }


    /// <summary>
    /// 应用击退效果
    /// </summary>
    private void ApplyKnockback(Enemy enemy, float distance)
    {
        if (enemy == null || enemy.GetComponent<Rigidbody2D>() == null)
            return;

        // 计算击退方向（从爆炸中心向外）
        Vector2 knockbackDirection = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;

        // 计算击退力度（距离越近越强）
        float knockbackForce = Mathf.Lerp(10f, 3f, distance / explosionRadius);

        // 应用力
        enemy.GetComponent<Rigidbody2D>().AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
    }

    /// <summary>
    /// 在编辑器中可视化爆炸范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
    }
}