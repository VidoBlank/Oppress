using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using static PlayerController;
using FOW;
using BayatGames.SaveGameFree;
using static RoleUpgradeDetailUI;
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class PlayerController : Unit
{
    //参数
    #region

    [Space]
    [Header("角色基本属性相关")]
    public Attribute attribute = new Attribute();

    //角色基本属性
    [Serializable]
    public class Attribute
    {
        [Header("角色的最大生命值")]
        public float maxhealth = 100f;

        [Header("角色的最大能量值")]
        public float maxEnergy = 100f;

        [Header("倒地状态的生命值")]
        public float knockdownHealth = 50f;

        [Header("角色交互时间")]
        public float interactspeed = 3f;

        [Header("角色恢复时间")]
        public float recoveredspeed = 2.5f;

        [Header("移动速度")]
        public float movingSpeed = 5f;

        [Header("攻击范围")]
        public CircleCollider2D attackRange;

        [Header("基础攻击范围，不随插槽改变")]
        public float baseAttackRange = 5.0f;  // 你自己根据实际值填写

        [Header("交互距离")]
        public float interactionRange = 2.0f;

        [Header("攻击伤害")]
        public int damage = 5;

        [Header("攻击间隔")]
        public float attackDelay = 1;

        [Header("子弹移动速度")]
        public float bulletSpeed = 20f;

        [Header("伤害减免比例")]
        [Tooltip("1 = 正常伤害；0.8 = 只承受80%伤害；数值越小减伤越大")]
        public float damageTakenMultiplier = 1.0f;

        [Header("固定伤害减免")]
        public float flatDamageReduction = 0f;

        [Header("技能冷却缩减速率")]
        [Tooltip("1 = 正常；1.2 = 快20%（冷却更快）；0.8 = 慢20%（冷却更慢）")]
        public float skillCooldownRate = 1.0f;
        [Header("每次攻击消耗的能量（弹药量）")]
        public float attackEnergyCost = 1f;


    }
    [Header("插槽效果附加属性（由插槽动态加成的增益数值）")]
    [Tooltip("由插槽系统额外加成的最大生命值")]
    public float additionalMaxHealthFromSlotEffect = 0f;

    [Tooltip("由插槽系统额外加成的最大能量值")]
    public float additionalMaxEnergyFromSlotEffect = 0f;

    [Tooltip("由插槽系统额外加成的倒地生命值")]
    public float additionalKnockdownHealthFromSlotEffect = 0f;

    [Tooltip("由插槽系统额外加成的攻击范围")]
    public float additionalAttackRangeFromSlotEffect = 0f;

    [Tooltip("由插槽系统额外加成的交互速度")]
    public float additionalInteractSpeedFromSlotEffect = 0f;

    [Tooltip("由插槽系统额外加成的恢复速度")]
    public float additionalRecoverSpeedFromSlotEffect = 0f;

    [Tooltip("由插槽系统额外加成的移动速度")]
    public float additionalMovingSpeedFromSlotEffect = 0f;

    [Tooltip("由插槽系统额外加成的交互距离")]
    public float additionalInteractionRangeFromSlotEffect = 0f;

    [Tooltip("由插槽系统额外加成的攻击伤害")]
    public int additionalDamageFromSlotEffect = 0;

    [Tooltip("由插槽系统额外加成的攻击间隔（减少表示更快）")]
    public float additionalAttackDelayFromSlotEffect = 0f;

    [Tooltip("由插槽系统额外加成的子弹速度")]
    public float additionalBulletSpeedFromSlotEffect = 0f;

    [Tooltip("由插槽系统额外加成的伤害承受倍率（越低越抗打）")]
    public float additionalDamageTakenMultiplierFromSlotEffect = 0f;

    [Tooltip("由插槽系统额外加成的固定减伤")]
    public float additionalFlatDamageReductionFromSlotEffect = 0f;

    [Tooltip("由插槽系统额外加成的技能冷却缩减（正值表示更快）")]
    public float additionalSkillCooldownRateFromSlotEffect = 0f;

    [Tooltip("由插槽系统额外加成的每次攻击能量消耗")]
    public float additionalAttackEnergyCostFromSlotEffect = 0f;

    [Tooltip("由插槽系统额外加成的视野半径")]
    public float additionalVisionRadiusFromSlotEffect = 0f;

    [Header("插槽效果百分比属性加成（百分比形式）")]
    [Tooltip("百分比增加的最大生命值，0.2 = 增加20%")]
    public float percentMaxHealthFromSlotEffect = 0f;

    [Tooltip("百分比增加的最大能量值，0.2 = 增加20%")]
    public float percentMaxEnergyFromSlotEffect = 0f;

    [Tooltip("百分比加成的交互速度，0.2 = 加快20%（数值减少）")]
    public float percentInteractSpeedFromSlotEffect = 0f;

    [Tooltip("百分比加成的恢复速度，0.2 = 加快20%（数值减少）")]
    public float percentRecoverSpeedFromSlotEffect = 0f;

    [Tooltip("百分比加成的移动速度，0.2 = 提升20%")]
    public float percentMovingSpeedFromSlotEffect = 0f;

    [Tooltip("百分比加成的攻击速度，0.2 = 提升20%（数值减少）")]
    public float percentAttackDelayFromSlotEffect = 0f;

        
    [Header("技能效果附加属性（由技能主动触发的临时增益）")]
    public float additionalMaxHealthFromSkillEffect = 0f;
    public float additionalMaxEnergyFromSkillEffect = 0f;
    public float additionalKnockdownHealthFromSkillEffect = 0f;
    public float additionalAttackRangeFromSkillEffect = 0f;
    public float additionalInteractSpeedFromSkillEffect = 0f;
    public float additionalRecoverSpeedFromSkillEffect = 0f;
    public float additionalMovingSpeedFromSkillEffect = 0f;
    public float additionalInteractionRangeFromSkillEffect = 0f;
    public int additionalDamageFromSkillEffect = 0;
    public float additionalAttackDelayFromSkillEffect = 0f;
    public float additionalBulletSpeedFromSkillEffect = 0f;
    public float additionalDamageTakenMultiplierFromSkillEffect = 0f;
    public float additionalFlatDamageReductionFromSkillEffect = 0f;
    public float additionalSkillCooldownRateFromSkillEffect = 0f;
    public float additionalAttackEnergyCostFromSkillEffect = 0f;
    public float additionalVisionRadiusFromSkillEffect = 0f;

    /// <summary>
    /// 对外暴露的最终属性（基础属性 + 插槽加成 + 技能附加）
    /// </summary>
    public float TotalMaxHealth =>
        (attribute.maxhealth
         + additionalMaxHealthFromSlotEffect
         + additionalMaxHealthFromSkillEffect)
        * (1 + percentMaxHealthFromSlotEffect);

    public float TotalMaxEnergy =>
        (attribute.maxEnergy
         + additionalMaxEnergyFromSlotEffect
         + additionalMaxEnergyFromSkillEffect)
        * (1 + percentMaxEnergyFromSlotEffect);

    public float TotalKnockdownHealth =>
        attribute.knockdownHealth
        + additionalKnockdownHealthFromSlotEffect
        + additionalKnockdownHealthFromSkillEffect;

    public float TotalAttackRange =>
        attribute.baseAttackRange
        + additionalAttackRangeFromSlotEffect
        + additionalAttackRangeFromSkillEffect;

    public float TotalInteractSpeed =>
        (attribute.interactspeed
         + additionalInteractSpeedFromSlotEffect
         + additionalInteractSpeedFromSkillEffect)
        * (1 - percentInteractSpeedFromSlotEffect);

    public float TotalRecoverSpeed =>
        (attribute.recoveredspeed
         + additionalRecoverSpeedFromSlotEffect
         + additionalRecoverSpeedFromSkillEffect)
        * (1 - percentRecoverSpeedFromSlotEffect);

    public float TotalMovingSpeed =>
        (attribute.movingSpeed
         + additionalMovingSpeedFromSlotEffect
         + additionalMovingSpeedFromSkillEffect)
        * (1 + percentMovingSpeedFromSlotEffect);

    public float TotalInteractionRange =>
        attribute.interactionRange
        + additionalInteractionRangeFromSlotEffect
        + additionalInteractionRangeFromSkillEffect;

    public int TotalDamage =>
        attribute.damage
        + additionalDamageFromSlotEffect
        + additionalDamageFromSkillEffect;

    public float TotalAttackDelay =>
        (attribute.attackDelay
         + additionalAttackDelayFromSlotEffect
         + additionalAttackDelayFromSkillEffect)
        * (1 + percentAttackDelayFromSlotEffect);

    public float TotalBulletSpeed =>
        attribute.bulletSpeed
        + additionalBulletSpeedFromSlotEffect
        + additionalBulletSpeedFromSkillEffect;

    public float TotalDamageTakenMultiplier =>
        attribute.damageTakenMultiplier
        + additionalDamageTakenMultiplierFromSlotEffect
        + additionalDamageTakenMultiplierFromSkillEffect;

    public float TotalFlatDamageReduction =>
        attribute.flatDamageReduction
        + additionalFlatDamageReductionFromSlotEffect
        + additionalFlatDamageReductionFromSkillEffect;

    public float TotalSkillCooldownRate =>
        attribute.skillCooldownRate
        + additionalSkillCooldownRateFromSlotEffect
        + additionalSkillCooldownRateFromSkillEffect;

    public float TotalAttackEnergyCost =>
        attribute.attackEnergyCost
        + additionalAttackEnergyCostFromSlotEffect
        + additionalAttackEnergyCostFromSkillEffect;

    public float TotalVisionRadius =>
        visionRadius
        + additionalVisionRadiusFromSlotEffect
        + additionalVisionRadiusFromSkillEffect;



    [Space]
    [Header("角色视野相关")]

    [Tooltip("角色的视野半径，决定 Light2D 的 size 和 Fog Of War Revealer3D 的 viewRadius")]
    public float visionRadius = 5f;

    [Tooltip("真实视野，受SoftenDistance和RevealHiderInFadeOutZonePercentage影响，仅供查看")]
    [Header("真实视野（只读）")]
    [SerializeField] private float actualVisionRadius;

    [Space]
    [Header("视野组件")]
    public GameObject vision; // 角色视野对象
    public CircleCollider2D visionCollider;  // 视野检测碰撞体
    public FunkyCode.Light2D playerLight2D;  // 角色的光照效果
    public FogOfWarRevealer3D fogRevealer3D; // Fog of War 视野管理器


    [Space]
    [Header("技能基础参数相关")]
    public Skill skill = new Skill();

    //技能
    [Serializable]
    public class Skill
    {
        [Header("技能冷却时长")]
        public List<float> skillCDs = new List<float>();

        [Header("技能耗能")]
        public List<float> skillEnergy = new List<float>();

        [Header("技能范围的半径")]
        public List<float> skillRadius = new List<float>();
        


    }


    // 已解锁的技能列表（不同等级解锁）
    [SerializeField]
    public List<IPlayerSkill> unlockedSkills = new List<IPlayerSkill>();

    // 战斗中装备的技能（已有）
    [SerializeField]
    public IPlayerSkill equippedSkill;



    public float equippedSkillCooldownTimer = 100f;  // 当前装备技能的冷却计时器
    [Space]
    [Header("角色标识相关")]
    public Symbol symbol = new Symbol();
    [Header("技能冷却管理器")]
    public SkillCooldownManager skillCooldownManager;
    // 存储所有天赋状态（每场战斗刷新）
    private Dictionary<SlotEffect, SlotEffectState> slotEffectStates = new Dictionary<SlotEffect, SlotEffectState>();

    //角色标识
    [Serializable]
    public class Symbol
    {
        [Header("角色名称")]
        public string Name;
        [Header("角色ID")]
        public int ID;  
        public string unitID;//唯一标识符
        public string unitName;
        [Header("角色头像")]
        public Sprite playerImage; // 用于存储玩家的头像图像

        [Header("子弹预制体")]
        public GameObject bullet;

        [Header("枪口位置")]
        public Transform muzzle;

    }



    //以下为角色状态
    #region
    [Header("角色状况相关")]

    [Header("角色的当前生命值")]
    public float health = 100f;

    [Header("角色的能量值，用于技能消耗")]
    public float energy = 50f;

    [Header("是否在准备技能")]
    public bool isPreparingSkill = false;

    [Header("是否在使用技能")]
    public bool isUsingSkill = false;

    [Header("是否移动")]
    public bool isMoving = false;

    [Header("是否倒地")]
    public bool isKnockedDown = false;

    [Header("是否死亡")]
    public bool isDead = false;

    [Header("是否正在交互"),]
    public bool isInteracting = false;

    [Header("是否正在被救援")]
    public bool isStanding;//是否正在被救援

    [Header("是否可以射击") ]
    public bool canShoot = true; // 控制是否允许射击

    [Header("是否开启手动攻击模式")]
    public bool isManualAttacking = false; // 是否开启手动攻击模式

    [Header("是否能攻击到障碍物")]
    public bool isObstaclecanhit = false; // 是否能攻击到Obstacle

    [Header("是否携带物品"), HideInInspector]
    public bool isCarrying = false; // 是否正在携带物品

    [Header("检测角色是否处于关卡中")]
    public bool isInBattle = false;

    [Header("是否无敌")]
    public bool isInvincible = false;
    #endregion


    //以下为角色的目标对象相关
    #region
    [Header("进入范围的敌人")]
    [SerializeField] public List<GameObject> enemys = new List<GameObject>();

    [Header("目标位置"), HideInInspector]
    protected Vector3 targetPosition = new Vector3(0f, 0f, 0f);

    [Header("行动目标"), ]
    public GameObject actionTarget = null;

    [SerializeField, HideInInspector]
    [Header("救援目标")]
    public GameObject targetOfRescue;

    [SerializeField, HideInInspector]
    [Header("当前交互的目标")]
    public GameObject currentInteractionTarget;

    [SerializeField, HideInInspector]
    [Header("当前救援的目标")]
    public GameObject rescueTarget; // 表示当前正在救援的目标

    [SerializeField]
    [Header("当前手动攻击的目标")]
    public GameObject manualTarget; // 当前手动选择的攻击目标

    [Header("当前携带的物品"), HideInInspector]
    public Interactable currentCarriedItem;
    #endregion


    //以下为杂项
    #region
    protected int skillIndex = 0;   //当前使用的技能序号

    protected float shootTimer; // 用于控制射击间隔的计时器

    public float[] skillTimers = new float[3]; // 用于控制技能冷却的计时器

    [Header("技能范围特效"), HideInInspector]
    public GameObject skillRange;

    [Header("动画器"), HideInInspector]
    public Animator animator;

    protected Collider2D collider;  //碰撞盒

    private StatMonitor statMonitor;
    public bool hasVersatileTalent = false;

    public event System.Action<PlayerController> OnPlayerDeath;
    public static event Action<PlayerController> OnPlayerKnockedDown; // 定义玩家倒地事件

    public static event Action<PlayerController> OnPlayerAdded;
    public static event Action<PlayerController> OnPlayerRemoved;
    public ManualAttackVisualizer manualAttackVisualizer;

    [Header("等级与插槽相关")]
    [Range(0, 3)]
    [Header("角色当前等级 (0-3)")]
    public int level = 0;
    private int previousLevel = -1; // 初始化为-1，确保第一次调用时肯定不等

    [Header("插槽列表")]
    [Tooltip("角色的插槽列表，根据等级动态调整数量")]
    public List<Slot> slots = new List<Slot>();


    private const string SaveFilePath = "PlayerData.json"; // 数据保存路径

    //角色行动状态（这个暂时没用到）
    //public enum ActionState
    //{
    //    Idle,   //站立
    //    Move,    //移动
    //    Attack,    //攻击
    //    Interact,   //交互
    //    Fall,   //倒地
    //    Die,    //死亡
    //    PrepareSkill,   //准备技能
    //    UsingSkill  //使用技能
    //};

    //public ActionState state = ActionState.Idle;
    #endregion

    #endregion


    //unity事件函数
    #region
    public event Action<PlayerController> OnPlayerHit;
    public event Action OnStatsChanged;
    public event Action<PlayerController, PlayerController> OnPlayerRescueSuccess;
    public event System.Action<Enemy, float> OnBulletHitEnemy;
    public event Action OnNormalAttack;
    public event Action<int> OnSkillUsed;
    public static event Action<PlayerController> OnPlayerRecovered;
    public event System.Action<IPlayerSkill> OnSkillEquipped;
    public event Action OnNameChanged;

    private string _nameOverride;
    public string NameOverride
    {
        get => _nameOverride;
        set
        {
            if (_nameOverride != value)
            {
                _nameOverride = value;
                symbol.Name = value;
                OnNameChanged?.Invoke();
            }
        }
    }

    public void TriggerNameChanged()
    {
        OnNameChanged?.Invoke();
    }


    public void NotifyStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }

    protected virtual void OnEnable()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (collider == null) collider = GetComponent<CapsuleCollider2D>();
        if (skillRange == null) skillRange = transform.Find("Skillrange")?.gameObject;

        OnPlayerAdded?.Invoke(this);

        // 根据等级初始化插槽

    }

    private void OnDisable()
    {
        OnPlayerRemoved?.Invoke(this);
    }

    protected virtual void Awake()
    {
        base.Awake();
     
        OnPlayerAdded?.Invoke(this); // 在玩家被创建时通知UI管理器
        animator = GetComponent<Animator>();
        collider = GetComponent<CapsuleCollider2D>();
        skillRange = transform.Find("Skillrange").gameObject;
        if (symbol.unitID == null)
        {
            symbol.unitID = System.Guid.NewGuid().ToString();
        }

        if (symbol.unitName == null)
        {
            symbol.unitName = "DefaultName"; // 默认名字（可以换成更适合的值）
        }


    }

    private void OnDestroy()
    {
        OnPlayerRemoved?.Invoke(this); // 在玩家被销毁时通知UI管理器

    }

    protected virtual void Start()
    {

        PlayerController.OnPlayerAdded?.Invoke(this);
        RefreshUnlockedSkills();
        
        // 没有已装备技能的话，自动装备第一个
        if (equippedSkill == null && unlockedSkills.Count > 0)
        {
            EquipSkill(unlockedSkills[0]);
        }
        TriggerStatsChanged();
        statMonitor = new StatMonitor(this);

        Debug.Log($"手动触发 OnPlayerAdded: {name}");
        // 启动计时器
        shootTimer = 10f;
        for (int i = 0; i < skillTimers.Length; i++)
        {
            skillTimers[i] = 100f;
        }
        OnPlayerAdded?.Invoke(this); // 在Start而非Awake中调用，避免初始化时重复添加
        if (attribute.attackRange == null)
        {
            Transform attackRangeTransform = transform.Find("AttackRange");
            if (attackRangeTransform != null)
            {
                attribute.attackRange = attackRangeTransform.GetComponent<CircleCollider2D>();
            }
        }
        // 获取 ManualAttackVisualizer 的引用
        manualAttackVisualizer = GetComponent<ManualAttackVisualizer>();
        if (manualAttackVisualizer == null)
        {
            Debug.LogError("未找到 ManualAttackVisualizer，请确保它与 PlayerController 在同一对象上。");
        }

        if (level >= 1 && equippedSkill == null && unlockedSkills.Count > 0)
        {
            EquipSkill(unlockedSkills[0]);
        }

        ValidateSlots();

    }




    public void TriggerStatsChanged()
    {
        OnStatsChanged?.Invoke();

        // 限制当前生命值与能量不超过最大值
        health = Mathf.Min(health, TotalMaxHealth);
        energy = Mathf.Min(energy, TotalMaxEnergy);

        if (attribute.attackRange != null)
        {
            attribute.attackRange.radius = TotalAttackRange;
            
        }
    }


    protected virtual void Update()
    {
        foreach (var effect in GetSlotEffects())
        {
            effect.Tick(this);
        }


        statMonitor?.CheckForChanges();
        UpdateVisionParameters();
        DrawView();
        if (!isKnockedDown && !isDead && health <= 0)
        {
            EnterKnockdownState();
        }
        if (stairMoving)
        {
            InterruptAction();
        }
        if (!isKnockedDown && !isDead && !isInteracting)
        {
            Acting();
            Moving();
            AutoShoot();
        }

        //更新攻击计时器和技能计时器
        shootTimer += Time.deltaTime;
        if (equippedSkill != null && equippedSkillCooldownTimer < equippedSkill.Cooldown)
        {
            equippedSkillCooldownTimer += Time.deltaTime * TotalSkillCooldownRate;
        }


        MouseCheck();
        HandleManualAttackInput();
        HandleTargetChange();
       
    }
    #endregion


    //角色行动相关

    //通用行动相关
    #region

    /// <summary>
    /// 对目标物体进行行动
    /// </summary>
    /// <param name="targetObject">目标物体</param>
    public void ActToTarget(GameObject targetObject)
    {
        //如果正在使用技能，或携带物品，则无法行动
        if (!isUsingSkill && !isPreparingSkill && !isCarrying)
        {
            actionTarget = targetObject;
            times = 10;
        }
    }

    int times = 10;

    private float outOfRangeTimer = 0f;
    private const float outOfRangeThreshold = 0f;      // 在范围外持续多长时间后才触发移动
    private const float minRepathDistance = 1f;         // 至少离开多少距离才重新走过去


    /// <summary>
    /// 行动逻辑
    /// </summary>
    public void Acting()
    {
        if (actionTarget == null) return;

        // 兼容挂在子物体上的 3D 碰撞体：向上查找 2D 碰撞体或使用主物体坐标
        Collider2D col2D = actionTarget.GetComponentInChildren<Collider2D>();
        Collider col3D = actionTarget.GetComponentInChildren<Collider>(); // 兼容 BoxCollider、CapsuleCollider 等

        Vector3 targetCenter = col2D != null ? col2D.bounds.center :
                               col3D != null ? col3D.bounds.center :
                               actionTarget.transform.position;


        // 交互物体
        if (actionTarget.CompareTag("Object"))
        {
            if (Vector2.Distance(transform.position, targetCenter) >= attribute.interactionRange)
            {
                if (times == 10)
                {
                    MoveToTarget(targetCenter);
                    times = 0;
                }
                times++;
            }
            else
            {
                StopMovement();
                if (!isInteracting)
                {
                    isInteracting = true;
                    InteractWithItem(actionTarget);
                    actionTarget = null;
                }
            }
        }
        // 救援目标
        else if (actionTarget.CompareTag("Player"))
        {
            PlayerController targetPlayer = actionTarget.GetComponent<PlayerController>();
            if (targetPlayer && (targetPlayer.isDead || targetPlayer.isStanding))
            {
                InterruptAction();
                return;
            }

            if (Vector2.Distance(transform.position, targetCenter) >= attribute.interactionRange)
            {
                if (times == 10)
                {
                    MoveToTarget(targetCenter);
                    times = 0;
                }
                times++;
            }
            else
            {
                StopMovement();
                if (!isInteracting)
                {
                    isInteracting = true;
                    RescueTarget(actionTarget);
                    actionTarget = null;
                }
            }
        }
        // 攻击敌人或障碍物
        else if (actionTarget.CompareTag("Enemy") || actionTarget.CompareTag("Obstacle"))
        {
            float effectiveAttackRange = attribute.attackRange.radius * Mathf.Abs(transform.lossyScale.x);
            float distance = Vector2.Distance(transform.position, targetCenter);

            if (actionTarget.CompareTag("Enemy"))
            {
                if (actionTarget.GetComponent<Enemy>() == null)
                {
                    Debug.Log($"Enemy {actionTarget.name} 已死亡（Enemy组件已销毁），取消目标");
                    InterruptAction();
                    return;
                }

                Collider2D targetCollider = actionTarget.GetComponent<Collider2D>();
                if (targetCollider != null && IsColliderWithinAttackRange(targetCollider))
                {
                    StopMovement();
                    manualTarget = actionTarget;
                    if (!enemys.Contains(manualTarget))
                    {
                        enemys.Add(manualTarget);
                        Debug.Log($"Enemy {manualTarget.name} 已加入 enemys 列表");
                    }
                    animator.SetBool("isShooting", true);
                    outOfRangeTimer = 0f; // ✅ 回到范围内时重置计时
                }
                else
                {
                    // ✅ 离得足够远才计时移动
                    if (distance > effectiveAttackRange + minRepathDistance)
                    {
                        outOfRangeTimer += Time.deltaTime;

                        if (outOfRangeTimer >= outOfRangeThreshold)
                        {
                            MoveToTarget(targetCenter);
                            manualTarget = null;
                            outOfRangeTimer = 0f;
                        }
                    }
                    else
                    {
                        // 离得近但略微超出，不移动，只计时
                        outOfRangeTimer = 0f;
                    }
                }
            }
            else if (actionTarget.CompareTag("Obstacle"))
            {
                Debug.Log($"Obstacle 距离：{distance}，攻击范围：{effectiveAttackRange}");
                // ✅ 插入调试代码
                if (manualTarget != null && actionTarget != null)
                {
                    Debug.Log($"manualTarget == actionTarget ? {manualTarget == actionTarget}");
                    Debug.Log($"manualTarget ID: {manualTarget.GetInstanceID()}, actionTarget ID: {actionTarget.GetInstanceID()}");
                }
                if (distance <= effectiveAttackRange)
                {
                    StopMovement();
                    manualTarget = actionTarget;
                    if (!enemys.Contains(manualTarget))
                    {
                        
                        Debug.Log($"Obstacle {manualTarget.name} 已加入 enemys 列表");
                    }
                    animator.SetBool("isShooting", true);
                }
                else
                {
                    Vector3 direction = (targetCenter - transform.position).normalized;
                    Vector3 desiredPos = targetCenter - direction * effectiveAttackRange;
                    MoveToTarget(desiredPos);
                    RemoveManualTarget();
                    

                    animator.SetBool("isShooting", false);
                }
            }
        }
    }




    /// <summary>
    /// 打断行动
    /// </summary>
    public void InterruptAction()
    {
        
        // 如果不是楼梯运动状态，则停止移动
        if (!stairMoving)
        {
            isMoving = false;
            animator.SetBool("isMoving", false);
        }

        // 停止手动攻击
        isManualAttacking = false;
        manualTarget = null; // 清除手动攻击目标

        // 清除敌人列表中已无效的目标（可选）
        for (int i = enemys.Count - 1; i >= 0; i--)
        {
            if (enemys[i] == null)
            {
                enemys.RemoveAt(i);
            }
        }

        // 停止救援协程
        if (currentRescueCoroutine != null)
        {
            StopCoroutine(currentRescueCoroutine);
            currentRescueCoroutine = null;
        }

        // 停止交互和救援
        isInteracting = false;
        isStanding = false;
        rescueTarget = null; // 清除救援目标
        currentInteractionTarget = null; // 清除当前交互目标

        actionTarget = null;    // 清除行动目标

        // 停止动画
        animator.SetBool("isShooting", false);
        animator.ResetTrigger("isInteractingWithPlayer");
        animator.ResetTrigger("isInteractWithItem");
        animator.SetTrigger("interrupt");

        Debug.Log($"{gameObject.name} 的所有主动行为已被打断");
    }



    #endregion

    //交互相关
    #region

    /// <summary>
    /// 与物品交互
    /// </summary>
    /// <param name="item">目标物品</param>
    public void InteractWithItem(GameObject item)
{
    animator.ResetTrigger("interrupt");
    animator.SetTrigger("isInteractWithItem");
    Debug.Log("进入交互状态");
    Interactable interactable = item.GetComponent<Interactable>();
    if (interactable != null)
    {
        currentInteractionTarget = item; // 设置当前交互对象
        StartCoroutine(ExitInteractionAfterDelay(TotalInteractSpeed));
    }
}

