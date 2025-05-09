using UnityEngine;

public class SkillEffectOnStateEnter : StateMachineBehaviour
{
    [Header("�����������")]
    public string parentObjectName = "weapon_C";  // ����������

    [Header("��Ч���������")]
    public string effectName = "FX_Shootfire_3";  // ��Ч��������

    private GameObject targetGameObject;
    private ParticleSystem particleSystem;

    // �ڶ���״̬����ʱ����
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // ���ҳ����еĸ�����
        GameObject parentObject = GameObject.Find(parentObjectName);

        if (parentObject != null)
        {
            // ���Ҹ������µ���Ч����
            Transform effectTransform = parentObject.transform.Find(effectName);

            if (effectTransform != null)
            {
                targetGameObject = effectTransform.gameObject;

                // ����GameObject
                targetGameObject.SetActive(true);

                // ��ȡParticleSystem���
                particleSystem = targetGameObject.GetComponent<ParticleSystem>();

                if (particleSystem != null)
                {
                    // ����������Ч
                    particleSystem.Play();
                }
                else
                {
                    Debug.LogWarning($"GameObject '{effectName}' û�и���ParticleSystem�����");
                }
            }
            else
            {
                Debug.LogWarning($"δ�ҵ� '{effectName}' �Ӷ���");
            }
        }
        else
        {
            Debug.LogWarning($"δ�ҵ���Ϊ '{parentObjectName}' �ĸ�����");
        }
    }

    // �ڶ���״̬�˳�ʱ����
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (targetGameObject != null)
        {
            // ֹͣ������Ч������GameObject
            if (particleSystem != null)
            {
                particleSystem.Stop();
            }

            // ����GameObject
            targetGameObject.SetActive(false);
        }
    }
}
