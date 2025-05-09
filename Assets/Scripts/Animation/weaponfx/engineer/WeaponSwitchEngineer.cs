using UnityEngine;

public class WeaponSwitchEngineer : StateMachineBehaviour
{
    // �ݹ����������
    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindChildByName(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    // �ڶ���״̬����ʱ����
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // �ݹ�����������е�����
        Transform weaponA = FindChildByName(animator.transform, "weapon_A");
        Transform weaponB = FindChildByName(animator.transform, "weapon_B");

        // ȷ��Ĭ�������ֻ�� weapon_A ���ڼ���״̬
        if (weaponA != null)
        {
            weaponA.gameObject.SetActive(true);
        }

        if (weaponB != null)
        {
            weaponB.gameObject.SetActive(false);  // ��ʼʱ�ر� weapon_B
        }

       
        if (stateInfo.IsName("Skill2") || stateInfo.IsName("Skill2shoot") )
        {
            if (weaponA != null) weaponA.gameObject.SetActive(false);
            if (weaponB != null) weaponB.gameObject.SetActive(true);
        }
    }

    // �ڶ���״̬�˳�ʱ����
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // �ݹ�����������е�����
        Transform weaponA = FindChildByName(animator.transform, "weapon_A");
        Transform weaponB = FindChildByName(animator.transform, "weapon_B");

        // �˳� Skill2 �� Skill2shoot ״̬ʱ���л��� weapon_A
        if (stateInfo.IsName("noskill") )
        {
            if (weaponA != null) weaponA.gameObject.SetActive(true);
            if (weaponB != null) weaponB.gameObject.SetActive(false);
        }
    }
}