/// <summary>
/// 退出交互状态
/// </summary>
/// <param name="delay">延时</param>
private IEnumerator ExitInteractionAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    if (isInteracting)
    {
        Interactable interactable = currentInteractionTarget.GetComponent<Interactable>();
        interactable.Interact(this);

        isInteracting = false; // 退出交互状态
        currentInteractionTarget.GetComponent<Interactable>().FinishInteraction();
        currentInteractionTarget = null; // 重置交互对象
        Debug.Log($"{gameObject.name} 完成与对象的交互");
    }
}


/// <summary>
/// 对目标进行救援
/// </summary>
/// <param name="target">目标角色</param>
public void RescueTarget(GameObject target)
{
    if (!isUsingSkill && !isPreparingSkill)
    {
        // 如果已有运行中的救援协程，停止它
        if (currentRescueCoroutine != null)
        {
            StopCoroutine(currentRescueCoroutine);
            currentRescueCoroutine = null;
        }

        animator.SetTrigger("isInteractingWithPlayer");
        rescueTarget = target; // 设置当前救援目标
        currentRescueCoroutine = StartCoroutine(ExitRescueAfterDelay(TotalRecoverSpeed));
    }
}

private Coroutine currentRescueCoroutine; // 当前运行的救援协程


/// <summary>
/// 退出救援状态
/// </summary>
/// <param name="delay">延时</param>
/// 
private IEnumerator ExitRescueAfterDelay(float delay)
{
    float elapsedTime = 0f; // 记录已进行的救援时间

    while (elapsedTime < delay)
    {
        if (!isInteracting || rescueTarget == null)
        {
            Debug.Log("救援被中断，退出协程！");
            yield break; // 中断协程
        }

        elapsedTime += Time.deltaTime;
        yield return null; // 等待下一帧
    }

    // 完成救援逻辑
    if (rescueTarget != null)
    {
        PlayerController rescuedPlayer = rescueTarget.GetComponent<PlayerController>();
        if (rescuedPlayer != null)
        {
            StartCoroutine(rescuedPlayer.Standing());
                OnPlayerRescueSuccess?.Invoke(this, rescuedPlayer);
            }
    }

    isInteracting = false; // 退出交互状态
    rescueTarget = null;   // 重置救援目标
    currentRescueCoroutine = null; // 清除协程引用

    Debug.Log($"{gameObject.name} 完成救援目标");
}


