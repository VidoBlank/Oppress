using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class UnitTalentTreeMapping
{
    public string unitName;                // 角色名称，比如 "Engineer", "Rifleman", "Sniper", "Support"
    public GameObject talentTreePrefab;      // 对应的天赋树预制体
}

public class RoleUpgradeManager : MonoBehaviour
{
    [Header("PlayerTeamManager 引用")]
    public PlayerTeamManager playerTeamManager;

    [Header("头像列表容器（需带 Horizontal Layout Group）")]
    public Transform avatarListContainer; // 放置头像的 UI 容器

    [Header("详细升级界面容器")]
    public Transform detailUIContainer;   // 详细升级界面容器

    [Header("天赋树 UI 容器")]
    public Transform talentTreeUIContainer; // 天赋树面板所在容器

    [Header("预制体引用")]
    public GameObject upgradeAvatarSlotPrefab;
    public GameObject roleUpgradeDetailUIPrefab;

    [Header("天赋树预制体引用")]
    public GameObject talentTreePrefab_Level0;      // 角色为0级时使用的天赋树预制体

    [Header("天赋树预制体映射")]
    public List<UnitTalentTreeMapping> talentTreeMappings;    // 针对 Engineer、Rifleman、Sniper、Support 等角色的映射
    public GameObject talentTreePrefab_Level1_3_Default;        // 默认预制体（若无匹配）

    private Dictionary<GameObject, GameObject> detailUIInstances = new Dictionary<GameObject, GameObject>();
    private Dictionary<GameObject, GameObject> talentTreeInstances = new Dictionary<GameObject, GameObject>();

    private GameObject currentActiveDetailUI = null;
    private GameObject currentActiveTalentTreeUI = null;

    private int lastUnitCount = -1;
    public static RoleUpgradeManager Instance;
    void OnEnable()
    {
        // 订阅场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // 取消订阅，避免重复注册
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (playerTeamManager == null)
            playerTeamManager = PlayerTeamManager.Instance;

        // 初次生成
        RefreshAvatarListUI();
        lastUnitCount = playerTeamManager.allUnitPrefabs.Count;
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 如果需要在场景切换时保留此对象，则调用
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
        // 如果干员数量发生变化，则刷新
        int currentCount = playerTeamManager.allUnitPrefabs.Count;
        if (currentCount != lastUnitCount)
        {
            RefreshAvatarListUI();
            lastUnitCount = currentCount;
        }
    }

    // 当场景加载时触发
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 当场景加载完成后，尝试在新场景中找到对应的 UI 对象
        var avatarListObj = GameObject.Find("AvatarListContainer");
        if (avatarListObj != null)
        {
            avatarListContainer = avatarListObj.transform;
        }
        else
        {
            Debug.LogWarning("[RoleUpgradeManager] 没有找到名为 'AvatarListContainer' 的对象！");
        }

        var detailObj = GameObject.Find("DetailUIContainer");
        if (detailObj != null)
        {
            detailUIContainer = detailObj.transform;
        }
        else
        {
            Debug.LogWarning("[RoleUpgradeManager] 没有找到名为 'DetailUIContainer' 的对象！");
        }

        var talentTreeObj = GameObject.Find("TalentTreeUIContainer");
        if (talentTreeObj != null)
        {
            talentTreeUIContainer = talentTreeObj.transform;
        }
        else
        {
            Debug.LogWarning("[RoleUpgradeManager] 没有找到名为 'TalentTreeUIContainer' 的对象！");
        }

