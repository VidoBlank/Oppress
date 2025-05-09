using UnityEngine;

public class Sniper : PlayerController
{
    [Header("����Transform")]
    public Transform weapon;

    [Header("ǹ�ڵ�Ŀ��λ��")]
    public Transform targetGameObject;

    [Header("�ѻ����ܲ���")]
    public float snipeChargeTime = 1.5f;        // ����ʱ��
    public float snipeBulletSpeed = 30f;        // �ӵ��ٶ�
    public float snipeBulletRange = 15f;        // ���
    public int snipeDamage = 50;                // �˺�
    public float snipeEnergyCost = 25f;         // ��������
    public float snipeCooldown = 10f;           // ��ȴʱ��

    [Header("�������ܲ���")]
    public float rollDistance = 2f;             
    public float rollSpeed = 8f;
    public float rollDuration = 0.35f;
    public float rollEnergyCost = 12f;
    public float rollCooldown = 6f;
    public bool isRollInvincible = false;        

    [Header("�ն����׼��ܲ���")]
    public GameObject decoyMinePrefab;          // �ն�����Ԥ����
    public float decoyDuration = 8f;            // ����ʱ��
    public float decoyExplosionRadius = 3f;     // ��ը��Χ
    public int decoyExplosionDamage = 40;       // ��ը�˺�
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
