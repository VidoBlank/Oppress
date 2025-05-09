using System.Collections;
using UnityEngine;

public class ShieldVibrationEffect : MonoBehaviour
{
    [Header("振动目标")]
    public GameObject vibrationObject; // 要振动的对象，若未指定则默认使用本物体

    [Header("振动参数")]
    public float vibrationDuration = 0.3f;    // 振动持续时间
    public float vibrationIntensity = 0.05f;  // 最大X轴位移
    public float frequency = 20f;             // 振动频率

    private Shield shield;         // 引用 Shield 组件
    private float lastHP;          // 上一帧的护盾血量
    private bool isVibrating = false;

    private void Start()
    {
        // 获取同一物体上的 Shield 组件
        shield = GetComponent<Shield>();
        if (shield == null)
        {
            Debug.LogError("ShieldVibrationEffect 需要依附在含有 Shield 组件的 GameObject 上！");
            enabled = false;
            return;
        }

        // 若未指定振动对象，则默认使用当前物体
        if (vibrationObject == null)
        {
            vibrationObject = gameObject;
        }

        // 初始化血量记录
        lastHP = shield.CurrentHP;
    }

    private void Update()
    {
        // 检测护盾血量是否降低（即受到伤害）
        if (shield.CurrentHP < lastHP)
        {
            if (!isVibrating)
            {
                StartCoroutine(Vibrate());
            }
        }
        lastHP = shield.CurrentHP;
    }

    private IEnumerator Vibrate()
    {
        isVibrating = true;
        // 记录初始局部位置，使用 localPosition 以免受父物体影响
        Vector3 originalPos = vibrationObject.transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < vibrationDuration)
        {
            // 衰减系数，让震动逐渐减弱
            float damping = 1 - (elapsedTime / vibrationDuration);
            // 计算正弦振动的X轴偏移（频率转换为 2π * frequency）
            float xOffset = Mathf.Sin(elapsedTime * frequency * Mathf.PI * 2) * vibrationIntensity * damping;
            vibrationObject.transform.localPosition = originalPos + new Vector3(xOffset, 0, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 震动结束后复位
        vibrationObject.transform.localPosition = originalPos;
        isVibrating = false;
    }
}
