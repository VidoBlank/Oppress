using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 步枪兵角色类 - 完整实现
/// </summary>
public class Rifleman : PlayerController
{
    [Header("武器Transform")]
    public Transform weapon;

    [Header("枪口的目标位置")]
    public Transform targetGameObject;

    #region 技能1：速射
    [Header("步枪速射技能参数")]
    public int rapidFireBulletCount = 15;
    public float rapidFireAttackDelay = 0.1f;
    public float rapidFireBulletSpeed = 20f;
    public float rapidFireBulletRange = 5f;
    public float rapidFireEnergyCost = 20f;
    public float rapidFireCooldown = 8f;
    #endregion

    #region 技能2：震撼弹
    [Header("震撼弹技能参数")]
    public float stunGrenadeCooldown = 12f;    // 冷却时间
    public float stunGrenadeRange = 8f;        // 施法范围
    public float stunGrenadeDamage = 35f;      // 震撼弹伤害
    public float stunGrenadeRadius = 3f;       // 影响范围
    public float stunGrenadeSlowFactor = 0.5f; // 减速效果(乘数)
    public float stunGrenadeSlowDuration = 4f; // 减速持续时间
    public float stunGrenadeSpeed = 6f;        // 飞行速度
    public float stunGrenadeArc = 0.5f;        // 抛物线高度系数
    public float stunGrenadeEnergyCost = 15f;  // 能量消耗
    public GameObject stunGrenadePrefab;       // 震撼弹预制体
    public GameObject stunGrenadeExplosionEffect; // 震撼弹爆炸特效预制体
    #endregion

    #region 技能3：重型火力
    [Header("重型火力技能参数")]
    public float heavyFirepowerCooldown = 5f;         // 冷却时间
    public float heavyFirepowerEnergyCost = 15f;      // 启动能量消耗
    public float heavyFirepowerAttackDelay = 1f;    // 榴弹发射间隔增加值
    public float heavyFirepowerBulletSpeed = 15f;     // 榴弹飞行速度调整值
    public float heavyFirepowerAttackEnergyCost = 3f; // 每次榴弹消耗能量
    public float heavyFirepowerDamage = 35f;          // 榴弹固定伤害值
    public float heavyFirepowerExplosionRadius = 2f;  // 榴弹爆炸范围
    public float heavyFirepowerAttackRangeBonus = 2f; // 重型火力提供的攻击范围加成
    public Color heavyFirepowerWeaponColor = new Color(1f, 0.5f, 0f, 1f); // 重型火力模式武器颜色

    [Header("榴弹抛物线设置")]
    public float heavyFirepowerArcHeight = 0.5f; // 榴弹抛物线高度
    public float heavyFirepowerArcAngle = 45f;   // 榴弹抛射角度

    [Header("榴弹地面弹跳设置")]
    public float heavyFirepowerBounceFactor = 0.3f;    // 弹跳系数 (0-1)
    public float heavyFirepowerBounceDelay = 0.8f;     // 弹跳后延迟爆炸时间
    public int heavyFirepowerMaxBounces = 1;           // 最大弹跳次数

    [Header("榴弹专用预制体")]
    public GameObject grenadeProjectilePrefab;       // 榴弹子弹预制体(可选)
    public GameObject grenadeExplosionEffect;        // 榴弹爆炸特效预制体(可使用震撼弹的)
    public AudioClip grenadeFireSound;               // 榴弹发射音效
    public AudioClip grenadeExplosionSound;          // 榴弹爆炸音效
    #endregion

    // 用于追踪榴弹模式状态的私有字段
    private bool isGrenadeMode = false;

    protected override void Awake()
    {
        base.Awake();
        RefreshUnlockedSkills();
    }

    protected override void Start()
    {
        base.Start();

        // 确保level至少为1，避免没有技能的情况
        if (level < 1)
            level = 1;

        // 初始化武器引用
        if (weapon == null)
        {
            weapon = transform.Find("Weapon");
            if (weapon == null && symbol.muzzle != null)
            {
                weapon = symbol.muzzle;
            }
        }

        // 初始化预制体
        if (stunGrenadePrefab == null)
        {
            stunGrenadePrefab = Resources.Load<GameObject>("Prefabs/StunGrenade");
            Debug.Log($"尝试加载震撼弹预制体: {(stunGrenadePrefab != null ? "成功" : "失败")}");
        }

        if (grenadeProjectilePrefab == null && stunGrenadePrefab != null)
        {
            // 如果没有专门的榴弹预制体，可以复用震撼弹预制体
            grenadeProjectilePrefab = stunGrenadePrefab;
            Debug.Log("使用震撼弹预制体作为榴弹预制体");
        }

        if (grenadeExplosionEffect == null && stunGrenadeExplosionEffect != null)
        {
            // 如果没有专门的爆炸效果，可以复用震撼弹爆炸效果
            grenadeExplosionEffect = stunGrenadeExplosionEffect;
            Debug.Log("使用震撼弹爆炸效果作为榴弹爆炸效果");
        }
    }

