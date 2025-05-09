using UnityEngine;

public class FinishAttackBehavior : StateMachineBehaviour
{
    // 当进入动画状态时调用
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 尝试在父层级中获取攻击脚本组件
        Enemy enemy = animator.GetComponentInParent<Enemy>();

        // 如果未找到，再尝试在子级中查找
        if (enemy == null)
        {
            enemy = animator.GetComponentInChildren<Enemy>();
        }

        // 如果最终找到了 Enemy 组件，则调用 FinishAttack 方法
        if (enemy != null)
        {
            enemy.FinishAttack();
        }
        else
        {
            Debug.LogWarning("未在父或子对象中找到 Enemy 组件！");
        }
    }
}
