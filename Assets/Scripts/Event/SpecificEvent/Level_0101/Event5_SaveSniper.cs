using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Event5_SaveSniper : BaseEvent
{
    public GameObject targetSniper; // 狙击手对象
    public CapsuleCollider2D sniperCollider;
    public Rigidbody2D sniperRigidbody;
    public TypewriterColorJitterEffect typewriterEffect;
    public float textDisplayDelay = 3f; // 每段文本显示之间的间隔

    public TextMeshProUGUI anotherTextMeshPro;

    // 新增引用：UI对象，包含CanvasGroup和test子物体
    public GameObject uiObject;

    // 新增引用：指定GameObject
    public GameObject specifiedGameObject;

    // 新增相机边界更新引用
    public PolygonCollider2D updatedCameraBounds;

    private CameraController cameraController;

    private void Start()
    {
        if (targetSniper != null)
        {
            sniperCollider = targetSniper.GetComponent<CapsuleCollider2D>();
            sniperRigidbody = targetSniper.GetComponent<Rigidbody2D>();
        }
        else
        {
            Debug.LogError("目标狙击手对象未指定！");
        }

        if (typewriterEffect == null)
        {
            typewriterEffect = FindObjectOfType<TypewriterColorJitterEffect>();
            if (typewriterEffect == null)
            {
                Debug.LogWarning("未找到TypewriterColorJitterEffect，文本将使用Debug.Log显示。");
            }
        }

        if (anotherTextMeshPro == null)
        {
            Debug.LogWarning("未分配anotherTextMeshPro，用于显示文本。");
        }

        cameraController = FindObjectOfType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("未找到CameraController，无法更新相机边界。");
        }
    }

    public override void EnableEvent()
    {
        Debug.Log("开始拯救狙击手事件");

        // 启用碰撞体
        if (sniperCollider != null)
        {
            sniperCollider.enabled = true;
        }

        // 取消Rigidbody2D位置冻结约束
        if (sniperRigidbody != null)
        {
            sniperRigidbody.constraints = RigidbodyConstraints2D.None;
            sniperRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // 开始协程事件流程
        StartCoroutine(SaveSniperProcess());
    }

    private IEnumerator SaveSniperProcess()
    {
        // 获取CanvasGroup并设alpha为1
        if (uiObject != null)
        {
            CanvasGroup cg = uiObject.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
            }
            else
            {
                Debug.LogWarning("uiObject上未找到CanvasGroup！");
            }

            // 关闭名为"test"的子物体
            Transform testChild = uiObject.transform.Find("test");
            if (testChild != null)
            {
                testChild.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("未找到名为 'test' 的子物体。");
            }
        }
        else
        {
            Debug.LogWarning("uiObject未设置！");
        }

        // 第一条文本（富文本颜色）
        string firstMessage = "发现一名<color=#FF0000>身负重伤</color>的干员。控制干员右键点击他即可<color=#00FF00>救援</color>该目标。";
        yield return DisplayText(firstMessage, useTypewriter: true);

        // 等待目标狙击手从倒地状态恢复
        PlayerController sniperController = targetSniper.GetComponent<PlayerController>();
        if (sniperController == null)
        {
            Debug.LogError("目标狙击手缺少 PlayerController 脚本，无法判断倒地状态。");
            EndEvent();
            yield break;
        }

        // 等待狙击手isKnockedDown为false且不为dead
        yield return new WaitUntil(() => !sniperController.isKnockedDown && !sniperController.isDead);
        Transform[] allTransforms = FindObjectsOfType<Transform>(true);
        foreach (Transform sniperframe in allTransforms)
        {
            if (sniperframe.name == "PlayerFrame_2")
            {
                sniperframe.gameObject.SetActive(true);
                break;
            }
        }
        // 显示“目标已连接上tau—prime站点网络”在独立的文本组件上
        string rescueMessage = "目标资料已重新连接上tau—prime站点网络。职能：狙击手";
        if (anotherTextMeshPro != null)
        {
            anotherTextMeshPro.text = rescueMessage;
            anotherTextMeshPro.gameObject.SetActive(true);

            // 显示一段时间后隐藏
            yield return new WaitForSeconds(textDisplayDelay);
            anotherTextMeshPro.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log(rescueMessage);
        }

        // 显示治疗教程文本
        string tutorialMessage = "使用步枪手干员的技能<color=#FFFF00>“急救枪”[快捷键W]</color>，选中并<color=#00FF00>治疗</color>目标友军。";
        // 启用指定的GameObject
        if (specifiedGameObject != null)
        {
            specifiedGameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("指定的GameObject未设置！");
        }
        yield return DisplayText(tutorialMessage, useTypewriter: true);

        string infoBoxMessage2 = "点击<color=#FFFF00>干员头像</color>或使用数字快捷键<color=#FFFF00>1，2，3，4</color>即可快速选择目标干员。";
        yield return DisplayText(infoBoxMessage2, useTypewriter: true);
        // 更新相机边界
        UpdateCameraBounds();

        // 事件结束
        EndEvent();
        // 等待治疗教程完成
        yield return WaitForHealing(sniperController);

        // 显示治疗完成后的信息框提示

           

        // 更新相机边界
        UpdateCameraBounds();

        // 事件结束
        EndEvent();
    }

    private void UpdateCameraBounds()
    {
        if (cameraController != null && updatedCameraBounds != null)
        {
            cameraController.SetCameraBounds(updatedCameraBounds);
            Debug.Log("相机边界已更新。");
        }
        else
        {
            Debug.LogWarning("无法更新相机边界，未设置CameraController或更新的边界。");
        }
    }

    private IEnumerator WaitForHealing(PlayerController sniperController)
    {
        // 等待狙击手的治疗完成
        yield return new WaitUntil(() => sniperController.health >= sniperController.attribute.maxhealth);

        // 停止教程相关操作
        Debug.Log("治疗教程完成！");
    }

    private IEnumerator DisplayText(string text, bool useTypewriter = false)
    {
        if (useTypewriter && typewriterEffect != null)
        {
            typewriterEffect.SetText(text);
            // 等待打字完成
            while (typewriterEffect.isTyping)
            {
                yield return null;
            }
        }
        else
        {
            // 如果未使用typewriter或没有typewriterEffect，则直接输出
            Debug.Log(text);
        }

        // 等待一段时间再继续下一段文本
        yield return new WaitForSeconds(textDisplayDelay);
    }
}
