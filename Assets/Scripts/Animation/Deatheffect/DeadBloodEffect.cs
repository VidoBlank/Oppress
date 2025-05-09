using UnityEngine;

public class DeadBloodEffect : StateMachineBehaviour
{
    public string effectName = "Blood_Dead"; // 子物体的名称

    private GameObject effectObject;
    private ParticleSystem particleSystem;

    // 动画状态进入时调用
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (effectObject == null)
        {
            // 使用递归方法查找深层子物体
            effectObject = FindDeepChild(animator.gameObject.transform, effectName)?.gameObject;
            if (effectObject != null)
            {
                particleSystem = effectObject.GetComponent<ParticleSystem>();
            }
            else
            {
                Debug.LogWarning($"未找到名称为 {effectName} 的子物体");
            }
        }

        if (effectObject != null)
        {
            effectObject.SetActive(true); // 启用特效
            particleSystem?.Play(); // 开始播放粒子效果
        }
    }

    // 动画状态退出时调用
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        

        // 清空缓存，便于下一次重新查找
        effectObject = null;
        particleSystem = null;
    }

    // 递归查找子物体的方法
    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            var result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }
}