    public override void RefreshUnlockedSkills()
    {
        unlockedSkills.Clear();
        Debug.Log($"🔁 刷新技能列表，当前等级为：{level}");

        if (level >= 1)
        {
            var skill1 = new RiflemanSkill1_RapidFire();
            skill1.Init(this);
            unlockedSkills.Add(skill1);
            Debug.Log("✅ 添加 skill1");
        }

        if (level >= 2)
        {
            var skill2 = new RiflemanSkill2_StunGrenade();
            skill2.Init(this);
            unlockedSkills.Add(skill2);
            Debug.Log("✅ 添加 skill2");
        }

        if (level >= 3)
        {
            var skill3 = new RiflemanSkill3_HeavyFirepower();
            skill3.Init(this);
            unlockedSkills.Add(skill3);
            Debug.Log("✅ 添加 skill3");
        }

        Debug.Log($"📋 当前 unlockedSkills.Count = {unlockedSkills.Count}");
        for (int i = 0; i < unlockedSkills.Count; i++)
        {
            Debug.Log($"🔎 skill{i} = {unlockedSkills[i]?.SkillName ?? "null"} ({unlockedSkills[i]?.GetType().Name ?? "null"})");
        }
    }

    /// <summary>
    /// 获取当前是否处于榴弹模式
    /// </summary>
    /// <returns>如果当前处于榴弹模式则为true，否则为false</returns>
    public bool IsInGrenadeMode()
    {
        return isGrenadeMode;
    }
    private Coroutine resetSpeedCoroutine;
    private Coroutine smoothSetSpeedCoroutine;

    /// <summary>
    /// 设置榴弹模式状态
    /// </summary>
    public void SetGrenadeMode(bool enabled)
    {
        isGrenadeMode = enabled;

        if (animator != null)
        {
            if (enabled)
            {
                // 开始 0.5 秒过渡设为 1
                if (resetSpeedCoroutine != null)
                {
                    StopCoroutine(resetSpeedCoroutine);
                    resetSpeedCoroutine = null;
                }

                if (smoothSetSpeedCoroutine != null)
                    StopCoroutine(smoothSetSpeedCoroutine);
                smoothSetSpeedCoroutine = StartCoroutine(SmoothSetSpeedTo(1f, 0.15f));
            }
            else
            {
                // 延迟 1 秒后再用 0.2 秒平滑设为 0
                if (resetSpeedCoroutine != null)
                    StopCoroutine(resetSpeedCoroutine);
                resetSpeedCoroutine = StartCoroutine(ResetSpeedSmoothlyAfterDelay(1.8f, 0.2f));

                if (smoothSetSpeedCoroutine != null)
                {
                    StopCoroutine(smoothSetSpeedCoroutine);
                    smoothSetSpeedCoroutine = null;
                }
            }
        }

        // 原先逻辑
        if (enabled)
        {
            Debug.Log($"[{name}] 切换至榴弹发射模式");
            if (skillCooldownManager != null)
                skillCooldownManager.SetSkillActiveVisual(true);
        }
        else
        {
            Debug.Log($"[{name}] 退出榴弹发射模式");
            if (skillCooldownManager != null)
                skillCooldownManager.SetSkillActiveVisual(false);
        }
    }

    /// <summary>
    /// 平滑将 speed 过渡为目标值
    /// </summary>
    private IEnumerator SmoothSetSpeedTo(float target, float duration)
    {
        float start = animator.GetFloat("speed");
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            animator.SetFloat("speed", Mathf.Lerp(start, target, t));
            yield return null;
        }

