using UnityEngine;

public class WeaponEffectOnStateEnter : StateMachineBehaviour
{
    private ParticleSystem fxShootfire;
    private bool isFacingRight = true; // 记录角色的朝向
    private PlayerController playerController; // 用于获取角色属性

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playerController == null)
        {
            playerController = animator.GetComponentInParent<PlayerController>();
        }

        Transform weaponA = FindChildByName(animator.transform, "weapon_A");
        Transform weaponB = FindChildByName(animator.transform, "weapon_B");
        Transform weaponC = FindChildByName(animator.transform, "weapon_C");
        Transform weaponD = FindChildByName(animator.transform, "weapon_D");

        isFacingRight = animator.transform.localScale.x > 0;

        if (weaponA != null && weaponA.gameObject.activeSelf)
        {
            PlayEffect(weaponA, "FX_Shootfire_1");
        }
        else if (weaponB != null && weaponB.gameObject.activeSelf)
        {
            PlayEffect(weaponB, "FX_Shootfire_2");
        }
        else if (weaponC != null && weaponC.gameObject.activeSelf)
        {
            PlayEffect(weaponC, "FX_Shootfire_3");
        }
        else if (weaponD != null && weaponD.gameObject.activeSelf)
        {
            PlayEffect(weaponD, "FX_Shootfire_4");
        }
        else
        {
            Debug.LogWarning("未找到已启用的武器或武器未启用！");
        }
    }

    private void PlayEffect(Transform weapon, string effectName)
    {
        Transform fxShootfireTransform = FindChildByName(weapon, effectName);

        if (fxShootfireTransform != null)
        {
            fxShootfire = fxShootfireTransform.GetComponent<ParticleSystem>();

            if (fxShootfire != null)
            {
                AdjustEffectSpeed();

                fxShootfire.gameObject.SetActive(true);
                fxShootfire.Play();

                FlipEffect(fxShootfireTransform);
            }
        }
        else
        {
            Debug.LogWarning($"未找到 {effectName} 特效！");
        }
    }

    private void AdjustEffectSpeed()
    {
        if (playerController != null && fxShootfire != null)
        {
            var mainModule = fxShootfire.main;

            // ✅ 使用插槽修正后的攻击间隔
            float attackDelay = playerController.TotalAttackDelay;

            mainModule.simulationSpeed = (attackDelay > 0) ? 1f / attackDelay : 1f;

            Debug.Log($"特效播放速度已调整为 {mainModule.simulationSpeed}，攻击间隔(TotalAttackDelay) = {attackDelay}");
        }
    }

    private void FlipEffect(Transform effectTransform)
    {
        Vector3 effectScale = effectTransform.localScale;
        effectScale.x = Mathf.Abs(effectScale.x) * (isFacingRight ? 1 : -1);
        effectTransform.localScale = effectScale;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (fxShootfire != null && !fxShootfire.isPlaying)
        {
            fxShootfire.Play();
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (fxShootfire != null)
        {
            fxShootfire.Stop();
            fxShootfire.gameObject.SetActive(false);
        }
    }

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
}
