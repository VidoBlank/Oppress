using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIDissolveController : MonoBehaviour
{
    [Header("ָ����Ҫ�滻����ʵ���� UI Image")]
    public Image targetImage;

    [Header("��ʱ��ÿ�ʼ���ɣ��룩")]
    public float delayBeforeTransition = 2f;

    [Header("���ɳ���ʱ�䣨�룩")]
    public float transitionDuration = 1f;

    void Start()
    {
        // ��� targetImage �Ƿ����
        if (targetImage == null)
        {
            Debug.LogError("��ָ��Ŀ�� UI Image��");
            return;
        }

        // �滻����ʵ��������Ӱ�칲�����
        if (targetImage.material != null)
        {
            targetImage.material = new Material(targetImage.material);
        }
        else
        {
            Debug.LogError("Ŀ�� UI Image û��ָ�����ʣ�");
            return;
        }

        // ��ʼ�� _FullAlphaDissolveFade Ϊ 1
        targetImage.material.SetFloat("_FullAlphaDissolveFade", 1f);

        // ��ʱ����������Ч��
        StartCoroutine(TransitionDissolveFade());
    }

    IEnumerator TransitionDissolveFade()
    {
        // ��ʱ�ȴ�
        yield return new WaitForSeconds(delayBeforeTransition);

        float elapsedTime = 0f;
        float startValue = 1f;
        float targetValue = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float currentValue = Mathf.Lerp(startValue, targetValue, elapsedTime / transitionDuration);
            targetImage.material.SetFloat("_FullAlphaDissolveFade", currentValue);
            yield return null;
        }
        // ȷ������ֵ����ΪĿ��ֵ
        targetImage.material.SetFloat("_FullAlphaDissolveFade", targetValue);
    }
}
