using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIDissolveController : MonoBehaviour
{
    [Header("指定需要替换材质实例的 UI Image")]
    public Image targetImage;

    [Header("延时多久开始过渡（秒）")]
    public float delayBeforeTransition = 2f;

    [Header("过渡持续时间（秒）")]
    public float transitionDuration = 1f;

    void Start()
    {
        // 检查 targetImage 是否存在
        if (targetImage == null)
        {
            Debug.LogError("请指定目标 UI Image！");
            return;
        }

        // 替换材质实例，避免影响共享材质
        if (targetImage.material != null)
        {
            targetImage.material = new Material(targetImage.material);
        }
        else
        {
            Debug.LogError("目标 UI Image 没有指定材质！");
            return;
        }

        // 初始化 _FullAlphaDissolveFade 为 1
        targetImage.material.SetFloat("_FullAlphaDissolveFade", 1f);

        // 延时后启动过渡效果
        StartCoroutine(TransitionDissolveFade());
    }

    IEnumerator TransitionDissolveFade()
    {
        // 延时等待
        yield return new WaitForSeconds(delayBeforeTransition);

        float elapsedTime = 0f;
        float startValue = 1f;
        float targetValue = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float currentValue = Mathf.Lerp(startValue, targetValue, elapsedTime / transitionDuration);
            targetImage.material.SetFloat("_FullAlphaDissolveFade", currentValue);
            yield return null;
        }
        // 确保最终值设置为目标值
        targetImage.material.SetFloat("_FullAlphaDissolveFade", targetValue);
    }
}
