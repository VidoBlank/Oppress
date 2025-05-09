using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : SingleTon<MonsterManager>
{
    [System.Serializable]
    public class MonsterGruop
    {
        [Header("����")]
        public Monster monster = Monster.Zombie;
        [Header("����λ��")]
        public Transform transform;
        [Header("��������")]
        public int monsterNumber = 1;
        [Header("���ˢ��ʱ��")]
        public float nextGenerateTime = 0.5f;
        [Header("x����ƫ��ֵ")]
        public float random_X = 0.5f;
        [Header("y����ƫ��ֵ")]
        public float random_Y = 0.5f;
        [Header("z����ƫ��ֵ")]
        public float random_Z = 0.5f;

    }

    [System.Serializable]
    public class GruopsParameter
    {
        [Header("ˢ��ʱ��")]
        public float time = 30;
        [Header("������")]
        public List<MonsterGruop> monstersGroup = new List<MonsterGruop>();
    }
    [SerializeField]
    [Header("���ﲨ�β���")]
    private List<GruopsParameter> gruopsParameters = new List<GruopsParameter>();

    [SerializeField]
    [Header("��ǰ�ؿ���")]
    private int level;

    [SerializeField]
    [Header("��ǰ�ؿ�ʱ��")]
    private float nowLevelTime;

    private float totalLevelTime=9999f;

    public GameObject monsters;

    /// <summary>
    /// ������Ӧ�Ĺؿ�������
    /// </summary>
    /// <param name="Order"></param>
    private void GroupGenerate(int Order)
    {

    }

    private void NextGroup()
    {
        if (level >= gruopsParameters.Count) { return; } //Debug.Log("����ˢ�½���");
        Debug.Log("��ʼ����");
        StartCoroutine(GroupsGenerateCoroutine(gruopsParameters[level].monstersGroup));
        level += 1;
        nowLevelTime = 0;
        if (level >= gruopsParameters.Count) { return; } //Debug.Log("����ˢ�½���");
        totalLevelTime = gruopsParameters[level].time;
    }

    IEnumerator GroupsGenerateCoroutine(List<MonsterGruop> monsterGruops)
    {
        for(int i = 0;i<monsterGruops.Count;i++)
        {
            yield return new WaitForSeconds(monsterGruops[i].nextGenerateTime);
            GenerateMonsterGroup(monsterGruops[i]);
        }
    }


    private void GenerateMonsterGroup(MonsterGruop monsterGruop)
    {
        if (monsterGruop.monsterNumber >= 1)
        {
            for (int i = 0; i < monsterGruop.monsterNumber; i++)
            {
                if (monsterGruop.transform == null) monsterGruop.transform = Instance.monsters.transform;
                Debug.Log("����ѡ��");
                float random_X_Distance = Random.Range(-monsterGruop.random_X, monsterGruop.random_X);
                float random_Y_Distance = Random.Range(-monsterGruop.random_Y, monsterGruop.random_Y);
                float random_Z_Distance = Random.Range(-monsterGruop.random_Z, monsterGruop.random_Z);
                Vector3 generatePosition = new Vector3(monsterGruop.transform.position.x + random_X_Distance, monsterGruop.transform.position.y + random_Y_Distance, monsters.transform.position.z + random_Z_Distance);
                if (monsterGruop.monster == Monster.Zombie)
                {
                    Debug.Log("��ʼ���ɽ�ʬ");
                    GameObject monster = Resources.Load<GameObject>("Enemy/Zombie");
                    GameObject monsterInstance = Instantiate(monster, monsters.transform);
                    monsterInstance.transform.position = generatePosition;
                }
                else if (monsterGruop.monster == Monster.Spider)
                {
                    GameObject monster = Resources.Load<GameObject>("Enemy/Spider");
                    GameObject monsterInstance = Instantiate(monster, monsters.transform);
                    monsterInstance.transform.position = generatePosition;
                }
                else if (monsterGruop.monster == Monster.Behemoth)
                {
                    GameObject monster = Resources.Load<GameObject>("Enemy/Behemoth");
                    GameObject monsterInstance = Instantiate(monster, monsters.transform);
                    monsterInstance.transform.position = generatePosition;
                }
                else if (monsterGruop.monster == Monster.Slinger)
                {
                    GameObject monster = Resources.Load<GameObject>("Enemy/Slinger");
                    GameObject monsterInstance = Instantiate(monster, monsters.transform);
                    monsterInstance.transform.position = generatePosition;
                }
            }
        }
    }

    void Awake()
    {
        CoreController.Instance.InitGameEntity += new CoreController.InitGameHandler(InitGame);
    }



    void InitGame()
    {
        level = 0;
        monsters = GameObject.Find("Monsters");
        nowLevelTime = 0;
        totalLevelTime = gruopsParameters[level].time;
    }




    // Update is called once per frame
    void Update()
    {
        // if (play != true) return;
        nowLevelTime += Time.deltaTime;
        if (nowLevelTime > totalLevelTime) NextGroup(); //�ؿ�δ����
    }
}
