using UnityEngine;

public class Support : PlayerController
{
    [Header("����Transform")]
    public Transform weapon;

    [Header("ǹ�ڵ�Ŀ��λ��")]
    public Transform targetSubObject;

    private SupportLevelSystem levelSystem;

    [Header("����ҩ�似�ܲ���")]
    public GameObject ammoBoxPrefab;
    public Transform deployPoint;
    public float ammoBoxDuration = 20f;
    public float ammoBoxEnergyCost = 10f;
    public float ammoBoxCooldown = 8f;

    [Header("ǿ�����弼�ܲ���")]
    public GameObject barrierPrefab;
    public float barrierDuration = 15f;
    public float barrierDeployRange = 5f;
    public float barrierEnergyCost = 12f;
    public float barrierCooldown = 10f;

    [Header("����ѹ�Ƽ��ܲ���")]
    public GameObject machineGunPrefab;
    public float suppressFireRate = 0.1f;
    public int suppressDamage = 5;
    public float suppressEnergyCostPerSec = 5f;
    public float suppressCooldown = 3f;

    protected override void Awake()
    {
        base.Awake();
        levelSystem = GetComponent<SupportLevelSystem>();
        if (levelSystem == null)
        {
            Debug.LogError("Support δ�ҵ� SupportLevelSystem �����");
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
            var skill1 = new SupportSkill1_DeployAmmoBox();
            skill1.Init(this);
            unlockedSkills.Add(skill1);
        }

        if (level >= 2)
        {
            var skill2 = new SupportSkill2_Barrier();
            skill2.Init(this);
            unlockedSkills.Add(skill2);
        }

        if (level >= 3)
        {
            var skill3 = new SupportSkill3_SuppressFire();
            skill3.Init(this);
            unlockedSkills.Add(skill3);
        }
    }
}
