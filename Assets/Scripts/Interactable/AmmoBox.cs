using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AmmoBox : Interactable
{
    [Header("��ҩ���������")]
    public float maxEnergy = 100f;     // �������
    public float currentEnergy = 100f; // ��ǰ����
    [Header("ÿ�ν���ʱ��ɫ�ָ�������")]
    public float energyRestoreAmount = 20f; // ÿ�ν���ʱ�ָ�������
    [Header("ÿ�β��䵯ҩ�����ȴʱ��")]
    public float cooldownTime = 1f;         // ÿ�β��䵯ҩ�����ȴʱ��
    [Header("ʰȡ��ҩ��ʱ����ɫ�ļ��ٱ���")]
    public float lowerspeed = 0f;  // ʰȡ��ҩ��ʱ����ɫ�ļ��ٱ���

    private string originalTag;
    private Dictionary<PlayerController, float> cooldownTimers = new Dictionary<PlayerController, float>();
    private bool isBeingCarried = false;

    private bool isReplenishing = false;
    private PlayerController carryingPlayer = null;
    private Vector3 carryOffset = new Vector3(0.1f, -0.9f, 0);
    private Vector3 originalLocalPosition; // ���ڴ洢������ĳ�ʼλ��
    public event System.Action OnPickedUp; // �����¼�
                                           // ���� Dictionary ����¼ÿ����ҵĲ���״̬
    private Dictionary<PlayerController, bool> replenishingPlayers = new Dictionary<PlayerController, bool>();



    private void LateUpdate()
    {
        if (isBeingCarried && carryingPlayer != null)
        {
            // ���� AmmoBox ��λ��Ϊ��ɫ��λ�ü���ƫ����
            transform.position = carryingPlayer.transform.position + carryOffset;
            transform.rotation = carryingPlayer.transform.rotation;
        }
    }

    private void Start()
    {
        originalTag = gameObject.tag; // ���浯ҩ��ĳ�ʼ tag

        // ����������ĳ�ʼλ��
        if (transform.childCount > 0)
        {
            originalLocalPosition = transform.GetChild(0).localPosition;
        }

        Collider2D objectCollider = GetComponent<Collider2D>();
        if (objectCollider == null) return; // ���û����ײ�壬���˳�

        // ���������� Player �� 2D ��ײ
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
        if (isReplenishing)  // ������ڲ��䵯ҩ��������������ҽ���
        {
            Debug.Log("AmmoBox ���ڲ��䵯ҩ���޷�������");
            return;
        }
        if (isFKeyInteraction)
        {
            if (isBeingCarried && carryingPlayer == player)
            {
                Drop(player); // ������Ʒ
            }
            else if (!isBeingCarried && !player.isCarrying)
            {
                Pickup(player);
            }
            else
            {
                Debug.Log("�����Я��������Ʒ���޷�ʰȡ�µ���Ʒ��");
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
        player.currentCarriedItem = this; // ��¼��ǰЯ������Ʒ

        IgnoreCollisionWithOtherAmmoBoxes(true);

        player.attribute.movingSpeed *= lowerspeed;
        player.canShoot = false;

        // �����������z��Ϊ0.1
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
        Debug.Log("��ҩ�䱻ʰȡ����ɫ����");

        OnPickedUp?.Invoke(); // �����¼���֪ͨ AmmoDepot ��ʰȡ
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

        // �ָ��������ԭʼλ��
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
                dropPosition += new Vector3(0.5f, 0, 0); // ����ƫ��
                positionAdjusted = true;
                break;
            }
        }

        if (positionAdjusted)
        {
            Debug.Log("����λ�ñ����������⼷������ AmmoBox��");
        }

        transform.position = dropPosition;
        gameObject.tag = originalTag;
        Debug.Log("��ҩ�䱻���£���ɫ�ָ��ٶ�");
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
            Debug.Log("��ҩ��������ȴ�У��޷��������䵯ҩ��");
            return;
        }

        if (player.energy >= player.attribute.maxEnergy)
        {
            Debug.Log("��ҵ������������޷����䵯ҩ��");
            return;
        }

        if (currentEnergy > 0)
        {
            if (!replenishingPlayers.ContainsKey(player) || !replenishingPlayers[player])
            {
                replenishingPlayers[player] = true; // ��Ǹ�������ڲ��䵯ҩ
                player.canShoot = false; // �����Զ����
                RestoreEnergyAfterInteraction(player, player.attribute.interactspeed);
                GetComponent<AmmoBoxUI>()?.ShowAmmoUI();
            }
        }
        else
        {
            Debug.Log("��ҩ���Ѻľ����޷�������");
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

            Debug.Log($"��Ҵӵ�ҩ������ {actualRestore} ����������ҩ��ʣ������: {currentEnergy}");
        }
        else
        {
            Debug.Log("��ҵ��������������ٲ��䵯ҩ��");
        }

        cooldownTimers[player] = Time.time + cooldownTime; // ��¼��ȴʱ��
        player.canShoot = true;

        // �ָ�����ҵĲ���״̬
        if (replenishingPlayers.ContainsKey(player))
        {
            replenishingPlayers[player] = false;
        }

        // �����ҩ�������ľ���������
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
        Debug.Log("��ҩ�������ľ������ݻ٣�");
        carryingPlayer.isCarrying = false;
        IgnoreCollisionWithOtherAmmoBoxes(false); // �ָ���ײ
        Destroy(gameObject);
    }

    private bool IsOnCooldown(PlayerController player)
    {
        return cooldownTimers.ContainsKey(player) && Time.time < cooldownTimers[player];
    }
}
