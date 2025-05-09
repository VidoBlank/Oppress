using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("攻击力")]
    private float damage;

    [Header("移动速度")]
    public float movingSpeed = 1f;

    [Header("存活时间")]
    public float lifetime = 3f;

    [Header("子弹类型")]
    public BulletType bulletType = BulletType.Normal;

    [Header("击中特效")]
    public GameObject hitEffectPrefab;
    [Header("子弹特效")]
    public ParticleSystem myParticleSystem;
    private float timer;
    private GameObject movingTarget;
    private Vector2 movingDirection;

    private PlayerController player;

    private int defaultLayer; // 记录子弹的默认物理层
    private bool ignoreEnemyCollision = false; // 是否屏蔽与敌人的碰撞
    private GameObject targetEnemy; // 存储手动攻击的目标敌人
    private HashSet<GameObject> collidedObjects = new HashSet<GameObject>(); // 记录已碰撞对象

    // 新增标记，确保子弹只触发一次碰撞逻辑
    private bool isHit = false;

    private enum ShootMode
    {
        ToTarget = 1,   //朝目标射击
        ToDirection = 2 //朝指定方向射击
    }

    private ShootMode mode = ShootMode.ToTarget; //射击方式

    public void StartMovingToTarget(GameObject target, float damage, float range, float speed, PlayerController player)
    {
        movingTarget = target;
        mode = ShootMode.ToTarget;
        this.damage = damage;
        movingSpeed = speed;
        lifetime = range / speed;
        this.player = player;
        defaultLayer = gameObject.layer;

        // 判断手动攻击目标类型，设置碰撞行为
        if (target.CompareTag("Obstacle"))
        {
            ignoreEnemyCollision = true;
            gameObject.layer = LayerMask.NameToLayer("IgnoreEnemy");
        }
        else if (target.CompareTag("Enemy"))
        {
            targetEnemy = target;
            ignoreEnemyCollision = false;
            gameObject.layer = defaultLayer;
        }
        else
        {
            ignoreEnemyCollision = false;
            gameObject.layer = defaultLayer;
        }
    }

    public void StartMovingToDirection(Vector2 direction, int damage, float range, float speed, PlayerController player)
    {
        movingDirection = direction;
        mode = ShootMode.ToDirection;
        this.damage = damage;
        movingSpeed = speed;
        lifetime = range / speed;
        this.player = player;
        defaultLayer = gameObject.layer;
    }

    private bool ShouldCollideWithObstacle()
    {
        // 只有在手动攻击模式下才与 Obstacle 发生碰撞
        return player != null && player.isObstaclecanhit;
    }

    // 修改 Bomb 方法，将参数改为 Collider2D，并使用 ClosestPoint 获取接触点
    private void Bomb(Collider2D collision)
    {
        if (myParticleSystem != null)
        {
            myParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        // 播放击中特效，位置设为子弹与碰撞体最近的接触点
        if (hitEffectPrefab != null)
        {
            // 获取子弹与碰撞体边缘的最近点
            Vector3 hitPosition = collision.ClosestPoint(transform.position);
            hitPosition.z = collision.transform.position.z;
            Vector3 bulletDirection = transform.right; // 默认子弹朝右
            Quaternion hitRotation = Quaternion.LookRotation(Vector3.forward, -bulletDirection);

            var hitEffect = Instantiate(hitEffectPrefab, hitPosition, hitRotation);
            if (hitEffect.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
            {
                Destroy(hitEffect, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(hitEffect, 2f);
            }
        }

        // 处理伤害逻辑，依据碰撞对象的 Tag 进行判断
        GameObject target = collision.gameObject;

        if (target.CompareTag("Enemy") && bulletType == BulletType.Normal)
        {
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.ReduceHp(damage);
                Debug.Log($"子弹命中敌人，造成 {damage} 点伤害");

                if (player != null)
                {
                    player.TriggerNormalAttack();
                    player.TriggerBulletHitEnemy(enemy, damage);

                    // 吸血等效果
                    foreach (var effect in player.GetSlotEffects())
                    {
                        if (effect is VampireShots vampire)
                        {
                            vampire.TriggerLifesteal(player, damage);
                        }
                    }
                }
            }
        



    }
        else if (target.CompareTag("Obstacle") && bulletType == BulletType.Normal)
        {
            Obstacle obstacle = target.GetComponent<Obstacle>();
            if (obstacle != null && player != null)
            {
                obstacle.ApplyDamageFromPlayer(player);
                Debug.Log($"手动攻击子弹命中障碍物，障碍物受到 {damage} 点伤害");
            }
        }
        else if (target.CompareTag("Enemy") && bulletType == BulletType.FAE)
        {
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.ReduceHp(damage);
                Debug.Log($"云爆弹命中敌人，造成 {damage} 点伤害");
                foreach (Enemy enemyTarget in GetComponent<FAESup>().enemies)
                {
                    enemyTarget.ReduceHp(damage);
                }
            }
        }
        else if (target.CompareTag("Obstacle") && bulletType == BulletType.FAE)
        {
            Obstacle obstacle = target.GetComponent<Obstacle>();
            if (obstacle != null && player != null)
            {
                obstacle.ApplyDamageFromPlayer(player);
                Debug.Log($"手动攻击子弹命中障碍物，障碍物受到 {player.attribute.damage} 点伤害");
                foreach (Enemy enemyTarget in GetComponent<FAESup>().enemies)
                {
                    enemyTarget.ReduceHp(damage);
                }
            }
        }
        else if (target.CompareTag("Enemy") && bulletType == BulletType.Petrol)
        {
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                GetComponent<PetrolSup>().Buring(damage);
            }
            return;
        }
        else if (target.CompareTag("Obstacle") && bulletType == BulletType.Petrol)
        {
            Obstacle obstacle = target.GetComponent<Obstacle>();
            if (obstacle != null && player != null)
            {
                obstacle.ApplyDamageFromPlayer(player);
                GetComponent<PetrolSup>().Buring(damage);
            }
            return;
        }
        else if (target.CompareTag("Enemy") && bulletType == BulletType.Grenade)
        {
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.ReduceHp(damage);
                Debug.Log($"榴弹命中敌人，造成 {damage} 点伤害");
                foreach (Enemy enemyTarget in GetComponent<GrenadeSup>().enemies)
                {
                    enemyTarget.ReduceHp(damage);
                }
            }
        }
        else if (target.CompareTag("Obstacle") && bulletType == BulletType.Grenade)
        {
            Obstacle obstacle = target.GetComponent<Obstacle>();
            if (obstacle != null && player != null)
            {
                obstacle.ApplyDamageFromPlayer(player);
                Debug.Log($"手动攻击子弹命中障碍物，障碍物受到 {player.attribute.damage} 点伤害");
                foreach (Enemy enemyTarget in GetComponent<GrenadeSup>().enemies)
                {
                    enemyTarget.ReduceHp(damage);
                }
            }
        }

        // 销毁子弹
        Destroy(gameObject);
    }

    private void Start()
    {
        timer = 0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 如果子弹已经命中过一次，则不再处理后续碰撞
        if (isHit)
            return;

        // 如果已经碰撞过这个对象，则直接返回
        if (collidedObjects.Contains(collision.gameObject))
            return;

        collidedObjects.Add(collision.gameObject);

        if (mode == ShootMode.ToTarget)
        {
            if (ignoreEnemyCollision && collision.CompareTag("Enemy"))
                return;

            if (targetEnemy != null && collision.CompareTag("Enemy") && collision.gameObject != targetEnemy)
                return;

            if (collision.CompareTag("Enemy") || collision.CompareTag("Obstacle"))
            {
                isHit = true; // 标记已命中，防止多次触发
                Bomb(collision);
            }
        }
        else if (mode == ShootMode.ToDirection)
        {
            if (collision.CompareTag("Enemy") || collision.CompareTag("Obstacle"))
            {
                isHit = true;
                Bomb(collision);
            }
        }
    }

    public void ChangeType(BulletType targetBulletType)
    {
        bulletType = targetBulletType;
        if (targetBulletType == BulletType.FAE)
        {
            gameObject.AddComponent<FAESup>();
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 direction = Vector2.zero;
        if (mode == ShootMode.ToTarget)
        {
            if (movingTarget != null)
            {
                Vector3 targetPosition = movingTarget.transform.position;
                if (movingTarget.TryGetComponent<Collider2D>(out Collider2D collider))
                {
                    targetPosition = collider.bounds.center;
                }
                direction = (targetPosition - transform.position).normalized;
                transform.Translate(direction * movingSpeed * Time.deltaTime, Space.World);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        else if (mode == ShootMode.ToDirection)
        {
            if (movingDirection != null)
            {
                direction = movingDirection.normalized;
                transform.Translate(direction * movingSpeed * Time.deltaTime, Space.World);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
