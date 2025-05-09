using UnityEngine;

public class Stopmoving : StateMachineBehaviour
{
    // �������״̬ʱ����
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerController playerController = animator.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.isMoving = false; // ȷ��״̬�ڼ�һֱ�����ƶ�
        }
    }

    // ���˳���״̬ʱ����
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("isMoving", false);
    }
}