/// <summary>
/// 起身
/// </summary>
/// <returns></returns>
public IEnumerator Standing()
{
    isStanding = true;
    animator.SetTrigger("isRecovered");
    yield return new WaitForSeconds(0.5f);

    if (!isDead)
    {
        isKnockedDown = false;
        health = 20f;
            OnPlayerRecovered?.Invoke(this);
        }
    
    isStanding = false;
}

    public void RecoverFromKnockdown()
    {
        if (isDead) return;

        isKnockedDown = false;
        health = 20f;
        OnPlayerRecovered?.Invoke(this);
    }








    #endregion

    //移动相关
    #region

    /// <summary>
    /// 移动到目标点
    /// </summary>
    /// <param name="targetPosition">目标点位置</param>
    public void MoveToTarget(Vector3 targetPosition)
{
    //如果正在使用技能，则无法移动
    if (!isUsingSkill && !isPreparingSkill)
    {
        isStanding = false;
        this.targetPosition = targetPosition;
        //设置目标点
        SetTargetPosition(targetPosition);
        isMoving = true;
        animator.SetBool("isMoving", true);
        //Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Player"), true);
        //Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);
        IgnoreUnitsCollision(true);    //忽略碰撞
    }
}

/*/角色移动
public void Moving()
{
    if(isMoving)
    {
        //后面需要修改为寻路的功能
        Vector2 targetDirection;
        if (targetPosition.x > transform.position.x)
        {
            targetDirection = new Vector2(1.0f, 0f);
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else
        {
            targetDirection = new Vector2(-1.0f, 0f);
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        transform.Translate(targetDirection * movingSpeed * Time.deltaTime);
        //Debug.Log((targetPosition - transform.position).magnitude);
        if ((targetPosition - transform.position).magnitude < positionError)
        {
            isMoving = false;
            animator.SetBool("isMoving", false);

            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Player"), false);
        }
    }
}*/

