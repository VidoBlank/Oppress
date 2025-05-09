using System.Collections;
using UnityEngine;

public class Event9_2_MeetEachOther : BaseEvent
{
    [Header("后续物体控制")]
    public GameObject[] objectsToEnableSequentially; // 依次开启的物体
    public float[] enableDelays = { 1.5f, 1.5f, 1.5f, 2f }; // 每个物体的开启间隔时间

    [Header("音效控制")]
    public AudioClip activationSound; // 触发时的音效
    public AudioSource audioSource; // 用于播放音效的 AudioSource

    public override void EnableEvent()
    {
        base.EnableEvent();
        Debug.Log("Event9_2_MeetEachOther 事件已触发");

        // 开始依次启用物体
        StartCoroutine(EnableObjectsSequentially());
    }

    private IEnumerator EnableObjectsSequentially()
    {
        Debug.Log("开始依次启用物体...");
        for (int i = 0; i < objectsToEnableSequentially.Length; i++)
        {
            // 等待指定的时间间隔（如果没有设置，默认 1.5 秒）
            yield return new WaitForSeconds(i < enableDelays.Length ? enableDelays[i] : 1.5f);

            // 启用物体
            if (objectsToEnableSequentially[i] != null)
            {
                objectsToEnableSequentially[i].SetActive(true);
                Debug.Log($"{objectsToEnableSequentially[i].name} 已被启用。");

                // 播放音效
                if (audioSource != null && activationSound != null)
                {
                    audioSource.PlayOneShot(activationSound);
                    Debug.Log("播放音效：" + activationSound.name);
                }
            }
        }
        Debug.Log("所有物体已依次启用。");
        EndEvent();
    }
}