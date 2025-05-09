using System.Collections;
using UnityEngine;

public class StairZSyncEnemy : MonoBehaviour
{
    private Enemy enemy; // �滻 PlayerController Ϊ Enemy
    private Transform stairParent; // ��¼������ StairTrigger �ĸ�����
    private bool isSyncing = false; // ��ֹ�ظ�����

    [Header("��Ҫͬ��Zֵ��Ŀ������")]
    public Transform syncTarget; // ����Ҫͬ��Zֵ������

    [Header("stairMoving ������ָ��� Z ֵ")]
    public float resetZValue = 0f; // �� stairMoving ������ָ��Ĺ̶� Z ֵ

    [Header("stairMoving �ڼ�����Ŵ�С")]
    public Vector3 enlargedScale = new Vector3(1.2f, 1.2f, 1.2f); // �Ŵ��Ĵ�С

    public Vector3 originalScale; // ��¼ԭʼ����ֵ

    private void Start()
    {
        enemy = GetComponent<Enemy>(); // ��ȡ Enemy ���

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
        if (enemy == null || syncTarget == null)
            return;

        if (enemy.stairMoving && stairParent != null)
        {
            // �� stairMoving �ڼ�ͬ�� Z ��ֵ
            syncTarget.position = new Vector3(syncTarget.position.x, syncTarget.position.y, stairParent.position.z);

            // �Ŵ�Ŀ������
            syncTarget.localScale = enlargedScale;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("StairTrigger"))
        {
            StairTrigger stairTrigger = collision.GetComponent<StairTrigger>();
            if (stairTrigger != null && stairTrigger.transform.parent != null)
            {
                stairParent = stairTrigger.transform.parent;
            }

            // �������� Z ������ţ���ʹ stairMoving ��δ������
            if (enemy.stairMoving)
            {
                syncTarget.position = new Vector3(syncTarget.position.x, syncTarget.position.y, stairParent.position.z);
                syncTarget.localScale = enlargedScale;
                isSyncing = true;
                StartCoroutine(MonitorStairMoving());
            }
        }
    }


    private IEnumerator MonitorStairMoving()
    {
        // ������� stairMoving �Ƿ����
        while (enemy.stairMoving)
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
