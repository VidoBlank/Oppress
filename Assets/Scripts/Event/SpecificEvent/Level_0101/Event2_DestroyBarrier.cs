using System.Collections;
using UnityEngine;

public class Event2_DestroyBarrier : BaseEvent
{
    public GameObject barrier;
    public PolygonCollider2D updatedCameraBounds; // 新的相机边界


    public string messageAfterDestroy = "士兵会自动锁定并攻击攻击范围内的敌人。";

    // 新的FOV设置
    public float newMinFOV = 10f;
    public float newMaxFOV = 30f;

    private CameraController cameraController;
    public TypewriterColorJitterEffect typewriterEffect;

    private void Start()
    {
        cameraController = FindObjectOfType<CameraController>();
        typewriterEffect = FindObjectOfType<TypewriterColorJitterEffect>();

        if (cameraController == null)
        {
            Debug.LogError("CameraController 未找到，请确保场景中有相机控制器。");
        }
        if (typewriterEffect == null)
        {
            Debug.LogWarning("TypewriterColorJitterEffect 未找到，文本显示将无法使用打字机效果！");
        }
    }

    public override void EnableEvent()
    {
        Debug.Log("开始摧毁障碍事件");
    }

    private void Update()
    {
        // 当障碍物不存在时（表示已被摧毁）
        if (!barrier)
        {
            // 更新相机边界
            if (cameraController != null && updatedCameraBounds != null)
            {
                cameraController.SetCameraBounds(updatedCameraBounds);
                Debug.Log("相机边界已更新。");
            }

            // 更新相机FOV范围
            if (cameraController != null)
            {
                cameraController.minFOV = newMinFOV;
                cameraController.maxFOV = newMaxFOV;
                Debug.Log($"相机FOV范围已更新: 最小 {newMinFOV}, 最大 {newMaxFOV}");
            }

            // 显示文本提示
            StartCoroutine(DisplayText(messageAfterDestroy));
            EndEvent();
        }
    }

    private IEnumerator DisplayText(string text)
    {
        if (typewriterEffect != null)
        {
            typewriterEffect.SetText(text);

            // 等待文本打字效果完成
            while (typewriterEffect.isTyping)
            {
                yield return null;
            }

        }
    }
}
