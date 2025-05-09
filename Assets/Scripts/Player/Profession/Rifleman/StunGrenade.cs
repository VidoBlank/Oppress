using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 用于管理闪光光源淡出效果的辅助组件
public class LightFader : MonoBehaviour
{
    public Light targetLight;
    public float flickerDuration = 0.2f;
    public float fadeDuration = 1.5f;
    public float intensityMultiplier = 1.0f;
    public float maxRandomIntensity = 15f;
    public float minRandomIntensity = 5f;

    void Start()
    {
        if (targetLight != null)
            StartCoroutine(FlickerAndFade());
    }

    private IEnumerator FlickerAndFade()
    {
        float elapsedTime = 0f;

        // 快速闪烁阶段
        while (elapsedTime < flickerDuration && targetLight != null)
        {
            targetLight.intensity = Random.Range(minRandomIntensity, maxRandomIntensity) * intensityMultiplier;
            elapsedTime += Time.deltaTime;
            yield return new WaitForSeconds(0.02f); // 50fps的闪烁速率
        }

        // 逐渐衰减阶段
        float initialIntensity = targetLight != null ? targetLight.intensity : 0f;
        elapsedTime = 0f;

        while (elapsedTime < fadeDuration && targetLight != null)
        {
            targetLight.intensity = Mathf.Lerp(initialIntensity, 0f, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (targetLight != null)
            targetLight.intensity = 0f;
    }
}

public class StunGrenade : MonoBehaviour
{
    // 震撼弹参数
    private float damage;
    private float radius;
    private float slowFactor;
    private float slowDuration;
    private float speed;
    private float arcHeight;
    private PlayerController owner;

    // 飞行路径
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float journeyLength;
    private float startTime;
    private float flightProgress = 0f; // 飞行进度(0-1)
    private bool hasExploded = false;
    private bool isFlying = false;

    // 组件引用
    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;
    private Light pointLight;

    // 爆炸特效预制体
    public GameObject explosionEffectPrefab;

    // 爆炸特效自定义参数
    [Header("爆炸特效参数")]
    public Material flashParticleMaterial; // 主闪光粒子材质
    public Material haloEffectMaterial; // 光环效果材质
    public Material shockwaveMaterial; // 冲击波效果材质
    public Material smokeMaterial; // 烟雾效果材质
    public float effectScale = 1.0f; // 特效整体缩放
    public Color flashColor = Color.white; // 闪光颜色
    public float flashIntensity = 1.0f; // 闪光强度系数
    public bool useExtraLights = true; // 是否使用额外光源
    public bool useSmoke = true; // 是否产生烟雾

    [Header("效果速度参数")]
    public float haloEffectDuration = 1.0f; // 光环持续时间
    public float haloGrowthSpeed = 0.2f; // 光环扩散速度(值越小扩散越快)
    public float shockwaveEffectDuration = 0.5f; // 冲击波持续时间
    public float shockwaveSpeed = 15.0f; // 冲击波扩散速度

    [Header("光源渐变参数")]
    public float mainLightFlickerTime = 0.3f; // 主光源闪烁持续时间
    public float mainLightFadeTime = 2.5f; // 主光源淡出持续时间

    // 碰撞检测频率
    private float collisionCheckInterval = 0.05f;
    private float lastCollisionCheckTime = 0f;

    // 存储敌人原始速度的字典，用于减速后恢复
    private Dictionary<Enemy, float> originalSpeedCache = new Dictionary<Enemy, float>();

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
        pointLight = GetComponent<Light>();

        // 如果没有这些组件，可以添加
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        if (trailRenderer == null)
            CreateTrailRenderer();

        if (pointLight == null)
            CreatePointLight();

        // 添加刚体组件用于物理检测
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            CreateRigidbody();

        // 添加碰撞器用于碰撞检测
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null)
            CreateCollider();
    }

    private void CreateTrailRenderer()
    {
        trailRenderer = gameObject.AddComponent<TrailRenderer>();
        trailRenderer.startWidth = 0.1f;
        trailRenderer.endWidth = 0.01f;
        trailRenderer.time = 0.2f; // 拖尾持续时间
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        trailRenderer.startColor = new Color(0.8f, 0.8f, 0.8f, 0.4f);
        trailRenderer.endColor = new Color(1f, 0.6f, 0.2f, 0f); // 完全透明
    }

