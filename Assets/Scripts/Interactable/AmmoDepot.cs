using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoDepot : Interactable
{
    [Header("弹药库属性")]
    public float energyAmount = 2000f; // 弹药库的初始总能量
    public float ammoBoxCapacity = 400f; // 每个 AmmoBox 的默认容量
    public float replenishCooldown = 1f; // 补充的冷却时间
    public GameObject ammoBoxPrefab; // AmmoBox 预制体
    public float spacing = 1.5f; // 生成 AmmoBox 的水平间隔
    public float ammoBoxSpawnCooldown = 1f; // 生成 AmmoBox 的冷却时间
    private float nextAmmoBoxSpawnTime = 0f; // 下次允许生成 AmmoBox 的时间

    private bool isNextSpawnLeft = true; // 控制生成方向
    private Dictionary<PlayerController, float> playerCooldowns = new Dictionary<PlayerController, float>();

    private void Start()
    {
        Collider2D depotCollider = GetComponent<Collider2D>();
        if (depotCollider == null) return;

        // 忽略与所有 Player 的碰撞
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                Physics2D.IgnoreCollision(depotCollider, playerCollider);
            }
        }
    }

    // 补充玩家的能量
    public void ReplenishAmmo(PlayerController player)
    {
        if (energyAmount <= 0)
        {
            Debug.Log("AmmoDepot 能量耗尽，无法补充能量。");
            return;
        }

        if (!IsPlayerOnCooldown(player))
        {
            // 计算恢复至最大能量所需的量
            float restoreAmount = Mathf.Min(player.attribute.maxEnergy - player.energy, energyAmount);
            player.RestoreEnergy(restoreAmount); // 将玩家能量恢复至最大值或能量库剩余能量
            energyAmount -= restoreAmount;

            StartCoroutine(StartCooldown(player)); // 启动冷却
            Debug.Log($"玩家的能量已恢复 {restoreAmount} 点。AmmoDepot 剩余能量: {energyAmount}");

            if (energyAmount <= 0)
            {
                Destroy(gameObject);
                Debug.Log("AmmoDepot 能量耗尽，已销毁。");
            }
        }
        else
        {
            Debug.Log("玩家正在冷却中，无法补充能量。");
        }
    }

    // 生成 AmmoBox
    public void SpawnAmmoBox()
    {
        // 检查是否处于冷却时间
        if (Time.time < nextAmmoBoxSpawnTime)
        {
            Debug.Log("生成 AmmoBox 处于冷却中，请稍后再试。");
            return;
        }

        if (energyAmount < 100)
        {
            Debug.Log("AmmoDepot 剩余能量不足，无法生成 AmmoBox。");
            return;
        }

        float currentBoxCapacity = Mathf.Min(ammoBoxCapacity, energyAmount);
        energyAmount -= currentBoxCapacity;

        // 根据 isNextSpawnLeft 确定生成位置
        float horizontalOffset = isNextSpawnLeft ? -spacing : spacing;
        Vector3 spawnPosition = transform.position + new Vector3(horizontalOffset, 0, 0);

        // 切换方向
        isNextSpawnLeft = !isNextSpawnLeft;

        GameObject newAmmoBox = Instantiate(ammoBoxPrefab, spawnPosition, Quaternion.identity);
        newAmmoBox.GetComponent<AmmoBox>().maxEnergy = currentBoxCapacity;
        newAmmoBox.GetComponent<AmmoBox>().currentEnergy = currentBoxCapacity;

        newAmmoBox.GetComponent<AmmoBox>().OnPickedUp += ResetAmmoBoxGenerated;

        nextAmmoBoxSpawnTime = Time.time + ammoBoxSpawnCooldown; // 设置下次生成时间

        Debug.Log($"生成了容量为 {currentBoxCapacity} 的 AmmoBox。剩余能量: {energyAmount}");

        if (energyAmount <= 0)
        {
            Destroy(gameObject);
            Debug.Log("AmmoDepot 能量耗尽，已销毁。");
        }
    }

    // 重置 AmmoBox 生成状态
    private void ResetAmmoBoxGenerated()
    {
        isNextSpawnLeft = true; // 重置生成方向，使下次生成从左侧开始
    }

    // 检查玩家是否在冷却中
    private bool IsPlayerOnCooldown(PlayerController player)
    {
        return playerCooldowns.ContainsKey(player) && Time.time < playerCooldowns[player];
    }

    // 为玩家启动冷却
    private IEnumerator StartCooldown(PlayerController player)
    {
        playerCooldowns[player] = Time.time + replenishCooldown;
        yield return new WaitForSeconds(replenishCooldown);
        playerCooldowns.Remove(player);
    }

    // 根据交互方式触发不同的行为
    public override void Interact(PlayerController player, bool isFKeyInteraction = false)
    {
        if (isFKeyInteraction)
        {
            SpawnAmmoBox(); // 如果按下F键，则生成 AmmoBox
        }
        else
        {
            // 触发 ReplenishAmmoAfterInteraction 协程，等待玩家完成交互后再补充弹药
            StartCoroutine(ReplenishAmmoAfterInteraction(player));
        }
    }

    // 协程：等待玩家完成交互后再补充弹药
    private IEnumerator ReplenishAmmoAfterInteraction(PlayerController player)
    {
        // 等待玩家的交互时间完成
        yield return new WaitForSeconds(player.attribute.interactspeed);

        // 执行弹药补充逻辑
        if (energyAmount <= 0)
        {
            Debug.Log("AmmoDepot 能量耗尽，无法补充能量。");
            yield break;
        }

        if (!IsPlayerOnCooldown(player))
        {
            // 计算恢复至最大能量所需的量
            float restoreAmount = Mathf.Min(player.attribute.maxEnergy - player.energy, energyAmount);
            player.RestoreEnergy(restoreAmount); // 将玩家能量恢复至最大值或能量库剩余能量
            energyAmount -= restoreAmount;

            StartCoroutine(StartCooldown(player)); // 启动冷却
            Debug.Log($"玩家的能量已恢复 {restoreAmount} 点。AmmoDepot 剩余能量: {energyAmount}");

            if (energyAmount <= 0)
            {
                Destroy(gameObject);
                Debug.Log("AmmoDepot 能量耗尽，已销毁。");
            }
        }
        else
        {
            Debug.Log("玩家正在冷却中，无法补充能量。");
        }
    }
}
