using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoDepot : Interactable
{
    [Header("��ҩ������")]
    public float energyAmount = 2000f; // ��ҩ��ĳ�ʼ������
    public float ammoBoxCapacity = 400f; // ÿ�� AmmoBox ��Ĭ������
    public float replenishCooldown = 1f; // �������ȴʱ��
    public GameObject ammoBoxPrefab; // AmmoBox Ԥ����
    public float spacing = 1.5f; // ���� AmmoBox ��ˮƽ���
    public float ammoBoxSpawnCooldown = 1f; // ���� AmmoBox ����ȴʱ��
    private float nextAmmoBoxSpawnTime = 0f; // �´��������� AmmoBox ��ʱ��

    private bool isNextSpawnLeft = true; // �������ɷ���
    private Dictionary<PlayerController, float> playerCooldowns = new Dictionary<PlayerController, float>();

    private void Start()
    {
        Collider2D depotCollider = GetComponent<Collider2D>();
        if (depotCollider == null) return;

        // ���������� Player ����ײ
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

    // ������ҵ�����
    public void ReplenishAmmo(PlayerController player)
    {
        if (energyAmount <= 0)
        {
            Debug.Log("AmmoDepot �����ľ����޷�����������");
            return;
        }

        if (!IsPlayerOnCooldown(player))
        {
            // ����ָ�����������������
            float restoreAmount = Mathf.Min(player.attribute.maxEnergy - player.energy, energyAmount);
            player.RestoreEnergy(restoreAmount); // ����������ָ������ֵ��������ʣ������
            energyAmount -= restoreAmount;

            StartCoroutine(StartCooldown(player)); // ������ȴ
            Debug.Log($"��ҵ������ѻָ� {restoreAmount} �㡣AmmoDepot ʣ������: {energyAmount}");

            if (energyAmount <= 0)
            {
                Destroy(gameObject);
                Debug.Log("AmmoDepot �����ľ��������١�");
            }
        }
        else
        {
            Debug.Log("���������ȴ�У��޷�����������");
        }
    }

    // ���� AmmoBox
    public void SpawnAmmoBox()
    {
        // ����Ƿ�����ȴʱ��
        if (Time.time < nextAmmoBoxSpawnTime)
        {
            Debug.Log("���� AmmoBox ������ȴ�У����Ժ����ԡ�");
            return;
        }

        if (energyAmount < 100)
        {
            Debug.Log("AmmoDepot ʣ���������㣬�޷����� AmmoBox��");
            return;
        }

        float currentBoxCapacity = Mathf.Min(ammoBoxCapacity, energyAmount);
        energyAmount -= currentBoxCapacity;

        // ���� isNextSpawnLeft ȷ������λ��
        float horizontalOffset = isNextSpawnLeft ? -spacing : spacing;
        Vector3 spawnPosition = transform.position + new Vector3(horizontalOffset, 0, 0);

        // �л�����
        isNextSpawnLeft = !isNextSpawnLeft;

        GameObject newAmmoBox = Instantiate(ammoBoxPrefab, spawnPosition, Quaternion.identity);
        newAmmoBox.GetComponent<AmmoBox>().maxEnergy = currentBoxCapacity;
        newAmmoBox.GetComponent<AmmoBox>().currentEnergy = currentBoxCapacity;

        newAmmoBox.GetComponent<AmmoBox>().OnPickedUp += ResetAmmoBoxGenerated;

        nextAmmoBoxSpawnTime = Time.time + ammoBoxSpawnCooldown; // �����´�����ʱ��

        Debug.Log($"����������Ϊ {currentBoxCapacity} �� AmmoBox��ʣ������: {energyAmount}");

        if (energyAmount <= 0)
        {
            Destroy(gameObject);
            Debug.Log("AmmoDepot �����ľ��������١�");
        }
    }

    // ���� AmmoBox ����״̬
    private void ResetAmmoBoxGenerated()
    {
        isNextSpawnLeft = true; // �������ɷ���ʹ�´����ɴ���࿪ʼ
    }

    // �������Ƿ�����ȴ��
    private bool IsPlayerOnCooldown(PlayerController player)
    {
        return playerCooldowns.ContainsKey(player) && Time.time < playerCooldowns[player];
    }

    // Ϊ���������ȴ
    private IEnumerator StartCooldown(PlayerController player)
    {
        playerCooldowns[player] = Time.time + replenishCooldown;
        yield return new WaitForSeconds(replenishCooldown);
        playerCooldowns.Remove(player);
    }

    // ���ݽ�����ʽ������ͬ����Ϊ
    public override void Interact(PlayerController player, bool isFKeyInteraction = false)
    {
        if (isFKeyInteraction)
        {
            SpawnAmmoBox(); // �������F���������� AmmoBox
        }
        else
        {
            // ���� ReplenishAmmoAfterInteraction Э�̣��ȴ������ɽ������ٲ��䵯ҩ
            StartCoroutine(ReplenishAmmoAfterInteraction(player));
        }
    }

    // Э�̣��ȴ������ɽ������ٲ��䵯ҩ
    private IEnumerator ReplenishAmmoAfterInteraction(PlayerController player)
    {
        // �ȴ���ҵĽ���ʱ�����
        yield return new WaitForSeconds(player.attribute.interactspeed);

        // ִ�е�ҩ�����߼�
        if (energyAmount <= 0)
        {
            Debug.Log("AmmoDepot �����ľ����޷�����������");
            yield break;
        }

        if (!IsPlayerOnCooldown(player))
        {
            // ����ָ�����������������
            float restoreAmount = Mathf.Min(player.attribute.maxEnergy - player.energy, energyAmount);
            player.RestoreEnergy(restoreAmount); // ����������ָ������ֵ��������ʣ������
            energyAmount -= restoreAmount;

            StartCoroutine(StartCooldown(player)); // ������ȴ
            Debug.Log($"��ҵ������ѻָ� {restoreAmount} �㡣AmmoDepot ʣ������: {energyAmount}");

            if (energyAmount <= 0)
            {
                Destroy(gameObject);
                Debug.Log("AmmoDepot �����ľ��������١�");
            }
        }
        else
        {
            Debug.Log("���������ȴ�У��޷�����������");
        }
    }
}