    private void CreatePointLight()
    {
        pointLight = gameObject.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.range = 2f;
        pointLight.intensity = 1f;
        pointLight.color = new Color(0.8f, 0.8f, 0.8f); // 橙黄色
    }

    private void CreateRigidbody()
    {
        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // 不受重力影响
        rb.isKinematic = true; // 运动学刚体，自己控制移动
    }

    private void CreateCollider()
    {
        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        col.radius = 0.2f; // 小碰撞范围
        col.isTrigger = true; // 设为触发器
    }

    public void Initialize(Vector3 start, Vector3 target, float speed, float arcHeight,
                          float damage, float radius, float slowFactor, float slowDuration,
                          PlayerController owner)
    {
        this.startPosition = start;
        this.targetPosition = target;
        this.speed = speed;
        this.arcHeight = arcHeight;
        this.damage = damage;
        this.radius = radius;
        this.slowFactor = slowFactor;
        this.slowDuration = slowDuration;
        this.owner = owner;

        // 计算飞行距离
        journeyLength = Vector3.Distance(start, target);
        startTime = Time.time;

        // 设置初始位置
        transform.position = startPosition;

        // 开始飞行
        isFlying = true;
        flightProgress = 0f;

        // 启动飞行协程
        StartCoroutine(FlyingCoroutine());

        // 销毁计时器（防止永不销毁）
        Destroy(gameObject, 10f);
    }

    private IEnumerator FlyingCoroutine()
    {
        while (isFlying && !hasExploded)
        {
            // 计算飞行进度
            float timeSinceStart = Time.time - startTime;
            float estimatedFlightTime = journeyLength / speed;
            flightProgress = timeSinceStart / estimatedFlightTime;

            // 判断是否到达终点
            if (flightProgress >= 1.0f)
            {
                Explode();
                yield break;
            }

            // 计算当前位置和旋转
            UpdatePositionAndRotation();

            // 等待下一帧
            yield return null;

            // 飞行过程中定期检查碰撞
            CheckCollisionsDuringFlight();
        }
    }