        // 重新刷新 UI
        RefreshAvatarListUI();
    }

    public void RefreshAvatarListUI()
    {
        // ✅ 清除旧 UI 实例引用（关键！）
        detailUIInstances.Clear();
        talentTreeInstances.Clear();
        currentActiveDetailUI = null;
        currentActiveTalentTreeUI = null;

        // 1. 清除旧的头像 UI
        foreach (Transform child in avatarListContainer)
        {
            Destroy(child.gameObject);
        }

        // 只隐藏 DetailUIContainer 下的子物体，不销毁它们
        foreach (Transform child in detailUIContainer)
        {
            child.gameObject.SetActive(false);
        }

        // 不清空 detailUIInstances 和 talentTreeInstances 字典，
        // 这样已创建的面板实例可以保留并复用
        // detailUIInstances.Clear();
        // talentTreeInstances.Clear();
        // currentActiveDetailUI = null;
        // currentActiveTalentTreeUI = null;

        // 2. 生成新的头像 UI
        foreach (GameObject unit in playerTeamManager.allUnitPrefabs)
        {
            GameObject avatarSlotInstance = Instantiate(upgradeAvatarSlotPrefab, avatarListContainer);
            UpgradeAvatarSlot avatarSlot = avatarSlotInstance.GetComponent<UpgradeAvatarSlot>();
            if (avatarSlot != null)
            {
                avatarSlot.Setup(unit, OnAvatarSlotClicked);
            }
        }
    }


    public void OnAvatarSlotClicked(GameObject unit)
    {
        if (unit == null)
        {
            // 隐藏所有详细升级界面和天赋树面板
            foreach (var detail in detailUIInstances.Values)
            {
                detail.SetActive(false);
            }
            foreach (var tree in talentTreeInstances.Values)
            {
                tree.SetActive(false);
            }
            currentActiveDetailUI = null;
            currentActiveTalentTreeUI = null;
            return;
        }

        // 隐藏除当前点击角色外所有的详细升级界面
        foreach (var kv in detailUIInstances)
        {
            if (kv.Key != unit)
            {
                kv.Value.SetActive(false);
            }
        }

        // 如果当前角色没有详细升级界面，则实例化 DetailUI
        if (!detailUIInstances.TryGetValue(unit, out GameObject detailUI))
        {
            detailUI = Instantiate(roleUpgradeDetailUIPrefab, detailUIContainer);
            RoleUpgradeDetailUI detailUIComponent = detailUI.GetComponent<RoleUpgradeDetailUI>();
            if (detailUIComponent != null)
            {
                detailUIComponent.Setup(unit);
            }
            detailUIInstances[unit] = detailUI;
        }
        else
        {
            RoleUpgradeDetailUI detailUIComponent = detailUI.GetComponent<RoleUpgradeDetailUI>();
            if (detailUIComponent != null)
            {
                detailUIComponent.ForceRefresh(); // 无论新旧，强制刷新
            }
        }

        // 激活当前角色的详细升级界面
        currentActiveDetailUI = detailUI;
        currentActiveDetailUI.SetActive(true);

        // 隐藏除当前角色外的所有天赋树面板
        foreach (var kv in talentTreeInstances)
        {
            if (kv.Key != unit)
            {
                kv.Value.SetActive(false);
            }
        }

        // 获取角色的 PlayerController
        PlayerController pc = unit.GetComponent<PlayerController>();
        if (pc == null)
        {
            Debug.LogError($"角色 {unit.name} 未找到 PlayerController 组件！");
            return;
        }

        // 检查是否已有该角色对应的天赋树实例
        GameObject talentTreeUI;
        if (!talentTreeInstances.TryGetValue(unit, out talentTreeUI))
        {
            // 根据角色等级和映射关系确定使用哪个预制体
            GameObject prefabToUse = null;
            if (pc.level == 0)
            {
                prefabToUse = talentTreePrefab_Level0;
            }
            else if (pc.level >= 1 && pc.level <= 3)
            {
                foreach (var mapping in talentTreeMappings)
                {
                    if (mapping.unitName == pc.symbol.unitName)
                    {
                        prefabToUse = mapping.talentTreePrefab;
                        break;
                    }
                }
                if (prefabToUse == null)
                {
                    prefabToUse = talentTreePrefab_Level1_3_Default;
                }
            }
            else
            {
                prefabToUse = talentTreePrefab_Level1_3_Default;
            }

            if (prefabToUse != null)
            {
                talentTreeUI = Instantiate(prefabToUse, talentTreeUIContainer);

                // 根据角色等级调用不同的 Setup 接口
                if (pc.level == 0)
                {
                    TalentTree_Level0 tt = talentTreeUI.GetComponent<TalentTree_Level0>();
                    if (tt != null)
                    {
                        tt.Setup(unit);
                        Debug.Log($"RoleUpgradeManager: 调用 TalentTree_Level0.Setup 成功，干员：{unit.name}");
                    }
                    else
                    {
                        Debug.LogWarning("RoleUpgradeManager: 预制体上未找到 TalentTree_Level0 组件！");
                    }
                }
                else if (pc.level >= 1 && pc.level <= 3)
                {
                    TalentTreeUI tt = talentTreeUI.GetComponent<TalentTreeUI>();
                    if (tt != null)
                    {
                        tt.SetupForPlayer(pc);
                        Debug.Log($"RoleUpgradeManager: 调用 TalentTreeUI.SetupForPlayer 成功，角色：{unit.name}");
                    }
                    else
                    {
                        Debug.LogWarning("RoleUpgradeManager: 预制体上未找到 TalentTreeUI 组件！");
                    }
                }
                // 存储创建的天赋树实例
                talentTreeInstances[unit] = talentTreeUI;
            }
        }
        // 激活天赋树面板
        if (talentTreeUI != null)
        {
            talentTreeUI.SetActive(true);
            currentActiveTalentTreeUI = talentTreeUI;
        }
    }

}

