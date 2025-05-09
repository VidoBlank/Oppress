using System;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    static GameData _Instance;
    public static GameData Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = new GameData();
            }
            return _Instance;
        }
    }
 
    public void Save()
    {
        PlayerPrefs.Save();
        // 在此实现数据保存逻辑，例如写入 PlayerPrefs 或文件
        Debug.Log("GameData: 数据已保存");
    }


    // 添加在 GameData 类的字段声明部分
    public Dictionary<string, Dictionary<string, string>> talentState;
    // RP 改变事件
    public delegate void RPChangedHandler(int newValue);
    public event RPChangedHandler OnRPChanged;
    AutoSaveData _gameData;

    public GameData()
    {
        _gameData = new AutoSaveData("Game");
        // 直接初始化为空列表，而不是读取存档数据
        _LevelPath = new List<S_GameLevelData>();
        talentState = new Dictionary<string, Dictionary<string, string>>();


    }




    public int LevelPathCount
    {
        get { return _LevelPath.Count; }
    }


    private int _rpCache = -1;

    public int RP
    {
        get
        {
            // 同步缓存与 PlayerPrefs
            _rpCache = _gameData.GetInt("RP");
            return _rpCache;
        }
        set
        {
            if (_rpCache != value)
            {
                _rpCache = value;
                _gameData.SetInt("RP", value);
                OnRPChanged?.Invoke(value); // ✅ 通知监听者
            }
        }
    }

    List<S_GameLevelData> _LevelPath;
    public IReadOnlyList<S_GameLevelData> LevelPath
    {
        get
        {
            return _LevelPath;
        }
    }


    public void PushLevelPath(int levelUid)
    {
        if (_LevelPath.Exists(p => p.levelUid == levelUid))
        {
            Debug.Log("PushLevelPath: 路径已存在，跳过添加 -> " + levelUid);
            return;
        }

        // 可以继续保存关卡数据，只是不保存关卡总数
        _LevelPath.Add(new S_GameLevelData(levelUid));
    }

    public void PopLevelPath()
    {
        if (_LevelPath.Count > 0)
        {
            var index = _LevelPath.Count - 1;
            var data = _LevelPath[index];
            data.DeleteValues();
            _LevelPath.RemoveAt(index);
        }
    }
    public void update()
    {
        Debug.Log(_LevelPath.Count);

    }
    public S_GameLevelData GetLevelData(int levelUid)
    {
        // 如果在 _LevelPath 里有这个关卡，就返回
        for (int i = 0; i < _LevelPath.Count; i++)
        {
            if (_LevelPath[i].levelUid == levelUid)
            {
                return _LevelPath[i];
            }
        }
        // 否则新建一个关卡数据
        var newData = new S_GameLevelData(levelUid);
        // 只有当该节点已通过时，才将它加入路径
        if (newData.IsPassed)
        {
            _LevelPath.Add(newData);
            _gameData.SetInt("LevelPathCount", _LevelPath.Count);
        }
        return newData;
    }


    public void ResetSaveDataCompletely()
    {
        PlayerPrefs.DeleteAll(); // 删光所有键值
        PlayerPrefs.Save();

        _LevelPath.Clear();  // 清空路径
        _Instance = null;

        // ✅ 清空干员存档
        if (PlayerTeamManager.Instance != null)
        {
            PlayerTeamManager.Instance.ClearAllUnits(); // 调用 `ClearAllUnits()` 清空干员
        }

        Debug.Log("存档已重置，所有干员已删除，等待 Main 处理节点状态");
    }
    public void Nextlevel()
    {
        // 先保存 CurrentStage 的值
        int currentStageValue = PlayerPrefs.GetInt("CurrentStage", 1);



        // 恢复 CurrentStage
        PlayerPrefs.SetInt("CurrentStage", currentStageValue);
        PlayerPrefs.Save();


        Debug.Log("进入下一阶段，节点状态已重置。");
    }






}

