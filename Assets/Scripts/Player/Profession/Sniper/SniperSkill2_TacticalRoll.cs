using UnityEngine;
using System.Collections;

public class SniperSkill2_TacticalRoll : IPlayerSkill
{
    public string SkillID => "sniper_skill_2";
    public string SkillName => "战术翻滚";

    public bool IsSustained => false;
    public bool IsActive => false;
    public bool IsInstantCast => true;

    private PlayerController player;
    private Sniper sniper;

    public void Init(PlayerController player)
    {
        this.player = player;
        this.sniper = player as Sniper;

        if (sniper == null)
            Debug.LogError("⚠️ SniperSkill2_TacticalRoll 初始化失败：player 不是 Sniper！");
    }

    public float NeedEnergy => sniper != null ? sniper.rollEnergyCost : 0f;
    public float Cooldown => sniper != null ? sniper.rollCooldown : 0f;

    public void Prepare(PlayerController player) { }

    public void HandleMouseInput(PlayerController player) { }

    public void OnSkillStart(PlayerController player)
    {
        this.player = player;
        this.sniper = player as Sniper;

        player.animator.SetTrigger("roll");

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y,
            Camera.main.WorldToScreenPoint(player.transform.position).z));

        float dir = (mouseWorldPos.x > player.transform.position.x) ? 1f : -1f;
        player.ChangeFaceDirection(dir);

        player.InterruptAction();

        player.StartCoroutine(RollCoroutine(dir));
    }

    private IEnumerator RollCoroutine(float dir)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        float timer = 0f;
        player.isInvincible = sniper.isRollInvincible;

        while (timer < sniper.rollDuration)
        {
            timer += Time.deltaTime;
            rb.velocity = new Vector2(dir * sniper.rollSpeed, rb.velocity.y);
            yield return null;
        }

        player.isInvincible = false;
        rb.velocity = Vector2.zero;

        player.canShoot = true;
        player.EndSkill();
    }

    public void OnSkillEnd(PlayerController player) { }

    public void Cancel(PlayerController player) { }

    public void Toggle(PlayerController player) { }
}
