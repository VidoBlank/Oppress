using UnityEngine;

public class Sniper : PlayerController
{
    [Header("武器Transform")]
    public Transform weapon;

    [Header("枪口的目标位置")]
    public Transform targetGameObject;

    [Header("狙击技能参数")]
    public float snipeChargeTime = 1.5f;        // 蓄力时间
    public float snipeBulletSpeed = 30f;        // 子弹速度
    public float snipeBulletRange = 15f;        // 射程
    public int snipeDamage = 50;                // 伤害
    public float snipeEnergyCost = 25f;         // 能量消耗
    public float snipeCooldown = 10f;           // 冷却时间

    [Header("翻滚技能参数")]
    public float rollDistance = 2f;             
    public float rollSpeed = 8f;
    public float rollDuration = 0.35f;
    public float rollEnergyCost = 12f;
    public float rollCooldown = 6f;
    public bool isRollInvincible = false;        

    [Header("诱饵诡雷技能参数")]
    public GameObject decoyMinePrefab;          // 诱饵诡雷预制体
    public float decoyDuration = 8f;            // 持续时间
    public float decoyExplosionRadius = 3f;     // 爆炸范围
    public int decoyExplosionDamage = 40;       // 爆炸伤害
    public float decoyEnergyCost = 20f;
    public float decoyCooldown = 12f;

    protected override void Awake()
    {
        base.Awake();
        RefreshUnlockedSkills();
    }

    protected override void Start()
    {
        base.Start();
        level = Mathf.Clamp(level, 1, 3);
    }

    public override void RefreshUnlockedSkills()
    {
        unlockedSkills.Clear();

        if (level >= 1)
        {
            var skill1 = new SniperSkill1_Snipe();
            skill1.Init(this);
            unlockedSkills.Add(skill1);
        }

        if (level >= 2)
        {
            var skill2 = new SniperSkill2_TacticalRoll();
            skill2.Init(this);
            unlockedSkills.Add(skill2);
        }

        if (level >= 3)
        {
            var skill3 = new SniperSkill3_DecoyMine();
            skill3.Init(this);
            unlockedSkills.Add(skill3);
        }
    }
}
