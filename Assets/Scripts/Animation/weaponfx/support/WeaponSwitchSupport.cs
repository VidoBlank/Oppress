using UnityEngine;

public class WeaponSwitchSupport : StateMachineBehaviour
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
        Transform weaponC = FindChildByName(animator.transform, "weapon_C"); // 如果有其他武器也可添加

        // 默认隐藏除 weapon_A 外的其他武器
        if (weaponA != null)
        {
            weaponA.gameObject.SetActive(true);  // 确保 weapon_A 是激活的
        }

        if (weaponB != null)
        {
            weaponB.gameObject.SetActive(false); // 默认隐藏 weapon_B
        }

        if (weaponC != null)
        {
            weaponC.gameObject.SetActive(false); // 默认隐藏其他武器 (可选)
        }

        // 根据状态切换武器
        if (weaponA != null && weaponB != null)
        {
            // Skill1、Skill1shoot、Skill2 和 Skill2shoot 切换到 weapon_B
            if (stateInfo.IsName("Skill1") || stateInfo.IsName("Skill1shoot") || stateInfo.IsName("Skill2") || stateInfo.IsName("Skill2shoot"))
            {
                weaponA.gameObject.SetActive(false);
                weaponB.gameObject.SetActive(true);
            }
            // NoSkill 状态下还原为 weapon_A
            else if (stateInfo.IsName("noskill"))
            {
                weaponA.gameObject.SetActive(true);
                weaponB.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("未找到指定的武器物体！");
        }
    }

    // 在动画状态退出时调用（无需还原武器）
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 退出状态时不需要做任何操作，不还原武器
    }
}
