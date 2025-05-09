using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 重型火力技能 - 使用榴弹发射器
/// </summary>
public class RiflemanSkill3_HeavyFirepower : IPlayerSkill
{
    public string SkillID => "rifleman_skill_3";
    public string SkillName => "重型火力";
    public bool IsSustained => true;   // 持续型技能
    public bool IsActive { get; private set; } = false;
    public bool IsInstantCast => true; // 即时施放，不需要瞄准

    private PlayerController player;
    private Rifleman rifleman;

    // 原始状态缓存
    private GameObject originalBulletPrefab;
    private GameObject grenadeBulletPrefab;
    private float originalAttackEnergyCost;
    private Color originalWeaponColor;

    // 属性修改追踪
    private float addedAttackDelay = 0f;
    private float addedBulletSpeed = 0f;
    private float addedAttackEnergyCost = 0f;
    private float addedAttackRange = 0f;

    // 临时对象引用
    private GameObject muzzleFlashEffect;

    // 协程引用
    private Coroutine weaponEffectCoroutine;

    public void Init(PlayerController player)
    {
        this.player = player;
        this.rifleman = player as Rifleman;

        // ✅ 改进：不返回！即使转换失败也不要退出初始化
        if (rifleman == null)
        {
            Debug.LogWarning("⚠️ RiflemanSkill3_HeavyFirepower 初始化警告：player 不是 Rifleman！");
            // 不直接return，继续初始化基本逻辑
        }

        // 预创建榴弹预制体
        PrepareGrenadePrefab();

        // ✅ 增加日志，帮助调试
        Debug.Log($"RiflemanSkill3_HeavyFirepower 初始化完成，SkillID={SkillID}");
    }

    public float NeedEnergy => rifleman != null ? rifleman.heavyFirepowerEnergyCost : 0f;
    public float Cooldown => rifleman != null ? rifleman.heavyFirepowerCooldown : 0f;

    public void Prepare(PlayerController player)
    {
        // 即时施放技能，不需要准备阶段
    }

    public void HandleMouseInput(PlayerController player)
    {
        // 即时施放技能，不需要特殊鼠标处理
    }

    public void OnSkillStart(PlayerController player)
    {
        if (IsActive) return;

        this.player = player;
        this.rifleman = player as Rifleman;

        if (rifleman == null)
        {
            Debug.LogWarning("⚠️ RiflemanSkill3_HeavyFirepower 激活警告：player 不是 Rifleman！");
            return;
        }

        IsActive = true;

        // 播放开启技能动画
        player.animator.ResetTrigger("skill3end");
        player.animator.SetTrigger("skill3start");

        // 保存原始状态（仅保存引用，不修改）
        SaveOriginalState();

        // 启用榴弹模式
        EnableGrenadeMode();

        // 显示技能激活状态
        if (player.skillCooldownManager != null)
        {
            player.skillCooldownManager.SetSkillActiveVisual(true);
        }

        Debug.Log($"[{player.name}] 切换至重型火力（榴弹模式）");
    }

    public void OnSkillEnd(PlayerController player)
    {
        if (!IsActive) return;

        IsActive = false;

        // 播放结束技能动画
        player.animator.SetBool("skill3Shooting", false);
        player.animator.SetTrigger("skill3end");

        // 禁用榴弹模式，恢复原始状态
        DisableGrenadeMode();

        // 显示技能非激活状态
        if (player.skillCooldownManager != null)
        {
            player.skillCooldownManager.SetSkillActiveVisual(false);
        }

        Debug.Log($"[{player.name}] 退出重型火力模式，恢复普通射击");
    }

    public void Cancel(PlayerController player)
    {
        // 对于持续型技能，Cancel通常与End相同
        OnSkillEnd(player);
    }

