using UnityEngine;

public class Medic : PlayerController
{
    [Header("医疗兵武器Transform")]
    public Transform weapon;

    [Header("治疗枪技能参数")]
    public float healAmountPerSecond = 10f;
    public float healRange = 6f;
    public float healEnergyCostPerSecond = 5f;
    public float healCooldown = 2f;

    [Header("医疗无人机技能参数")]
    public GameObject dronePrefab;
    public float droneDuration = 15f;
    public float droneHealInterval = 2f;
    public float droneHealAmount = 20f;
    public float droneRange = 4f;
    public int droneHealth = 100;
    public float droneEnergyCost = 20f;
    public float droneCooldown = 10f;

    [Header("急救力场技能参数")]
    public GameObject healingFieldPrefab;
    public float fieldDuration = 8f;
    public float healPercentPerSecond = 0.05f;   // 每秒恢复5%最大生命
    public float damageReductionPercent = 0.2f;  // 减伤20%
    public float fieldEnergyCost = 25f;
    public float fieldCooldown = 12f;

    protected override void Awake()
    {
        base.Awake();
        RefreshUnlockedSkills();
    }

    protected override void Start()
    {
        base.Start();
        level = Mathf.Clamp(level, 1, 4);
    }

    public override void RefreshUnlockedSkills()
    {
        unlockedSkills.Clear();

        if (level >= 1)
        {
            var skill1 = new MedicSkill1_HealGun();
            skill1.Init(this);
            unlockedSkills.Add(skill1);
        }

        if (level >= 2)
        {
            var skill2 = new MedicSkill2_DeployDrone();
            skill2.Init(this);
            unlockedSkills.Add(skill2);
        }

        if (level >= 3)
        {
            var skill3 = new MedicSkill3_HealingField();
            skill3.Init(this);
            unlockedSkills.Add(skill3);
        }
    }
}
