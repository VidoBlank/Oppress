using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Event11_Success : BaseEvent
{
    [Header("UI设置")]
    public CanvasGroup uiCanvasGroup; // 指定的UI物体上的CanvasGroup
    public float fadeDuration = 2f; // UI从0到1的过渡时间

    public override void EnableEvent()
    {
        StartCoroutine(HandleEventSuccess());
    }

    private IEnumerator HandleEventSuccess()
    {
        if (uiCanvasGroup != null)
        {
            
            // 将CanvasGroup的alpha从0过渡到1
            float elapsedTime = 0f;
            uiCanvasGroup.alpha = 0f;
            uiCanvasGroup.gameObject.SetActive(true);

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                uiCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }

            uiCanvasGroup.alpha = 1f;
        }
        else
        {
            Debug.LogWarning("未指定CanvasGroup组件！");
        }

        // 暂停游戏
        Time.timeScale = 0f;
        Debug.Log("游戏已暂停。");

        // 等待用户确认或自动结束场景
        yield return new WaitForSecondsRealtime(3f); // 使用实时等待，不受Time.timeScale影响
        Time.timeScale = 1f;
        // 结束场景并触发结束事件
        SceneManager.LoadScene("LoadScene_out"); // 替换为你的结束场景名称
        EndEvent();
    }
}