/// <summary>
/// 人物移动
/// </summary>
public void Moving()
{
    //不在移动状态，则无法执行移动行为
    if (!isMoving)
    {
        return;
    }
    if (pathFindingTarget != null
        && Mathf.Abs((pathFindingTarget.transform.position.x + pathFindingTarget.GetComponent<Collider2D>().offset.x) - (transform.position.x + GetComponent<Collider2D>().offset.x)) < minOffsetDistance
        && Mathf.Abs((pathFindingTarget.transform.position.y + pathFindingTarget.GetComponent<Collider2D>().offset.y) - (transform.position.y + GetComponent<Collider2D>().offset.y)) < 1f)
    {
        StopMovement();
        IgnoreUnitsCollision(false);    //恢复碰撞
                                        //按逻辑来说不用加下面这行代码，但会出问题，不知道怎么回事
                                        //Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Player"), false);
    }
    else if (pathFindingTarget != null)
    {
        Vector2 targetDirection;
        if (unitDirection == Direction.Right)
        {
            targetDirection = new Vector2(1.0f, 0f);
            transform.localScale = new Vector3(1f, 1f, 1f);
            transform.Translate(targetDirection * TotalMovingSpeed * Time.deltaTime);
        }
        else if (unitDirection == Direction.Left)
        {
            targetDirection = new Vector2(-1.0f, 0f);
            transform.localScale = new Vector3(-1f, 1f, 1f);
            transform.Translate(targetDirection * TotalMovingSpeed * Time.deltaTime);
        }
    }
    else
    {
        StopMovement();
        IgnoreUnitsCollision(false);    //恢复碰撞
    }
}


