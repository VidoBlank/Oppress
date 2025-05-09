using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class Enemy : Unit
{
    [Header("名称")]
    public string enemyName;
    [Header("序列编号")]
    public int orderNumber;
    [Header("生命值")]
    public float HP;
    [Header("攻击伤害")]
    public int attackDamage;
    [Header("攻击间隔")]
    public float attackInterval;
    [Header("移动速度")]
    public float movingSpeed;

    // 怪物视野
    [Header("视野设置")]
    public bool hasVision = false; // 是否开启视野检测
    public float visionRadius = 5f; // 视野范围半径
    public float chaseDuration = 3f; // 脱离视野后继续追踪的时间
    private float chaseTimer = 0f; // 脱离视野计时器

    private Unit lastTarget = null; // 最后一个脱离视野的目标

    // 受击减速
    [Header("减速效果持续时间")]
    public float slowDuration = 2f; // 减速持续时间，默认 2 秒
    [Header("(韧性)受击减速比例")]
    public float slowFactor = 0.5f; // 减速比例，默认减速到 50%
    public float slowTimer = 0f; // 减速计时
    public bool isSlowed = false; // 是否处于减速状态
    private float _originalMovingSpeed;
    public bool _isStunSlowed = false;

    // 用于外部访问的属性，以获取真正的原始速度
    public float OriginalMovingSpeed
    {
        get { return _originalMovingSpeed; }
    }







    public bool isDead = false; // 死亡状态检测

    public Animator animator; // 动画状态机

    protected float nextAttackTime = 0f;

    public List<Unit> attackTargets = new List<Unit>(); // 攻击范围内目标
    [Header("攻击范围内障碍物目标")]
    public List<Obstacle> obstacleTargets = new List<Obstacle>(); // 障碍物目标列表

    public enum EnemyState { Idle = 1, Moving = 2, Attack = 3, }
    [SerializeField, Header("行动状态")]
    public EnemyState state = EnemyState.Idle;

    protected bool attackLocked;

    // 以下缓存由 Unit 基类提供（_transform、_collider 等），若未缓存请确保在 Unit 中进行

    private void OnEnable()
    {
        PlayerController.OnPlayerKnockedDown += HandlePlayerKnockedDown;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerKnockedDown -= HandlePlayerKnockedDown;
    }

    // 玩家倒地时，移除攻击目标并重新选择目标
    private void HandlePlayerKnockedDown(PlayerController player)
    {
        Unit playerUnit = player.GetComponent<Unit>();
        if (attackTargets.Contains(playerUnit))
        {
            attackTargets.Remove(playerUnit);
#if UNITY_EDITOR
            Debug.Log($"玩家 {player.gameObject.name} 倒地，从攻击目标中移除");
#endif
        }
        if (attackTargets.Count == 0 && obstacleTargets.Count == 0)
        {
            SelectTarget();
        }
    }

    /// <summary>
    /// 检查目标是否在攻击范围内，执行攻击
    /// </summary>
    public virtual void AttackCheck()
    {
        bool attacking = false;
        // 检查玩家目标
        for (int i = attackTargets.Count - 1; i >= 0; i--)
        {
            Unit unit = attackTargets[i];
            PlayerController player = unit.GetComponent<PlayerController>();

            // 如果玩家已倒地或死亡则移除
            if (player != null && (player.isKnockedDown || player.isDead))
            {
                attackTargets.RemoveAt(i);
#if UNITY_EDITOR
                Debug.Log($"移除倒地或死亡的玩家: {player.gameObject.name}");
#endif
                continue;
            }

            if (unit.GetComponent<PlayerController>() && unit.CompareTag("Player"))
            {
                Attacking(unit);
                attacking = true;
            }
        }

        // 攻击障碍物逻辑
        foreach (Obstacle obs in obstacleTargets)
        {
            obs.TakeDamage(attackDamage, null, true);
#if UNITY_EDITOR
            Debug.Log($"攻击障碍物 {obs.gameObject.name}，造成 {attackDamage} 点伤害");
#endif
            animator.SetTrigger("isAttacking");
            state = EnemyState.Attack;
            attacking = true;
        }

        nextAttackTime = Time.time + attackInterval;
        if (!attacking)
            FinishAttack();
    }

    public virtual void Attacking(Unit unit)
    {
        // 根据目标位置调整朝向
        if (unit.transform.position.x < transform.position.x)
        {
            ChangeDirecition(Direction.Left);
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else
        {
            ChangeDirecition(Direction.Right);
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        // 造成伤害
        unit.GetComponent<PlayerController>().TakeDamage(attackDamage);
        animator.SetTrigger("isAttacking");
        state = EnemyState.Attack;
        attackLocked = true;
    }

    public void FinishAttack() { attackLocked = false; }

    public virtual void ReduceHp(float num)
    {
        HP -= num;
        animator.SetTrigger("isAffected");

        // 添加血液效果（生成位置将优先使用 BloodEffectSpawner 中指定的目标 GameObject 的位置）
        BloodEffectSpawner spawner = GetComponent<BloodEffectSpawner>();
        if (spawner != null)
        {
            spawner.SpawnBlood(transform.position); // 若 spawner.targetGameObject 不为空，则会使用其位置
        }

        DeadCheck();
        ApplySlowEffect();
    }




    private void ApplySlowEffect()
    {
        isSlowed = true;
        slowTimer = slowDuration;
#if UNITY_EDITOR
        Debug.Log($"{gameObject.name} 被减速，持续时间：{slowDuration} 秒");
#endif
    }

    private void DeadCheck()
    {
        if (HP <= 0 && !isDead)
        {
            isDead = true;
            animator.SetTrigger("isDead");
            DisableAllFunctions();
            foreach (var player in FindObjectsOfType<PlayerController>())
            {
                player.enemys.Remove(gameObject);
                if (player.manualTarget == gameObject)
                {
                    player.manualTarget = null;
                    player.isObstaclecanhit = false;
                }
            }
            SinkingEffect sinkingEffect = GetComponent<SinkingEffect>();
            if (sinkingEffect != null)
                sinkingEffect.StartSinkingEffect();

            // 延迟 2 秒后销毁 Enemy 组件（可根据需要延迟或立即销毁）
            Destroy(this, 0f);
        }
    }


    private void DisableAllFunctions()
    {
        this.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.simulated = false;
        }
    }

    private float targetSelectInterval = 0.3f;
    private float targetSelectTimer = 0f;

    public void Update()
    {
        float dt = Time.deltaTime;

        // 处理减速效果
        if (isSlowed)
        {
            slowTimer -= dt;
            if (slowTimer <= 0)
            {
                isSlowed = false;
#if UNITY_EDITOR
                Debug.Log($"{gameObject.name} 的减速效果结束");
#endif
            }
        }

        if (state == EnemyState.Attack)
        {
            animator.SetBool("isMoving", false);
        }

        // 定时调用 SelectTarget，降低全局搜索频率
        targetSelectTimer += dt;
        if (!attackLocked && targetSelectTimer >= targetSelectInterval)
        {
            SelectTarget();
            targetSelectTimer = 0f;
        }

        int numAttackTargets = attackTargets.Count;
        int numObstacleTargets = obstacleTargets.Count;
        bool hasPathTarget = (pathFindingTarget != null);

        if (numAttackTargets == 0 && numObstacleTargets == 0 && !hasPathTarget)
        {
            if (lastTarget != null)
            {
                chaseTimer += dt;
                if (chaseTimer < chaseDuration)
                {
                    pathFindingTarget = lastTarget.GetComponent<PathFindTarget>();
                    state = EnemyState.Moving;
                }
                else
                {
                    lastTarget = null;
                    chaseTimer = 0f;
                    state = EnemyState.Idle;
                    animator.SetBool("isMoving", false);
                }
            }
            else
            {
                state = EnemyState.Idle;
            }
        }
        else if (Time.time > nextAttackTime && (numAttackTargets != 0 || numObstacleTargets != 0))
        {
            AttackCheck();
        }
        else if (numAttackTargets == 0 && numObstacleTargets == 0 && hasPathTarget && !attackLocked)
        {
            state = EnemyState.Moving;
        }

        // 根据当前状态决定是否移动
        if (state == EnemyState.Moving)
        {
            animator.SetBool("isMoving", true);
            Moving();
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }



    public void Moving()
    {
        if (pathFindingTarget != null && state == EnemyState.Moving)
        {
            Unit targetUnit = pathFindingTarget.GetComponent<Unit>();
            bool targetOnStairs = (targetUnit != null && targetUnit.stairMoving);

            // 缓存自身和目标的位置及 collider offset
            Vector2 myPos = new Vector2(transform.position.x + GetComponent<Collider2D>().offset.x,
                                          transform.position.y + GetComponent<Collider2D>().offset.y);
            Vector2 targetPos = new Vector2(pathFindingTarget.transform.position.x + pathFindingTarget.GetComponent<Collider2D>().offset.x,
                                            pathFindingTarget.transform.position.y + pathFindingTarget.GetComponent<Collider2D>().offset.y);

            float xDiff = Mathf.Abs(targetPos.x - myPos.x);
            float yDiff = Mathf.Abs(targetPos.y - myPos.y);

            if (!targetOnStairs)
            {
                if (xDiff < minOffsetDistance && yDiff < 1f)
                {
                    StopMovement();
                    Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"),
                                                   LayerMask.NameToLayer("Player"), false);
                    return;
                }
            }
            // 如果目标在楼梯运动中，则不使用 yDiff 停止条件
        }
        else if (state != EnemyState.Moving)
        {
            StopMovement();
            return;
        }

        if (state == EnemyState.Moving)
        {
            animator.SetBool("isMoving", true);
            float currentSpeed = isSlowed ? movingSpeed * slowFactor : movingSpeed;
            Vector2 moveDir;
            Unit targetUnit = pathFindingTarget.GetComponent<Unit>();
            bool targetOnStairs = (targetUnit != null && targetUnit.stairMoving);

            if (targetOnStairs)
            {
                // 计算完整方向向量（x、y）
                Vector2 diff = new Vector2(
                    (pathFindingTarget.transform.position.x + pathFindingTarget.GetComponent<Collider2D>().offset.x)
                    - (transform.position.x + GetComponent<Collider2D>().offset.x),
                    (pathFindingTarget.transform.position.y + pathFindingTarget.GetComponent<Collider2D>().offset.y)
                    - (transform.position.y + GetComponent<Collider2D>().offset.y)
                );
                moveDir = diff.normalized;
                if (diff.x >= 0)
                    transform.localScale = new Vector3(1f, 1f, 1f);
                else
                    transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            else
            {
                if (unitDirection == Direction.Right)
                {
                    moveDir = new Vector2(1f, 0f);
                    transform.localScale = new Vector3(1f, 1f, 1f);
                }
                else
                {
                    moveDir = new Vector2(-1f, 0f);
                    transform.localScale = new Vector3(-1f, 1f, 1f);
                }
            }
            transform.Translate(moveDir * currentSpeed * Time.deltaTime);
        }
        else
        {
            StopMovement();
        }
    }

    protected virtual void Start()
    {
        _originalMovingSpeed = movingSpeed; // 保存真正的原始速度
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (!GetComponent<PathFindTarget>())
            gameObject.AddComponent<PathFindTarget>();
    }

    protected void SelectTarget()
    {
        float closestDistance = float.MaxValue;
        Unit closestTarget = null;
        foreach (Unit unit in FindObjectsOfType<Unit>())
        {
            PlayerController player = unit.GetComponent<PlayerController>();
            if (player != null && (player.isKnockedDown || player.isDead))
                continue;

            if (hasVision)
            {
                float distance = Vector2.Distance(transform.position, unit.transform.position);
                if (distance > visionRadius)
                    continue;
            }

            if (unit.CompareTag("Player"))
            {
                float distance = Vector2.Distance(transform.position, unit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = unit;
                }
            }
        }

        if (closestTarget != null)
        {
            pathFindingTarget = closestTarget.GetComponent<PathFindTarget>();
            lastTarget = closestTarget;
            SetTarget(closestTarget.gameObject);
            FindingStairsPath();
        }
        else
        {
            if (hasVision)
                pathFindingTarget = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (hasVision)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, visionRadius);
        }
    }

    private void StopMovement()
    {
        UnsetTargetPosition();
        state = EnemyState.Idle;
        animator.SetBool("isMoving", false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            StopMovement();
        }
        GetPlatform(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        ResetPlatform(collision);
    }










}
