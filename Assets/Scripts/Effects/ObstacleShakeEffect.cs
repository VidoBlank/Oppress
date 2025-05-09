using UnityEngine;
using System.Collections;

public class ObstacleImpactVibration : MonoBehaviour
{
    [Header("振动目标")]
    public GameObject vibrationObject; // 要振动的对象，若未指定则默认使用本物体

    [Header("振动参数")]
    public float vibrationDuration = 0.3f;    // 振动持续时间
    public float vibrationIntensity = 0.05f;  // 最大X轴位移
    public float frequency = 20f;             // 振动频率

    private Obstacle obstacle;   // 引用障碍物组件
    private float lastHealth;    // 上一次的生命值记录
    private bool isVibrating = false; // 标记是否正在振动

    private void Start()
    {
        // 获取同一物体上的 Obstacle 组件
        obstacle = GetComponent<Obstacle>();
        if (obstacle == null)
        {
            Debug.LogError("ObstacleImpactVibration 需要依附在含有 Obstacle 组件的 GameObject 上！");
            enabled = false;
            return;
        }

        // 若未指定振动对象，则默认使用当前物体
        if (vibrationObject == null)
        {
            vibrationObject = gameObject;
        }

        // 初始化生命值记录
        lastHealth = obstacle.currentHealth;
    }

    private void Update()
    {
        // 检测生命值是否降低（受到伤害）
        if (obstacle.currentHealth < lastHealth)
        {
            if (!isVibrating)
            {
                StartCoroutine(Vibrate());
            }
        }
        lastHealth = obstacle.currentHealth;
    }

    private IEnumerator Vibrate()
    {
        isVibrating = true;
        // 记录初始局部位置，使用 localPosition 可避免受到父物体影响
        Vector3 originalPos = vibrationObject.transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < vibrationDuration)
        {
            // 使用一个随时间衰减的系数，使振动逐渐减弱
            float damping = 1 - (elapsedTime / vibrationDuration);
            // 计算正弦振动的X轴偏移（2π频率转换）
            float xOffset = Mathf.Sin(elapsedTime * frequency * Mathf.PI * 2) * vibrationIntensity * damping;
            vibrationObject.transform.localPosition = originalPos + new Vector3(xOffset, 0, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 振动结束后复位
        vibrationObject.transform.localPosition = originalPos;
        isVibrating = false;
    }
}