/// <summary>
/// 停止移动
/// </summary>
private void StopMovement()
{
    PlayerManager.instance.UnselectTargetPosition(this);    //取消选中的目标位置
    UnsetTargetPosition();
    isMoving = false;
    animator.SetBool("isMoving", false);
}


/// <summary>
/// 忽略与其他角色和敌人间的碰撞
/// </summary>
/// <param name="ignore">是否忽略</param>
private void IgnoreUnitsCollision(bool ignore)
{
    if (ignore)
    {
        collider.excludeLayers = (1 << 7) | (1 << 9);   //忽略角色和敌人间的碰撞
        collider.layerOverridePriority = 1;
    }
    else
    {
        collider.excludeLayers = 0;   //恢复角色和敌人间的碰撞
        collider.layerOverridePriority = 0;
    }
}


// 碰撞检测处理
private void OnCollisionEnter2D(Collision2D collision)
{

    if (collision.gameObject.CompareTag("Obstacle"))
    {
        Debug.Log($"{gameObject.name} 撞到了 {collision.gameObject.name}，停止移动");
        StopMovement(); // 停止移动
    }
    GetPlatform(collision);
}

private void OnCollisionExit2D(Collision2D collision)
{
    ResetPlatform(collision);
}

    #endregion

    //攻击相关
    #region

    /// <summary>
    /// 自动射击
    /// </summary>
    protected virtual void AutoShoot()
    {
        // 检查是否允许射击
        if (!canShoot || energy <= 0 || isInteracting)
        {
            animator.SetBool("isShooting", false);
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

        // ✅ 非移动状态且冷却完成才能开火
        if (!isMoving && shootTimer > TotalAttackDelay)
        {
            if (target != null)
            {
                animator.SetBool("isShooting", true);

                // 调整朝向
                ChangeFaceDirection(target.transform.position.x - transform.position.x);

                // 发射子弹
                GameObject bulletInstance = Instantiate(symbol.bullet, symbol.muzzle.position, transform.rotation);
                bulletInstance.GetComponent<Bullet>().StartMovingToTarget(
                    target, TotalDamage, attribute.attackRange.radius + 0.5f, TotalBulletSpeed, this);

                // 扣能量
                energy -= TotalAttackEnergyCost;
                energy = Mathf.Max(energy, 0);

                shootTimer = 0f;
            }
            else
            {
                // ✅ 如果有手动目标，即使没开火，也保持动画激活
                if (manualTarget != null)
                {
                    animator.SetBool("isShooting", true);
                }
                else
                {
                    animator.SetBool("isShooting", false);
                }
            }
        }
        else
        {
            // ✅ 射击冷却中也保持动画状态，只在没有目标或能量时才关闭
            if (manualTarget != null)
            {
                animator.SetBool("isShooting", true);
            }
            else if (enemys.Count <= 0 || isMoving || energy <= 0)
            {
                animator.SetBool("isShooting", false);
            }
        }
    }




    //手动射击
    private void HandleManualAttackInput()
{
    // 检查是否进入了这些特殊状态，如果进入，移除目标并退出手动攻击模式
    if (isDead || isKnockedDown || isCarrying || isInteracting || isMoving || isUsingSkill)
    {
        if (manualTarget != null)
        {
            this.isObstaclecanhit = false;

            Debug.Log($"玩家进入特殊状态，取消手动攻击目标: {manualTarget.name}");
            manualTarget = null;

        }
        isManualAttacking = false;
        return;
    }

        if (PlayerInput.instance == null || PlayerInput.instance.selectedPlayers == null || !PlayerInput.instance.selectedPlayers.Contains(this))
        {
            return;
        }


        if (Input.GetKeyDown(KeyCode.A))
    {
        if (attribute.attackRange == null)
        {
            Debug.LogError("AttackRange 碰撞体未找到，请检查子物体设置。");
            return;
        }

        isManualAttacking = true;
        Debug.Log("进入手动攻击模式");
    }

    if (isManualAttacking && (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.C)))
    {
        isManualAttacking = false;
        Debug.Log("触发其他状态，退出手动攻击模式");
    }

    if (isManualAttacking && Input.GetMouseButtonDown(0))
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity,
            (1 << LayerMask.NameToLayer("Enemy")) | (1 << LayerMask.NameToLayer("Obstacle")));
        Debug.DrawRay(worldPos, Vector3.forward * 10, Color.red, 5f);

        if (hit.collider != null)
        {
            float distanceToRangeCenter = Vector2.Distance(hit.point, attribute.attackRange.transform.position);

            if (distanceToRangeCenter <= attribute.attackRange.radius)
            {
                bool foundTarget = false;

                if (hit.collider.CompareTag("Obstacle"))
                {
                    manualTarget = hit.collider.gameObject;

                    if (!enemys.Contains(manualTarget))
                    {
                        enemys.Add(manualTarget);
                        Debug.Log($"手动选择了 Obstacle 作为攻击目标: {manualTarget.name}");
                    }

                    foundTarget = true;
                    isObstaclecanhit = true;
                }
                else if (hit.collider.CompareTag("Enemy"))
                {
                    manualTarget = hit.collider.gameObject;

                    if (!enemys.Contains(manualTarget))
                    {
                        enemys.Add(manualTarget);
                        Debug.Log($"手动选择了 Enemy 作为攻击目标: {manualTarget.name}");
                    }

                    foundTarget = true;
                    isObstaclecanhit = false;
                }

                if (foundTarget)
                {
                    Debug.Log("开始攻击目标");
                    isManualAttacking = false;


                }
            }
            else
            {
                Debug.Log("点击位置不在攻击范围内");
                isManualAttacking = false;

                // 退出模式时也重置高亮
                if (manualAttackVisualizer != null)
                {
                    manualAttackVisualizer.ResetLastHighlight();
                }
            }
        }
        else
        {
            Debug.Log("点击了空白区域，没有选择有效目标");
            isManualAttacking = false;

            // 退出模式时重置高亮
            if (manualAttackVisualizer != null)
            {
                manualAttackVisualizer.ResetLastHighlight();
            }
        }
    }



}

    // 判断目标的碰撞体是否仍在攻击范围内
    private bool IsColliderWithinAttackRange(Collider2D targetCollider)
    {
        if (targetCollider == null) return false;

        float distance = Vector2.Distance(transform.position, targetCollider.bounds.center);
        return distance <= TotalAttackRange + 0.5f;
    }




    /// <summary>
    /// 处理目标的变更情况（基于距离判断）
    /// </summary>
    public void HandleTargetChange()
    {
        for (int i = enemys.Count - 1; i >= 0; i--)
        {
            GameObject target = enemys[i];

            // 判空处理
            if (target == null)
            {
                enemys.RemoveAt(i);
                continue;
            }

            // ✅ 计算与目标的实际距离
            float distance = Vector2.Distance(transform.position, target.transform.position);

            // ✅ 引入缓冲范围（容差）避免敌人一进一出
            float buffer = 0.5f;
            if (distance > TotalAttackRange + buffer)
            {
                Debug.Log($"移除超出攻击范围的目标：{target.name}，距离：{distance:F3}，攻击范围：{TotalAttackRange}");

                enemys.RemoveAt(i);

                if (manualTarget == target)
                {
                    manualTarget = null;
                    isObstaclecanhit = false;
                    Debug.Log($"手动目标 {target.name} 被移除，重置手动攻击状态");
                }
            }
        }
    }







    /// <summary>
    /// 将障碍物从敌人列表移除
    /// </summary>
    /// <param name="obstacle">障碍物</param>
    private void RemoveObstacleFromEnemyList(GameObject obstacle)
    {
        if (obstacle == null) return;

        Debug.Log($"🚨 尝试移除 Obstacle: {obstacle.name}, id: {obstacle.GetInstanceID()}");

        for (int i = enemys.Count - 1; i >= 0; i--)
        {
            GameObject e = enemys[i];

            if (e == null)
            {
                Debug.Log($"⚠️ index {i} 是 null，移除");
                enemys.RemoveAt(i);
                continue;
            }

            Debug.Log($"检查列表中第 {i} 个元素：{e.name}, id: {e.GetInstanceID()}, 相等: {e == obstacle}");

            if (e == obstacle)
            {
                Debug.Log($"✅ 找到匹配 Obstacle，移除成功");
                enemys.RemoveAt(i);
            }
        }

        if (enemys.Contains(obstacle))
        {
            Debug.Log($"❌ 尝试移除失败：Obstacle 仍然在 enemys 中");
        }
        else
        {
            Debug.Log($"✅ 最终确认：Obstacle 已从 enemys 中移除");
        }
    }


    public void RemoveManualTarget()
    {
        if (manualTarget != null)
        {
            if (manualTarget.CompareTag("Obstacle"))
            {
                RemoveObstacleFromEnemyList(manualTarget);  // 用已有方法，自动处理 enemys 和 manualTarget
            }
            else
            {
                enemys.Remove(manualTarget);   // enemy 的话 只移除 enemys
                Debug.Log($"移除 manualTarget {manualTarget.name} 并退出手动攻击");
                manualTarget = null;
            }

            isObstaclecanhit = false;
        }
    }


    #endregion

    //技能相关
    #region

    /// <summary>
    /// 每次升级后刷新已解锁技能列表（由子类重写）
    /// </summary>
    public virtual void RefreshUnlockedSkills()
    {
        unlockedSkills.Clear();
    }

    public void EquipSkill(IPlayerSkill skill)
    {
        if (skill == null)
        {
            equippedSkill = null;
            OnSkillEquipped?.Invoke(null);
            return;
        }

        if (unlockedSkills.Contains(skill))
        {
            equippedSkill = skill;
            OnSkillEquipped?.Invoke(skill);
            Debug.Log($"[{symbol.Name}] 已装备技能：{skill.SkillName}");
        }

        var display = GetComponentInChildren<SkillButtonDisplay>();
        if (display != null)
        {
            // ✅ 直接调用新版 Init，内部从 SkillGroupManager 获取
            display.Init(this);
        }
    }






    /// <summary>
    /// 使用技能
    /// </summary>
    /// <param name="index">技能序号</param>
    public virtual void UseSkill()
    {
        if (isPreparingSkill || isUsingSkill) return;
        if (stairMoving) return;

        if (equippedSkill == null)
        {
            Debug.LogWarning($"[{name}] 未装备技能！");
            return;
        }

        // 持续技能逻辑
        if (equippedSkill.IsSustained)
        {
            equippedSkill.Toggle(this);
            return;
        }

        if (equippedSkillCooldownTimer < equippedSkill.Cooldown)
        {
            Debug.Log($"{name} 技能冷却中");
            return;
        }

        if (energy < equippedSkill.NeedEnergy)
        {
            Debug.Log($"{name} 能量不足！");
            return;
        }

        if (equippedSkill.IsInstantCast) // 瞬发技能
        {
            StartSkill();
        }
        else // 普通技能：进入准备状态
        {
            isPreparingSkill = true;
            canShoot = false;
            equippedSkill.Prepare(this);
        }
    }







    /// <summary>
    /// 开始技能释放，同时触发冷却
    /// </summary>
    public void StartSkill()
    {
        if (equippedSkill == null) return;

        isPreparingSkill = false;
        isUsingSkill = true;
        equippedSkillCooldownTimer = 0f;

        energy -= equippedSkill.NeedEnergy;

        if (skillCooldownManager != null)
            skillCooldownManager.StartCooldownNow(equippedSkill);


        equippedSkill.OnSkillStart(this);
    }



    /// <summary>
    /// 结束技能释放
    /// </summary>
    public virtual void EndSkill()
    {
        isUsingSkill = false;
        isPreparingSkill = false;
        canShoot = true;

        // 安全调用
        equippedSkill?.OnSkillEnd(this);
    }





    /// <summary>
    /// 取消技能
    /// </summary>
    public void CancelSkill()
    {
        if (equippedSkill == null) return;

        isPreparingSkill = false;
        canShoot = true;

        equippedSkill.Cancel(this);
    }



    public override void MouseCheck()
    {
        if (isPreparingSkill && equippedSkill != null)
        {
            equippedSkill.HandleMouseInput(this);
        }
    }


    /// <summary>
    /// 对于技能释放操作的检测
    /// </summary>

    #endregion

    //角色状态相关
    #region

    /// <summary>
    /// 角色受伤检测
    /// </summary>
    /// <param name="damageAmount">伤害值</param>
    public void TakeDamage(float damageAmount, bool isNaturalDamage = false)
    {
        // 如果玩家死亡，直接返回
        if (isDead) return;
        if (isInvincible)
        {
            Debug.Log($"{gameObject.name} 当前处于无敌状态，免疫伤害");
            return;
        }
        if (stairMoving)
        {
            Debug.Log($"{gameObject.name} 正在楼梯移动，无法受伤");
            return;
        }
        // 如果处于倒地状态且不是自然掉血的伤害，不进行伤害
        if (isKnockedDown && !isNaturalDamage)
        {
            Debug.Log($"{gameObject.name} 在倒地状态下，不能受到普通伤害");
            return;
        }

        //如果有盾牌则扣除盾牌血量
        Shield shield = GetComponentInChildren<Shield>();
        if (shield)
        {
            shield.GetDamage(damageAmount);
            return;
        }

        // 触发受伤事件
        OnPlayerHit?.Invoke(this);

        // 如果有血效果生成器
        BloodEffectSpawner spawner = GetComponent<BloodEffectSpawner>();
        if (spawner != null)
        {
            spawner.SpawnBlood(transform.position);
        }

     
        // 1) 先做固定减伤
        float finalDamage = damageAmount - TotalFlatDamageReduction;
        if (finalDamage < 0f)
            finalDamage = 0f;

        // 2) 再乘百分比减伤
        finalDamage *= TotalDamageTakenMultiplier;

        // 折算后取 0 不要出现负伤害
        if (finalDamage < 0f)
            finalDamage = 0f;

        // 进行扣血
        health -= finalDamage;

        // 播放受击动画
        animator.SetTrigger("gethit");

        // 如果血量 <=0 则进入倒地
        if (health <= 0)
        {
            EnterKnockdownState();
        }

        // Debug日志
        Debug.Log($"{gameObject.name} 受到伤害: 原伤害={damageAmount:F2}, " +
                  $"固定减伤={attribute.flatDamageReduction:F2}, " +
                  $"百分比减伤后最终伤害={finalDamage:F2}, 余下血量={health:F2}/{attribute.maxhealth:F2}");
    }



    /// <summary>
    /// 进入倒地状态
    /// </summary>
    public void EnterKnockdownState()
{
    PlayerManager.instance.UnselectPlayer(this);    //角色倒地则取消选择
    isKnockedDown = true;
    animator.SetTrigger("isKnocked");
    isMoving = false;
    animator.SetBool("isShooting", false);
        if (equippedSkill != null && equippedSkill.IsSustained && equippedSkill.IsActive)
        {
            equippedSkill.Toggle(this); // 自动结束持续技能
        }
        Debug.Log($"{gameObject.name} 进入倒地状态");

    OnPlayerKnockedDown?.Invoke(this); // 触发倒地事件，通知怪物移除玩家

    StartCoroutine(KnockdownHealthDrain());

    //Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);
    IgnoreUnitsCollision(true);    //忽略碰撞
    Debug.Log($"{gameObject.name} 倒地！");
}

