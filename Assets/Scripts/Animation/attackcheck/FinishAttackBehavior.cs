using UnityEngine;

public class FinishAttackBehavior : StateMachineBehaviour
{
    // �����붯��״̬ʱ����
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // �����ڸ��㼶�л�ȡ�����ű����
        Enemy enemy = animator.GetComponentInParent<Enemy>();

        // ���δ�ҵ����ٳ������Ӽ��в���
        if (enemy == null)
        {
            enemy = animator.GetComponentInChildren<Enemy>();
        }

        // ��������ҵ��� Enemy ���������� FinishAttack ����
        if (enemy != null)
        {
            enemy.FinishAttack();
        }
        else
        {
            Debug.LogWarning("δ�ڸ����Ӷ������ҵ� Enemy �����");
        }
    }
}
