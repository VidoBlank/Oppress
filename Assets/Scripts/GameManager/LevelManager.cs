using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Michsky.UI.Dark;

public class LevelManager : MonoBehaviour
{
    public Transform startPoint;
    public Transform[] respawnPoints;

    public Button deployButton;
    public Button returnButton;
    public string returnSceneName;

    private List<GameObject> activeUnits = new List<GameObject>();

    private void Start()
    {
        Camera.main.transform.position = new Vector3(startPoint.position.x, startPoint.position.y, Camera.main.transform.position.z);
        if (deployButton != null)
        {
            deployButton.gameObject.SetActive(true);
            deployButton.onClick.AddListener(OnDeployButtonClicked);
        }
        if (returnButton != null)
        {
            returnButton.gameObject.SetActive(true);
            returnButton.onClick.AddListener(OnReturnButtonClicked);
        }
    }

    private void OnDeployButtonClicked()
    {
        EnableSelectedUnits();
        returnButton.gameObject.SetActive(false);
        deployButton.gameObject.SetActive(false);
    }

    private void OnReturnButtonClicked()
    {
        if (!string.IsNullOrEmpty(returnSceneName))
        {
            SceneManager.LoadScene(returnSceneName);
        }
        else
        {
            Debug.LogWarning("未设置返回场景名称！");
        }
    }

    private void EnableSelectedUnits()
    {
        if (PlayerTeamManager.Instance == null)
        {
            Debug.LogError("⚠️ PlayerTeamManager.Instance 为空，无法启用干员！");
            return;
        }
        List<GameObject> selectedUnits = PlayerTeamManager.Instance.selectedUnitPrefabs;
        if (selectedUnits.Count == 0)
        {
            Debug.LogWarning("⚠️ 没有选中的干员，关卡不会生成单位！");
            return;
        }
        int unitsToSpawn = Mathf.Min(selectedUnits.Count, 4);
        for (int i = 0; i < unitsToSpawn; i++)
        {
            GameObject unit = selectedUnits[i];
            if (unit == null) continue;
            unit.SetActive(true);
            unit.SetActive(false);

            Vector3 spawnPosition;
            if (respawnPoints != null && respawnPoints.Length > i && respawnPoints[i] != null)
            {
                // 使用重生点位置，但确保z轴为0
                spawnPosition = new Vector3(
                    respawnPoints[i].position.x,
                    respawnPoints[i].position.y,
                    0f  // 强制z轴为0
                );
            }
            else
            {
                // 使用随机位置，但确保z轴为0
                spawnPosition = new Vector3(
                    startPoint.position.x + Random.Range(-2f, 2f),
                    startPoint.position.y + Random.Range(-2f, 2f),
                    0f  // 强制z轴为0
                );
            }

            unit.transform.position = spawnPosition;
            unit.SetActive(true);

            if (unit.GetComponent<PlayerHighlight>() == null)
            {
                unit.AddComponent<PlayerHighlight>();
                Debug.Log($"✅ 为 {unit.name} 添加了 PlayerHighlight 组件");
            }

            var player = unit.GetComponent<PlayerController>();
            if (player != null)
            {
                player.isInBattle = true;
                player.ResetSlotEffectStates();
            }

            if (!activeUnits.Contains(unit))
            {
                activeUnits.Add(unit);
            }
        }
        Debug.Log($"✅ 已启用 {activeUnits.Count} 名干员进入关卡！");
    }
    // 修改后的 OnLevelComplete 方法：
    public void OnLevelComplete()
    {
        Debug.Log("🎉 关卡完成！");
        int currentNodeUid = Main.currentLevelUid;
        if (currentNodeUid >= 0)
        {
            var levelData = GameData.Instance.GetLevelData(currentNodeUid);
            levelData.IsPassed = true;
            Main.levelCompleted = true;

            // 加资源
            GameData.Instance.RP += LevelNode.currentRewardRP;
            Debug.Log($"获得奖励 RP：{LevelNode.currentRewardRP}，当前总RP：{GameData.Instance.RP}");

          
        }

        HandlePostBattleStatus(success: true);

        if (SupplyStation.Instance != null)
            SupplyStation.Instance.ApplySupplyEffect();

        if (!string.IsNullOrEmpty(returnSceneName))
            SceneManager.LoadScene(returnSceneName);
    }


    public void OnLevelFailed()
    {
        Debug.Log("❌ 关卡失败！");
        HandlePostBattleStatus(success: false);

        if (SupplyStation.Instance != null)
            SupplyStation.Instance.ApplySupplyEffect();

        if (!string.IsNullOrEmpty(returnSceneName))
            SceneManager.LoadScene(returnSceneName);
    }
    private void HandlePostBattleStatus(bool success)
    {
        var teamMgr = PlayerTeamManager.Instance;
        if (teamMgr == null) return;

        List<GameObject> unitsToRemove = new List<GameObject>();

        foreach (var unit in teamMgr.selectedUnitPrefabs)
        {
            if (unit == null) continue;

            var pc = unit.GetComponent<PlayerController>();
            if (pc == null) continue;

            pc.isInBattle = false;
            pc.ResetSlotEffectStates();
            unit.SetActive(false);

            if (success)
            {
                if (pc.isDead)
                {
                    unitsToRemove.Add(unit);
                }
                else if (pc.isKnockedDown)
                {
                    pc.RecoverFromKnockdown();
                }
            }
            else
            {
                // ❌ 关卡失败：所有出战单位移除
                unitsToRemove.Add(unit);
            }
        }

        // 清理队伍
        foreach (var unit in unitsToRemove)
        {
            teamMgr.DeleteUnit(unit);
        }

        
        activeUnits.Clear();
    }

    private void DisableActiveUnits()
    {
        foreach (var unit in activeUnits)
        {
            if (unit != null)
            {
                var player = unit.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.isInBattle = false;
                    player.ResetSlotEffectStates();
                    unit.SetActive(false);
                }
            }
        }
        activeUnits.Clear();
    }
}
