using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReachingTargetCheck : BaseEvent
{
    [Header("目标点")]
    public Transform targetPosition;

    [Header("提示文本组件")]
    public TypewriterColorJitterEffect typewriterEffect;

    [Header("消息设置")]
    public string[] messages;
    public int messageCount = 1;
    public float messageDisplayInterval = 3f; // 可配置的文本显示间隔

    [Header("相机控制选项")]
    public bool updateCameraBounds = false;
    public PolygonCollider2D newCameraBounds;

    public bool updateCameraFOV = false;
    public float newMinFOV = 15f;
    public float newMaxFOV = 60f;

    [Header("触发对象控制")]
    public GameObject[] objectsToEnable; // 指定需要开启的GameObject数组
    public GameObject[] objectsToDisable; // 指定需要关闭的GameObject数组

    [Header("玩家检测设置")]
    public int requiredPlayerCount = 1; // 需要达到的玩家数量

    private BoxCollider2D triggerCollider;
    private HashSet<GameObject> playersInside = new HashSet<GameObject>(); // 用于记录进入触发器的玩家
    private CameraController cameraController;

    public override void EnableEvent()
    {
        Debug.Log("开始检测是否到达目标");

        if (targetPosition != null)
        {
            transform.position = targetPosition.position;

            // 添加触发器并动态调整大小
            triggerCollider = gameObject.AddComponent<BoxCollider2D>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector2(2f, 2f); // 根据实际需求调整触发器大小
            Debug.Log($"触发器已添加，大小为：{triggerCollider.size}");
        }
        else
        {
            Debug.LogError("目标点未设置，请检查目标点是否已分配。");
        }

        // 缓存 CameraController
        cameraController = FindObjectOfType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogWarning("未找到 CameraController，相机相关功能可能无法正常工作。");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playersInside.Add(collision.gameObject); // 添加进入触发器的玩家
            Debug.Log($"当前玩家数量：{playersInside.Count}/{requiredPlayerCount}");

            if (playersInside.Count >= requiredPlayerCount)
            {
                TriggerEvent();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playersInside.Remove(collision.gameObject); // 移除离开触发器的玩家
            Debug.Log($"当前玩家数量：{playersInside.Count}/{requiredPlayerCount}");
        }
    }

    private void TriggerEvent()
    {
        Debug.Log("检测到足够的玩家到达目标点");

        if (updateCameraBounds && newCameraBounds != null && cameraController != null)
        {
            cameraController.SetCameraBounds(newCameraBounds);
            Debug.Log("相机边界已更新。");
        }

        if (updateCameraFOV && cameraController != null)
        {
            cameraController.minFOV = newMinFOV;
            cameraController.maxFOV = newMaxFOV;
            Debug.Log($"相机FOV范围已更新：Min = {newMinFOV}, Max = {newMaxFOV}");
        }

        // 批量开启GameObject
        if (objectsToEnable != null && objectsToEnable.Length > 0)
        {
            foreach (var obj in objectsToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    Debug.Log($"{obj.name} 已被启用。");
                }
            }
        }

        // 批量关闭GameObject
        if (objectsToDisable != null && objectsToDisable.Length > 0)
        {
            foreach (var obj in objectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    Debug.Log($"{obj.name} 已被禁用。");
                }
            }
        }

        StartCoroutine(DisplayMultipleMessages());
        EndEvent();
    }

    private IEnumerator DisplayMultipleMessages()
    {
        if (messages == null || messages.Length == 0)
        {
            Debug.LogWarning("消息数组为空，无法显示消息。");
            yield break;
        }

        int count = Mathf.Min(messageCount, messages.Length);
        for (int i = 0; i < count; i++)
        {
            yield return DisplayText(messages[i], useTypewriter: true);
        }
    }

    private IEnumerator DisplayText(string text, bool useTypewriter = false)
    {
        if (useTypewriter && typewriterEffect != null)
        {
            typewriterEffect.SetText(text);
            while (typewriterEffect.isTyping)
            {
                yield return null;
            }
        }
        else
        {
            Debug.Log(text);
        }

        yield return new WaitForSeconds(messageDisplayInterval);
    }
}
