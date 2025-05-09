using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
public class PlayerTeamManager : MonoBehaviour
{
    public static PlayerTeamManager Instance;

    [Header("UI")]
    public Transform characterListPanel;          // 预期在场景中找到的角色列表面板
    public GameObject characterSlotPrefab;
    public TextMeshProUGUI selectedCountText;
    [Header("提示 UI")]
    public GameObject classLimitNoticePanel; // 外部挂载提示框面板
    public TextMeshProUGUI classLimitNoticeText; // 外部挂载提示文字组件

    [Header("当前实际使用的干员列表 (运行时)")]
    public List<GameObject> allUnitPrefabs; // 初始保持空

    [Header("已选中的干员预制体")]
    public List<GameObject> selectedUnitPrefabs = new List<GameObject>();

    public Dictionary<PlayerController, CharacterSlot> unitSlotMap = new Dictionary<PlayerController, CharacterSlot>();
    // 新增：记录需要播放动画的干员
    private List<GameObject> newlyAddedUnits = new List<GameObject>();

    private const string SAVE_KEY_ALL_UNITS = "AllUnitsList";
    private const string SAVE_KEY_SELECTED_UNITS = "SelectedUnits";


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnApplicationQuit()
    {
        SaveAllUnits();
        Debug.Log("[PlayerTeamManager] 应用退出，已保存干员数据。");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 当场景加载后，重新查找 UI 对象（如果它们为空），并刷新UI
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 尝试根据名称查找 UI 对象，确保名称与场景中实际的对象名称一致
        if (characterListPanel == null)
        {
            GameObject panelObj = GameObject.Find("CharacterListPanel");
            if (panelObj != null)
            {
                characterListPanel = panelObj.transform;
            }
            else
            {
                Debug.LogWarning("未能在新场景中找到 CharacterListPanel");
            }
        }
        if (selectedCountText == null)
        {
            GameObject textObj = GameObject.Find("SelectedCountText");
            if (textObj != null)
            {
                selectedCountText = textObj.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                Debug.LogWarning("未能在新场景中找到 SelectedCountText");
            }
        }
        // ✅ 新增：重新绑定 classLimitNoticePanel 和 classLimitNoticeText
        if (classLimitNoticePanel == null)
        {
            classLimitNoticePanel = GameObject.Find("ClassLimitNoticePanel");
            if (classLimitNoticePanel == null)
                Debug.LogWarning("未能在新场景中找到 ClassLimitNoticePanel");
        }

        if (classLimitNoticeText == null && classLimitNoticePanel != null)
        {
            classLimitNoticeText = classLimitNoticePanel.GetComponentInChildren<TextMeshProUGUI>();
            if (classLimitNoticeText == null)
                Debug.LogWarning("ClassLimitNoticePanel 下未找到 TextMeshProUGUI 组件");
        }
        // 刷新UI
        InitializeCharacterSlots();
        UpdateSelectedCountText();

    }

    private void Start()
    {
        LoadAllUnits();
        PrintAllPlayerPrefs();

        StartCoroutine(DelayedRefreshActiveUnits());
        // 初始化 UI（首次加载时，如果 UI 引用有效）
        InitializeCharacterSlots();
        UpdateSelectedCountText();
    }

