using UnityEngine;

public class WeaponSwitchRifle : StateMachineBehaviour
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
        Transform weaponC = FindChildByName(animator.transform, "weapon_C");
        Transform weaponD = FindChildByName(animator.transform, "weapon_D");

        if (weaponA != null && weaponB != null && weaponC != null && weaponD != null)
        {
            // Skill1 �� Skill1shoot �л��� weapon_B
            if (stateInfo.IsName("Skill1") || stateInfo.IsName("Skill1shoot"))
            {
                weaponA.gameObject.SetActive(false);
                weaponB.gameObject.SetActive(true);
                weaponC.gameObject.SetActive(false);
                weaponD.gameObject.SetActive(false);
            }
            // Skill2 �� Skill2shoot �л��� weapon_C
            else if (stateInfo.IsName("Skill2") || stateInfo.IsName("Skill2shoot"))
            {
                weaponA.gameObject.SetActive(false);
                weaponB.gameObject.SetActive(false);
                weaponC.gameObject.SetActive(true);
                weaponD.gameObject.SetActive(false);
            }
            // Skill3 �л��� weapon_D
            else if (stateInfo.IsName("Skill3") || stateInfo.IsName("Skill3shoot"))
            {
                weaponA.gameObject.SetActive(false);
                weaponB.gameObject.SetActive(false);
                weaponC.gameObject.SetActive(false);
                weaponD.gameObject.SetActive(true);
            }
            // NoSkill ״̬�»�ԭΪ weapon_A
            else if (stateInfo.IsName("noskill"))
            {
                weaponA.gameObject.SetActive(true);
                weaponB.gameObject.SetActive(false);
                weaponC.gameObject.SetActive(false);
                weaponD.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("δ�ҵ�ָ�����������壡");
        }
    }
}
