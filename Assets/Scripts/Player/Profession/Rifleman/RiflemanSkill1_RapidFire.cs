using UnityEngine;
using System.Collections;

public class RiflemanSkill1_RapidFire : IPlayerSkill
{
    public string SkillID => "rifleman_skill_1";
    public string SkillName => "步枪速射";
    public bool IsSustained => false;
    public bool IsActive => false;
    public bool IsInstantCast => false;

    private PlayerController player;
    private Rifleman rifleman;

    public void Init(PlayerController player)
    {
        this.player = player;
        this.rifleman = player as Rifleman;

        if (rifleman == null)
            Debug.LogError("⚠️ RiflemanSkill1_RapidFire 初始化失败：player 不是 Rifleman！");
    }

    public float NeedEnergy => rifleman != null ? rifleman.rapidFireEnergyCost : 0f;
    public float Cooldown => rifleman != null ? rifleman.rapidFireCooldown : 0f;

    private Quaternion originalWeaponRotation;

    public void Prepare(PlayerController player)
    {
        this.player = player;
        this.rifleman = player as Rifleman;
        player.animator.SetTrigger("skill1start");
        player.skillRange.SetActive(true);

        SkillRange rangeComp = player.skillRange.GetComponent<SkillRange>();
        if (rangeComp != null)
        {
            rangeComp.SetRadius(rifleman.rapidFireBulletRange);
        }

        originalWeaponRotation = rifleman.weapon.localRotation;
    }

    public void HandleMouseInput(PlayerController player)
    {
        if (Input.GetMouseButtonDown(0))
        {
            player.animator.SetTrigger("skill1end");
            player.CancelSkill();
        }

        if (Input.GetMouseButtonDown(1))
        {
            float zDepth = Camera.main.WorldToScreenPoint(player.transform.position).z;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, zDepth));

            player.skillRange.SetActive(false);
            player.StartSkill();
            player.StartCoroutine(ShootRapidFire(mouseWorldPos));
        }
    }

    private IEnumerator ShootRapidFire(Vector3 targetPos)
    {
        player.isMoving = false;
        player.animator.SetBool("isMoving", false);
        player.animator.SetBool("skill1Shooting", true);

        if (targetPos.x > player.transform.position.x)
            player.ChangeFaceDirection(1);
        else
            player.ChangeFaceDirection(-1);

        Vector2 dir = (targetPos - player.transform.position).normalized;

        for (int i = 0; i < rifleman.rapidFireBulletCount; i++)
        {
            FireBullet(dir);
            yield return new WaitForSeconds(rifleman.rapidFireAttackDelay);
        }

        player.animator.SetBool("skill1Shooting", false);
        player.animator.SetTrigger("skill1end");

        player.EndSkill();
    }

    private void FireBullet(Vector2 dir)
    {
        // 使用 targetGameObject 作为发射点
        Transform muzzle = rifleman.targetGameObject;

        if (muzzle == null)
        {
            Debug.LogWarning("❗targetGameObject（技能发射点）未设置，使用默认 muzzle 替代");
            muzzle = player.symbol.muzzle;
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (player.transform.localScale.x > 0)
            angle += 180f;

        Quaternion bulletRotation = Quaternion.AngleAxis(angle, Vector3.forward);

        GameObject bullet = GameObject.Instantiate(
            player.symbol.bullet,
            muzzle.position,
            bulletRotation
        );

        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.StartMovingToDirection(
                dir,
                player.TotalDamage,
                rifleman.rapidFireBulletRange,
                rifleman.rapidFireBulletSpeed,
                player
            );
        }

        rifleman.weapon.rotation = bulletRotation;
    }

    public void OnSkillStart(PlayerController player) { }

    public void OnSkillEnd(PlayerController player)
    {
        rifleman.weapon.localRotation = originalWeaponRotation;
    }

    public void Cancel(PlayerController player)
    {
        player.skillRange.SetActive(false);
        player.animator.SetBool("skill1Shooting", false);
        player.animator.SetTrigger("skill1end");

        rifleman.weapon.localRotation = originalWeaponRotation;
        player.EndSkill();
    }

    public void Toggle(PlayerController player) { }
}
