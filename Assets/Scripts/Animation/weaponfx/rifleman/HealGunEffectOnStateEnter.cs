using UnityEngine;

public class SkillEffectOnStateEnter : StateMachineBehaviour
{
    [Header("父对象的名称")]
    public string parentObjectName = "weapon_C";  // 父对象名称

    [Header("特效对象的名称")]
    public string effectName = "FX_Shootfire_3";  // 特效对象名称

    private GameObject targetGameObject;
    private ParticleSystem particleSystem;

    // 在动画状态进入时调用
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 查找场景中的父对象
        GameObject parentObject = GameObject.Find(parentObjectName);

        if (parentObject != null)
        {
            // 查找父对象下的特效对象
            Transform effectTransform = parentObject.transform.Find(effectName);

            if (effectTransform != null)
            {
                targetGameObject = effectTransform.gameObject;

                // 激活GameObject
                targetGameObject.SetActive(true);

                // 获取ParticleSystem组件
                particleSystem = targetGameObject.GetComponent<ParticleSystem>();

                if (particleSystem != null)
                {
                    // 播放粒子特效
                    particleSystem.Play();
                }
                else
                {
                    Debug.LogWarning($"GameObject '{effectName}' 没有附加ParticleSystem组件！");
                }
            }
            else
            {
                Debug.LogWarning($"未找到 '{effectName}' 子对象！");
            }
        }
        else
        {
            Debug.LogWarning($"未找到名为 '{parentObjectName}' 的父对象！");
        }
    }

    // 在动画状态退出时调用
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (targetGameObject != null)
        {
            // 停止粒子特效并禁用GameObject
            if (particleSystem != null)
            {
                particleSystem.Stop();
            }

            // 禁用GameObject
            targetGameObject.SetActive(false);
        }
    }
}
