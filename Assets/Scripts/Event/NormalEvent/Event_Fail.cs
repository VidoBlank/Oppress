using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Event_Fail : BaseEvent
{
    public CanvasGroup uiCanvasGroup; // 游戏失败提示 UI 的 CanvasGroup
    public float fadeDuration = 2f; // UI 渐显的时间
    public string failSceneName = "LoadScene_Fail"; // 游戏失败后加载的场景名称

    private List<PlayerController> playersInScene = new List<PlayerController>(); // 场景内的玩家列表
    private bool isGameOver = false; // 是否已经触发游戏结束

    private void Start()
    {
        // 获取场景内的所有玩家
        playersInScene.AddRange(FindObjectsOfType<PlayerController>());

        if (playersInScene.Count == 0)
        {
            Debug.LogError("场景内没有找到玩家！");
        }

        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 0f;
            uiCanvasGroup.gameObject.SetActive(false); // 初始隐藏 UI
        }
    }

    private void Update()
    {
        if (isGameOver) return; // 如果游戏已结束，跳过检测

        // 检测所有玩家是否都处于倒地状态
        bool allPlayersDown = true;
        foreach (var player in playersInScene)
        {
            if (!player.isKnockedDown)
            {
                allPlayersDown = false;
                break;
            }
        }

        if (allPlayersDown)
        {
            StartCoroutine(HandleGameOver());
        }
    }

    private IEnumerator HandleGameOver()
    {
        isGameOver = true;

        // 暂停游戏
        Time.timeScale = 0f;
        Debug.Log("所有玩家都倒地，游戏失败。");

        // 显示失败 UI
        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.gameObject.SetActive(true);

            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime; // 使用 unscaledDeltaTime，不受 Time.timeScale 影响
                uiCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }

            uiCanvasGroup.alpha = 1f;
        }

        // 等待一段时间
        yield return new WaitForSecondsRealtime(3f);

        // 恢复游戏并加载失败场景
        Time.timeScale = 1f;
        SceneManager.LoadScene(failSceneName);
    }
}