public class S_GameLevelData
{
    AutoSaveData _saveData;

    public int levelUid { get; private set; }

    public S_GameLevelData(int levelUid)
    {
        this.levelUid = levelUid;
        _saveData = new AutoSaveData($"Level_{levelUid}");
    }

    public float Percent
    {
        get
        {
            return _saveData.GetFloat("Percent");
        }
        set
        {
            _saveData.SetFloat("Percent", value);
        }
    }
    ////////// 存储节点状态///////////
    public bool IsPass
    {
        get { return _saveData.GetBool("IsPass"); }
        set { _saveData.SetBool("IsPass", value); }
    }


    public bool InPath
    {
        get { return _saveData.GetBool("InPath"); }
        set { _saveData.SetBool("InPath", value); }
    }




    public bool IsSelectable
    {
        get { return _saveData.GetBool("IsSelectable"); }
        set { _saveData.SetBool("IsSelectable", value); }
    }



    ////////////////////

    public bool IsPassed
    {
        get
        {
            return _saveData.GetBool("IsPassed");
        }
        set
        {
            _saveData.SetBool("IsPassed", value);
        }
    }

    public void DeleteValues()
    {
        _saveData.DeleteAll();
    }





}

//角色数据
[System.Serializable]
public class NameListWrapper
{
    public List<string> names;
    public NameListWrapper(List<string> list)
    {
        names = list;
    }
}
[System.Serializable]


public class UnitData
{
    public string unitName;
    public string unitID;
    public float health;
    public float energy;
    public int level;
    public List<string> slotEffects;    // 存储插槽效果
    public bool hasVersatileTalent;     // 是否拥有多面手天赋
    public string equippedSkillID; // 新增字段
    public string overrideName; // ✅ 存储自定义显示名称


    // 新增：存储角色所有基本属性，包括视野
    public AttributeData attributeData;

    // 更新构造函数
    public UnitData(string name, string id, float hp, float en, int lvl, List<string> slots, bool versatileTalent, AttributeData attributeData)
    {
        unitName = name;
        unitID = id;
        health = hp;
        energy = en;
        level = lvl;
        slotEffects = slots ?? new List<string>();
        hasVersatileTalent = versatileTalent;
        this.attributeData = attributeData;
    }
}

[System.Serializable]
public class UnitDataWrapper
{
    public List<UnitData> units;

    public UnitDataWrapper(List<UnitData> units)
    {
        this.units = units;
    }
}
[System.Serializable]
public class AttributeData
{
    public float maxhealth;
    public float maxEnergy;
    public float knockdownHealth;
    public float interactspeed;
    public float recoveredspeed;
    public float movingSpeed;
    public float interactionRange;
    public int damage;
    public float attackDelay;
    public float bulletSpeed;
    public float damageTakenMultiplier;
    public float flatDamageReduction;
    public float skillCooldownRate;
    public float visionRadius; // 新增：角色视野半径

    // 可选：提供一个构造函数，直接从 PlayerController.Attribute 和 visionRadius 复制数据
    public AttributeData(PlayerController.Attribute attr, float visionRadius)
    {
        this.maxhealth = attr.maxhealth;
        this.maxEnergy = attr.maxEnergy;
        this.knockdownHealth = attr.knockdownHealth;
        this.interactspeed = attr.interactspeed;
        this.recoveredspeed = attr.recoveredspeed;
        this.movingSpeed = attr.movingSpeed;
        this.interactionRange = attr.interactionRange;
        this.damage = attr.damage;
        this.attackDelay = attr.attackDelay;
        this.bulletSpeed = attr.bulletSpeed;
        this.damageTakenMultiplier = attr.damageTakenMultiplier;
        this.flatDamageReduction = attr.flatDamageReduction;
        this.skillCooldownRate = attr.skillCooldownRate;
        this.visionRadius = visionRadius;
    }

}
