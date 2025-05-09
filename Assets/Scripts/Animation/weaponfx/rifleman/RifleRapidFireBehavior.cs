using UnityEngine;

public class RifleRapidFireBehavior : StateMachineBehaviour
{
    [Header("速射频率")]
    public float rapidFireRate = 0.1f; // 速射的频率，每0.1秒一次

    private float nextFireTime = 0f;
    private ParticleSystem fxShootfire;
    private bool isFacingRight = true; // 记录角色的朝向
    private Transform weaponB;

    // 在动画状态进入时调用
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 查找当前的武器 B 并获取特效
        weaponB = FindChildByName(animator.transform, "weapon_B");

        if (weaponB != null)
        {
            Transform fxShootfireTransform = FindChildByName(weaponB, "FX_Shootfire_2");
            if (fxShootfireTransform != null)
            {
                fxShootfire = fxShootfireTransform.GetComponent<ParticleSystem>();
            }
            else
            {
                Debug.LogWarning("未找到 FX_Shootfire_2 特效！");
            }
        }

        // 获取角色的朝向
        isFacingRight = animator.transform.localScale.x > 0;

        // 初始化下次射击时间
        nextFireTime = Time.time + rapidFireRate;
    }

    // 在动画状态更新时调用
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 检查是否达到下次射击的时间
        if (Time.time >= nextFireTime)
        {
            // 播放特效
            PlayShootfireEffect();
            // 设置下一次射击时间
            nextFireTime = Time.time + rapidFireRate;
        }
    }

    // 在动画状态退出时调用
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 停止并禁用特效
        if (fxShootfire != null)
        {
            fxShootfire.Stop();
            fxShootfire.gameObject.SetActive(false);
        }
    }

    // 播放特效
    private void PlayShootfireEffect()
    {
        if (fxShootfire != null)
        {
            if (!fxShootfire.isPlaying)
            {
                fxShootfire.gameObject.SetActive(true);
            }
            fxShootfire.Play();
            FlipEffect(fxShootfire.transform); // 根据角色朝向翻转特效
        }
    }

    // 翻转特效以匹配角色的朝向
    private void FlipEffect(Transform effectTransform)
    {
        Vector3 effectScale = effectTransform.localScale;
        effectScale.x = Mathf.Abs(effectScale.x) * (isFacingRight ? 1 : -1);
        effectTransform.localScale = effectScale;
    }

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
}
