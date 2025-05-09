using System.Collections.Generic;
using UnityEngine;

public class BloodEffectSpawner : MonoBehaviour
{
    [Header("ѪҺЧ��Ԥ�����б�˳�򲥷ţ�")]
    public List<GameObject> bloodPrefabs;

    [Header("�Ƿ�ʹ�û��з��߷��򣨷����������")]
    public bool useHitNormal = false;

    [Header("ƫ�������������ɫ�ص���")]
    public Vector3 offset = Vector3.zero;

    [Header("ѪҺЧ������λ�õ�Ŀ�� GameObject���ܻ��㣩")]
    public GameObject targetGameObject; // �� Inspector ��ָ���������Ϊ�գ���ѪҺЧ�����ڴ�λ������

    private int currentIndex = 0; // ��ǰʹ�õ�Ԥ��������

    public void SpawnBlood(Vector3 hitPoint, Vector3? hitNormal = null)
    {
        if (bloodPrefabs == null || bloodPrefabs.Count == 0)
            return;

        // ��˳����б���ѡ��Ԥ����
        GameObject prefab = bloodPrefabs[currentIndex];
        currentIndex = (currentIndex + 1) % bloodPrefabs.Count;

        // ���ָ����Ŀ�� GameObject����ʹ����λ�ã�����ʹ�ô���� hitPoint
        Vector3 originPoint = targetGameObject != null ? targetGameObject.transform.position : hitPoint;

        // ������ƫ�ƣ�������λ�ò���ȫһ��
        Vector3 randomOffset = new Vector3(
            Random.Range(-0.2f, 0.2f),
            Random.Range(-0.2f, 0.2f),
            Random.Range(-0.2f, 0.2f)
        );
        Vector3 spawnPos = originPoint + offset + randomOffset;

        Quaternion rotation;
        if (useHitNormal && hitNormal.HasValue)
        {
            // ʹ�����з��߼��������ת
            Quaternion baseRotation = Quaternion.LookRotation(hitNormal.Value);
            // ���������ӽ�ƽ�棬����������ϼ�������� Y �����ת
            if (Vector3.Dot(hitNormal.Value, Vector3.up) > 0.9f)
            {
                baseRotation *= Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            }
            // ����Ԥ������Ҫ����У������������Ĭ�ϳ���ʱ����ת90�ȣ�
            rotation = baseRotation * Quaternion.Euler(90f, 0f, 0f);
        }
        else
        {
            // δ���뷨��ʱ��ʹ����ȫ������� Y ����ת
            rotation = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);
        }

        // ʵ����ѪҺЧ��
        GameObject bloodInstance = Instantiate(prefab, spawnPos, rotation);

      
       
        
            Destroy(bloodInstance, 5f);
        
    }

}
