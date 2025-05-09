using UnityEngine;

public class WeaponSwitchEngineer : StateMachineBehaviour
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
        // 递归查找子物体中的武器
        Transform weaponA = FindChildByName(animator.transform, "weapon_A");
        Transform weaponB = FindChildByName(animator.transform, "weapon_B");

        // 确保默认情况下只有 weapon_A 处于激活状态
        if (weaponA != null)
        {
            weaponA.gameObject.SetActive(true);
        }

        if (weaponB != null)
        {
            weaponB.gameObject.SetActive(false);  // 初始时关闭 weapon_B
        }

       
        if (stateInfo.IsName("Skill2") || stateInfo.IsName("Skill2shoot") )
        {
            if (weaponA != null) weaponA.gameObject.SetActive(false);
            if (weaponB != null) weaponB.gameObject.SetActive(true);
        }
    }

    // 在动画状态退出时调用
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 递归查找子物体中的武器
        Transform weaponA = FindChildByName(animator.transform, "weapon_A");
        Transform weaponB = FindChildByName(animator.transform, "weapon_B");

        // 退出 Skill2 或 Skill2shoot 状态时，切换回 weapon_A
        if (stateInfo.IsName("noskill") )
        {
            if (weaponA != null) weaponA.gameObject.SetActive(true);
            if (weaponB != null) weaponB.gameObject.SetActive(false);
        }
    }
}