    private void InitializeCharacterSlots()
    {
        if (characterListPanel == null || characterSlotPrefab == null)
        {
            Debug.LogError("UI 组件未正确分配！");
            return;
        }

        foreach (Transform child in characterListPanel)
        {
            Destroy(child.gameObject);
        }

        unitSlotMap.Clear();

        foreach (var unitPrefab in allUnitPrefabs)
        {
            if (unitPrefab == null) continue;

            GameObject newSlot = Instantiate(characterSlotPrefab, characterListPanel);
            var slot = newSlot.GetComponent<CharacterSlot>();
            PlayerController pc = unitPrefab.GetComponent<PlayerController>();

            if (slot != null && pc != null)
            {
                slot.Setup(unitPrefab);
                slot.SetRemoveButtonActive(false);
                unitSlotMap[pc] = slot;

                if (newlyAddedUnits.Contains(unitPrefab))
                {
                    CanvasGroup cg = newSlot.GetComponent<CanvasGroup>();
                    if (cg == null) cg = newSlot.AddComponent<CanvasGroup>();

                    cg.alpha = 0;
                    newSlot.transform.localScale = new Vector3(1.1f, 1.1f, 1f); // 稍大，冷启动感

                   
                    cg.DOFade(1f, 0.25f).SetEase(Ease.OutQuad);

                    // 冷启动缩放（先略放大再回到原状）
                    newSlot.transform.DOScale(Vector3.one, 0.3f)
                        .SetEase(Ease.InOutSine)
                        .SetDelay(0.05f); 

                }

                else
                {
                    // 对非新加入的干员直接设置到正常状态（避免动画）
                    CanvasGroup cg = newSlot.GetComponent<CanvasGroup>();
                    if (cg == null) cg = newSlot.AddComponent<CanvasGroup>();
                    cg.alpha = 1f;
                    newSlot.transform.localScale = Vector3.one;
                }
            }
        }

        // 播放动画后，清空新加入干员列表
        newlyAddedUnits.Clear();
    }


    public void SelectCharacter(GameObject unitPrefab)
    {
        if (unitPrefab == null) return;

        if (selectedUnitPrefabs.Count >= 4)
        {
            Debug.LogWarning("最多只能选择 4 名干员！");
            return;
        }

        var newPC = unitPrefab.GetComponent<PlayerController>();
        if (newPC == null)
        {
            Debug.LogWarning("无法识别该干员的控制器组件");
            return;
        }

        string newUnitClass = newPC.symbol.unitName;
        int sameParentClassCount = 0;

        foreach (var selectedPrefab in selectedUnitPrefabs)
        {
            var pc = selectedPrefab.GetComponent<PlayerController>();
            if (pc != null && pc.symbol.unitName == newUnitClass)
            {
                sameParentClassCount++;
            }
        }

        // ✅ 获取 slot 一次
        unitSlotMap.TryGetValue(newPC, out var slot);

        if (sameParentClassCount >= 2)
        {
            Debug.LogWarning("每种职业最多只能携带 2 名干员！");
            ShowClassLimitPopup(newPC.symbol.unitName);

            // ✅ 触发视觉反馈（防止重复声明 slot）
            slot?.PlaySelectionFailedEffect();  // 确保有 slot 才调用
            return;
        }

        selectedUnitPrefabs.Add(unitPrefab);
        unitPrefab.SetActive(true);
        UpdateSelectedCountText();

        slot?.SetRemoveButtonActive(true);  // ✅ 继续复用 slot
    }

    private Coroutine classLimitCoroutine;

    private void ShowClassLimitPopup(string className)
    {
        if (classLimitNoticePanel == null || classLimitNoticeText == null)
        {
            Debug.LogWarning("⚠️ 缺少提示面板或文字绑定！");
            return;
        }
        UIAudioManager.Instance?.PlayFail();
        // 设置提示内容
        classLimitNoticeText.text = $"相同分类干员部署上限已达，类型: <color=#00FFFF>{className}</color>\n当前配备: [2]\n终端拒绝重复部署此类干员。";

        // 激活面板并启动隐藏协程
        classLimitNoticePanel.SetActive(true);
        if (classLimitCoroutine != null) StopCoroutine(classLimitCoroutine);
        classLimitCoroutine = StartCoroutine(HideClassLimitPopupAfterDelay(3f));
    }

