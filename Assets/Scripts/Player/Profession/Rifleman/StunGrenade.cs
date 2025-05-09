using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ���ڹ��������Դ����Ч���ĸ������
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

        // ������˸�׶�
        while (elapsedTime < flickerDuration && targetLight != null)
        {
            targetLight.intensity = Random.Range(minRandomIntensity, maxRandomIntensity) * intensityMultiplier;
            elapsedTime += Time.deltaTime;
            yield return new WaitForSeconds(0.02f); // 50fps����˸����
        }

        // ��˥���׶�
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
    // �𺳵�����
    private float damage;
    private float radius;
    private float slowFactor;
    private float slowDuration;
    private float speed;
    private float arcHeight;
    private PlayerController owner;

    // ����·��
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float journeyLength;
    private float startTime;
    private float flightProgress = 0f; // ���н���(0-1)
    private bool hasExploded = false;
    private bool isFlying = false;

    // �������
    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;
    private Light pointLight;

    // ��ը��ЧԤ����
    public GameObject explosionEffectPrefab;

    // ��ը��Ч�Զ������
    [Header("��ը��Ч����")]
    public Material flashParticleMaterial; // ���������Ӳ���
    public Material haloEffectMaterial; // �⻷Ч������
    public Material shockwaveMaterial; // �����Ч������
    public Material smokeMaterial; // ����Ч������
    public float effectScale = 1.0f; // ��Ч��������
    public Color flashColor = Color.white; // ������ɫ
    public float flashIntensity = 1.0f; // ����ǿ��ϵ��
    public bool useExtraLights = true; // �Ƿ�ʹ�ö����Դ
    public bool useSmoke = true; // �Ƿ��������

    [Header("Ч���ٶȲ���")]
    public float haloEffectDuration = 1.0f; // �⻷����ʱ��
    public float haloGrowthSpeed = 0.2f; // �⻷��ɢ�ٶ�(ֵԽС��ɢԽ��)
    public float shockwaveEffectDuration = 0.5f; // ���������ʱ��
    public float shockwaveSpeed = 15.0f; // �������ɢ�ٶ�

    [Header("��Դ�������")]
    public float mainLightFlickerTime = 0.3f; // ����Դ��˸����ʱ��
    public float mainLightFadeTime = 2.5f; // ����Դ��������ʱ��

    // ��ײ���Ƶ��
    private float collisionCheckInterval = 0.05f;
    private float lastCollisionCheckTime = 0f;

    // �洢����ԭʼ�ٶȵ��ֵ䣬���ڼ��ٺ�ָ�
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

        // ���û����Щ������������
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        if (trailRenderer == null)
            CreateTrailRenderer();

        if (pointLight == null)
            CreatePointLight();

        // ��Ӹ����������������
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            CreateRigidbody();

        // �����ײ��������ײ���
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null)
            CreateCollider();
    }

    private void CreateTrailRenderer()
    {
        trailRenderer = gameObject.AddComponent<TrailRenderer>();
        trailRenderer.startWidth = 0.1f;
        trailRenderer.endWidth = 0.01f;
        trailRenderer.time = 0.2f; // ��β����ʱ��
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        trailRenderer.startColor = new Color(0.8f, 0.8f, 0.8f, 0.4f);
        trailRenderer.endColor = new Color(1f, 0.6f, 0.2f, 0f); // ��ȫ͸��
    }

    private void CreatePointLight()
    {
        pointLight = gameObject.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.range = 2f;
        pointLight.intensity = 1f;
        pointLight.color = new Color(0.8f, 0.8f, 0.8f); // �Ȼ�ɫ
    }

    private void CreateRigidbody()
    {
        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // ��������Ӱ��
        rb.isKinematic = true; // �˶�ѧ���壬�Լ������ƶ�
    }

    private void CreateCollider()
    {
        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        col.radius = 0.2f; // С��ײ��Χ
        col.isTrigger = true; // ��Ϊ������
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

        // ������о���
        journeyLength = Vector3.Distance(start, target);
        startTime = Time.time;

        // ���ó�ʼλ��
        transform.position = startPosition;

        // ��ʼ����
        isFlying = true;
        flightProgress = 0f;

        // ��������Э��
        StartCoroutine(FlyingCoroutine());

        // ���ټ�ʱ������ֹ�������٣�
        Destroy(gameObject, 10f);
    }

    private IEnumerator FlyingCoroutine()
    {
        while (isFlying && !hasExploded)
        {
            // ������н���
            float timeSinceStart = Time.time - startTime;
            float estimatedFlightTime = journeyLength / speed;
            flightProgress = timeSinceStart / estimatedFlightTime;

            // �ж��Ƿ񵽴��յ�
            if (flightProgress >= 1.0f)
            {
                Explode();
                yield break;
            }

            // ���㵱ǰλ�ú���ת
            UpdatePositionAndRotation();

            // �ȴ���һ֡
            yield return null;

            // ���й����ж��ڼ����ײ
            CheckCollisionsDuringFlight();
        }
    }

    private void UpdatePositionAndRotation()
    {
        // ���㵱ǰλ��
        Vector3 newPosition = CalculatePosition(flightProgress);
        transform.position = newPosition;

        // ������з�����ת����
        if (flightProgress < 0.98f) // �ӽ��յ�ǰ�ż��㷽��
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

        // ��̬�����ƹ�ǿ�Ⱥ���ɫ
        if (pointLight != null)
        {
            pointLight.intensity = Mathf.Lerp(0.8f, 1.2f, Mathf.PingPong(Time.time * 2f, 1f));
            pointLight.range = Mathf.Lerp(1.5f, 2.5f, Mathf.PingPong(Time.time * 3f, 1f));
        }
    }

    private Vector3 CalculatePosition(float progress)
    {
        // ���Բ�ֵ����λ��
        Vector3 position = Vector3.Lerp(startPosition, targetPosition, progress);

        // ��������߸߶�
        position.y += Mathf.Sin(progress * Mathf.PI) * arcHeight;

        return position;
    }

    private void CheckCollisionsDuringFlight()
    {
        // ������ײ���Ƶ�ʣ��������Ƶ��
        if (Time.time - lastCollisionCheckTime < collisionCheckInterval)
            return;

        lastCollisionCheckTime = Time.time;

        // ���뾶���Ƿ�����ײ��
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 0.25f);
        foreach (var hitCollider in hitColliders)
        {
            // ���������������
            if (hitCollider.gameObject == gameObject ||
                (owner != null && hitCollider.gameObject == owner.gameObject))
                continue;

            // ����������桢���˻��ϰ����ը
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
        // ���������������
        if (other.gameObject == gameObject ||
            (owner != null && other.gameObject == owner.gameObject))
            return;

        // ����Ƿ������˵��λ����
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

        // ������ը��Ч
        CreateExplosionEffect();

        // ��ⷶΧ�ڵĵ���
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
        List<Enemy> affectedEnemies = new List<Enemy>();

        foreach (var hitCollider in hitColliders)
        {
            Enemy enemy = hitCollider.GetComponent<Enemy>();
            if (enemy != null && !enemy.isDead && !affectedEnemies.Contains(enemy))
            {
                // Ӧ���˺�
                enemy.ReduceHp(damage);

                // Ӧ�ü���Ч��
                ApplySlowEffect(enemy);

                // ��ӵ���Ӱ���б�
                affectedEnemies.Add(enemy);
            }
        }

        // �����𺳵�ģ��
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (trailRenderer != null)
            trailRenderer.enabled = false;

        // �𽥹رյƹ�
        if (pointLight != null)
            StartCoroutine(FadeOutLight());

        // ���ٶ���
        Destroy(gameObject, 0.5f);
    }

    private void CreateExplosionEffect()
    {
        // ʹ��Ԥ���岥�ű�ը��Ч
        if (explosionEffectPrefab != null)
        {
            GameObject explosionEffect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(explosionEffect, 2f); // 2���������Ч
            return;
        }

        // �������ⵯ��Ч����
        GameObject stunEffect = new GameObject("StunGrenadeExplosion");
        stunEffect.transform.position = transform.position;

        // �����������Դ
        Light flashLight = CreateFlashLight(stunEffect);

        // ������������ϵͳ
        ParticleSystem flashParticles = CreateFlashParticles(stunEffect);

        // �����⻷Ч��
        CreateHaloEffect(stunEffect);

        // ���������Ч��
        CreateShockwaveEffect(stunEffect);

        // ��������Ч������ѡ��
        if (useSmoke)
            CreateSmokeEffect(stunEffect);

        // ������Ҫ����ϵͳ
        flashParticles.Play();

        // ��Ӷ�������ƣ���ѡ��
        if (useExtraLights)
            CreateExtraLights(stunEffect);

        // ��ӹ�Դ�������
        LightFader lightFader = stunEffect.AddComponent<LightFader>();
        lightFader.targetLight = flashLight;
        lightFader.flickerDuration = mainLightFlickerTime;
        lightFader.fadeDuration = mainLightFadeTime;
        lightFader.intensityMultiplier = flashIntensity;

        // ����������Ч����
        Destroy(stunEffect, mainLightFadeTime + 1.5f);

        // ���ű�ը��Ч
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

        // �����Ⱦ����������ò���
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
        main.gravityModifier = -0.1f; // ������΢����

        // ����������
        var emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 50) });

        // ��״����
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;
        shape.radiusThickness = 1f; // ��������淢��

        // ��ɫ���������ڱ仯
        SetupFlashParticleColor(particles);

        // ��С���������ڱ仯
        SetupFlashParticleSize(particles);

        // �ٶ����������ڱ仯
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(3f * effectScale, 6f * effectScale);

        // �����βЧ��
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
        sizeOverLifetimeCurve.AddKey(0f, 0.1f);     // ��ʼʱ��С
        sizeOverLifetimeCurve.AddKey(0.1f, 1f);     // Ѹ�ٱ��
        sizeOverLifetimeCurve.AddKey(1f, 0.1f);     // ����ʱ��С
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeOverLifetimeCurve);
    }

    private void SetupFlashParticleTrails(ParticleSystem particles)
    {
        var trails = particles.trails;
        trails.enabled = true;
        trails.ratio = 0.3f; // 30%����������β
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

        // �����Ⱦ����������ò���
        ParticleSystemRenderer haloRenderer = haloParticles.GetComponent<ParticleSystemRenderer>();
        SetupParticleRenderer(haloRenderer, haloEffectMaterial);

        // ���Ź⻷����
        haloParticles.Play();
    }

    private void SetupHaloParticleSystem(ParticleSystem particles)
    {
        var main = particles.main;
        main.duration = haloEffectDuration;
        main.loop = false;
        main.startLifetime = haloEffectDuration;
        main.startSpeed = 0f; // ���ƶ����̶�λ��
        main.startSize = radius * 2f * effectScale; // ��⻷
        main.maxParticles = 10;

        // ����������
        var emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 2) });

        // ��״����
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        // ��ɫ���������ڱ仯
        SetupHaloParticleColor(particles);

        // ��С��ʱ��仯
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
        haloSizeCurve.AddKey(0f, 0.1f);             // ��ʼ��С
        haloSizeCurve.AddKey(haloGrowthSpeed, 1f);  // ʹ���Զ�����ɢ�ٶ�
        haloSizeCurve.AddKey(1f, 2f);               // ��������
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, haloSizeCurve);
    }

    private void CreateShockwaveEffect(GameObject parent)
    {
        GameObject shockwaveObj = new GameObject("StunGrenadeShockwave");
        shockwaveObj.transform.parent = parent.transform;
        shockwaveObj.transform.localPosition = Vector3.zero;

        ParticleSystem shockwaveParticles = shockwaveObj.AddComponent<ParticleSystem>();
        SetupShockwaveParticleSystem(shockwaveParticles);

        // �����Ⱦ����������ò���
        ParticleSystemRenderer shockwaveRenderer = shockwaveParticles.GetComponent<ParticleSystemRenderer>();
        SetupParticleRenderer(shockwaveRenderer, shockwaveMaterial);

        // ���ų��������
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

        // ����������
        var emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

        // ��״����
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        // ��ɫ���������ڱ仯
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

        // �����Ⱦ����������ò���
        ParticleSystemRenderer smokeRenderer = smokeParticles.GetComponent<ParticleSystemRenderer>();
        SetupParticleRenderer(smokeRenderer, smokeMaterial);

        // ������������
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
        main.startRotation = new ParticleSystem.MinMaxCurve(0, 360 * Mathf.Deg2Rad); // �����ת
        main.maxParticles = 20;

        // ����������
        var emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });

        // ��״����
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        // ��ɫ���������ڱ仯
        SetupSmokeParticleColor(particles);

        // ��С���������ڱ仯
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
            // ����Ĭ�����Ӳ���
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
            material.SetFloat("_Mode", 1); // ����Ϊ��͸��ģʽ
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

            // ��Ӹ�����Դ�ĵ��뵭��Ч��������
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
            audioSource.spatialBlend = 1f; // 3D��Ч
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 20f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.priority = 0; // ������ȼ�
            audioSource.volume = 1f;
        }

        // �������Ч��Դ��������
        AudioClip explosionClip = Resources.Load<AudioClip>("Sounds/StunGrenadeExplosion");
        if (explosionClip != null)
        {
            audioSource.PlayOneShot(explosionClip);
        }
    }

    private void ApplySlowEffect(Enemy enemy)
    {
        if (enemy == null || enemy.isDead) return;

        // �������֮ǰû�б��𺳵�����
        if (!enemy._isStunSlowed)
        {
            // ��ǵ���Ϊ�Ѽ���״̬
            enemy._isStunSlowed = true;
            // ����Ҫ�ټ�¼ԭʼ�ٶȣ���Ϊ�Ѿ���Start�м�¼��
        }

        // �����Ƿ�֮ǰ�Ѽ��٣�������������ԭʼ�ٶ�Ӧ�ü���
        enemy.movingSpeed = enemy.OriginalMovingSpeed * slowFactor;

        // �����ָ�Э��
        // ֹ֮ͣǰ�Ļָ�Э�̣�����У�
        enemy.StopAllCoroutines();
        enemy.StartCoroutine(RestoreSpeedAfterDelay(enemy, slowDuration));

        // Ӧ���Ӿ�Ч��
        ApplyVisualEffect(enemy);
    }

    private IEnumerator RestoreSpeedAfterDelay(Enemy enemy, float duration)
    {
        yield return new WaitForSeconds(duration);

        // �ָ�ԭʼ�ٶ�
        if (enemy != null && enemy.enabled && !enemy.isDead)
        {
            enemy.movingSpeed = enemy.OriginalMovingSpeed;
            enemy._isStunSlowed = false;
        }
    }
    private void ApplyVisualEffect(Enemy enemy)
    {
        // �Ӿ���ʾ����ѡ��
        SpriteRenderer enemySprite = enemy.GetComponent<SpriteRenderer>();
        if (enemySprite != null)
        {
            // ��Ӽ����Ӿ�Ч�����������ɫ��
            Color originalColor = enemySprite.color;
            enemySprite.color = new Color(0.5f, 0.5f, 1f, originalColor.a);

            // �ӳٻָ�ԭʼ��ɫ
            StartCoroutine(ResetColorAfterDelay(enemySprite, originalColor, slowDuration));
        }
    }

    private IEnumerator ResetColorAfterDelay(SpriteRenderer spriteRenderer, Color originalColor, float delay)
    {
        yield return new WaitForSeconds(delay);

        // �������Ƿ���Ȼ����
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    // �����ڱ༭���п��ӻ���ը��Χ
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}