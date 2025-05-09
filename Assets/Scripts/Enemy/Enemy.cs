using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class Enemy : Unit
{
    [Header("����")]
    public string enemyName;
    [Header("���б��")]
    public int orderNumber;
    [Header("����ֵ")]
    public float HP;
    [Header("�����˺�")]
    public int attackDamage;
    [Header("�������")]
    public float attackInterval;
    [Header("�ƶ��ٶ�")]
    public float movingSpeed;

    // ������Ұ
    [Header("��Ұ����")]
    public bool hasVision = false; // �Ƿ�����Ұ���
    public float visionRadius = 5f; // ��Ұ��Χ�뾶
    public float chaseDuration = 3f; // ������Ұ�����׷�ٵ�ʱ��
    private float chaseTimer = 0f; // ������Ұ��ʱ��

    private Unit lastTarget = null; // ���һ��������Ұ��Ŀ��

    // �ܻ�����
    [Header("����Ч������ʱ��")]
    public float slowDuration = 2f; // ���ٳ���ʱ�䣬Ĭ�� 2 ��
    [Header("(����)�ܻ����ٱ���")]
    public float slowFactor = 0.5f; // ���ٱ�����Ĭ�ϼ��ٵ� 50%
    public float slowTimer = 0f; // ���ټ�ʱ
    public bool isSlowed = false; // �Ƿ��ڼ���״̬
    private float _originalMovingSpeed;
    public bool _isStunSlowed = false;

    // �����ⲿ���ʵ����ԣ��Ի�ȡ������ԭʼ�ٶ�
    public float OriginalMovingSpeed
    {
        get { return _originalMovingSpeed; }
    }







    public bool isDead = false; // ����״̬���

    public Animator animator; // ����״̬��

    protected float nextAttackTime = 0f;

    public List<Unit> attackTargets = new List<Unit>(); // ������Χ��Ŀ��
    [Header("������Χ���ϰ���Ŀ��")]
    public List<Obstacle> obstacleTargets = new List<Obstacle>(); // �ϰ���Ŀ���б�

    public enum EnemyState { Idle = 1, Moving = 2, Attack = 3, }
    [SerializeField, Header("�ж�״̬")]
    public EnemyState state = EnemyState.Idle;

    protected bool attackLocked;

    // ���»����� Unit �����ṩ��_transform��_collider �ȣ�����δ������ȷ���� Unit �н���

    private void OnEnable()
    {
        PlayerController.OnPlayerKnockedDown += HandlePlayerKnockedDown;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerKnockedDown -= HandlePlayerKnockedDown;
    }

    // ��ҵ���ʱ���Ƴ�����Ŀ�겢����ѡ��Ŀ��
    private void HandlePlayerKnockedDown(PlayerController player)
    {
        Unit playerUnit = player.GetComponent<Unit>();
        if (attackTargets.Contains(playerUnit))
        {
            attackTargets.Remove(playerUnit);
#if UNITY_EDITOR
            Debug.Log($"��� {player.gameObject.name} ���أ��ӹ���Ŀ�����Ƴ�");
#endif
        }
        if (attackTargets.Count == 0 && obstacleTargets.Count == 0)
        {
            SelectTarget();
        }
    }

    /// <summary>
    /// ���Ŀ���Ƿ��ڹ�����Χ�ڣ�ִ�й���
    /// </summary>
    public virtual void AttackCheck()
    {
        bool attacking = false;
        // ������Ŀ��
        for (int i = attackTargets.Count - 1; i >= 0; i--)
        {
            Unit unit = attackTargets[i];
            PlayerController player = unit.GetComponent<PlayerController>();

            // �������ѵ��ػ��������Ƴ�
            if (player != null && (player.isKnockedDown || player.isDead))
            {
                attackTargets.RemoveAt(i);
#if UNITY_EDITOR
                Debug.Log($"�Ƴ����ػ����������: {player.gameObject.name}");
#endif
                continue;
            }

            if (unit.GetComponent<PlayerController>() && unit.CompareTag("Player"))
            {
                Attacking(unit);
                attacking = true;
            }
        }

        // �����ϰ����߼�
        foreach (Obstacle obs in obstacleTargets)
        {
            obs.TakeDamage(attackDamage, null, true);
#if UNITY_EDITOR
            Debug.Log($"�����ϰ��� {obs.gameObject.name}����� {attackDamage} ���˺�");
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
        // ����Ŀ��λ�õ�������
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
        // ����˺�
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

        // ���ѪҺЧ��������λ�ý�����ʹ�� BloodEffectSpawner ��ָ����Ŀ�� GameObject ��λ�ã�
        BloodEffectSpawner spawner = GetComponent<BloodEffectSpawner>();
        if (spawner != null)
        {
            spawner.SpawnBlood(transform.position); // �� spawner.targetGameObject ��Ϊ�գ����ʹ����λ��
        }

        DeadCheck();
        ApplySlowEffect();
    }




    private void ApplySlowEffect()
    {
        isSlowed = true;
        slowTimer = slowDuration;
#if UNITY_EDITOR
        Debug.Log($"{gameObject.name} �����٣�����ʱ�䣺{slowDuration} ��");
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

            // �ӳ� 2 ������� Enemy ������ɸ�����Ҫ�ӳٻ��������٣�
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

        // �������Ч��
        if (isSlowed)
        {
            slowTimer -= dt;
            if (slowTimer <= 0)
            {
                isSlowed = false;
#if UNITY_EDITOR
                Debug.Log($"{gameObject.name} �ļ���Ч������");
#endif
            }
        }

        if (state == EnemyState.Attack)
        {
            animator.SetBool("isMoving", false);
        }

        // ��ʱ���� SelectTarget������ȫ������Ƶ��
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

        // ���ݵ�ǰ״̬�����Ƿ��ƶ�
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

            // ���������Ŀ���λ�ü� collider offset
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
            // ���Ŀ����¥���˶��У���ʹ�� yDiff ֹͣ����
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
                // ������������������x��y��
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
        _originalMovingSpeed = movingSpeed; // ����������ԭʼ�ٶ�
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