    public void Toggle(PlayerController player)
    {
        // ✅ 增加防御性检查
        if (player == null)
        {
            Debug.LogError("⚠️ Toggle时player为空！");
            return;
        }

        // 检查冷却是否完成
        if (!IsActive && player.equippedSkillCooldownTimer < Cooldown)
        {
            Debug.Log($"重型火力处于冷却中 ({(Cooldown - player.equippedSkillCooldownTimer):F1}s)");
            return;
        }

        // 如果是激活状态，则关闭；否则开启
        if (IsActive)
        {
            OnSkillEnd(player);
            // 只有从激活到关闭时才启动冷却
            player.equippedSkillCooldownTimer = 0f;
            if (player.skillCooldownManager != null)
            {
                player.skillCooldownManager.StartCooldownNow(this);
            }
        }
        else if (player.energy >= NeedEnergy)
        {
            // 消耗能量
            player.energy -= NeedEnergy;
            OnSkillStart(player);
        }
        else
        {
            Debug.Log($"能量不足！需要 {NeedEnergy} 点能量，当前 {player.energy} 点");
        }
    }

    /// <summary>
    /// 预创建榴弹预制体
    /// </summary>
    private void PrepareGrenadePrefab()
    {
        // ✅ 增加空引用检查
        if (grenadeBulletPrefab != null)
        {
            Debug.Log("榴弹预制体已存在，跳过创建");
            return;
        }

        try
        {
            // 创建榴弹预制体
            grenadeBulletPrefab = new GameObject("GrenadeBullet_Prefab");

            // 添加榴弹组件
            GrenadeBullet grenadeBullet = grenadeBulletPrefab.AddComponent<GrenadeBullet>();
            grenadeBullet.damage = 30f;
            grenadeBullet.speed = 15f;
            grenadeBullet.maxRange = 10f;
            grenadeBullet.explosionRadius = 2f;
            grenadeBullet.arcHeight = 0.5f;
            grenadeBullet.arcAngle = 45f;
            grenadeBullet.bounceFactor = 0.3f;
            grenadeBullet.bounceDelay = 0.8f;
            grenadeBullet.maxBounces = 1;

            // 添加渲染器
            SpriteRenderer spriteRenderer = grenadeBulletPrefab.AddComponent<SpriteRenderer>();
            if (rifleman != null && rifleman.symbol != null && rifleman.symbol.bullet != null)
            {
                SpriteRenderer bulletRenderer = rifleman.symbol.bullet.GetComponent<SpriteRenderer>();
                if (bulletRenderer != null && bulletRenderer.sprite != null)
                {
                    spriteRenderer.sprite = bulletRenderer.sprite;
                    Debug.Log("从rifleman.symbol.bullet获取到有效的sprite");
                }
                else
                {
                    spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Grenade") ??
                                          Resources.Load<Sprite>("Sprites/DefaultBullet");
                    Debug.Log("使用Resources加载sprite: " + (spriteRenderer.sprite != null ? "成功" : "失败"));
                }
            }
            else
            {
                spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Grenade") ??
                                      Resources.Load<Sprite>("Sprites/DefaultBullet");
                Debug.Log("使用Resources加载sprite: " + (spriteRenderer.sprite != null ? "成功" : "失败"));
            }

            spriteRenderer.color = new Color(1f, 0.5f, 0f, 1f); // 橙色

            // 不要在场景切换时销毁
            GameObject.DontDestroyOnLoad(grenadeBulletPrefab);

            // 隐藏预制体
            grenadeBulletPrefab.SetActive(false);

            Debug.Log("榴弹预制体创建成功");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"创建榴弹预制体时出错: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// 保存原始状态以便还原
    /// </summary>
    private void SaveOriginalState()
    {
        if (rifleman != null && rifleman.symbol != null)
        {
            originalBulletPrefab = rifleman.symbol.bullet;
            originalAttackEnergyCost = rifleman.attribute.attackEnergyCost;

            // 保存武器颜色
            if (rifleman.weapon != null)
            {
                SpriteRenderer weaponRenderer = rifleman.weapon.GetComponent<SpriteRenderer>();
                if (weaponRenderer != null)
                {
                    originalWeaponColor = weaponRenderer.color;
                }
            }

            Debug.Log("成功保存原始状态");
        }
        else
        {
            Debug.LogWarning("⚠️ 无法保存原始状态，rifleman或symbol为空");
        }
    }

    /// <summary>
    /// 启用榴弹模式
    /// </summary>
    private void EnableGrenadeMode()
    {
        if (rifleman == null)
        {
            Debug.LogWarning("⚠️ 启用榴弹模式失败：rifleman为空");
            return;
        }

        try
        {
            // 订阅攻击事件
            rifleman.OnNormalAttack += HandleGrenadeAttack;

            // 计算技能附加值
            float skillDelay = rifleman.heavyFirepowerAttackDelay;
            float skillBspd = rifleman.heavyFirepowerBulletSpeed - rifleman.attribute.bulletSpeed;
            float skillEnergy = rifleman.heavyFirepowerAttackEnergyCost - rifleman.attribute.attackEnergyCost;
            float skillRange = rifleman.heavyFirepowerAttackRangeBonus;

            // 应用到“技能附加属性”中
            rifleman.additionalAttackDelayFromSkillEffect += skillDelay;
            rifleman.additionalBulletSpeedFromSkillEffect += skillBspd;
            rifleman.additionalAttackEnergyCostFromSkillEffect += skillEnergy;
            rifleman.additionalAttackRangeFromSkillEffect += skillRange;

            // 设置榴弹模式
            rifleman.SetGrenadeMode(true);
            rifleman.TriggerStatsChanged();

            // 其他视觉、音效逻辑……
            ChangeWeaponAppearance(true);
            CreateMuzzleFlash();
            PlayToggleSound(true);

            Debug.Log("榴弹模式启用成功");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"启用榴弹模式时出错: {e.Message}\n{e.StackTrace}");
        }
    }

    private void DisableGrenadeMode()
    {
        if (rifleman == null)
        {
            Debug.LogWarning("⚠️ 禁用榴弹模式失败：rifleman为空");
            return;
        }

        try
        {
            // 取消订阅事件
            rifleman.OnNormalAttack -= HandleGrenadeAttack;

            // 移除之前加到“技能附加属性”上的加成
            rifleman.additionalAttackDelayFromSkillEffect -= rifleman.heavyFirepowerAttackDelay;
            rifleman.additionalBulletSpeedFromSkillEffect -= (rifleman.heavyFirepowerBulletSpeed - rifleman.attribute.bulletSpeed);
            rifleman.additionalAttackEnergyCostFromSkillEffect -= (rifleman.heavyFirepowerAttackEnergyCost - rifleman.attribute.attackEnergyCost);
            rifleman.additionalAttackRangeFromSkillEffect -= rifleman.heavyFirepowerAttackRangeBonus;

            // 更新状态
            rifleman.TriggerStatsChanged();
            rifleman.SetGrenadeMode(false);

            // 恢复视觉、特效
            ChangeWeaponAppearance(false);
            CleanupEffects();
            PlayToggleSound(false);

            Debug.Log("榴弹模式已禁用，恢复普通射击");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"禁用榴弹模式时出错: {e.Message}\n{e.StackTrace}");
        }
    }


