using UnityEngine;

public class LevelBasedStateSpeed : StateMachineBehaviour
{
    private PlayerController playerController;

    // 在动画状态进入时调用
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playerController == null)
        {
            // 获取 PlayerController
            playerController = animator.GetComponentInParent<PlayerController>();
        }

        if (playerController != null)
        {
            // ✅ 使用 TotalAttackDelay（插槽修正后的最终值）
            float attackDelay = playerController.TotalAttackDelay;

            // 防止除以 0
            float speedValue = attackDelay > 0f ? 1f / attackDelay : 1f;

            // 设置 Animator 参数
            animator.SetFloat("Attackspeed", speedValue);

            Debug.Log($"状态 {stateInfo.shortNameHash} 的播放速度已根据攻速设置为 {speedValue} (攻击间隔 = {attackDelay})");
        }
        else
        {
            Debug.LogError("未找到 PlayerController，无法设置动画速度！");
        }
    }
}