        animator.SetFloat("speed", target);
    }

    /// <summary>
    /// 延迟后平滑将 speed 设为目标值（退出模式）
    /// </summary>
    private IEnumerator ResetSpeedSmoothlyAfterDelay(float delay, float transitionDuration)
    {
        yield return new WaitForSeconds(delay);
        smoothSetSpeedCoroutine = StartCoroutine(SmoothSetSpeedTo(0f, transitionDuration));
    }


    // 覆盖父类的AutoShoot方法，添加榴弹发射逻辑
    protected override void AutoShoot()
    {
        // 如果开启了榴弹模式，使用榴弹发射逻辑
        if (IsInGrenadeMode())
        {
            AutoShootGrenadeMode();
        }
        else
        {
            // 否则使用父类的普通射击逻辑
            base.AutoShoot();
        }
    }

    // 榴弹发射模式下的自动射击
    private void AutoShootGrenadeMode()
    {
        // 检查是否允许射击
        if (!canShoot || energy <= 0 || isInteracting)
        {
            
            return;
        }

        // 检查手动目标是否超出攻击范围
        if (manualTarget != null)
        {
            float dis = Vector2.Distance(transform.position, manualTarget.transform.position);
            if (dis > TotalAttackRange + 0.5f)
            {
                RemoveManualTarget();
            }
        }

        GameObject target = null;

        // 优先手动目标
        if (manualTarget != null)
        {
            target = manualTarget;
        }
        else
        {
            // 自动查找最近且在攻击范围内的敌人
            float closestDistance = Mathf.Infinity;

            for (int i = enemys.Count - 1; i >= 0; i--)
            {
                GameObject enemy = enemys[i];
                if (enemy == null)
                {
                    enemys.RemoveAt(i);
                    continue;
                }

                float dis = Vector2.Distance(transform.position, enemy.transform.position);
                if (dis <= TotalAttackRange + 0.5f && dis < closestDistance)
                {
                    closestDistance = dis;
                    target = enemy;
                }
            }
        }

        // 非移动状态且冷却完成才能开火
        if (!isMoving && shootTimer > TotalAttackDelay)
        {
            if (target != null)
            {
                

                // 调整朝向
                ChangeFaceDirection(target.transform.position.x - transform.position.x);

                // 获取发射点
                Transform muzzle = symbol.muzzle;
                if (muzzle == null && weapon != null)
                {
                    muzzle = weapon;
                }

                // 获取榴弹预制体
                GameObject grenadePrefab = null;
                if (grenadeProjectilePrefab != null)
                {
                    grenadePrefab = grenadeProjectilePrefab;
                }
                else
                {
                    // 获取HeavyFirepower技能
                    RiflemanSkill3_HeavyFirepower heavyFirepower = null;
                    foreach (var skill in unlockedSkills)
                    {
                        if (skill is RiflemanSkill3_HeavyFirepower)
                        {
                            heavyFirepower = skill as RiflemanSkill3_HeavyFirepower;
                            break;
                        }
                    }

                    // 如果找到了技能实例，请求它来发射榴弹
                    if (heavyFirepower != null)
                    {
                        heavyFirepower.FireGrenade(target.transform.position);

                        // 扣能量
                        energy -= TotalAttackEnergyCost;
                        energy = Mathf.Max(energy, 0);

                        shootTimer = 0f;
                        return; // 已通过技能发射，退出方法
                    }

                    Debug.LogWarning("无法找到重型火力技能实例或榴弹预制体，使用普通子弹");
                    // 如果找不到技能实例或预制体，使用普通方式
                    GameObject bulletInstance = Instantiate(symbol.bullet, muzzle.position, transform.rotation);
                    bulletInstance.GetComponent<Bullet>().StartMovingToTarget(
                        target, TotalDamage, attribute.attackRange.radius + 0.5f, TotalBulletSpeed, this);

                    // 扣能量
                    energy -= TotalAttackEnergyCost;
                    energy = Mathf.Max(energy, 0);

                    shootTimer = 0f;
                    return;
                }

                // 创建和发射榴弹
                GameObject grenadeObj = Instantiate(grenadePrefab, muzzle.position, Quaternion.identity);
                grenadeObj.SetActive(true);

                // 初始化榴弹
                GrenadeBullet grenade = grenadeObj.GetComponent<GrenadeBullet>();
                if (grenade != null)
                {
                    // 使用重型火力技能定义的固定伤害值
                    grenade.Initialize(
                        muzzle.position,
                        target.transform.position,
                        heavyFirepowerDamage, // 使用固定榴弹伤害
                        TotalBulletSpeed,
                        heavyFirepowerExplosionRadius,
                        this
                    );

                    // 设置抛物线参数
                    grenade.arcHeight = heavyFirepowerArcHeight;
                    grenade.arcAngle = heavyFirepowerArcAngle;

                    // 设置弹跳参数
                    grenade.bounceFactor = heavyFirepowerBounceFactor;
                    grenade.bounceDelay = heavyFirepowerBounceDelay;
                    grenade.maxBounces = heavyFirepowerMaxBounces;
                    grenade.groundLayerMask = LayerMask.GetMask("Ground");

                    // 如果有爆炸特效预制体，设置给榴弹
                    if (grenadeExplosionEffect != null)
                    {
                        grenade.explosionEffectPrefab = grenadeExplosionEffect;
                    }

                    // 播放发射音效
                    if (grenadeFireSound != null)
                    {
                        AudioSource audio = GetComponent<AudioSource>();
                        if (audio != null)
                        {
                            audio.PlayOneShot(grenadeFireSound, 0.7f);
                        }
                    }

                    // 触发发射事件
                    TriggerNormalAttack();
                    animator.SetTrigger("skill3shooting");
                }
                else
                {
                    Debug.LogError("生成的榴弹缺少GrenadeBullet组件！");
                }

                // 扣能量
                energy -= TotalAttackEnergyCost;
                energy = Mathf.Max(energy, 0);

                shootTimer = 0f;
            }
            else
            {
             
            }
        }
        else
        {
         
        }
    }
}