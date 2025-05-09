using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RiflemanSkill2_StunGrenade : IPlayerSkill
{
    public string SkillID => "rifleman_skill_2";
    public string SkillName => "震撼弹";
    public bool IsSustained => false;
    public bool IsActive => false;
    public bool IsInstantCast => false; // 需要瞄准，非即时施放

    private PlayerController player;
    private Rifleman rifleman;
    private Vector3 targetPosition;
    private bool isAiming;

    // 确保组件已初始化的标志
    private static bool skillRangeInitialized = false;

    // ✅ 强绑定注入方法：调用 Init 时注入 player 与 rifleman
    public void Init(PlayerController player)
    {
        this.player = player;
        this.rifleman = player as Rifleman;

        if (rifleman == null)
        {
            Debug.LogWarning("⚠️ RiflemanSkill2_StunGrenade 初始化失败：player 不是 Rifleman！");
            // 不 return！继续初始化最基本的逻辑
        }

        // 只有在类型转换成功后才初始化 skillRange
        if (!skillRangeInitialized && player.skillRange != null)
        {
            bool wasActive = player.skillRange.activeSelf;
            player.skillRange.SetActive(true);

            SkillRange rangeComp = player.skillRange.GetComponent<SkillRange>();
            if (rangeComp != null)
            {
                // 预先初始化抛物线功能
                rangeComp.EnableParabolicPath(false); // 保持禁用状态，只是初始化组件
                Debug.Log("预初始化SkillRange抛物线组件完成");
            }

            if (!wasActive)
                player.skillRange.SetActive(false);

            skillRangeInitialized = true;
            Debug.Log("预初始化SkillRange组件完成");
        }
    }


    public float NeedEnergy => rifleman != null ? rifleman.stunGrenadeEnergyCost : 0f;
    public float Cooldown => rifleman != null ? rifleman.stunGrenadeCooldown : 0f;

    public void Prepare(PlayerController player)
    {
        this.player = player;
        this.rifleman = player as Rifleman;

        if (player == null || player.skillRange == null)
        {
            Debug.LogError("震撼弹技能: Prepare阶段失败，skillRange为空");
            return;
        }

        isAiming = true;
        player.animator.ResetTrigger("skill2end");
        player.animator.SetTrigger("skill2start"); // 可以添加对应的动画触发器

        // 先激活技能范围
        player.skillRange.SetActive(true);

        // 延迟设置半径和抛物线，让SkillRange有机会完成初始化
        player.StartCoroutine(DelayedPrepare());
    }

    // 延迟设置，让SkillRange组件有时间初始化
    private IEnumerator DelayedPrepare()
    {
        yield return null; // 等待一帧

        if (player != null && player.skillRange != null && player.skillRange.activeSelf)
        {
            SkillRange rangeComp = player.skillRange.GetComponent<SkillRange>();
            if (rangeComp != null && rifleman != null)
            {
                try
                {
                    // 设置技能范围半径
                    rangeComp.SetRadius(rifleman.stunGrenadeRange);

                    // 启用抛物线路径预览，并设置抛物线高度参数
                    rangeComp.EnableParabolicPath(true, rifleman.stunGrenadeArc);

                    // 设置抛物线起点为武器位置
                    if (rifleman.weapon != null)
                    {
                        rangeComp.SetPathOrigin(rifleman.weapon);

                        // 为确保立即更新抛物线，设置一个初始终点位置（玩家前方）
                        Vector3 defaultEndPoint = player.transform.position + new Vector3(
                            player.transform.localScale.x > 0 ? rifleman.stunGrenadeRange : -rifleman.stunGrenadeRange,
                            0,
                            0);
                        rangeComp.UpdatePathEndPoint(defaultEndPoint);
                    }

                    // 强制刷新抛物线
                    rangeComp.ForceUpdatePath();

                    Debug.Log($"震撼弹技能: 准备阶段完成，设置半径: {rifleman.stunGrenadeRange}，抛物线高度: {rifleman.stunGrenadeArc}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"设置震撼弹技能范围时出错: {e.Message}");
                }
            }
            else
            {
                Debug.LogError("无法获取SkillRange组件或rifleman为空");
            }
        }
        else
        {
            Debug.LogError("player或skillRange已经为空或未激活");
        }
    }

    public void HandleMouseInput(PlayerController player)
    {
        if (!isAiming) return;

        // 左键取消（延用一技能的操作习惯）
        if (Input.GetMouseButtonDown(0))
        {
            player.animator.SetTrigger("skill2end");
            player.CancelSkill();
            return;
        }

        if (Camera.main == null) return;

        // 获取鼠标世界坐标（每帧更新，用于预览）
        float zDepth = Camera.main.WorldToScreenPoint(player.transform.position).z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, zDepth));

        // 更新目标位置（限制在范围内）
        float distanceToPlayer = Vector2.Distance(
            new Vector2(mouseWorldPos.x, mouseWorldPos.y),
            new Vector2(player.transform.position.x, player.transform.position.y));

        if (distanceToPlayer > rifleman.stunGrenadeRange)
        {
            Vector2 direction = new Vector2(
                mouseWorldPos.x - player.transform.position.x,
                mouseWorldPos.y - player.transform.position.y).normalized;

            mouseWorldPos = new Vector3(
                player.transform.position.x + direction.x * rifleman.stunGrenadeRange,
                player.transform.position.y + direction.y * rifleman.stunGrenadeRange,
                mouseWorldPos.z);
        }

        // 更新抛物线的终点位置
        SkillRange rangeComp = player.skillRange.GetComponent<SkillRange>();
        if (rangeComp != null && rangeComp.IsParabolicPathEnabled())
        {
            rangeComp.UpdatePathEndPoint(mouseWorldPos);
        }

        // 右键确认（发射震撼弹）
        if (Input.GetMouseButtonDown(1))
        {
            targetPosition = mouseWorldPos;

            // 隐藏技能范围指示器
            player.skillRange.SetActive(false);

            // 开始技能
            player.StartSkill();

            // 投掷震撼弹
            player.StartCoroutine(ThrowStunGrenadeCoroutine());
        }
    }

    public void OnSkillStart(PlayerController player)
    {
        // 这个方法会在StartSkill时被调用，但我们的主要逻辑在HandleMouseInput中
        this.player = player;
        this.rifleman = player as Rifleman;
    }

    private IEnumerator ThrowStunGrenadeCoroutine()
    {
        // 保存原始移动状态（仅用于调试）
        bool wasMoving = player.isMoving;
        if (wasMoving)
        {
            Debug.Log("玩家在移动中释放技能，将会在技能结束后中断移动");
        }

        // 确保角色朝向目标方向
        if (targetPosition.x > player.transform.position.x)
            player.ChangeFaceDirection(1);
        else
            player.ChangeFaceDirection(-1);

        // 禁止移动
        player.isMoving = false;
        player.animator.SetBool("isMoving", false);

        // 播放投掷动画
        player.animator.SetTrigger("skill2Shooting");

        // 等待一小段时间，确保角色有足够时间转向和开始动画
        yield return new WaitForSeconds(0.1f);

        // 实例化震撼弹预制体
        if (rifleman.stunGrenadePrefab != null)
        {
            // 确定发射点
            Transform muzzle = null;
            if (rifleman.targetGameObject != null)
            {
                muzzle = rifleman.targetGameObject;
            }
            else if (rifleman.weapon != null)
            {
                muzzle = rifleman.weapon;
            }
            else
            {
                muzzle = player.symbol.muzzle;
            }

            GameObject grenadeObj = GameObject.Instantiate(
                rifleman.stunGrenadePrefab,
                muzzle.position,
                Quaternion.identity);

            StunGrenade grenade = grenadeObj.GetComponent<StunGrenade>();
            if (grenade == null)
            {
                grenade = grenadeObj.AddComponent<StunGrenade>();
            }

            // 设置震撼弹参数
            grenade.Initialize(
                muzzle.position,
                targetPosition,
                rifleman.stunGrenadeSpeed,
                rifleman.stunGrenadeArc,
                rifleman.stunGrenadeDamage,
                rifleman.stunGrenadeRadius,
                rifleman.stunGrenadeSlowFactor,
                rifleman.stunGrenadeSlowDuration,
                player);

            // 设置爆炸特效预制体
            grenade.explosionEffectPrefab = rifleman.stunGrenadeExplosionEffect;
        }
        else
        {
            Debug.LogError("未设置震撼弹预制体！");
        }

        // 等待投掷动画完成
        yield return new WaitForSeconds(0.3f);

        // 结束技能
        player.EndSkill();

        // 完全中断当前的移动指令，保持投掷后的朝向和位置
        player.InterruptAction();

        Debug.Log("震撼弹技能释放完成，已中断所有行为");
    }

    public void OnSkillEnd(PlayerController player)
    {
        isAiming = false;

        // 关闭技能范围指示器及抛物线预览
        if (player.skillRange != null && player.skillRange.activeSelf)
        {
            SkillRange rangeComp = player.skillRange.GetComponent<SkillRange>();
            if (rangeComp != null)
            {
                rangeComp.EnableParabolicPath(false);
            }
            player.skillRange.SetActive(false);
        }

        player.animator.SetTrigger("skill2end"); // 结束动画
    }

    public void Cancel(PlayerController player)
    {
        isAiming = false;

        // 关闭技能范围指示器及抛物线预览
        if (player.skillRange != null && player.skillRange.activeSelf)
        {
            SkillRange rangeComp = player.skillRange.GetComponent<SkillRange>();
            if (rangeComp != null)
            {
                rangeComp.EnableParabolicPath(false);
            }
            player.skillRange.SetActive(false);
        }

        player.animator.SetTrigger("skill2end"); // 结束动画
    }

    public void Toggle(PlayerController player) { }
}