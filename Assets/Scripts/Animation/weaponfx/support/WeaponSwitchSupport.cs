using UnityEngine;

public class WeaponSwitchSupport : StateMachineBehaviour
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
        // �����������е�����
        Transform weaponA = FindChildByName(animator.transform, "weapon_A");
        Transform weaponB = FindChildByName(animator.transform, "weapon_B");
        Transform weaponC = FindChildByName(animator.transform, "weapon_C"); // �������������Ҳ�����

        // Ĭ�����س� weapon_A �����������
        if (weaponA != null)
        {
            weaponA.gameObject.SetActive(true);  // ȷ�� weapon_A �Ǽ����
        }

        if (weaponB != null)
        {
            weaponB.gameObject.SetActive(false); // Ĭ������ weapon_B
        }

        if (weaponC != null)
        {
            weaponC.gameObject.SetActive(false); // Ĭ�������������� (��ѡ)
        }

        // ����״̬�л�����
        if (weaponA != null && weaponB != null)
        {
            // Skill1��Skill1shoot��Skill2 �� Skill2shoot �л��� weapon_B
            if (stateInfo.IsName("Skill1") || stateInfo.IsName("Skill1shoot") || stateInfo.IsName("Skill2") || stateInfo.IsName("Skill2shoot"))
            {
                weaponA.gameObject.SetActive(false);
                weaponB.gameObject.SetActive(true);
            }
            // NoSkill ״̬�»�ԭΪ weapon_A
            else if (stateInfo.IsName("noskill"))
            {
                weaponA.gameObject.SetActive(true);
                weaponB.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("δ�ҵ�ָ�����������壡");
        }
    }

    // �ڶ���״̬�˳�ʱ���ã����軹ԭ������
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // �˳�״̬ʱ����Ҫ���κβ���������ԭ����
    }
}
