using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf_02 : Enemy
{
    [Header("冲刺攻击参数")]
    [Tooltip("冲刺速度")]
    public float dashSpeed = 10f;

    [Tooltip("冲刺持续时间")]
    public float dashDuration = 0.5f;

    [Tooltip("用于检测目标是否进入冲刺范围的 Collider2D（需设置 IsTrigger）")]
    public Collider2D dashCollider;

    [Header("首次攻击后修改的攻击力")]
    [Tooltip("第一次攻击后修改的攻击力")]
    public int newAttackDamage = 15;

    // 标记是否已经进行了冲刺攻击（第一次攻击）
    private bool hasDashed = false;
    // 标记当前是否处于冲刺状态
    private bool isDashing = false;
    // 记录冲刺目标（假设目标 Tag 为 "Player"）
    private Transform dashTarget;

    private Rigidbody2D rb;

    /// <summary>
    /// 在子类 Start() 中调用 base.Start()，并初始化 Rigidbody2D
    /// </summary>
    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        if(rb == null)
        {
            Debug.LogError($"{gameObject.name} 缺少 Rigidbody2D 组件！");
        }

        if(dashCollider == null)
        {
            Debug.LogError($"{gameObject.name} 缺少 dashCollider，请在 Inspector 中指定！");
        }
    }

    /// <summary>
    /// 当目标进入 dashCollider 范围时触发冲刺攻击
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 假设目标的 Tag 为 "Player"
        if (!hasDashed && !isDashing && collision.CompareTag("Player"))
        {
            dashTarget = collision.transform;
            StartCoroutine(DashAttack());
        }
    }

    /// <summary>
    /// 冲刺攻击协程：在冲刺期间，将状态设置为非 Moving（例如 Attack），冲刺结束后恢复状态为 Moving
    /// </summary>
    private IEnumerator DashAttack()
    {
        isDashing = true;
        hasDashed = true;
        animator.SetFloat("speed", 1f);
        // 在冲刺期间，将状态设置为 Attack，暂停正常移动逻辑
        state = EnemyState.Attack;
        Debug.Log($"{gameObject.name} 开始冲刺攻击！");

        // 计算冲刺方向（归一化）
        Vector2 dashDir = (dashTarget.position - transform.position).normalized;

        float timer = 0f;
        while (timer < dashDuration)
        {
            // 使用直接修改 transform.position 进行冲刺（也可用 rb.MovePosition 代替）
            transform.position += (Vector3)(dashDir * dashSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // 冲刺结束后，将状态恢复为 Moving，使基类 Update() 恢复调用 Moving() 方法
        state = EnemyState.Moving;
        isDashing = false;
        Debug.Log($"{gameObject.name} 冲刺攻击结束，恢复正常移动！");
    }

    /// <summary>
    /// 重写普通攻击方法：第一次攻击（冲刺攻击后）成功后，将攻击力修改为指定值
    /// </summary>
    public override void Attacking(Unit unit)
    {
        base.Attacking(unit);

        // 如果已经完成冲刺攻击（第一次攻击），则修改攻击力
        if (hasDashed)
        {
            attackDamage = newAttackDamage;
            Debug.Log($"{gameObject.name} 第一次攻击后，攻击力修改为 {newAttackDamage}");
            animator.SetFloat("speed", 0f);
        }
    }
}
