using UnityEngine;

public class RifleRapidFireBehavior : StateMachineBehaviour
{
    [Header("����Ƶ��")]
    public float rapidFireRate = 0.1f; // �����Ƶ�ʣ�ÿ0.1��һ��

    private float nextFireTime = 0f;
    private ParticleSystem fxShootfire;
    private bool isFacingRight = true; // ��¼��ɫ�ĳ���
    private Transform weaponB;

    // �ڶ���״̬����ʱ����
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // ���ҵ�ǰ������ B ����ȡ��Ч
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
                Debug.LogWarning("δ�ҵ� FX_Shootfire_2 ��Ч��");
            }
        }

        // ��ȡ��ɫ�ĳ���
        isFacingRight = animator.transform.localScale.x > 0;

        // ��ʼ���´����ʱ��
        nextFireTime = Time.time + rapidFireRate;
    }

    // �ڶ���״̬����ʱ����
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // ����Ƿ�ﵽ�´������ʱ��
        if (Time.time >= nextFireTime)
        {
            // ������Ч
            PlayShootfireEffect();
            // ������һ�����ʱ��
            nextFireTime = Time.time + rapidFireRate;
        }
    }

    // �ڶ���״̬�˳�ʱ����
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // ֹͣ��������Ч
        if (fxShootfire != null)
        {
            fxShootfire.Stop();
            fxShootfire.gameObject.SetActive(false);
        }
    }

    // ������Ч
    private void PlayShootfireEffect()
    {
        if (fxShootfire != null)
        {
            if (!fxShootfire.isPlaying)
            {
                fxShootfire.gameObject.SetActive(true);
            }
            fxShootfire.Play();
            FlipEffect(fxShootfire.transform); // ���ݽ�ɫ����ת��Ч
        }
    }

    // ��ת��Ч��ƥ���ɫ�ĳ���
    private void FlipEffect(Transform effectTransform)
    {
        Vector3 effectScale = effectTransform.localScale;
        effectScale.x = Mathf.Abs(effectScale.x) * (isFacingRight ? 1 : -1);
        effectTransform.localScale = effectScale;
    }

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
}
