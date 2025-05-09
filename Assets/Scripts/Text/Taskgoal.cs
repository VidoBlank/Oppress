using UnityEngine;
using TMPro;
using System.Collections;

public class Taskgoal : MonoBehaviour
{
    // 预制体变量，预制体中需包含 Canvas（且带 CanvasGroup 组件）和 TextMeshProUGUI 组件
    public GameObject taskObjectivePrefab;

    // 对应的任务目标文本
    [TextArea]
    public string objectiveText = "任务目标文本";

    // 渐变进入和退出的时长（单位：秒）
    public float fadeDuration = 1f;

    // 显示完全显示状态的时长（单位：秒）
    public float displayDuration = 3f;

    public CanvasGroup canvasGroup;
    public TextMeshProUGUI textComponent;
    private GameObject instance;

    // 供 UI 触发调用的方法
    public void ShowTaskGoal()
    {
        if (taskObjectivePrefab != null)
        {
            // 实例化预制体
            instance = Instantiate(taskObjectivePrefab);

            // 获取 CanvasGroup 组件（用于控制透明度）
            canvasGroup = instance.GetComponentInChildren<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogError("预制体中未找到 CanvasGroup 组件，请确保预制体上挂载了 CanvasGroup 以便控制透明度。");
                return;
            }

            // 获取 TextMeshProUGUI 组件，并设置文本内容
            textComponent = instance.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = objectiveText;
            }
            else
            {
                Debug.LogError("预制体中未找到 TextMeshProUGUI 组件，请确保预制体中包含该组件。");
            }

            // 初始化 Canvas 透明度为0（全透明）
            canvasGroup.alpha = 0;

            // 开启协程实现渐变动画
            StartCoroutine(FadeInAndOut());
        }
        else
        {
            Debug.LogError("未设置任务目标预制体！");
        }
    }

    IEnumerator FadeInAndOut()
    {
        float timer = 0f;

        // 渐变进入：从 alpha 0 到 1
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1;

        // 保持显示 displayDuration 秒
        yield return new WaitForSeconds(displayDuration);

        // 渐变退出：从 alpha 1 到 0
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0;

        // 动画结束后关闭预制体
        instance.SetActive(false);
    }
}
