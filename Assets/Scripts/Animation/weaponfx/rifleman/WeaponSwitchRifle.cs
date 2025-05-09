using UnityEngine;

public class WeaponSwitchRifle : StateMachineBehaviour
{
    // 递归查找子物体
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

    // 在动画状态进入时调用
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 查找子物体中的武器
        Transform weaponA = FindChildByName(animator.transform, "weapon_A");
        Transform weaponB = FindChildByName(animator.transform, "weapon_B");
        Transform weaponC = FindChildByName(animator.transform, "weapon_C");
        Transform weaponD = FindChildByName(animator.transform, "weapon_D");

        if (weaponA != null && weaponB != null && weaponC != null && weaponD != null)
        {
            // Skill1 和 Skill1shoot 切换到 weapon_B
            if (stateInfo.IsName("Skill1") || stateInfo.IsName("Skill1shoot"))
            {
                weaponA.gameObject.SetActive(false);
                weaponB.gameObject.SetActive(true);
                weaponC.gameObject.SetActive(false);
                weaponD.gameObject.SetActive(false);
            }
            // Skill2 和 Skill2shoot 切换到 weapon_C
            else if (stateInfo.IsName("Skill2") || stateInfo.IsName("Skill2shoot"))
            {
                weaponA.gameObject.SetActive(false);
                weaponB.gameObject.SetActive(false);
                weaponC.gameObject.SetActive(true);
                weaponD.gameObject.SetActive(false);
            }
            // Skill3 切换到 weapon_D
            else if (stateInfo.IsName("Skill3") || stateInfo.IsName("Skill3shoot"))
            {
                weaponA.gameObject.SetActive(false);
                weaponB.gameObject.SetActive(false);
                weaponC.gameObject.SetActive(false);
                weaponD.gameObject.SetActive(true);
            }
            // NoSkill 状态下还原为 weapon_A
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
            Debug.LogWarning("未找到指定的武器物体！");
        }
    }
}
