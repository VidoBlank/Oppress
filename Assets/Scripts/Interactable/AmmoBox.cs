using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AmmoBox : Interactable
{
    [Header("弹药箱基础属性")]
    public float maxEnergy = 100f;     // 最大能量
    public float currentEnergy = 100f; // 当前能量
    [Header("每次交互时角色恢复的能量")]
    public float energyRestoreAmount = 20f; // 每次交互时恢复的能量
    [Header("每次补充弹药后的冷却时间")]
    public float cooldownTime = 1f;         // 每次补充弹药后的冷却时间
    [Header("拾取弹药箱时，角色的减速倍率")]
    public float lowerspeed = 0f;  // 拾取弹药箱时，角色的减速倍率

    private string originalTag;
    private Dictionary<PlayerController, float> cooldownTimers = new Dictionary<PlayerController, float>();
    private bool isBeingCarried = false;

    private bool isReplenishing = false;
    private PlayerController carryingPlayer = null;
    private Vector3 carryOffset = new Vector3(0.1f, -0.9f, 0);
    private Vector3 originalLocalPosition; // 用于存储子物体的初始位置
    public event System.Action OnPickedUp; // 定义事件
                                           // 新增 Dictionary 来记录每个玩家的补充状态
    private Dictionary<PlayerController, bool> replenishingPlayers = new Dictionary<PlayerController, bool>();



    private void LateUpdate()
    {
        if (isBeingCarried && carryingPlayer != null)
        {
            // 设置 AmmoBox 的位置为角色的位置加上偏移量
            transform.position = carryingPlayer.transform.position + carryOffset;
            transform.rotation = carryingPlayer.transform.rotation;
        }
    }

    private void Start()
    {
        originalTag = gameObject.tag; // 保存弹药箱的初始 tag

        // 保存子物体的初始位置
        if (transform.childCount > 0)
        {
            originalLocalPosition = transform.GetChild(0).localPosition;
        }

        Collider2D objectCollider = GetComponent<Collider2D>();
        if (objectCollider == null) return; // 如果没有碰撞体，则退出

        // 忽略与所有 Player 的 2D 碰撞
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                Physics2D.IgnoreCollision(objectCollider, playerCollider);
            }
        }
    }

    public override void Interact(PlayerController player, bool isFKeyInteraction = false)
    {
        if (isReplenishing)  // 如果正在补充弹药，则不允许其他玩家交互
        {
            Debug.Log("AmmoBox 正在补充弹药，无法交互。");
            return;
        }
        if (isFKeyInteraction)
        {
            if (isBeingCarried && carryingPlayer == player)
            {
                Drop(player); // 放下物品
            }
            else if (!isBeingCarried && !player.isCarrying)
            {
                Pickup(player);
            }
            else
            {
                Debug.Log("玩家已携带其他物品，无法拾取新的物品。");
            }
        }
        else
        {
            HandleAmmoBoxInteraction(player);
        }
    }

    private void Pickup(PlayerController player)
    {
        isBeingCarried = true;
        carryingPlayer = player;
        player.isCarrying = true;
        player.currentCarriedItem = this; // 记录当前携带的物品

        IgnoreCollisionWithOtherAmmoBoxes(true);

        player.attribute.movingSpeed *= lowerspeed;
        player.canShoot = false;

        // 设置子物体的z轴为0.1
        if (transform.childCount > 0)
        {
            Vector3 newPosition = transform.GetChild(0).localPosition;
            newPosition.z = 0.1f;
            transform.GetChild(0).localPosition = newPosition;
        }

        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isCarryingItem", true);
        }

        gameObject.tag = "Untagged";
        Debug.Log("弹药箱被拾取，角色减速");

        OnPickedUp?.Invoke(); // 触发事件，通知 AmmoDepot 已拾取
    }

    private void Drop(PlayerController player)
    {
        isBeingCarried = false;
        carryingPlayer = null;
        player.isCarrying = false;
        player.currentCarriedItem = null;

        IgnoreCollisionWithOtherAmmoBoxes(false);

        player.attribute.movingSpeed /= lowerspeed;
        player.canShoot = true;

        // 恢复子物体的原始位置
        if (transform.childCount > 0)
        {
            transform.GetChild(0).localPosition = originalLocalPosition;
        }

        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isCarryingItem", false);
        }

        Vector3 dropPosition = transform.position;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(dropPosition, 0.5f);

        bool positionAdjusted = false;
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("AmmoBox") && collider.gameObject != gameObject)
            {
                dropPosition += new Vector3(0.5f, 0, 0); // 向右偏移
                positionAdjusted = true;
                break;
            }
        }

        if (positionAdjusted)
        {
            Debug.Log("放置位置被调整，避免挤走其他 AmmoBox。");
        }

        transform.position = dropPosition;
        gameObject.tag = originalTag;
        Debug.Log("弹药箱被放下，角色恢复速度");
    }

    private void IgnoreCollisionWithOtherAmmoBoxes(bool ignore)
    {
        Collider2D thisCollider = GetComponent<Collider2D>();
        AmmoBox[] otherAmmoBoxes = FindObjectsOfType<AmmoBox>();

        foreach (var ammoBox in otherAmmoBoxes)
        {
            if (ammoBox != this && ammoBox.TryGetComponent(out Collider2D otherCollider))
            {
                Physics2D.IgnoreCollision(thisCollider, otherCollider, ignore);
            }
        }
    }

    private void HandleAmmoBoxInteraction(PlayerController player)
    {
        if (IsOnCooldown(player))
        {
            Debug.Log("弹药箱正在冷却中，无法立即补充弹药。");
            return;
        }

        if (player.energy >= player.attribute.maxEnergy)
        {
            Debug.Log("玩家的能量已满，无法补充弹药。");
            return;
        }

        if (currentEnergy > 0)
        {
            if (!replenishingPlayers.ContainsKey(player) || !replenishingPlayers[player])
            {
                replenishingPlayers[player] = true; // 标记该玩家正在补充弹药
                player.canShoot = false; // 禁用自动射击
                RestoreEnergyAfterInteraction(player, player.attribute.interactspeed);
                GetComponent<AmmoBoxUI>()?.ShowAmmoUI();
            }
        }
        else
        {
            Debug.Log("弹药箱已耗尽，无法交互。");
        }
    }

    private void RestoreEnergyAfterInteraction(PlayerController player, float delay)
    {
        //yield return new WaitForSeconds(delay);

        if (player.energy < player.attribute.maxEnergy)
        {
            float actualRestore = Mathf.Min(energyRestoreAmount, currentEnergy, player.attribute.maxEnergy - player.energy);
            player.RestoreEnergy(actualRestore);
            currentEnergy -= actualRestore;

            Debug.Log($"玩家从弹药箱获得了 {actualRestore} 点能量，弹药箱剩余能量: {currentEnergy}");
        }
        else
        {
            Debug.Log("玩家的能量已满，不再补充弹药。");
        }

        cooldownTimers[player] = Time.time + cooldownTime; // 记录冷却时间
        player.canShoot = true;

        // 恢复该玩家的补充状态
        if (replenishingPlayers.ContainsKey(player))
        {
            replenishingPlayers[player] = false;
        }

        // 如果弹药箱能量耗尽，销毁它
        if (currentEnergy <= 0)
        {
            DestroyAmmoBox();
        }
    }

    private IEnumerator StartTagCooldown()
    {
        gameObject.tag = "Untagged";
        yield return new WaitForSeconds(cooldownTime);
        gameObject.tag = originalTag;
    }

    private void DestroyAmmoBox()
    {
        Debug.Log("弹药箱能量耗尽，被摧毁！");
        carryingPlayer.isCarrying = false;
        IgnoreCollisionWithOtherAmmoBoxes(false); // 恢复碰撞
        Destroy(gameObject);
    }

    private bool IsOnCooldown(PlayerController player)
    {
        return cooldownTimers.ContainsKey(player) && Time.time < cooldownTimers[player];
    }
}
