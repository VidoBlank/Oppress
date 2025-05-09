using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;

public class SupplyStation : MonoBehaviour
{
    public static SupplyStation Instance { get; private set; }

    #region Inspector Fields

    [Header("恢复设置")]
    [Range(0f, 1f)]
    public float healthRestorePercent = 0.2f;
    [Range(0f, 1f)]
    public float energyRestorePercent = 0.2f;

    [Header("延时设置")]
    public float delayBeforeRecovery = 1.0f;

    [Header("UI 提示")]
    public BootSequenceTyper typer;
    public float messageDisplayDuration = 1.5f;

    [Header("指定要寻找的 Typer 对象名（场景内）")]
    public string typerObjectName = "SupplyMessageTyper";

    #endregion

    private AudioSource typerAudioSource;

    #region MonoBehaviour Methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindTyper();
    }

    #endregion

    #region Typer Binding

    /// <summary>
    /// 尝试查找并绑定场景中指定名称的 BootSequenceTyper 以及其 AudioSource。
    /// </summary>
    private void FindTyper()
    {
        if (typer == null && !string.IsNullOrEmpty(typerObjectName))
        {
            GameObject found = GameObject.Find(typerObjectName);
            if (found != null)
            {
                typer = found.GetComponent<BootSequenceTyper>();
                typerAudioSource = found.GetComponent<AudioSource>();

                if (typer != null)
                {
                    Debug.Log($"✅ SupplyStation: 成功绑定指定 Typer 对象 [{typerObjectName}]");
                }
                else
                {
                    Debug.LogWarning($"⚠️ SupplyStation: 找到对象 [{typerObjectName}]，但未挂 BootSequenceTyper 组件");
                }

                if (typerAudioSource == null)
                {
                    Debug.LogWarning($"⚠️ SupplyStation: [{typerObjectName}] 没有找到 AudioSource 组件");
                }
            }
            else
            {
                Debug.LogWarning($"❌ SupplyStation: 找不到名为 [{typerObjectName}] 的对象！");
            }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 外部调用入口，触发补给站效果。
    /// </summary>
    public void ApplySupplyEffect()
    {
        StartCoroutine(DelayedSupply());
    }

    #endregion

    #region Supply Effect Coroutine

    /// <summary>
    /// 延时执行补给效果，更新单位状态并显示 UI 提示信息。
    /// </summary>
    private IEnumerator DelayedSupply()
    {
        yield return new WaitForSeconds(delayBeforeRecovery);

        if (typer == null)
        {
            FindTyper();
        }

        if (PlayerTeamManager.Instance == null)
        {
            Debug.LogWarning("SupplyStation: PlayerTeamManager.Instance 为空！");
            yield break;
        }

        var teamManager = PlayerTeamManager.Instance;
        List<GameObject> allUnits = teamManager.allUnitPrefabs;
        List<GameObject> selectedUnits = teamManager.selectedUnitPrefabs;

        bool hasRestored = false;

        foreach (GameObject unit in allUnits)
        {
            if (unit == null || selectedUnits.Contains(unit))
                continue;

            PlayerController pc = unit.GetComponent<PlayerController>();
            if (pc != null)
            {
                // ✅ 修改：使用 TotalMaxHealth / TotalMaxEnergy 判断是否需要补给
                bool needRestore = pc.health < pc.TotalMaxHealth || pc.energy < pc.TotalMaxEnergy;
                if (!needRestore) continue;

                // ✅ 修改：使用 TotalMaxXXX 计算补给值
                float restoreHealth = pc.TotalMaxHealth * healthRestorePercent;
                float restoreEnergy = pc.TotalMaxEnergy * energyRestorePercent;

                pc.RecoverHealth(restoreHealth);
                pc.RestoreEnergy(restoreEnergy);

                if (teamManager.unitSlotMap.TryGetValue(pc, out CharacterSlot slot))
                {
                    slot.PlayRecoveryEffect();
                }

                hasRestored = true;
                yield return new WaitForSeconds(0.3f);
            }
        }


        if (hasRestored && typer != null && typer.textComponent != null)
        {
            typer.textComponent.gameObject.SetActive(true);

            if (typerAudioSource != null)
            {
                typerAudioSource.Play();
            }

            typer.ShowBootAndDescription("<b><color=#00FFFF>补给站提示：</color></b>\n待命的干员状态已恢复。");

            yield return new WaitForSeconds(messageDisplayDuration);

            typer.textComponent.gameObject.SetActive(false);
        }
    }

    #endregion
}