    /// <summary>
    /// 修改武器外观
    /// </summary>
    private void ChangeWeaponAppearance(bool isGrenadeMode)
    {
        if (rifleman == null || rifleman.weapon == null)
        {
            Debug.LogWarning("⚠️ 无法修改武器外观：rifleman或weapon为空");
            return;
        }

        // 获取武器渲染器
        SpriteRenderer weaponRenderer = rifleman.weapon.GetComponent<SpriteRenderer>();
        if (weaponRenderer == null)
        {
            Debug.LogWarning("⚠️ 无法获取武器渲染器");
            return;
        }

        // 根据模式设置颜色
        if (isGrenadeMode)
        {
            // 使用Rifleman中定义的榴弹模式武器颜色
            Color grenadeColor = rifleman.heavyFirepowerWeaponColor;
            // 如果颜色为默认值，使用备用颜色
            if (grenadeColor == Color.clear)
                grenadeColor = new Color(1f, 0.5f, 0f, 1f);

            weaponRenderer.color = grenadeColor;

            // 添加闪烁效果
            if (weaponEffectCoroutine == null)
            {
                weaponEffectCoroutine = rifleman.StartCoroutine(WeaponBlinkEffect(weaponRenderer, grenadeColor));
            }
        }
        else
        {
            // 停止闪烁效果
            if (weaponEffectCoroutine != null)
            {
                rifleman.StopCoroutine(weaponEffectCoroutine);
                weaponEffectCoroutine = null;
            }

            // 恢复原始颜色
            weaponRenderer.color = originalWeaponColor;
        }
    }