/// <summary>
/// 减少倒地血量
/// </summary>
public IEnumerator KnockdownHealthDrain()
{
    while (isKnockedDown && !isDead)
    {
        // 如果不是在救援状态，才减少倒地血量
        if (!isStanding)
        {
            UpdateKnockdownHealth(1f); // 
        }
        yield return new WaitForSeconds(1f); // 每秒减少1点倒地血量
    }
}

/// <summary>
/// 更新倒地血量
/// </summary>
/// <param name="amount"></param>
public void UpdateKnockdownHealth(float amount)
{
    attribute.knockdownHealth -= amount;

    if (attribute.knockdownHealth <= 0)
    {
        Die();
    }
}

/// <summary>
/// 角色死亡
/// </summary>
public void Die()
{
    isDead = true;
    animator.SetTrigger("isDead");
    GetComponent<Collider2D>().enabled = false;
    GetComponent<Rigidbody2D>().isKinematic = true;
    GetComponent<Rigidbody2D>().simulated = false;
    vision.SetActive(false);
    // 触发玩家死亡事件
    OnPlayerDeath?.Invoke(this);
}

/// <summary>
/// 能量值恢复
/// </summary>
/// <param name="amount">恢复值</param>
public void RestoreEnergy(float amount)
{

    energy = Mathf.Min(energy + amount, TotalMaxEnergy);
    Debug.Log($"{gameObject.name} 恢复了 {amount} 点能量，当前能量: {energy}");
}

