using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


/*
 * 管理所有角色的状态、行为、UI
 */
public class PlayerManager : MonoBehaviour
{
    public List<PlayerController> players = new List<PlayerController>();   //所有角色的集合
    public List<PlayerController> selectedPlayers = new List<PlayerController>();     //被选中角色的集合

    PlayerUIManager playerUI;   //角色UI管理器
    private PlayerInput playerInput;
    public static PlayerManager instance;

    public GameObject actionTarget;     //行动目标
    public Vector3? targetPosition;     //移动的目标位置

    public int highlightedPlayerIndex = -1;     //被突出角色的在被选中角色中的序号

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;  // 如果没有实例，创建一个单例
        }
        else
        {
            Destroy(gameObject);  // 确保单例存在时不创建新的实例
        }
        playerUI = GetComponent<PlayerUIManager>();
        playerInput = FindObjectOfType<PlayerInput>();
        
    }


    private void Start()
    {
        // 检测并添加场景中所有已存在的玩家
        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        foreach (var player in allPlayers)
        {
            if (!players.Contains(player))
            {
                AddPlayer(player);
                Debug.Log($"在 Start 中添加玩家: {player.name}");
            }
        }
    }

    private void Update()
    {
        
    }

    /// <summary>
    /// 添加角色
    /// </summary>
    /// <param name="player">目标角色</param>
    public void AddPlayer(PlayerController player)
    {
        players.Add(player);
    }

    /// <summary>
    /// 移除角色
    /// </summary>
    /// <param name="player">目标角色</param>
    public void RemovePlayer(PlayerController player)
    {
        players.Remove(player);
    }

    /// <summary>
    /// 选择角色
    /// </summary>
    /// <param name="player">目标角色</param>
    public void SelectPlayer(PlayerController player)
    {
        if (selectedPlayers.Contains(player)) return;

        // 如果角色不可选中（死亡或倒地），跳过
        if (player == null || player.isDead || player.isKnockedDown) return;

        // 显示选中效果并添加到列表
        playerUI.SelectPlayer(player);
        selectedPlayers.Add(player);

        // 同步到 PlayerInput
        if (!playerInput.selectedPlayers.Contains(player))
        {
            playerInput.selectedPlayers.Add(player);
        }

        Debug.Log($"角色 {player.name} 已被选中");

        // 更新高亮
        playerUI.HighlightSelectedPlayers(selectedPlayers, highlightedPlayerIndex);
    }
    /// <summary>
    /// 选择角色
    /// </summary>
    /// <param name="index">目标角色的序号</param>
    public void SelectPlayer(int index)
    {
        if (index < 0 || index >= players.Count)
        {
            Debug.LogWarning($"无效的角色索引: {index}");
            return;
        }
        PlayerController player = players[index];
        //如果角色已经被选择，跳过
        if (selectedPlayers.Contains(player))
        {
            return;
        }
        //如果没有被高亮的角色，就把这个角色高亮
        if (highlightedPlayerIndex == -1)
        {
            HighlightPlayer(player);
            highlightedPlayerIndex = 0;
        }
        SelectPlayer(player);
    }

    /// <summary>
    /// 选择单个角色
    /// </summary>
    /// <param name="index">目标角色的序号</param>
    public void SelectSinglePlayer(int index)
    {
        if (index < 0 || index >= players.Count)
        {
            Debug.LogWarning($"无效的角色索引: {index}");
            return;
        }
        //先取消选择当前所有角色
        for (int i = selectedPlayers.Count - 1; i >= 0; i--)
        {
            PlayerController player = selectedPlayers[i];
            UnselectPlayer(player);
        }
        PlayerController player1 = players[index];
        HighlightPlayer(player1);
        highlightedPlayerIndex = 0;
        SelectPlayer(player1);
    }

    /// <summary>
    /// 突出角色
    /// </summary>
    /// <param name="player">目标角色</param>
    public void HighlightPlayer(PlayerController player)
    {
        //如果角色已经死亡或倒地，则无法突出
        if (player == null || player.isDead || player.isKnockedDown)
        {
            return;
        }
        playerUI.HightlightPlayer(player);
        // 更新 UI 高亮
        //if (playerUI)
        //    playerUI.HighlightSelectedPlayers(selectedPlayers, highlightedPlayerIndex);
    }


    /// <summary>
    /// 突出角色
    /// </summary>
    /// <param name="index">目标角色在被选中角色中的序号</param>
    public void HighlightPlayer(int index)
    {
        if (index < 0 || index < selectedPlayers.Count)
        {
            Debug.LogWarning($"无效的角色索引: {index}");
            return;
        }
        //如果角色还未被突出，则突出角色
        if (index != highlightedPlayerIndex)
        {
            PlayerController player = selectedPlayers[index];
            PlayerController unhighlightedPlayer = selectedPlayers[highlightedPlayerIndex];
            HighlightPlayer(player);
            UnhighlightPlayer(unhighlightedPlayer);     //取消原来被突出的角色
        }
    }

    /// <summary>
    /// 切换突出的角色
    /// </summary>
    public void HighlightNextPlayer()
    {
        if (selectedPlayers.Count <= 1)
        {
            Debug.Log("只有一个角色被选中，无法切换高亮。");
            return;
        }

        PlayerController currentPlayer = selectedPlayers[highlightedPlayerIndex];
        Debug.Log($"当前高亮角色: {currentPlayer.name}, 索引: {highlightedPlayerIndex}");
        //目标原角色停止正在释放的技能
        currentPlayer.EndSkill();
        currentPlayer.CancelSkill();
        currentPlayer.isPreparingSkill = false;
        currentPlayer.skillRange.SetActive(false);

        int tempIndex = highlightedPlayerIndex;
        UnhighlightPlayer(currentPlayer); // 取消原突出角色

        tempIndex = (tempIndex + 1) % selectedPlayers.Count; // 循环索引
        highlightedPlayerIndex = tempIndex;

        PlayerController nextPlayer = selectedPlayers[highlightedPlayerIndex];
        Debug.Log($"切换到新高亮角色: {nextPlayer.name}, 新索引: {highlightedPlayerIndex}");
        HighlightPlayer(nextPlayer);

        // 通知 UI 更新
        if (playerUI != null)
        {
            playerUI.HighlightSelectedPlayers(selectedPlayers, highlightedPlayerIndex);
        }
        else
        {
            Debug.LogWarning("PlayerUIManager 未找到，无法更新高亮。");
        }
    }




    /// <summary>
    /// 取消选择角色
    /// </summary>
    /// <param name="player">目标角色</param>
    public void UnselectPlayer(PlayerController player)
    {
        if (!selectedPlayers.Contains(player)) return;

        // 如果角色在准备技能阶段，取消技能
        if (player.isPreparingSkill)
        {
            player.CancelSkill();
        }
        UnselectActionTarget();
        UnselectTargetPosition();
        UnhighlightPlayer(player);
        playerUI.UnselectPlayer(player);
        selectedPlayers.Remove(player);

        // 同步到 PlayerInput
        if (playerInput.selectedPlayers.Contains(player))
        {
            playerInput.selectedPlayers.Remove(player);
        }

        Debug.Log($"角色 {player.name} 已取消选中");

        // 更新高亮
        playerUI.HighlightSelectedPlayers(selectedPlayers, highlightedPlayerIndex);
    }

    /// <summary>
    /// 取消突出角色
    /// </summary>
    /// <param name="player">目标角色</param>
    public void UnhighlightPlayer(PlayerController player)
    {
        //如果目标角色已被突出，则取消突出
        if (highlightedPlayerIndex >= 0 && player == selectedPlayers[highlightedPlayerIndex])
        {
            playerUI.UnhightlightPlayer(player);
            highlightedPlayerIndex = -1;
        }
    }
    

    /// <summary>
    /// 用框选的方式选择角色
    /// </summary>
    /// <param name="colliders">被框选的碰撞体</param>
    public void SelectPlayersByBox(Collider2D[] colliders)
    {
        //先取消选择当前所有角色
        for (int index = selectedPlayers.Count - 1; index >= 0; index--)
        {
            PlayerController player = selectedPlayers[index];
            UnselectPlayer(player);
        }

        highlightedPlayerIndex = -1;

        foreach (Collider2D collider in colliders)
        {
            if (collider.tag == "Player")
            {
                PlayerController player = collider.GetComponent<PlayerController>();

                // 检查角色是否死亡，若角色死亡，将其移除 player 列表
                if (player != null && !player.isDead && !player.isKnockedDown)
                {
                    if (selectedPlayers.Count == 0) // 显示第一个角色的技能
                    {
                        HighlightPlayer(player);
                        highlightedPlayerIndex = 0;
                    }
                    if (selectedPlayers.Count < 4) // 限制最多只能选中4名玩家
                    {
                        SelectPlayer(player);
                    }
                    else
                    {
                        Debug.Log("已达到最多选中玩家的上限（4名玩家）");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 控制选中角色执行移动到目标点行动
    /// </summary>
    /// <param name="targetPosition">目标位置</param>
    public void MoveToTarget(Vector3? targetPosition)
    {
        if (targetPosition.HasValue)
        {
            foreach (PlayerController player in selectedPlayers)
            {
                player.InterruptAction();   //打断当前行动
                player.MoveToTarget(targetPosition.Value);
            }
            SelectTargetPosition(targetPosition.Value);
        }
    }


    /// <summary>
    /// 选中行动的目标
    /// </summary>
    /// <param name="target">目标物体</param>
    public void SelectActionTarget(GameObject target)
    {
        if(target == null)
        {
            return;
        }
        UnselectActionTarget();
        actionTarget = target;
        //playerUI.SelectActionTarget(target);    //更新UI
    }

    /// <summary>
    /// 取消选中行动的目标
    /// </summary>
    public void UnselectActionTarget()
    {
        if(actionTarget == null)
        {
            return;
        }
        //playerUI.UnselectActionTarget(actionTarget);  //更新UI
        actionTarget = null;
    }

    /// <summary>
    /// 取消选中行动的目标
    /// </summary>
    /// <param name="player">目标角色</param>
    public void UnselectActionTarget(PlayerController player)
    {
        if (!selectedPlayers.Contains(player)) return;
        if (actionTarget == null)
        {
            return;
        }
        //playerUI.UnselectActionTarget(actionTarget);  //更新UI
        actionTarget = null;
    }


    /// <summary>
    /// 取消选中行动的目标
    /// </summary>
    /// <param name="player">目标角色</param>
    public void UnselectActionTarget(GameObject target)
    {
        if (actionTarget != target) return;
        if (actionTarget == null)
        {
            return;
        }
        //playerUI.UnselectActionTarget(actionTarget);  //更新UI
        actionTarget = null;
    }


    /// <summary>
    /// 选中移动的目标位置
    /// </summary>
    /// <param name="position">目标位置</param>
    public void SelectTargetPosition(Vector3 position)
    {
        targetPosition = position;
        //playerUI.SelectTargetPosition(position);    //更新UI
    }

    /// <summary>
    /// 取消移动的目标位置
    /// </summary>
    public void UnselectTargetPosition()
    {
        if(targetPosition == null)
        {
            return;
        }
        //playerUI.UnselectTargetPosition(targetPosition.Value);  //更新UI
        targetPosition = null;
    }


    /// <summary>
    /// 取消移动的目标位置
    /// </summary>
    /// <param name="player">目标角色</param>
    public void UnselectTargetPosition(PlayerController player)
    {
        if (!selectedPlayers.Contains(player)) return;
        if (targetPosition == null)
        {
            return;
        }
        //playerUI.UnselectTargetPosition(targetPosition.Value);  //更新UI
        targetPosition = null;
    }

    /// <summary>
    /// 对目标进行行动
    /// </summary>
    /// <param name="targetCollider">目标对象的碰撞体</param>
    /// <returns>如果可以对目标进行行动，返回true,否则返回false</returns>
    public bool ActToTarget(Collider2D targetCollider)
    {
        bool canAct = false;    //判断是否可以对目标进行交互
        // 当目标对象不为空时，执行与目标对象的交互行为
        if(targetCollider)
        {
            // 如果目标对象为伤员，执行救援行为
            if(targetCollider.tag == "Player")
            {
                PlayerController targetPlayer = targetCollider.GetComponent<PlayerController>();
                if (targetPlayer && targetPlayer.isKnockedDown)
                {
                    canAct = true;
                }
            }
            // 如果目标对象为可交互物体，执行交互行为
            else if (targetCollider.tag == "Object" || targetCollider.tag == "Enemy" || targetCollider.tag == "Obstacle")
            {
                canAct = true;
            }
        }
        if(canAct)
        {
            foreach (PlayerController player in selectedPlayers)
            {
                player.InterruptAction();   //打断当前行动
                player.ActToTarget(targetCollider.gameObject);
            }
            SelectActionTarget(targetCollider.gameObject);      //选中行动目标
        }
        return canAct;
    }
    /// <summary>
    /// 对目标进行行动（支持 3D Collider，自动上溯查找 Collider2D）
    /// </summary>
    /// <param name="targetCollider">命中的 3D Collider</param>
    /// <returns>是否成功执行行动</returns>
    public bool ActToTarget(Collider targetCollider)
    {
        if (targetCollider == null) return false;

        // 向上查找拥有 Collider2D 的父对象
        Collider2D linkedCollider2D = targetCollider.GetComponentInParent<Collider2D>();
        if (linkedCollider2D == null)
        {
            Debug.LogWarning($"[ActToTarget] 找不到 {targetCollider.name} 的关联 Collider2D，交互失败");
            return false;
        }

        return ActToTarget(linkedCollider2D); // 使用已有的 2D 逻辑继续处理
    }

    /// <summary>
    /// 控制角色使用技能
    /// </summary>
    /// <param name="index">技能编号</param>
    public void UseSkill()
    {
        // 确保有选中的玩家
        if (selectedPlayers.Count == 0) return;

        if (highlightedPlayerIndex < 0 || highlightedPlayerIndex >= selectedPlayers.Count)
        {
            Debug.LogWarning($"无效的角色索引: {highlightedPlayerIndex}");
            return;
        }

        PlayerController activePlayer = selectedPlayers[highlightedPlayerIndex]; // 当前技能栏显示的玩家

        activePlayer.UseSkill();
    }



    void UpdateSelectionEffect()
    {
        // 遍历所有玩家头像框并重置效果
        foreach (var frame in playerUI.playerFrames.Values)
        {
            frame.transform.localScale = Vector3.one;
            frame.GetComponent<Image>().DOColor(Color.white, 0.2f); // 重置颜色
        }

        // 给选中的玩家头像框增加效果
        foreach (PlayerController player in selectedPlayers)
        {
            if (playerUI.playerFrames.TryGetValue(player, out GameObject frame))
            {
                frame.transform.DOScale(1.1f, 0.2f); // 放大效果
                frame.GetComponent<Image>().DOColor(Color.yellow, 0.2f); // 泛光效果
            }
        }
    }






}
