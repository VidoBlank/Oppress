using UnityEngine;

public class Engineer : PlayerController
{
    [Header("�����ܼ��ܲ���")]
    public GameObject shieldPrefab;
    public Transform shieldPosition;
    public int shieldHealth = 200;
    public float shieldDuration = 30f;

    [Header("����ǹ���ܲ���")]
    public int grapeshotPelletCount = 5;     // ��������
    public float grapeshotSpreadAngle = 30f; // ��ɢ�Ƕ�
    public float grapeshotRange = 4f;
    public float grapeshotEnergyCost = 15f;
    public float grapeshotCooldown = 6f;

    [Header("������̨���ܲ���")]
    public GameObject turretPrefab;
    public Transform deployPoint;
    public float turretDuration = 20f;
    public float turretEnergyCost = 25f;
    public float turretCooldown = 12f;

    private EngineerLevelSystem levelSystem;

    protected override void Awake()
    {
        base.Awake();
        levelSystem = GetComponent<EngineerLevelSystem>();
        if (levelSystem == null)
        {
            Debug.LogError("Engineer δ�ҵ� EngineerLevelSystem �����");
        }
        RefreshUnlockedSkills();
    }

    protected override void Start()
    {
        base.Start();
        level = Mathf.Clamp(level, 1, 4);
        levelSystem?.SetAttributesByLevel(level);
    }

    public override void SetLevel(int newLevel)
    {
        if (newLevel < 1 || newLevel > 4) return;
        base.SetLevel(newLevel);
        levelSystem?.SetAttributesByLevel(level);
    }

    public override void RefreshUnlockedSkills()
    {
        unlockedSkills.Clear();

        if (level >= 1)
        {
            var skill1 = new EngineerSkill1_RiotShield();
            skill1.Init(this);
            unlockedSkills.Add(skill1);
        }

        if (level >= 2)
        {
            var skill2 = new EngineerSkill2_Grapeshot();
            skill2.Init(this);
            unlockedSkills.Add(skill2);
        }

        if (level >= 3)
        {
            var skill3 = new EngineerSkill3_Turret();
            skill3.Init(this);
            unlockedSkills.Add(skill3);
        }
    }
}