/// <summary>
/// 恢复生命值
/// </summary>
/// <param name="amount">恢复值</param>
public void RecoverHealth(float amount)
{
    // 确保恢复后不会超过最大生命值
    float previousHealth = health;
    health = Mathf.Min(TotalMaxHealth, health + amount);

    // 输出日志信息
    Debug.Log($"{name} 恢复了 {amount:F1} 点生命值，当前生命值：{health:F1}/{TotalMaxHealth:F1}");
}
    


/// <summary>
/// 更新视野的方法
/// </summary>
private void UpdateVisionParameters()
{
    if (playerLight2D != null)
    {
        playerLight2D.size = TotalVisionRadius;
    }

    if (fogRevealer3D != null)
    {
        fogRevealer3D.ViewRadius = TotalVisionRadius;

    }

    // 如果 visionCollider 没有被设置，或设置不正确，则继续初始化
    if (visionCollider == null)
    {
        visionCollider = vision?.GetComponent<CircleCollider2D>();
    }

    // 设置 visionCollider 的范围
    if (visionCollider != null)
    {
            visionCollider.radius = 1 + (fogRevealer3D.SoftenDistance + TotalVisionRadius) * fogRevealer3D.RevealHiderInFadeOutZonePercentage;


        }
    if (fogRevealer3D != null)
    {
            actualVisionRadius = 1 + (fogRevealer3D.SoftenDistance + TotalVisionRadius) * fogRevealer3D.RevealHiderInFadeOutZonePercentage;
        }

}

    public List<SlotEffect> GetSlotEffects()
    {
        List<SlotEffect> effects = new List<SlotEffect>();
        foreach (var slot in slots)
        {
            if (slot != null && slot.slotEffect != null)
            {
                effects.Add(slot.slotEffect);
            }
        }
        return effects;
    }


    // 获取某个天赋的状态（自动创建）
    public SlotEffectState GetSlotEffectState(SlotEffect effect)
    {
        if (!slotEffectStates.ContainsKey(effect))
        {
            slotEffectStates[effect] = new SlotEffectState();
        }
        return slotEffectStates[effect];
    }

    // 开始一场新战斗时清空状态
    public void ResetSlotEffectStates()
    {
        foreach (var state in slotEffectStates.Values)
        {
            state.ResetBattleState();
        }
    }

    public void TriggerNormalAttack()
    {
        OnNormalAttack?.Invoke();
    }



    public void TriggerBulletHitEnemy(Enemy enemy, float damage)
    {
        OnBulletHitEnemy?.Invoke(enemy, damage);
    }




    public void ChangeFaceDirection(float dir)
    {
        // 正右1，正左-1
        if (dir > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }





    #endregion

    //角色升级相关
    #region

    /// <summary>
    /// 根据角色等级检测插槽数量，并应用已有装备的效果
    /// </summary>
    [ContextMenu("Validate Slots")]
    // 确保插槽数量不超过等级限制
    public void ValidateSlots()
    {
        if (slots == null)
            slots = new List<Slot>();

        // 当角色拥有多面手天赋时，插槽上限直接设置为5，否则根据等级限制
        int maxSlotCount = hasVersatileTalent ? 5 : level switch
        {
            1 => 1,
            2 => 2,
            3 => 3,
            _ => 0, // 等级为 0 时无插槽
        };

        if (slots.Count == 0)
        {
            Debug.LogWarning("没有插槽，跳过插槽效果应用");
            return; // 如果没有插槽，则跳过后续的处理逻辑
        }

        // 移除多余的插槽
        while (slots.Count > maxSlotCount)
        {
            var slotToRemove = slots[slots.Count - 1];
            // 只移除 SlotEffect，因为不再有 equippedGameObject
            slotToRemove.RemoveEffect(this);
            slots.RemoveAt(slots.Count - 1);
        }

        foreach (var slot in slots)
        {
            if (slot.slotEffect != null)
            {
                var state = GetSlotEffectState(slot.slotEffect);
                if (!state.isApplied)
                {
                    slot.slotEffect.ApplyEffect(this);
                }
            }
        }


        Debug.Log($"等级 {level} 校验后插槽数量：{slots.Count}");
    }





#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying) return; // 非运行时直接返回
        TriggerStatsChanged();
        ValidateSlots();
    }
