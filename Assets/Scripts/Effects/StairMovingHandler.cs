using System.Collections;
using UnityEngine;

public class StairMovingHandler : MonoBehaviour
{
    [Header("Ŀ������")]
    public GameObject targetObject; // ָ����Ŀ�����壬����� stairMoving ״̬

    [Header("��Ҫ�޸�λ�õ�����")]
    public GameObject affectedObject; // ��Ҫ�ı� position.z ������

    [Header("Z��ֵ����")]
    public float stairMovingZValue = 4.48f; // stairMoving ʱ�� Z ��ֵ

    private float originalZValue; // ��¼ԭʼ�� Z ��ֵ
    private Unit targetUnit; // ���ڻ�ȡĿ��� Unit ���

    private void Start()
    {
        if (targetObject == null || affectedObject == null)
        {
            Debug.LogError("Ŀ���������Ҫ�޸ĵ�����δ���ã�");
            enabled = false;
            return;
        }

        targetUnit = targetObject.GetComponent<Unit>();
        if (targetUnit == null)
        {
            Debug.LogError("Ŀ������δ�ҵ� Unit �����");
            enabled = false;
            return;
        }

        originalZValue = affectedObject.transform.position.z;
    }

    private void Update()
    {
        if (targetUnit.stairMoving)
        {
            // �޸� Z ��ֵΪָ��ֵ
            Vector3 newPosition = affectedObject.transform.position;
            newPosition.z = stairMovingZValue;
            affectedObject.transform.position = newPosition;
        }
        else
        {
            // �ָ� Z ���ԭʼֵ
            Vector3 originalPosition = affectedObject.transform.position;
            originalPosition.z = originalZValue;
            affectedObject.transform.position = originalPosition;
        }
    }
}
