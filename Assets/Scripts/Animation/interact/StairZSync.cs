using System.Collections;
using UnityEngine;

public class StairZSync : MonoBehaviour
{
    private PlayerController playerController;
    private Transform stairParent; // ��¼������StairTrigger�ĸ�����
    private bool isSyncing = false; // ��ֹ�ظ�����

    [Header("��Ҫͬ��Zֵ��Ŀ������")]
    public Transform syncTarget; // ����Ҫͬ��Zֵ������

    [Header("stairMoving ������ָ��� Z ֵ")]
    public float resetZValue = 0f; // �� stairMoving ������ָ��Ĺ̶� Z ֵ

    [Header("stairMoving �ڼ�����Ŵ�С")]
    public Vector3 enlargedScale = new Vector3(1.2f, 1.2f, 1.2f); // �Ŵ��Ĵ�С

    private Vector3 originalScale; // ��¼ԭʼ����ֵ

    private void Start()
    {
        playerController = GetComponent<PlayerController>();

        if (syncTarget == null)
        {
            Debug.LogError("���� Inspector ��ָ����Ҫͬ��Zֵ��Ŀ�����壡");
        }
        else
        {
            originalScale = syncTarget.localScale; // ��¼ԭʼ����ֵ
        }
    }

    private void Update()
    {
        if (playerController == null || syncTarget == null)
            return;

        if (playerController.stairMoving && stairParent != null)
        {
            // �� stairMoving �ڼ�ͬ�� Z ��ֵ
            syncTarget.position = new Vector3(syncTarget.position.x, syncTarget.position.y, stairParent.position.z);

            // �Ŵ�Ŀ������
            syncTarget.localScale = enlargedScale;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("StairTrigger") && playerController.stairMoving)
        {
            if (!isSyncing) // ȷ��ֻ�ڵ�һ�ν���ʱִ��
            {
                StairTrigger stairTrigger = collision.GetComponent<StairTrigger>();
                if (stairTrigger != null && stairTrigger.transform.parent != null)
                {
                    stairParent = stairTrigger.transform.parent;
                    isSyncing = true;

                    // ����Э�̼�� stairMoving ״̬
                    StartCoroutine(MonitorStairMoving());
                }
            }
        }
    }

    private IEnumerator MonitorStairMoving()
    {
        // ������� stairMoving �Ƿ����
        while (playerController.stairMoving)
        {
            yield return null; // ÿ֡�ȴ�
        }

        // �� stairMoving �������ָ�Ϊָ���� resetZValue
        syncTarget.position = new Vector3(syncTarget.position.x, syncTarget.position.y, resetZValue);

        // �ָ�ԭʼ����
        syncTarget.localScale = originalScale;

        isSyncing = false; // ������һ�ν���¥��ʱͬ�� Z ֵ
    }
}
