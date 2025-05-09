using UnityEngine;

public class Stopmoving : StateMachineBehaviour
{
    // 当进入此状态时调用
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerController playerController = animator.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.isMoving = false; // 确保状态期间一直禁用移动
        }
    }

    // 当退出此状态时调用
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("isMoving", false);
    }
}
