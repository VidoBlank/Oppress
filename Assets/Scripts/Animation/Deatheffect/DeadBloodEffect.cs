using UnityEngine;

public class DeadBloodEffect : StateMachineBehaviour
{
    public string effectName = "Blood_Dead"; // �����������

    private GameObject effectObject;
    private ParticleSystem particleSystem;

    // ����״̬����ʱ����
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (effectObject == null)
        {
            // ʹ�õݹ鷽���������������
            effectObject = FindDeepChild(animator.gameObject.transform, effectName)?.gameObject;
            if (effectObject != null)
            {
                particleSystem = effectObject.GetComponent<ParticleSystem>();
            }
            else
            {
                Debug.LogWarning($"δ�ҵ�����Ϊ {effectName} ��������");
            }
        }

        if (effectObject != null)
        {
            effectObject.SetActive(true); // ������Ч
            particleSystem?.Play(); // ��ʼ��������Ч��
        }
    }

    // ����״̬�˳�ʱ����
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        

        // ��ջ��棬������һ�����²���
        effectObject = null;
        particleSystem = null;
    }

    // �ݹ����������ķ���
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
