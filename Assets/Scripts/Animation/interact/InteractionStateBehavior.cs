using UnityEngine;

public class InteractionStateBehavior : StateMachineBehaviour
{
    private PlayerController playerController;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        playerController = animator.GetComponent<PlayerController>();

        if (playerController != null)
        {
            //playerController.isInteracting = true;
            Debug.Log($"{playerController.gameObject.name} 进入交互状态");
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playerController != null)
        {
            //playerController.isInteracting = false;
            playerController.canShoot = true;

            Debug.Log($"{playerController.gameObject.name} 退出交互状态");
        }
    }
}
