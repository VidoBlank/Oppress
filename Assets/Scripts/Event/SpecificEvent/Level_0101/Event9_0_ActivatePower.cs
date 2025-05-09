using System.Collections;
using UnityEngine;

public class Event9_0_ActivatePower : BaseEvent
{
    [Header("指定的目标物体")]
    public GameObject targetObject; // 用于触发动画和组件逻辑的目标物体

    [Header("指定的发光材质")]
    public Material[] emissionMaterials; // 需要进行发光效果的材质数组

    [Header("发光过渡参数")]
    public Color targetEmissionColor = Color.yellow; // 目标发光颜色（黄色）
    public float transitionDuration = 2f;            // 发光过渡时间

    [Header("新物体控制")]
    public GameObject objectToActivate;        // 要启用的物体
    public Transform moveTargetPosition;       // 移动的目标位置

    private Animator targetAnimator;           // Animator 组件
    private BoxCollider2D targetCollider;      // BoxCollider2D 组件
    private MonoBehaviour ammoBoxComponent;    // 假设 AmmoBox 是 MonoBehaviour 的子类
    public Outline targetOutline;              // Outline 组件
    public AudioSource audioSource;            // 用于播放音效的音频源

    public override void EnableEvent()
    {
        Debug.Log("开始触发 PowerUp 事件");

        // 处理目标物体逻辑
        HandleTargetObject();

        // 处理发光材质过渡逻辑
        if (emissionMaterials != null && emissionMaterials.Length > 0)
        {
            StartCoroutine(TransitionEmission());
        }
        else
        {
            Debug.LogWarning("未指定任何发光材质！");
        }

        // 启用新物体并移动
        ActivateAndMoveObject();
    }

    private void HandleTargetObject()
    {
        if (targetObject == null)
        {
            Debug.LogError("未设置目标物体，请在 Inspector 中指定！");
            return;
        }

        // 获取并触发 Animator 动画
        targetAnimator = targetObject.GetComponent<Animator>();
        if (targetAnimator != null)
        {
            targetAnimator.SetTrigger("powerup");
            StartCoroutine(FadeInAudio());
            Debug.Log("目标物体 Animator 已触发 powerup Trigger");
        }

        // 启用 Outline 组件
        targetOutline = targetObject.GetComponent<Outline>();
        if (targetOutline != null)
        {
            targetOutline.enabled = true; // 启用 Outline
            Debug.Log("目标物体的 Outline 组件已启用");
        }
        else
        {
            Debug.LogWarning("目标物体上未找到 Outline 组件！");
        }

        // 禁用 AmmoBox 组件
        ammoBoxComponent = targetObject.GetComponent<MonoBehaviour>();
        if (ammoBoxComponent != null)
        {
            ammoBoxComponent.enabled = false;
            Debug.Log("AmmoBox 组件已关闭");
        }

        // 禁用 BoxCollider2D
        targetCollider = targetObject.GetComponent<BoxCollider2D>();
        if (targetCollider != null)
        {
            targetCollider.enabled = false;
            Debug.Log("BoxCollider2D 组件已关闭");
        }
    }

    private IEnumerator TransitionEmission()
    {
        Debug.Log("开始发光效果过渡...");

        foreach (Material mat in emissionMaterials)
        {
            if (mat != null && mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION"); // 启用发光
                Color initialColor = mat.GetColor("_EmissionColor"); // 初始颜色
                float elapsedTime = 0f;

                while (elapsedTime < transitionDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsedTime / transitionDuration);
                    Color currentColor = Color.Lerp(initialColor, targetEmissionColor, t);
                    mat.SetColor("_EmissionColor", currentColor);
                    yield return null;
                }

                mat.SetColor("_EmissionColor", targetEmissionColor);
                Debug.Log($"材质 {mat.name} 发光颜色过渡完成。");
            }
            else
            {
                Debug.LogWarning($"材质 {mat?.name} 未找到 _EmissionColor 属性");
            }
        }
    }

    private IEnumerator FadeInAudio()
    {
        if (audioSource != null)
        {
            audioSource.volume = 0f;
            audioSource.Play();
            float elapsedTime = 0f;

            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                audioSource.volume = Mathf.Clamp01(elapsedTime / transitionDuration);
                yield return null;
            }

            audioSource.volume = 1f;
            Debug.Log("音效音量已从 0 过渡到 1。");
        }
        else
        {
            Debug.LogWarning("未找到 AudioSource 组件，无法播放音效。");
        }
    }

    private void ActivateAndMoveObject()
    {
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
            Debug.Log($"物体已启用: {objectToActivate.name}");

            // 移动物体到指定位置
            PlayerController playerController = objectToActivate.GetComponent<PlayerController>();
            if (playerController != null && moveTargetPosition != null)
            {
                playerController.MoveToTarget(moveTargetPosition.position);
                Debug.Log($"{objectToActivate.name} 正在移动到指定位置: {moveTargetPosition.position}");
            }
            else
            {
                Debug.LogWarning("PlayerController 组件或目标位置未找到，无法移动物体！");
            }
        }
        else
        {
            Debug.LogWarning("未指定要启用的物体！");
        }
    }
}
