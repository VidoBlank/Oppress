using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : SingleTon<MonsterManager>
{
    [System.Serializable]
    public class MonsterGruop
    {
        [Header("怪物")]
        public Monster monster = Monster.Zombie;
        [Header("生成位置")]
        public Transform transform;
        [Header("怪物数量")]
        public int monsterNumber = 1;
        [Header("组间刷新时间")]
        public float nextGenerateTime = 0.5f;
        [Header("x坐标偏差值")]
        public float random_X = 0.5f;
        [Header("y坐标偏差值")]
        public float random_Y = 0.5f;
        [Header("z坐标偏差值")]
        public float random_Z = 0.5f;

    }

    [System.Serializable]
    public class GruopsParameter
    {
        [Header("刷新时间")]
        public float time = 30;
        [Header("怪物组")]
        public List<MonsterGruop> monstersGroup = new List<MonsterGruop>();
    }
    [SerializeField]
    [Header("怪物波次参数")]
    private List<GruopsParameter> gruopsParameters = new List<GruopsParameter>();

    [SerializeField]
    [Header("当前关卡数")]
    private int level;

    [SerializeField]
    [Header("当前关卡时间")]
    private float nowLevelTime;

    private float totalLevelTime=9999f;

    public GameObject monsters;

    /// <summary>
    /// 生成相应的关卡怪物组
    /// </summary>
    /// <param name="Order"></param>
    private void GroupGenerate(int Order)
    {

    }

    private void NextGroup()
    {
        if (level >= gruopsParameters.Count) { return; } //Debug.Log("波次刷新结束");
        Debug.Log("开始生成");
        StartCoroutine(GroupsGenerateCoroutine(gruopsParameters[level].monstersGroup));
        level += 1;
        nowLevelTime = 0;
        if (level >= gruopsParameters.Count) { return; } //Debug.Log("波次刷新结束");
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
                Debug.Log("生成选择");
                float random_X_Distance = Random.Range(-monsterGruop.random_X, monsterGruop.random_X);
                float random_Y_Distance = Random.Range(-monsterGruop.random_Y, monsterGruop.random_Y);
                float random_Z_Distance = Random.Range(-monsterGruop.random_Z, monsterGruop.random_Z);
                Vector3 generatePosition = new Vector3(monsterGruop.transform.position.x + random_X_Distance, monsterGruop.transform.position.y + random_Y_Distance, monsters.transform.position.z + random_Z_Distance);
                if (monsterGruop.monster == Monster.Zombie)
                {
                    Debug.Log("开始生成僵尸");
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
        if (nowLevelTime > totalLevelTime) NextGroup(); //关卡未结束
    }
}
