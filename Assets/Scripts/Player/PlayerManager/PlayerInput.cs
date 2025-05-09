using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using System;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput instance;

    //以下两个变量移到PlayerManager里面了，后续会删除
    [SerializeField] public List<PlayerController> playersInScene = new List<PlayerController>();
    [SerializeField] public List<GameObject> players = new List<GameObject>();

    public bool onDrawingRect; // 是否正在画框

    private Vector3 startPoint; // 框的起始点，即按下鼠标左键时指针的位置
    private Vector3 currentPoint; // 在拖移过程中，玩家鼠标指针所在的实时位置
    private Vector3 endPoint; // 框的终止点，即放开鼠标左键时指针的位置

    private float minBoxSize = 0.05f;   //鼠标拖选框的最小大小，小于这个值则视为点选
    private PlayerController playerController;

    private float interactionRange = 2.0f;//普通交互距离
    private float exinteractionRange = 1.0f;//特殊交互距离

    public List<PlayerController> selectedPlayers = new List<PlayerController>();

    private PlayerUIManager playerUIManager;

    public int currentSkillBarIndex = 0; // 当前显示技能栏的角色索引

    private PlayerManager playerManager;

    void Start()
    {
        // 检测并添加场景中所有已存在的玩家
        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        foreach (var player in allPlayers)
        {
            if (!playersInScene.Contains(player))
            {
                playersInScene.Add(player);
                Debug.Log($"在 Start 中添加玩家: {player.name}");
            }
        }
    }

    private void OnEnable()
    {
        PlayerController.OnPlayerAdded += AddPlayerToScene;
        PlayerController.OnPlayerRemoved += RemovePlayerFromScene;

    }

    private void OnDisable()
    {
        PlayerController.OnPlayerAdded -= AddPlayerToScene;
        PlayerController.OnPlayerRemoved -= RemovePlayerFromScene;
    }

    private void AddPlayerToScene(PlayerController player)
    {
        if (!playersInScene.Contains(player))
        {
            playersInScene.Add(player);
            playersInScene.Sort((a, b) => a.symbol.ID.CompareTo(b.symbol.ID)); // 按ID排序
        }

        // 同步到 PlayerManager
        if (!playerManager.players.Contains(player))
        {
            playerManager.AddPlayer(player); // 保证 PlayerManager.players 也有统一顺序
        }
    }

    private void RemovePlayerFromScene(PlayerController player)
    {
        if (playersInScene.Contains(player))
        {
            playersInScene.Remove(player);
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;  // 如果没有实例，创建一个单例
        }
        else
        {
            Destroy(gameObject);  // 确保单例存在时不创建新的实例
        }
        playerUIManager = FindObjectOfType<PlayerUIManager>();
        playerManager = GetComponent<PlayerManager>();
        playerController = GetComponent<PlayerController>();

        // 如果未在同一对象上找到 PlayerController，则尝试在场景中查找
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }

        if (playerController == null)
        {
            Debug.LogError("PlayerController 未找到，请检查对象设置。");
        }
    }

    void Update()
    {
        HandleShortcutSelection();

        HandleSkillHotkeys();
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            HandleTabSwitch();
        }
        MouseCheck();
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("F键已按下，尝试触发特殊交互");
            HandleSpecialInteraction();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            InterruptSelectedPlayers();
        }

        //// 每帧更新 UI 选中效果
        //if (playerUIManager)
        //    playerUIManager.HighlightSelectedPlayers(selectedPlayers, currentSkillBarIndex);

    }


    /// <summary>
    /// 鼠标操作检测
    /// </summary>
    public void MouseCheck()
    {

        if (EventSystem.current.IsPointerOverGameObject())
        {
            
            return;
        }
        bool isAnyPlayerBusyWithSkill = selectedPlayers.Exists(player => player.isPreparingSkill || player.isUsingSkill);

      

        PlayerSelectionCheck();
        PlayerActionCheck();
    }

    /// <summary>
    /// 对于角色选取操作的检测（按下鼠标左键）
    /// </summary>
    public void PlayerSelectionCheck()
    {
        if (Input.GetMouseButtonDown(0)) // 按下左键，开始框选
        {
            onDrawingRect = true;
            startPoint = Input.mousePosition;
        }

        if (onDrawingRect) // 框选中，实时更新
        {
            currentPoint = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0)) // 松开左键，完成框选
        {
            onDrawingRect = false;
            endPoint = Input.mousePosition;

            // 获取框选范围
            Vector3 startPointWorld = GetMouseWorldPos(startPoint);
            Vector3 endPointWorld = GetMouseWorldPos(endPoint);

            Vector2 boxCenter = new Vector2((startPointWorld.x + endPointWorld.x) / 2, (startPointWorld.y + endPointWorld.y) / 2);
            Vector2 boxSize = new Vector2(Mathf.Abs(startPointWorld.x - endPointWorld.x), Mathf.Abs(startPointWorld.y - endPointWorld.y));

            Collider2D[] colliders = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0);

            // 框选逻辑
            playerManager.SelectPlayersByBox(colliders);

            // 移除未被框选的角色
            foreach (var player in playerManager.selectedPlayers.ToArray())
            {
                if (!Array.Exists(colliders, collider => collider.GetComponent<PlayerController>() == player))
                {
                    playerManager.UnselectPlayer(player);
                }
            }
            if (playerManager.selectedPlayers.Count == 0)
            {
                playerUIManager.ClearAllHighlights();
            }
        }
    }


    /// <summary>
    /// 对于角色行动操作的检测（按下鼠标右键）
    /// </summary>
    private void PlayerActionCheck()
    {
        if (Input.GetMouseButtonDown(1))
        {
            // 移动到目标点行为的检测
            Vector3 mousePos = Input.mousePosition;
            Vector3 mouseWorldPos = GetMouseWorldPos(mousePos);

            // 获得行动的目标对象
            Collider2D targetCollider = GetTargetCollider(mouseWorldPos);

            if(targetCollider != null)
            {
                Debug.Log("找到目标对象");
                Debug.Log(targetCollider.name);
            }
            else
            {
                Debug.Log("未找到目标对象");
            }

            // 如果可以对目标对象进行行动，则执行行动逻辑
            if (playerManager.ActToTarget(targetCollider))
            {
                return;
            }

            // 获得行动的目标点
            Vector3? targetPosition;
            if (targetCollider != null && targetCollider.tag == "Ground")
            {
                // 如果点到地面，则将目标点移到地面上方
                targetPosition = GetTargetPosition(new Vector3(mouseWorldPos.x, mouseWorldPos.y + 5f, mouseWorldPos.z));
            }
            else
            {
                targetPosition = GetTargetPosition(mouseWorldPos);
            }
            playerManager.MoveToTarget(targetPosition);
        }
    }


    /// <summary>
    /// 处理特殊交互
    /// </summary>
    public void HandleSpecialInteraction()
    {
        if (players.Count > 0)
        {
            foreach (GameObject player in players)
            {
                PlayerController playerController = player.GetComponent<PlayerController>();

                if (playerController != null && !playerController.isInteracting)
                {
                    // 检查玩家是否在携带物品
                    if (playerController.isCarrying && playerController.currentCarriedItem != null)
                    {
                        playerController.currentCarriedItem.Interact(playerController, true);
                    }
                    else
                    {
                        // 获取在范围内的所有可交互物体
                        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.transform.position, exinteractionRange);
                        Interactable closestInteractable = null;
                        float closestDistance = float.MaxValue;

                        foreach (Collider2D collider in colliders)
                        {
                            if (collider.TryGetComponent(out Interactable interactable) && !ReferenceEquals(interactable, playerController.currentCarriedItem))
                            {
                                float distance = Vector2.Distance(player.transform.position, collider.transform.position);
                                if (distance < closestDistance)
                                {
                                    closestDistance = distance;
                                    closestInteractable = interactable;
                                }
                            }
                        }

                        // 只与最近的物体发生交互
                        if (closestInteractable != null)
                        {
                            closestInteractable.Interact(playerController, true);
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    /// 快捷键选择角色
    /// </summary>
    private void HandleShortcutSelection()
    {
        //如果按下Ctrl键则多选，否则单选
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        //按下ctrl键好像在unity中无法测试，先用A键代替
        if (Input.GetKey(KeyCode.A))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                playerManager.SelectPlayer(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                playerManager.SelectPlayer(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                playerManager.SelectPlayer(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                playerManager.SelectPlayer(3);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                //Debug.Log("按下1键");
                playerManager.SelectSinglePlayer(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                playerManager.SelectSinglePlayer(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                playerManager.SelectSinglePlayer(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                playerManager.SelectSinglePlayer(3);
            }
        }
    }


    /// <summary>
    /// 快捷键切换角色
    /// </summary>
    private void HandleTabSwitch()
    {
        Debug.Log("切换角色");
        playerManager.HighlightNextPlayer();
    }


    /// <summary>
    /// 快捷键释放技能
    /// </summary>
    private void HandleSkillHotkeys()
    {
        if (playerManager.selectedPlayers.Count == 0) return;

        PlayerController activePlayer = playerManager.selectedPlayers[playerManager.highlightedPlayerIndex];

        // 只有Q键，用装备的技能
        if (Input.GetKeyDown(KeyCode.Q))
        {
            activePlayer.UseSkill();
        }
    }


    /// <summary>
    /// 获得行动目标对象的碰撞体
    /// </summary>
    /// <param name="mousePosition">鼠标点击的位置</param>
    /// <returns>返回碰撞体</returns>
    public Collider2D GetTargetCollider(Vector3 mousePosition)
    {
        // 1. 射线从相机出发，穿过鼠标位置
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // 2. 优先检测 3D 碰撞体
        int layerMask3D = LayerMask.GetMask("Ground", "Enemy", "Obstacle", "Object");
        RaycastHit hit3D;
        if (Physics.Raycast(ray, out hit3D, 100f, layerMask3D))
        {
            // 命中了带 3D 碰撞体的子物体
            GameObject hitObject = hit3D.collider.gameObject;
            Debug.Log($"[3D命中] {hitObject.name}");

            // 向上查找是否有主物体带有 Collider2D（你的角色主物体）
            Transform current = hitObject.transform;
            while (current != null)
            {
                Collider2D col2D = current.GetComponent<Collider2D>();
                if (col2D != null)
                {
                    Debug.Log($"找到主物体 2D 碰撞体：{col2D.name}");
                    return col2D;
                }
                current = current.parent;
            }
        }

        // 3. 如果没命中 3D，就回退使用原本的 2D 点检测（点击地面）
        int layerMask2D = (1 << 6) | (1 << 7) | (1 << 9) | (1 << 13) | (1 << 14); // 与地面、角色、敌人、物体、障碍物交互
        Collider2D fallbackCollider = Physics2D.OverlapPoint(mousePosition, layerMask2D);
        if (fallbackCollider != null)
        {
            Debug.Log($"[2D命中] {fallbackCollider.name}");
        }

        return fallbackCollider;
    }


    /// <summary>
    /// 获得行动的目标位置
    /// </summary>
    /// <param name="mousePosition">鼠标点击的位置</param>
    /// <returns>返回位置</returns>
    public Vector3? GetTargetPosition(Vector3 mousePosition)
    {
        Vector3? targetPosition = null;

        int layerMask = 1 << 6;     //只与地面产生碰撞
        RaycastHit2D hitInfo;
        hitInfo = Physics2D.Raycast(mousePosition, Vector3.down, 1000000, layerMask);

        if (hitInfo.collider != null)
        {
            targetPosition = hitInfo.point;
        }

        return targetPosition;
    }

    /// <summary>
    /// 获得鼠标点下后的世界坐标
    /// </summary>
    /// <param name="mousePos">鼠标点击的屏幕坐标</param>
    /// <returns>返回坐标</returns>
    public Vector3 GetMouseWorldPos(Vector3 mousePos)
    {
        // 如果玩家位于Z=0的平面上,将Z值设置为场景中玩家所在的深度
        float zDepth = 0f;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.WorldToScreenPoint(new Vector3(0, 0, zDepth)).z));

        return mouseWorldPos;
    }

    //画鼠标框选的边框（待优化）
    void OnGUI()
    {
        if (onDrawingRect)
        {
            // 获取确定矩形框各角坐标所需的各个数值
            float Xmin = Mathf.Min(startPoint.x, currentPoint.x);
            float Xmax = Mathf.Max(startPoint.x, currentPoint.x);
            float Ymin = Mathf.Min(startPoint.y, currentPoint.y);
            float Ymax = Mathf.Max(startPoint.y, currentPoint.y);

            GUI.color = Color.white;

            // 确定方框的定位点（左上角点）的横坐标、纵坐标，以及方框的横向宽度和纵向高度
            Rect rect = new Rect(Xmin, Screen.height - Ymax, Xmax - Xmin, Ymax - Ymin);

            // 画框
            GUI.Box(rect, "");
        }
    }

    private void ShowSkillBar(PlayerController playerController)
    {
        Transform skillBar = playerController.transform.Find("SkillBar");
        if (skillBar != null && !playerController.isKnockedDown)
        {
            // 新
            var canvas = skillBar.GetComponent<Canvas>();
            if (canvas != null)
                canvas.enabled = true;
            // 并且
           // playerController.skillCooldownManager?.SyncCooldown();
            Debug.Log($"技能栏已开启: {playerController.name}");
        }
    }

    private void HideAllSkillBars()
    {
        foreach (var player in playersInScene)
        {
            HideSkillBar(player);
        }
    }

    private void HideSkillBar(PlayerController playerController)
    {
        Transform skillBar = playerController.transform.Find("SkillBar");
        if (skillBar != null)
        {
            var canvas = skillBar.GetComponent<Canvas>();
            if (canvas != null) canvas.enabled = false;
        }
    }


    //以下方法暂时没有被用到
    #region

    private void UpdateCheckSymbol()
    {
        // 遍历所有选中的玩家
        for (int i = 0; i < selectedPlayers.Count; i++)
        {
            PlayerController player = selectedPlayers[i];

            if (i == currentSkillBarIndex)
            {
                // 对 Tab 切换到的角色开启 CheckSymbol
                player.transform.Find("CheckSymbol").gameObject.SetActive(true);
            }
            else
            {
                // 其他角色关闭 CheckSymbol
                player.transform.Find("CheckSymbol").gameObject.SetActive(false);
            }
        }
    }

    void UpdateSelectionEffect()
    {
        // 遍历所有玩家头像框并重置效果
        foreach (var frame in playerUIManager.playerFrames.Values)
        {
            frame.transform.localScale = Vector3.one;
            frame.GetComponent<Image>().DOColor(Color.white, 0.2f); // 重置颜色
        }

        // 给选中的玩家头像框增加效果
        foreach (PlayerController player in selectedPlayers)
        {
            if (playerUIManager.playerFrames.TryGetValue(player, out GameObject frame))
            {
                frame.transform.DOScale(1.1f, 0.2f); // 放大效果
                frame.GetComponent<Image>().DOColor(Color.yellow, 0.2f); // 泛光效果
            }
        }
    }


    private void InterruptSelectedPlayers()
    {
        foreach (PlayerController player in selectedPlayers)
        {
            player.InterruptAction(); // 调用每个选中角色的打断方法
        }

    }

    private void SelectPlayerByIndex(int index)
    {
        if (index < 0 || index >= playersInScene.Count)
        {
            Debug.LogWarning($"无效的角色索引: {index}");
            return;
        }

        PlayerController playerController = playersInScene[index];

        // 单选逻辑
        if (!(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            // 对当前所有已选角色关闭技能栏和技能范围
            foreach (var player in selectedPlayers)
            {
                if (player.isUsingSkill)
                {
                    Debug.Log($"切换时结束技能: {player.name}");
                    player.EndSkill(); // 使用 EndSkill 结束技能
                    player.skillRange.SetActive(false); // 隐藏技能范围
                }

                HideSkillBar(player);
                player.transform.Find("CheckSymbol").gameObject.SetActive(false);
            }

            selectedPlayers.Clear();
            players.Clear();
        }
        if (selectedPlayers.Contains(playerController))
        {
            Debug.Log($"玩家 {playerController.name} 已经被选中，跳过");
            return;
        }
        // 添加新选中的角色
        selectedPlayers.Add(playerController);
        players.Add(playerController.gameObject);
        playerController.transform.Find("CheckSymbol").gameObject.SetActive(true);

        // 确保只显示首个选中角色的技能栏
        HideAllSkillBars(); // 隐藏其他角色的技能栏
        if (selectedPlayers.Count > 0)
        {
            currentSkillBarIndex = 0; // 重置索引
            ShowSkillBar(selectedPlayers[currentSkillBarIndex]);
        }

        Debug.Log($"选中角色: {playerController.name}");

        // 更新 UI 高亮
        if (playerUIManager)
            playerUIManager.HighlightSelectedPlayers(selectedPlayers, currentSkillBarIndex);

    }

    // 触发技能的方法
    private void TriggerSkill(PlayerController player)
    {
        if (player == null) return;

        if (player.equippedSkill != null)
        {
            player.UseSkill();
            Debug.Log($"玩家 {player.name} 释放技能：{player.equippedSkill.SkillName}");
        }
        else
        {
            Debug.Log($"玩家 {player.name} 未装备技能！");
        }
    }


    private bool IsPointerOverUIObject()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            Debug.Log($"鼠标点击检测到 UI 对象: {result.gameObject.name}");
        }

        return results.Count > 0; // 如果检测到任何 UI 对象，返回 true
    }


    private void SelectSinglePlayerByIndex(int index)
    {
        if (index < 0 || index >= playersInScene.Count)
        {
            Debug.LogWarning($"无效的角色索引: {index}");
            return;
        }

        // 清空当前选中的玩家
        foreach (var player in selectedPlayers)
        {
            player.transform.Find("CheckSymbol").gameObject.SetActive(false);
            player.transform.Find("SkillBar").gameObject.SetActive(false); // 隐藏所有技能栏
        }
        selectedPlayers.Clear();
        players.Clear();

        // 选中目标玩家
        PlayerController playerController = playersInScene[index];
        selectedPlayers.Add(playerController);
        players.Add(playerController.gameObject);

        // 激活选中效果
        playerController.transform.Find("CheckSymbol").gameObject.SetActive(true);

        // 开启技能栏
        Transform skillBar = playerController.transform.Find("SkillBar");
        if (skillBar != null && !playerController.isKnockedDown) // 确保玩家未倒地
        {
            // 新
            var canvas = skillBar.GetComponent<Canvas>();
            if (canvas != null)
                canvas.enabled = true;
            // 并且
            playerController.skillCooldownManager?.SyncCooldown();
            Debug.Log($"技能栏已开启: {playerController.name}");
        }
        else
        {
            Debug.LogWarning($"技能栏未找到或玩家倒地: {playerController.name}");
        }

        Debug.Log($"切换到角色: {playerController.name}");

        // 更新 UI 高亮
        if (playerUIManager)
            playerUIManager.HighlightSelectedPlayers(selectedPlayers, currentSkillBarIndex);

    }

    #endregion

}


