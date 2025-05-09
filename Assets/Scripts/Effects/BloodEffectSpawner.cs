using System.Collections.Generic;
using UnityEngine;

public class BloodEffectSpawner : MonoBehaviour
{
    [Header("血液效果预制体列表（顺序播放）")]
    public List<GameObject> bloodPrefabs;

    [Header("是否使用击中法线方向（否则随机方向）")]
    public bool useHitNormal = false;

    [Header("偏移量（避免与角色重叠）")]
    public Vector3 offset = Vector3.zero;

    [Header("血液效果生成位置的目标 GameObject（受击点）")]
    public GameObject targetGameObject; // 在 Inspector 中指定，如果不为空，则血液效果将在此位置生成

    private int currentIndex = 0; // 当前使用的预制体索引

    public void SpawnBlood(Vector3 hitPoint, Vector3? hitNormal = null)
    {
        if (bloodPrefabs == null || bloodPrefabs.Count == 0)
            return;

        // 按顺序从列表中选择预制体
        GameObject prefab = bloodPrefabs[currentIndex];
        currentIndex = (currentIndex + 1) % bloodPrefabs.Count;

        // 如果指定了目标 GameObject，则使用其位置，否则使用传入的 hitPoint
        Vector3 originPoint = targetGameObject != null ? targetGameObject.transform.position : hitPoint;

        // 添加随机偏移，让生成位置不完全一致
        Vector3 randomOffset = new Vector3(
            Random.Range(-0.2f, 0.2f),
            Random.Range(-0.2f, 0.2f),
            Random.Range(-0.2f, 0.2f)
        );
        Vector3 spawnPos = originPoint + offset + randomOffset;

        Quaternion rotation;
        if (useHitNormal && hitNormal.HasValue)
        {
            // 使用命中法线计算基础旋转
            Quaternion baseRotation = Quaternion.LookRotation(hitNormal.Value);
            // 如果命中面接近平面，则在其基础上加上随机绕 Y 轴的旋转
            if (Vector3.Dot(hitNormal.Value, Vector3.up) > 0.9f)
            {
                baseRotation *= Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            }
            // 根据预制体需要再做校正（例如贴花默认朝上时，旋转90度）
            rotation = baseRotation * Quaternion.Euler(90f, 0f, 0f);
        }
        else
        {
            // 未传入法线时，使用完全随机的绕 Y 轴旋转
            rotation = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);
        }

        // 实例化血液效果
        GameObject bloodInstance = Instantiate(prefab, spawnPos, rotation);

      
       
        
            Destroy(bloodInstance, 5f);
        
    }

}