    private IEnumerator HideClassLimitPopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        classLimitNoticePanel.SetActive(false);
    }



    public void DeselectCharacter(GameObject unitPrefab)
    {
        if (selectedUnitPrefabs.Contains(unitPrefab))
        {
            selectedUnitPrefabs.Remove(unitPrefab);
            unitPrefab.SetActive(false);
            UpdateSelectedCountText();
            PlayerController pc = unitPrefab.GetComponent<PlayerController>();
            if (pc != null && unitSlotMap.ContainsKey(pc))
            {
                unitSlotMap[pc].SetRemoveButtonActive(true);
            }
        }
    }

    private void UpdateSelectedCountText()
    {
        if (selectedCountText != null)
        {
            selectedCountText.text = $"已选干员：{selectedUnitPrefabs.Count}/4";
        }
    }

    /// <summary>
    /// 在游戏里实例化选中的干员
    /// </summary>
    public void GenerateSelectedUnitsInGame()
    {
        foreach (var unitPrefab in selectedUnitPrefabs)
        {
            if (unitPrefab == null) continue;
            Instantiate(unitPrefab, GetSpawnPosition(), Quaternion.identity);
        }
    }

    private Vector3 GetSpawnPosition()
    {
        return new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
    }


    // ---------------------------------
    //  下面是【allUnitPrefabs】的存/读 (使用 Resources.Load)
    // ---------------------------------

    /// <summary>
    /// 将 allUnitPrefabs 保存 (只存名字)
    /// </summary>
    public void SaveAllUnits()
    {
        if (allUnitPrefabs == null || allUnitPrefabs.Count == 0)
        {
            PlayerPrefs.DeleteKey(SAVE_KEY_ALL_UNITS);
            PlayerPrefs.Save();
            return;
        }

        List<UnitData> unitDataList = new List<UnitData>();
        foreach (var instance in allUnitPrefabs)
        {
            PlayerController playerController = instance.GetComponent<PlayerController>();
            if (playerController != null)
            {
                List<string> slotEffectNames = new List<string>();
                foreach (var slot in playerController.slots)
                {
                    if (slot != null && slot.slotEffect != null)
                        slotEffectNames.Add(slot.slotEffect.name);
                }

                AttributeData attrData = new AttributeData(playerController.attribute, playerController.visionRadius);

                // ✅ 获取当前技能 ID
                string skillID = playerController.equippedSkill != null ? playerController.equippedSkill.SkillID : null;

                UnitData data = new UnitData(
                    playerController.symbol.unitName,
                    playerController.symbol.unitID,
                    playerController.health,
                    playerController.energy,
                    playerController.level,
                    slotEffectNames,
                    playerController.hasVersatileTalent,
                    attrData
                );
                data.equippedSkillID = skillID; // ✅ 保存技能 ID
                data.overrideName = playerController.symbol.Name;
                unitDataList.Add(data);
            }
        }

        string json = JsonUtility.ToJson(new UnitDataWrapper(unitDataList));
        PlayerPrefs.SetString(SAVE_KEY_ALL_UNITS, json);
        PlayerPrefs.Save();

        Debug.Log("[PlayerTeamManager] ✅ 已保存干员数据到 PlayerPrefs。");
    }







    public void LoadAllUnits()
    {
        allUnitPrefabs.Clear();

        if (!PlayerPrefs.HasKey(SAVE_KEY_ALL_UNITS))
        {
            Debug.Log("[PlayerTeamManager] ❌ 存档中没有干员数据，可能是首次运行或存档为空。");
            return;
        }

        string json = PlayerPrefs.GetString(SAVE_KEY_ALL_UNITS);
        UnitDataWrapper wrapper = JsonUtility.FromJson<UnitDataWrapper>(json);

        foreach (UnitData unitData in wrapper.units)
        {
            GameObject prefab = Resources.Load<GameObject>("Player/" + unitData.unitName);
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab);
                instance.transform.SetParent(this.transform);

                PlayerController playerController = instance.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.symbol.unitID = unitData.unitID;
                    playerController.symbol.unitName = unitData.unitName;
                    if (!string.IsNullOrEmpty(unitData.overrideName))
                    {
                        playerController.symbol.Name = unitData.overrideName; // ✅ 恢复自定义干员名称
                    }
                    playerController.health = unitData.health;
                    playerController.energy = unitData.energy;
                    playerController.level = unitData.level;
                    playerController.hasVersatileTalent = unitData.hasVersatileTalent;

                    // 属性恢复
                    if (unitData.attributeData != null)
                    {
                        var attr = playerController.attribute;
                        var saved = unitData.attributeData;

                        attr.maxhealth = saved.maxhealth;
                        attr.maxEnergy = saved.maxEnergy;
                        attr.knockdownHealth = saved.knockdownHealth;
                        attr.interactspeed = saved.interactspeed;
                        attr.recoveredspeed = saved.recoveredspeed;
                        attr.movingSpeed = saved.movingSpeed;
                        attr.interactionRange = saved.interactionRange;
                        attr.damage = saved.damage;
                        attr.attackDelay = saved.attackDelay;
                        attr.bulletSpeed = saved.bulletSpeed;
                        attr.damageTakenMultiplier = saved.damageTakenMultiplier;
                        attr.flatDamageReduction = saved.flatDamageReduction;
                        attr.skillCooldownRate = saved.skillCooldownRate;
                        playerController.visionRadius = saved.visionRadius;
                    }

                    // 插槽恢复
                    playerController.slots = new List<Slot>();
                    foreach (string effectName in unitData.slotEffects)
                    {
                        SlotEffect effect = Resources.Load<SlotEffect>("SlotEffects/" + effectName);
                        if (effect != null)
                            playerController.slots.Add(new Slot(effectName, effect));
                    }

                    // ✅ 解锁技能并恢复已装备技能
                    playerController.RefreshUnlockedSkills();
                    if (!string.IsNullOrEmpty(unitData.equippedSkillID))
                    {
                        IPlayerSkill found = playerController.unlockedSkills.Find(skill => skill.SkillID == unitData.equippedSkillID);
                        if (found != null)
                        {
                            playerController.EquipSkill(found);
                            Debug.Log($"✅ 恢复装备技能：{found.SkillName}");

                            // ✅ 手动同步 SkillButtonDisplay
                            var skillDisplay = playerController.GetComponentInChildren<SkillButtonDisplay>();
                            if (skillDisplay != null)
                            {
                                // ✅ 不再通过 UI，而是 SkillGroupManager 获取技能组
                                var group = SkillGroupManager.Instance.GetSkillGroupFor(playerController.symbol.unitName);
                                if (group != null)
                                {
                                    skillDisplay.Init(playerController);
                                }
                                else
                                {
                                    Debug.LogWarning($"⚠️ 未找到角色 {playerController.symbol.unitName} 的技能组数据（来自 SkillGroupManager）");
                                }
                            }

                        }
                    }


                    allUnitPrefabs.Add(instance);

            }
        }
            foreach (var unit in allUnitPrefabs)
            {
                var pc = unit.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.TriggerNameChanged(); // ✅ 正确方式触发事件
                }
            }

            Debug.Log($"[PlayerTeamManager] ✅ 已加载 {allUnitPrefabs.Count} 个干员数据。");
        }
    }



    private IEnumerator DelayedRefreshActiveUnits()
    {
        yield return new WaitForSeconds(0.1f);
        RefreshActiveUnits();
    }


    /// <summary>
    /// 确保未选中的角色保持关闭，仅激活已选中角色
    /// </summary>
    private void RefreshActiveUnits()
    {
        foreach (var unit in allUnitPrefabs)
        {
            if (unit == null) continue;
            unit.SetActive(selectedUnitPrefabs.Contains(unit));
        }
    }








    // ------------------------------------
    // ★ 新增：清空干员列表并删除存档
    // ------------------------------------
    public void ClearAllUnits()
    {
        // 1. 清空运行时的列表
        allUnitPrefabs.Clear();

        // 2. 在 PlayerPrefs 中删除与干员列表相关的键
        PlayerPrefs.DeleteKey(SAVE_KEY_ALL_UNITS);
        PlayerPrefs.Save();

        Debug.Log($"[PlayerTeamManager] 已清空所有干员, 列表Count={allUnitPrefabs.Count}");

        // 3. 你可以选择重新保存，这里直接不存
        //    如果你想把空列表写入存档，也可再次调用 SaveAllUnits();

        // 4. 刷新UI (如果想让界面立刻恢复到空)
        InitializeCharacterSlots();
    }

    // ---------------------------
    // ★ 新增：运行时添加新干员
    // ---------------------------
    public void AddNewUnit(string unitName)
    {
        GameObject newPrefab = Resources.Load<GameObject>("Player/" + unitName);
        if (newPrefab != null)
        {
            GameObject newInstance = Instantiate(newPrefab, this.transform);
            newInstance.SetActive(false);

            PlayerController playerController = newInstance.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.symbol.unitID = unitName + "_" + System.Guid.NewGuid().ToString();
                playerController.symbol.unitName = unitName;
            }

            allUnitPrefabs.Add(newInstance);
            newlyAddedUnits.Add(newInstance); // ✅ 标记为新加入干员

            Debug.Log($"✅ 成功添加干员：{unitName}");

            // 更新 UI
            InitializeCharacterSlots();
            RefreshActiveUnits();
            UIAudioManager.Instance?.PlayAddUnit();
        }
        else
        {
            Debug.LogWarning($"⚠️ Resources 中没有名为 {unitName} 的预制体，无法添加！");
        }
    }







    public void AddUnitFromUI(string unitName)
    {
        AddNewUnit(unitName);
    }

    public void AddRandomUnit()
    {
        // 1. 获取 Resources/Player 目录下的所有可用干员
        string path = "Player";
        GameObject[] allUnits = Resources.LoadAll<GameObject>(path);

        if (allUnits.Length == 0)
        {
            Debug.LogWarning("⚠️ Resources/Player 目录下没有找到任何干员预制体！");
            return;
        }

        // 2. 随机选择一个干员
        GameObject randomUnit = allUnits[UnityEngine.Random.Range(0, allUnits.Length)];
        string unitName = randomUnit.name; // 只获取名字

        // 3. 调用 AddNewUnit() 添加
        AddNewUnit(unitName);
        Debug.Log($"✅ 随机添加干员：{unitName}");
    }




    public void AddRandomUnitFromUI()
    {
        PlayerTeamManager.Instance.AddRandomUnit();
    }





    void PrintAllPlayerPrefs()
    {
        // 检查 PlayerPrefs 是否已经存储了 "AllUnitsList" 键
        if (PlayerPrefs.HasKey("AllUnitsList"))
        {
            // 获取存储的字符串（以逗号分隔的 unitName 列表）
            string storedUnitNames = PlayerPrefs.GetString("AllUnitsList");

            // 将字符串拆分为单个 unitName，并遍历输出
            foreach (string unitName in storedUnitNames.Split(','))
            {
                Debug.Log("Stored unitName: " + unitName);
            }
        }
        else
        {
            Debug.Log("没有找到 AllUnitsList 键，未存储任何干员信息！");
        }
    }


    public void DeleteUnit(GameObject unitPrefab)
    {
        if (unitPrefab == null) return;
        if (allUnitPrefabs.Contains(unitPrefab))
        {
            allUnitPrefabs.Remove(unitPrefab);
            
            if (selectedUnitPrefabs.Contains(unitPrefab))
            {
                selectedUnitPrefabs.Remove(unitPrefab);
            }
            Destroy(unitPrefab);
            Debug.Log($"✅ 删除干员：{unitPrefab.name}");
            
            // 更新UI（重新生成槽位、更新选中数量显示等）
            InitializeCharacterSlots();
            UpdateSelectedCountText();

            // 新增：更新存档数据，确保删除操作被持久化
            SaveAllUnits();
        }
    }





    public void UpgradeUnit(GameObject oldUnit, GameObject newUnitPrefab)
    {
        if (oldUnit == null || newUnitPrefab == null)
        {
            Debug.LogError("UpgradeUnit 参数为空！");
            return;
        }

        // 删除旧干员
        DeleteUnit(oldUnit);

        // 实例化新干员
        GameObject newInstance = Instantiate(newUnitPrefab, this.transform);
        newInstance.SetActive(false); // 初始状态保持未激活

        // 对新干员进行必要初始化（例如设置唯一ID、名字等）
        PlayerController playerController = newInstance.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.symbol.unitID = newUnitPrefab.name + "_" + System.Guid.NewGuid().ToString();
            playerController.symbol.unitName = newUnitPrefab.name;
        }

        // 将新干员添加到干员列表中
        allUnitPrefabs.Add(newInstance);

        // 更新 UI
        InitializeCharacterSlots();
        RefreshActiveUnits();

        Debug.Log($"升级干员成功，新干员：{newInstance.name}");
    }







#if UNITY_EDITOR
    [ContextMenu("Save All Units (Editor)")]
    void EditorSaveAllUnits()
    {
        // 在编辑器右键菜单里加个选项，点击后会在编辑器模式下执行
        SaveAllUnits();
        Debug.Log("在编辑器模式下手动保存了 allUnitPrefabs 到 PlayerPrefs");
    }
#endif
}