    private void UpdatePositionAndRotation()
    {
        // 计算当前位置
        Vector3 newPosition = CalculatePosition(flightProgress);
        transform.position = newPosition;

        // 计算飞行方向并旋转物体
        if (flightProgress < 0.98f) // 接近终点前才计算方向
        {
            float nextProgress = Mathf.Min(flightProgress + 0.01f, 0.98f);
            Vector3 nextPos = CalculatePosition(nextProgress);
            Vector3 direction = (nextPos - newPosition).normalized;

            if (direction != Vector3.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }

        // 动态调整灯光强度和颜色
        if (pointLight != null)
        {
            pointLight.intensity = Mathf.Lerp(0.8f, 1.2f, Mathf.PingPong(Time.time * 2f, 1f));
            pointLight.range = Mathf.Lerp(1.5f, 2.5f, Mathf.PingPong(Time.time * 3f, 1f));
        }
    }

    private Vector3 CalculatePosition(float progress)
    {
        // 线性插值基础位置
        Vector3 position = Vector3.Lerp(startPosition, targetPosition, progress);

        // 添加抛物线高度
        position.y += Mathf.Sin(progress * Mathf.PI) * arcHeight;

        return position;
    }

    private void CheckCollisionsDuringFlight()
    {
        // 控制碰撞检测频率，避免过于频繁
        if (Time.time - lastCollisionCheckTime < collisionCheckInterval)
            return;

        lastCollisionCheckTime = Time.time;

        // 检查半径内是否有碰撞体
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 0.25f);
        foreach (var hitCollider in hitColliders)
        {
            // 忽略自身和所有者
            if (hitCollider.gameObject == gameObject ||
                (owner != null && hitCollider.gameObject == owner.gameObject))
                continue;

            // 如果碰到地面、敌人或障碍物，则爆炸
            if (hitCollider.CompareTag("Ground") ||
                hitCollider.CompareTag("Enemy") ||
                hitCollider.CompareTag("Obstacle"))
            {
                Explode();
                return;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 忽略自身和所有者
        if (other.gameObject == gameObject ||
            (owner != null && other.gameObject == owner.gameObject))
            return;

        // 检查是否碰到了地形或敌人
        if (other.CompareTag("Ground") ||
            other.CompareTag("Enemy") ||
            other.CompareTag("Obstacle"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        isFlying = false;

        // 创建爆炸特效
        CreateExplosionEffect();

        // 检测范围内的敌人
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
        List<Enemy> affectedEnemies = new List<Enemy>();

        foreach (var hitCollider in hitColliders)
        {
            Enemy enemy = hitCollider.GetComponent<Enemy>();
            if (enemy != null && !enemy.isDead && !affectedEnemies.Contains(enemy))
            {
                // 应用伤害
                enemy.ReduceHp(damage);

                // 应用减速效果
                ApplySlowEffect(enemy);

                // 添加到已影响列表
                affectedEnemies.Add(enemy);
            }
        }

        // 隐藏震撼弹模型
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (trailRenderer != null)
            trailRenderer.enabled = false;

        // 逐渐关闭灯光
        if (pointLight != null)
            StartCoroutine(FadeOutLight());

        // 销毁对象
        Destroy(gameObject, 0.5f);
    }

    private void CreateExplosionEffect()
    {
        // 使用预制体播放爆炸特效
        if (explosionEffectPrefab != null)
        {
            GameObject explosionEffect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(explosionEffect, 2f); // 2秒后销毁特效
            return;
        }

        // 创建闪光弹特效对象
        GameObject stunEffect = new GameObject("StunGrenadeExplosion");
        stunEffect.transform.position = transform.position;

        // 创建主闪光光源
        Light flashLight = CreateFlashLight(stunEffect);

        // 创建闪光粒子系统
        ParticleSystem flashParticles = CreateFlashParticles(stunEffect);

        // 创建光环效果
        CreateHaloEffect(stunEffect);

        // 创建冲击波效果
        CreateShockwaveEffect(stunEffect);

        // 创建烟雾效果（可选）
        if (useSmoke)
            CreateSmokeEffect(stunEffect);

        // 播放主要粒子系统
        flashParticles.Play();

        // 添加额外闪光灯（可选）
        if (useExtraLights)
            CreateExtraLights(stunEffect);

        // 添加光源淡出组件
        LightFader lightFader = stunEffect.AddComponent<LightFader>();
        lightFader.targetLight = flashLight;
        lightFader.flickerDuration = mainLightFlickerTime;
        lightFader.fadeDuration = mainLightFadeTime;
        lightFader.intensityMultiplier = flashIntensity;

        // 销毁整个特效对象
        Destroy(stunEffect, mainLightFadeTime + 1.5f);

        // 播放爆炸音效
        PlayExplosionSound();
    }

    private Light CreateFlashLight(GameObject parent)
    {
        Light flashLight = parent.AddComponent<Light>();
        flashLight.type = LightType.Point;
        flashLight.range = radius * 2.5f * effectScale;
        flashLight.intensity = 8f * flashIntensity;
        flashLight.color = flashColor;
        flashLight.shadows = LightShadows.Hard;
        flashLight.bounceIntensity = 1.5f * flashIntensity;
        return flashLight;
    }

    private ParticleSystem CreateFlashParticles(GameObject parent)
    {
        ParticleSystem flashParticles = parent.AddComponent<ParticleSystem>();
        SetupFlashParticleSystem(flashParticles);

        // 添加渲染器组件并设置材质
        ParticleSystemRenderer flashRenderer = flashParticles.GetComponent<ParticleSystemRenderer>();
        SetupParticleRenderer(flashRenderer, flashParticleMaterial);

        return flashParticles;
    }

    private void SetupFlashParticleSystem(ParticleSystem particles)
    {
        var main = particles.main;
        main.duration = 0.2f;
        main.loop = false;
        main.startLifetime = 0.2f;
        main.startSpeed = 3f * effectScale;
        main.startSize = 0.05f * effectScale;
        main.maxParticles = 15;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        main.gravityModifier = -0.1f; // 粒子略微上升

        // 发射器设置
        var emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 50) });

        // 形状设置
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;
        shape.radiusThickness = 1f; // 从球体表面发射

        // 颜色随生命周期变化
        SetupFlashParticleColor(particles);

        // 大小随生命周期变化
        SetupFlashParticleSize(particles);

        // 速度随生命周期变化
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(3f * effectScale, 6f * effectScale);

        // 添加拖尾效果
        SetupFlashParticleTrails(particles);
    }

    private void SetupFlashParticleColor(ParticleSystem particles)
    {
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient flashGradient = new Gradient();
        flashGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 1f, 1f), 0.0f),
                new GradientColorKey(new Color(0.8f, 0.9f, 1f), 0.3f),
                new GradientColorKey(new Color(0.6f, 0.8f, 1f), 1.0f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(0.8f, 0.1f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colorOverLifetime.color = flashGradient;
    }

    private void SetupFlashParticleSize(ParticleSystem particles)
    {
        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeOverLifetimeCurve = new AnimationCurve();
        sizeOverLifetimeCurve.AddKey(0f, 0.1f);     // 开始时较小
        sizeOverLifetimeCurve.AddKey(0.1f, 1f);     // 迅速变大
        sizeOverLifetimeCurve.AddKey(1f, 0.1f);     // 结束时变小
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeOverLifetimeCurve);
    }

