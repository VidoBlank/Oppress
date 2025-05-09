using UnityEngine;

public class TimeIndependentAnimationBehaviour : StateMachineBehaviour
{
    private float originalSpeed = 1f; // ��¼ԭʼ�����ٶ�

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.updateMode = AnimatorUpdateMode.UnscaledTime; // ȷ�� Animator ʹ�� UnscaledTime
        originalSpeed = animator.speed; // ��¼ԭʼ�ٶ�
        animator.speed = originalSpeed / Mathf.Max(Time.timeScale, 0.01f); // ��ֹ����
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // ȷ����������ʹ�� UnscaledTime
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        animator.speed = originalSpeed / Mathf.Max(Time.timeScale, 0.01f);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.speed = originalSpeed; // �ָ������ٶ�
        animator.updateMode = AnimatorUpdateMode.Normal;
    }
}
