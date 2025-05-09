using System.Collections;
using UnityEngine;
using TMPro; // 引入TextMeshPro命名空间

// 倒计时事件，当倒计时结束后，事件结束
public class TimerEvent : BaseEvent
{
    [Header("倒计时时长")]
    public float time = 0f; // 总倒计时时长

    [Header("文本设置")]
    public TMP_Text timerText; // 用于显示剩余时间的TextMeshPro组件
    public string timerPrefixText = "剩余时间："; // 倒计时的文本前缀

    [Header("中途开启的物体")]
    public GameObject halfwayObject; // 在倒计时一半时要开启的物体
    public GameObject endwayObject; // 在倒计时结束时要开启的物体

    [Header("结束后触发动画的物体")]
    public GameObject elevatorTarget; // 用于获取动画控制器的目标物体

    public TypewriterColorJitterEffect typewriterEffect; // 用于显示结束后的提示文本
    public string startText = "坚持到撤离电梯抵达。"; // 倒计时开始时的提示文本
    public string middleText = "小心<color=#FF0000>上方</color>落下的怪物，用狙击手处理这些敌人。"; // 倒计时结束后的提示文本
    public string endText = "倒计时结束，将所有干员移动至<color=#00FF00>撤离区域</color>以撤离。"; // 倒计时结束后的提示文本

    private float timer = 0f;        // 计时器
    private bool isTiming = false;  // 是否正在计时
    private bool isHalfwayTriggered = false; // 标记是否触发过一半事件
    public Animator elevatorAnimator; // 电梯的Animator
    public AudioSource audioSource;
    public AudioSource audioSource2;


    public override void EnableEvent()
    {
        Debug.Log("开始计时器事件");
        audioSource.Play();
        timer = 0f;
        isTiming = true;
        isHalfwayTriggered = false;

        // 获取电梯的Animator组件
        if (elevatorTarget != null)
        {
            elevatorAnimator = elevatorTarget.GetComponent<Animator>();
            if (elevatorAnimator == null)
            {
                Debug.LogWarning($"{elevatorTarget.name} 上未找到 Animator 组件。");
            }
        }

        // 更新开始提示文本
        if (typewriterEffect != null)
        {
            typewriterEffect.SetText(startText);
        }

        // 初始化文本显示
        UpdateTimerText(time);
    }

    private void Update()
    {
        if (isTiming)
        {
            timer += Time.deltaTime;

            // 剩余时间
            float remainingTime = Mathf.Max(time - timer, 0f);

            // 更新文本显示
            UpdateTimerText(remainingTime);

            // 检测是否到达一半时间，触发中途事件
            if (!isHalfwayTriggered && timer >= time / 2)
            {
                isHalfwayTriggered = true;
                typewriterEffect.SetText(middleText);
                if (halfwayObject != null)
                {
                    halfwayObject.SetActive(true);
                    Debug.Log($"{halfwayObject.name} 已在倒计时一半时开启。");
                }
            }
            if (remainingTime <= 15f)
            {

                endwayObject.SetActive(true);
            }

            // 检测倒计时是否结束
            if (timer >= time)
            {
                isTiming = false;
                Debug.Log("计时结束，触发事件结束。");
                HandleTimerEnd();
                EndEvent();
            }
        }
    }

    private void UpdateTimerText(float remainingTime)
    {
        if (timerText != null)
        {
            string timeText;

            // 在剩余时间 <= 20 秒时，改变数字颜色为红色
            if (remainingTime <= 20f)
            {
                timeText = $"{timerPrefixText}<color=red>{remainingTime:F1}秒</color>";
            }
            else
            {
                timeText = $"{timerPrefixText}{remainingTime:F1}秒";
            }

            timerText.text = timeText; // 更新TextMeshPro文本内容
        }
    }

    private void HandleTimerEnd()
    {
        // 更新结束提示文本
        if (typewriterEffect != null)
        {
            typewriterEffect.SetText(endText);
        }
        
        audioSource2.Play();
        // 触发电梯动画
        if (elevatorAnimator != null)
        {
            elevatorAnimator.SetTrigger("elevatorstart");
            Debug.Log($"触发 {elevatorTarget.name} 的 elevatorStart 动画触发器。");
        }
    }
}
