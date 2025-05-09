using UnityEngine;

public class PlayerButtonHandler : MonoBehaviour
{
    private PlayerController player;
    private PlayerManager playerManager;

    // 初始化方法
    public void Initialize(PlayerManager manager, PlayerController player)
    {
        this.playerManager = manager;
        this.player = player;
    }

    // 按钮点击事件
    public void OnButtonClick()
    {
        int playerIndex = playerManager.players.IndexOf(player);
        if (playerIndex >= 0)
        {
            Debug.Log($"按钮被点击，调用 PlayerManager.SelectSinglePlayer({playerIndex}) - 玩家：{player.name}");
            playerManager.SelectSinglePlayer(playerIndex);

            // ✅ 根据是否启用自动聚焦决定是否跳转相机
            if (CameraController.enableAutoFocusOnClick && CameraController.instance != null)
            {
                CameraController.instance.FocusOnPlayer(player);
            }
        }
        else
        {
            Debug.LogError("无法找到玩家索引，玩家可能已被移除！");
        }
    }

}