    /// <summary>
    /// 武器闪烁效果
    /// </summary>
    private IEnumerator WeaponBlinkEffect(SpriteRenderer renderer, Color baseColor)
    {
        // 创建更明亮的闪烁颜色
        Color glowColor = new Color(
            Mathf.Min(baseColor.r + 0.3f, 1f),
            Mathf.Min(baseColor.g + 0.3f, 1f),
            Mathf.Min(baseColor.b + 0.3f, 1f),
            baseColor.a
        );

        while (IsActive && renderer != null)
        {
            // 随机间隔闪烁
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.3f));

            // 闪烁到高亮色
            renderer.color = glowColor;

            // 短暂高亮
            yield return new WaitForSeconds(0.05f);

            // 回到基础色
            renderer.color = baseColor;
        }
    }

    /// <summary>
    /// 创建枪口闪光效果
    /// </summary>
    private void CreateMuzzleFlash()
    {
        if (rifleman == null || rifleman.symbol == null || rifleman.symbol.muzzle == null)
        {
            Debug.LogWarning("⚠️ 无法创建枪口闪光：rifleman或muzzle为空");
            return;
        }

        try
        {
            // 创建闪光对象
            GameObject flashObj = new GameObject("HeavyFireMuzzleFlash");
            flashObj.transform.SetParent(rifleman.symbol.muzzle);
            flashObj.transform.localPosition = Vector3.zero;
            flashObj.transform.localRotation = Quaternion.identity;

            // 添加粒子系统
            ParticleSystem ps = flashObj.AddComponent<ParticleSystem>();

            // 配置粒子系统
            var main = ps.main;
            main.startColor = new Color(1f, 0.5f, 0f, 0.8f); // 橙色
            main.startSize = 0.3f;
            main.startLifetime = 0.2f;
            main.loop = false;

            // 设置发射模块
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 10) });

            // 配置形状模块
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;

            // 保存引用
            muzzleFlashEffect = flashObj;

            Debug.Log("枪口闪光效果创建成功");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"创建枪口闪光效果时出错: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// 播放模式切换音效
    /// </summary>
    private void PlayToggleSound(bool activate)
    {
        AudioSource audioSource = rifleman?.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("⚠️ 无法播放切换音效：未找到AudioSource");
            return;
        }

        try
        {
            // 播放音效
            if (activate)
            {
                // 提高音调表示激活
                audioSource.pitch = 1.2f;
                if (audioSource.clip != null)
                {
                    audioSource.PlayOneShot(audioSource.clip, 0.5f);
                }
            }
            else
            {
                // 降低音调表示关闭
                audioSource.pitch = 0.8f;
                if (audioSource.clip != null)
                {
                    audioSource.PlayOneShot(audioSource.clip, 0.5f);
                }
            }

            // 恢复正常音调
            audioSource.pitch = 1.0f;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"播放切换音效时出错: {e.Message}");
        }
    }

    /// <summary>
    /// 处理榴弹攻击事件
    /// </summary>
    private void HandleGrenadeAttack()
    {
        if (!IsActive || rifleman == null) return;

        // 触发动画
        rifleman.animator.SetBool("skill3Shooting", true);

        // 播放枪口闪光
        PlayMuzzleFlash();

        // 短暂延迟后重置动画状态
        rifleman.StartCoroutine(ResetShootingAnimation());
    }

    /// <summary>
    /// 播放枪口闪光效果
    /// </summary>
    private void PlayMuzzleFlash()
    {
        if (muzzleFlashEffect == null)
        {
            Debug.LogWarning("⚠️ 无法播放枪口闪光：muzzleFlashEffect为空");
            return;
        }

        // 获取粒子系统
        ParticleSystem ps = muzzleFlashEffect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // 清除现有粒子并播放新的
            ps.Clear(true);
            ps.Play();
        }
    }

    /// <summary>
    /// 重置射击动画
    /// </summary>
    private IEnumerator ResetShootingAnimation()
    {
        // 短暂延迟
        yield return new WaitForSeconds(0.2f);

        // 重置动画状态
        if (rifleman != null && rifleman.animator != null)
        {
            rifleman.animator.SetBool("skill3Shooting", false);
        }
    }

    /// <summary>
    /// 清理特效
    /// </summary>
    private void CleanupEffects()
    {
        // 停止武器闪烁效果
        if (weaponEffectCoroutine != null && rifleman != null)
        {
            rifleman.StopCoroutine(weaponEffectCoroutine);
            weaponEffectCoroutine = null;
        }

        // 销毁枪口闪光
        if (muzzleFlashEffect != null)
        {
            GameObject.Destroy(muzzleFlashEffect);
            muzzleFlashEffect = null;
        }
    }

    /// <summary>
    /// 发射榴弹的具体实现
    /// </summary>
    /// <remarks>
    /// 当AutoShootGrenadeMode找不到预制体时会调用此方法
    /// </remarks>
    public void FireGrenade(Vector3 target)
    {
        if (!IsActive || rifleman == null) return;

        // 获取发射点
        Transform muzzle = rifleman.symbol.muzzle;
        if (muzzle == null && rifleman.weapon != null)
        {
            muzzle = rifleman.weapon;
        }

        if (muzzle == null)
        {
            Debug.LogWarning("⚠️ 无法发射榴弹：muzzle为空");
            return;
        }

        try
        {
            // 决定使用哪个榴弹预制体
            GameObject grenadeToUse = null;

            if (rifleman.grenadeProjectilePrefab != null)
            {
                grenadeToUse = rifleman.grenadeProjectilePrefab;
            }
            else if (grenadeBulletPrefab != null)
            {
                grenadeToUse = grenadeBulletPrefab;
            }
            else
            {
                Debug.LogError("⚠️ 没有有效的榴弹预制体可用！");
                return;
            }

            // 创建榴弹实例
            GameObject grenadeObj = GameObject.Instantiate(grenadeToUse, muzzle.position, Quaternion.identity);
            grenadeObj.SetActive(true);

            // 初始化榴弹
            GrenadeBullet grenade = grenadeObj.GetComponent<GrenadeBullet>();
            if (grenade != null)
            {
                // 使用Rifleman中定义的固定伤害值，而不是使用伤害倍率
                grenade.Initialize(
                    muzzle.position,
                    target,
                    rifleman.heavyFirepowerDamage, // 使用固定伤害值
                    rifleman.TotalBulletSpeed,
                    rifleman.heavyFirepowerExplosionRadius,
                    rifleman
                );

                // 设置抛物线参数
                grenade.arcHeight = rifleman.heavyFirepowerArcHeight;
                grenade.arcAngle = rifleman.heavyFirepowerArcAngle;

                // 设置弹跳参数
                grenade.bounceFactor = rifleman.heavyFirepowerBounceFactor;
                grenade.bounceDelay = rifleman.heavyFirepowerBounceDelay;
                grenade.maxBounces = rifleman.heavyFirepowerMaxBounces;
                grenade.groundLayerMask = LayerMask.GetMask("Ground");

                // 设置爆炸特效
                if (rifleman.grenadeExplosionEffect != null)
                {
                    grenade.explosionEffectPrefab = rifleman.grenadeExplosionEffect;
                }

                // 播放发射音效
                if (rifleman.grenadeFireSound != null)
                {
                    AudioSource audioSource = rifleman.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.PlayOneShot(rifleman.grenadeFireSound, 0.7f);
                    }
                }
            }
            else
            {
                Debug.LogError("⚠️ 榴弹预制体缺少GrenadeBullet组件！");
            }

            // 触发动画和特效
            HandleGrenadeAttack();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"发射榴弹时出错: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// 切换武器模式的检测
    /// </summary>
    public void CheckForModeSwitch()
    {
        // 如果有专用按键，可以在这里添加模式切换检测
        if (Input.GetKeyDown(KeyCode.G))
        {
            Toggle(rifleman);
        }
    }
}