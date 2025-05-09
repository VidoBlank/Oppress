using UnityEngine;

public class TimeIndependentAnimationBehaviour : StateMachineBehaviour
{
    private float originalSpeed = 1f; // 记录原始动画速度

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.updateMode = AnimatorUpdateMode.UnscaledTime; // 确保 Animator 使用 UnscaledTime
        originalSpeed = animator.speed; // 记录原始速度
        animator.speed = originalSpeed / Mathf.Max(Time.timeScale, 0.01f); // 防止除零
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 确保动画持续使用 UnscaledTime
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        animator.speed = originalSpeed / Mathf.Max(Time.timeScale, 0.01f);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.speed = originalSpeed; // 恢复正常速度
        animator.updateMode = AnimatorUpdateMode.Normal;
    }
}