#endif

    public void EquipSlot(Slot slot)
{
    if (slots.Contains(slot))
    {
        Debug.Log($"⚠️ 该插槽 {slot.slotName} 已经装备，无法重复装备！");
        return;
    }

    // 限制插槽数量
    if (slots.Count >= level)
    {
        Debug.Log($"⚠️ 插槽已满 ({slots.Count}/{level})，无法装备更多！");
        return;
    }

    // 添加插槽并应用效果
    slots.Add(slot);
    slot.ApplyEffect(this);

    Debug.Log($"✅ 装备插槽 {slot.slotName}，已应用效果！");
}

public void UnequipSlot(Slot slot)
{
    if (slots.Contains(slot))
    {
        slot.RemoveEffect(this);
        slots.Remove(slot);
        Debug.Log($"❌ 移除插槽 {slot.slotName}，效果已移除！");
    }
}


    /// <summary>
    /// 动态调整等级并校验插槽
    /// </summary>
    public event System.Action<int> OnLevelChanged;

   
    public virtual void SetLevel(int newLevel)
    {
        if (newLevel == previousLevel)
            return;

        previousLevel = newLevel;

        level = Mathf.Clamp(newLevel, 0, 3); // 限制等级范围

        if (level == 0)
        {
            RookieLevelSystem rookieLevelSystem = GetComponent<RookieLevelSystem>();
            if (rookieLevelSystem != null)
            {
                rookieLevelSystem.SetRookieAttributes(this);
            }
            else
            {
                Debug.LogError("未找到 RookieLevelSystem，无法设置 0 级属性！");
            }
        }
        else
        {
            CharacterLevelSystem levelSystem = GetComponent<CharacterLevelSystem>();
            if (levelSystem != null)
            {
                levelSystem.SetAttributesByLevel(level);
                health = attribute.maxhealth;
                energy = attribute.maxEnergy;
                Debug.Log($"🎉 {name} 升级至 {level}，生命值已恢复至 {health}/{attribute.maxhealth}，能量恢复至 {energy}/{attribute.maxEnergy}");
            }
            else
            {
                Debug.LogError($"❌ {name} 没有绑定 CharacterLevelSystem 组件，无法设置等级 {level}！");
            }
        }

        OnStatsChanged?.Invoke();           // 原有的属性变更通知
        OnLevelChanged?.Invoke(level);      // ✅ 新增：通知 UI 等订阅者等级变更
        RefreshUnlockedSkills(); // ✅ 新增
        if (equippedSkill == null && unlockedSkills.Count > 0)
        {
            EquipSkill(unlockedSkills[0]); // 自动装备第一个技能
        }

    }
}










#endregion
public interface IPlayerSkill
{
    string SkillName { get; }
    float NeedEnergy { get; }
    float Cooldown { get; }
    string SkillID { get; }
    void Prepare(PlayerController player);            // 可空
    void HandleMouseInput(PlayerController player);   // 可空
    void OnSkillStart(PlayerController player);       // 释放时调用
    void OnSkillEnd(PlayerController player);         // 状态终止调用
    void Cancel(PlayerController player);             // 准备状态取消调用

    // ✅ 新增：
    bool IsSustained { get; }                         // 是否为持续技能

    bool IsInstantCast { get; }                       // 是否瞬发技能（不需要 Prepare 和 HandleMouseInput）

    bool IsActive { get; }                            // 当前是否激活
    void Toggle(PlayerController player);             // 切换状态：激活 → 结束，结束 → 激活
}






