    private void SetupFlashParticleTrails(ParticleSystem particles)
    {
        var trails = particles.trails;
        trails.enabled = true;
        trails.ratio = 0.3f; // 30%的粒子有拖尾
        trails.lifetime = new ParticleSystem.MinMaxCurve(0.1f);
        trails.minVertexDistance = 0.1f;
        trails.widthOverTrail = new ParticleSystem.MinMaxCurve(0.1f * effectScale, 0f);
    }

    private void CreateHaloEffect(GameObject parent)
    {
        GameObject haloObj = new GameObject("StunGrenadeHalo");
        haloObj.transform.parent = parent.transform;
        haloObj.transform.localPosition = Vector3.zero;

        ParticleSystem haloParticles = haloObj.AddComponent<ParticleSystem>();
        SetupHaloParticleSystem(haloParticles);

        // 添加渲染器组件并设置材质
        ParticleSystemRenderer haloRenderer = haloParticles.GetComponent<ParticleSystemRenderer>();
        SetupParticleRenderer(haloRenderer, haloEffectMaterial);

        // 播放光环粒子
        haloParticles.Play();
    }

    private void SetupHaloParticleSystem(ParticleSystem particles)
    {
        var main = particles.main;
        main.duration = haloEffectDuration;
        main.loop = false;
        main.startLifetime = haloEffectDuration;
        main.startSpeed = 0f; // 不移动，固定位置
        main.startSize = radius * 2f * effectScale; // 大光环
        main.maxParticles = 10;

        // 发射器设置
        var emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 2) });

        // 形状设置
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        // 颜色随生命周期变化
        SetupHaloParticleColor(particles);

        // 大小随时间变化
        SetupHaloParticleSize(particles);
    }

    private void SetupHaloParticleColor(ParticleSystem particles)
    {
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient haloGradient = new Gradient();
        haloGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 1f, 1f), 0.0f),
                new GradientColorKey(new Color(0.8f, 0.9f, 1f), 0.5f),
                new GradientColorKey(new Color(0.3f, 0.6f, 1f), 1.0f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.8f, 0.0f),
                new GradientAlphaKey(0.4f, 0.2f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colorOverLifetime.color = haloGradient;
    }

    private void SetupHaloParticleSize(ParticleSystem particles)
    {
        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve haloSizeCurve = new AnimationCurve();
        haloSizeCurve.AddKey(0f, 0.1f);             // 开始很小
        haloSizeCurve.AddKey(haloGrowthSpeed, 1f);  // 使用自定义扩散速度
        haloSizeCurve.AddKey(1f, 2f);               // 继续扩大
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, haloSizeCurve);
    }

    private void CreateShockwaveEffect(GameObject parent)
    {
        GameObject shockwaveObj = new GameObject("StunGrenadeShockwave");
        shockwaveObj.transform.parent = parent.transform;
        shockwaveObj.transform.localPosition = Vector3.zero;

        ParticleSystem shockwaveParticles = shockwaveObj.AddComponent<ParticleSystem>();
        SetupShockwaveParticleSystem(shockwaveParticles);

        // 添加渲染器组件并设置材质
        ParticleSystemRenderer shockwaveRenderer = shockwaveParticles.GetComponent<ParticleSystemRenderer>();
        SetupParticleRenderer(shockwaveRenderer, shockwaveMaterial);

        // 播放冲击波粒子
        shockwaveParticles.Play();
    }

    private void SetupShockwaveParticleSystem(ParticleSystem particles)
    {
        var main = particles.main;
        main.duration = shockwaveEffectDuration;
        main.loop = false;
        main.startLifetime = shockwaveEffectDuration;
        main.startSpeed = shockwaveSpeed * effectScale;
        main.startSize = 0.3f * effectScale;
        main.maxParticles = 30;

        // 发射器设置
        var emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

        // 形状设置
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        // 颜色随生命周期变化
        SetupShockwaveParticleColor(particles);
    }

    private void SetupShockwaveParticleColor(ParticleSystem particles)
    {
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient shockwaveGradient = new Gradient();
        shockwaveGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 0.9f, 0.7f), 0.0f),
                new GradientColorKey(new Color(0.7f, 0.8f, 1f), 1.0f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.7f, 0.0f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colorOverLifetime.color = shockwaveGradient;
    }

    private void CreateSmokeEffect(GameObject parent)
    {
        GameObject smokeObj = new GameObject("StunGrenadeSmoke");
        smokeObj.transform.parent = parent.transform;
        smokeObj.transform.localPosition = Vector3.zero;

        ParticleSystem smokeParticles = smokeObj.AddComponent<ParticleSystem>();
        SetupSmokeParticleSystem(smokeParticles);

        // 添加渲染器组件并设置材质
        ParticleSystemRenderer smokeRenderer = smokeParticles.GetComponent<ParticleSystemRenderer>();
        SetupParticleRenderer(smokeRenderer, smokeMaterial);

        // 播放烟雾粒子
        smokeParticles.Play();
    }

    private void SetupSmokeParticleSystem(ParticleSystem particles)
    {
        var main = particles.main;
        main.duration = 2f;
        main.loop = false;
        main.startLifetime = 2f;
        main.startSpeed = 2f * effectScale;
        main.startSize = 0.5f * effectScale;
        main.startRotation = new ParticleSystem.MinMaxCurve(0, 360 * Mathf.Deg2Rad); // 随机旋转
        main.maxParticles = 20;

        // 发射器设置
        var emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });

        // 形状设置
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        // 颜色随生命周期变化
        SetupSmokeParticleColor(particles);

        // 大小随生命周期变化
        SetupSmokeParticleSize(particles);
    }

    private void SetupSmokeParticleColor(ParticleSystem particles)
    {
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient smokeGradient = new Gradient();
        smokeGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.8f, 0.8f, 0.8f), 0.0f),
                new GradientColorKey(new Color(0.6f, 0.6f, 0.6f), 1.0f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.7f, 0.0f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colorOverLifetime.color = smokeGradient;
    }

    private void SetupSmokeParticleSize(ParticleSystem particles)
    {
        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve smokeSizeCurve = new AnimationCurve();
        smokeSizeCurve.AddKey(0f, 0.5f);
        smokeSizeCurve.AddKey(1f, 2f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, smokeSizeCurve);
    }

    private void SetupParticleRenderer(ParticleSystemRenderer renderer, Material customMaterial)
    {
        if (renderer == null) return;

        if (customMaterial != null)
        {
            renderer.material = customMaterial;
            if (renderer.trailMaterial != null)
                renderer.trailMaterial = customMaterial;
        }
        else
        {
            // 创建默认粒子材质
            Material defaultMaterial = CreateDefaultParticleMaterial();
            if (defaultMaterial != null)
            {
                renderer.material = defaultMaterial;
                if (renderer.trailMaterial != null)
                    renderer.trailMaterial = defaultMaterial;
            }
        }

        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }

    private Material CreateDefaultParticleMaterial()
    {
        Material material = new Material(Shader.Find("Particles/Standard Unlit"));
        if (material == null)
        {
            material = new Material(Shader.Find("Particles/Standard Surface"));
            if (material == null)
                material = new Material(Shader.Find("Standard"));
        }

        if (material != null)
        {
            material.SetFloat("_Mode", 1); // 设置为半透明模式
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }

        return material;
    }

    private void CreateExtraLights(GameObject parent)
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject extraLightObj = new GameObject($"StunGrenadeExtraLight_{i}");
            extraLightObj.transform.parent = parent.transform;
            extraLightObj.transform.localPosition = Random.insideUnitSphere * 0.3f * effectScale;

            Light extraLight = extraLightObj.AddComponent<Light>();
            extraLight.type = LightType.Point;
            extraLight.range = radius * 1.0f * effectScale;
            extraLight.intensity = Random.Range(3f, 5f) * flashIntensity;
            extraLight.color = new Color(
                Mathf.Lerp(0.8f, 1f, Random.value),
                Mathf.Lerp(0.8f, 1f, Random.value),
                Mathf.Lerp(0.8f, 1f, Random.value));
            extraLight.shadows = LightShadows.None;

            // 添加辅助光源的淡入淡出效果控制器
            LightFader extraLightFader = extraLightObj.AddComponent<LightFader>();
            extraLightFader.targetLight = extraLight;
            extraLightFader.flickerDuration = 0.1f;
            extraLightFader.fadeDuration = 0.6f;
            extraLightFader.intensityMultiplier = flashIntensity;
        }
    }

    private IEnumerator FadeOutLight()
    {
        float duration = 0.3f;
        float initialIntensity = pointLight.intensity;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            pointLight.intensity = Mathf.Lerp(initialIntensity, 0f, timer / duration);
            yield return null;
        }

        pointLight.enabled = false;
    }

    private void PlayExplosionSound()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D音效
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 20f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.priority = 0; // 最高优先级
            audioSource.volume = 1f;
        }

        // 如果有音效资源，播放它
        AudioClip explosionClip = Resources.Load<AudioClip>("Sounds/StunGrenadeExplosion");
        if (explosionClip != null)
        {
            audioSource.PlayOneShot(explosionClip);
        }
    }

    private void ApplySlowEffect(Enemy enemy)
    {
        if (enemy == null || enemy.isDead) return;

        // 如果敌人之前没有被震撼弹减速
        if (!enemy._isStunSlowed)
        {
            // 标记敌人为已减速状态
            enemy._isStunSlowed = true;
            // 不需要再记录原始速度，因为已经在Start中记录了
        }

        // 无论是否之前已减速，都基于真正的原始速度应用减速
        enemy.movingSpeed = enemy.OriginalMovingSpeed * slowFactor;

        // 启动恢复协程
        // 停止之前的恢复协程（如果有）
        enemy.StopAllCoroutines();
        enemy.StartCoroutine(RestoreSpeedAfterDelay(enemy, slowDuration));

        // 应用视觉效果
        ApplyVisualEffect(enemy);
    }

    private IEnumerator RestoreSpeedAfterDelay(Enemy enemy, float duration)
    {
        yield return new WaitForSeconds(duration);

        // 恢复原始速度
        if (enemy != null && enemy.enabled && !enemy.isDead)
        {
            enemy.movingSpeed = enemy.OriginalMovingSpeed;
            enemy._isStunSlowed = false;
        }
    }
    private void ApplyVisualEffect(Enemy enemy)
    {
        // 视觉提示（可选）
        SpriteRenderer enemySprite = enemy.GetComponent<SpriteRenderer>();
        if (enemySprite != null)
        {
            // 添加减速视觉效果（例如变蓝色）
            Color originalColor = enemySprite.color;
            enemySprite.color = new Color(0.5f, 0.5f, 1f, originalColor.a);

            // 延迟恢复原始颜色
            StartCoroutine(ResetColorAfterDelay(enemySprite, originalColor, slowDuration));
        }
    }

    private IEnumerator ResetColorAfterDelay(SpriteRenderer spriteRenderer, Color originalColor, float delay)
    {
        yield return new WaitForSeconds(delay);

        // 检查组件是否仍然存在
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    // 用于在编辑器中可视化爆炸范围
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}