using System.Collections.Generic;
using UnityEngine;

public class PlayerHighlight : MonoBehaviour
{
    private PlayerInput playerInput; // 引用 PlayerInput 脚本
    private PlayerManager playermanager;
    private Dictionary<GameObject, Color> originalOutlineColors = new Dictionary<GameObject, Color>(); // 存储原始颜色

    void Start()
    {
        // 在场景加载时自动查找 PlayerInput 脚本
        playerInput = FindObjectOfType<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("PlayerInput 脚本未找到，请确保场景中存在 PlayerInput 组件。");
        }
        playermanager = FindObjectOfType<PlayerManager>();
    }

    void Update()
    {
        if (playermanager == null) return;

        // 处理选中玩家的高亮
        HandleHighlight();
        // 更新所有玩家的轮廓颜色
        UpdateOutlineColors();
    }

    public void HandleHighlight()
    {
        // 高亮选中玩家
        foreach (var player in playerInput.selectedPlayers)
        {
            Outline outline = player.gameObject.GetComponentInChildren<Outline>();
            if (outline != null)
            {
                outline.enabled = true;
                outline.OutlineWidth = 1.5f; // 高亮宽度
                SetOutlineAlpha(player.gameObject, 0.65f); // 高亮透明度
            }
            else
            {
                Debug.LogWarning($"未找到 Outline 组件: {player.name}");
            }
        }
        if (playerInput.selectedPlayers.Count > 1
       && PlayerManager.instance.highlightedPlayerIndex >= 0
       && PlayerManager.instance.highlightedPlayerIndex < playerInput.selectedPlayers.Count)
        {
            PlayerController highlightedPlayer = playerInput.selectedPlayers[PlayerManager.instance.highlightedPlayerIndex];
            Outline outline = highlightedPlayer.gameObject.GetComponentInChildren<Outline>();
            if (outline != null)
            {
                outline.OutlineWidth = 1.75f; // 更高的宽度
                SetOutlineAlpha(highlightedPlayer.gameObject, 0.85f); // 更强的高亮透明度
            }
            else
            {
                Debug.LogWarning($"未找到 Outline 组件: {highlightedPlayer.name}");
            }
        }
        // 还原未选中的玩家
        foreach (var player in playerInput.playersInScene)
        {
            if (!playerInput.selectedPlayers.Contains(player))
            {
                Outline outline = player.gameObject.GetComponentInChildren<Outline>();
                if (outline != null)
                {
                    outline.OutlineWidth = 1f; // 默认宽度
                    SetOutlineAlpha(player.gameObject, 0.35f); // 默认透明度
                }
            }
        }
    }


    // 更新所有玩家的轮廓颜色，使用插值实现平滑过渡
    private void UpdateOutlineColors()
    {
        foreach (PlayerController playerController in FindObjectsOfType<PlayerController>())
        {
            Outline outline = playerController.GetComponentInChildren<Outline>();
            if (outline != null)
            {
                if (playerInput.players.Contains(playerController.gameObject))
                {
                    // 玩家已选中，跳过颜色更新
                    continue;
                }

                if (playerController.isDead)
                {
                    outline.enabled = false;
                    continue;
                }

                float currentHealth = playerController.health;
                float maxHealth = playerController.attribute.maxhealth;

                Color targetColor = GetOutlineColor(currentHealth, maxHealth);
                // 检查倒地状态
                if (playerController.isKnockedDown)
                {
                    HighlightDownedPlayer(playerController.gameObject);
                    continue;
                }
              
                Color newColor = targetColor;
                newColor.a = outline.OutlineColor.a;

                outline.OutlineColor = Color.Lerp(outline.OutlineColor, newColor, Time.deltaTime * 5);
            }
        }
    }
    private void HighlightDownedPlayer(GameObject player)
    {
        Outline outline = player.GetComponentInChildren<Outline>();
        if (outline != null)
        {
          
            outline.OutlineColor = new Color(0.75f, 0, 0, 1); // 倒地为红色
            outline.OutlineWidth = 2f;     // 增加宽度
        }
    }
    // 根据生命值设置目标 Outline 颜色
    private Color GetOutlineColor(float currentHealth, float maxHealth)
    {
        float healthPercentage = currentHealth / maxHealth;

        // 使用 Lerp 函数在黄色和深红色之间进行插值
        Color fullHealthColor = Color.yellow;
        Color lowHealthColor = new Color(0.75f, 0, 0); // 深红色
        return Color.Lerp(lowHealthColor, fullHealthColor, healthPercentage);
    }

    // 设置 Outline 组件的 alpha 值
    private void SetOutlineAlpha(GameObject player, float alpha)
    {
        Outline outline = player.GetComponentInChildren<Outline>();
        if (outline != null)
        {
            if (!originalOutlineColors.ContainsKey(player))
            {
                originalOutlineColors[player] = outline.OutlineColor; // 保存原始颜色
            }
            Color newColor = outline.OutlineColor;
            newColor.a = alpha;
            outline.OutlineColor = newColor;
        }
    }
}
