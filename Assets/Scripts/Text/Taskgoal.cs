using UnityEngine;
using TMPro;
using System.Collections;

public class Taskgoal : MonoBehaviour
{
    // Ԥ���������Ԥ����������� Canvas���Ҵ� CanvasGroup ������� TextMeshProUGUI ���
    public GameObject taskObjectivePrefab;

    // ��Ӧ������Ŀ���ı�
    [TextArea]
    public string objectiveText = "����Ŀ���ı�";

    // ���������˳���ʱ������λ���룩
    public float fadeDuration = 1f;

    // ��ʾ��ȫ��ʾ״̬��ʱ������λ���룩
    public float displayDuration = 3f;

    public CanvasGroup canvasGroup;
    public TextMeshProUGUI textComponent;
    private GameObject instance;

    // �� UI �������õķ���
    public void ShowTaskGoal()
    {
        if (taskObjectivePrefab != null)
        {
            // ʵ����Ԥ����
            instance = Instantiate(taskObjectivePrefab);

            // ��ȡ CanvasGroup ��������ڿ���͸���ȣ�
            canvasGroup = instance.GetComponentInChildren<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogError("Ԥ������δ�ҵ� CanvasGroup �������ȷ��Ԥ�����Ϲ����� CanvasGroup �Ա����͸���ȡ�");
                return;
            }

            // ��ȡ TextMeshProUGUI ������������ı�����
            textComponent = instance.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = objectiveText;
            }
            else
            {
                Debug.LogError("Ԥ������δ�ҵ� TextMeshProUGUI �������ȷ��Ԥ�����а����������");
            }

            // ��ʼ�� Canvas ͸����Ϊ0��ȫ͸����
            canvasGroup.alpha = 0;

            // ����Э��ʵ�ֽ��䶯��
            StartCoroutine(FadeInAndOut());
        }
        else
        {
            Debug.LogError("δ��������Ŀ��Ԥ���壡");
        }
    }

    IEnumerator FadeInAndOut()
    {
        float timer = 0f;

        // ������룺�� alpha 0 �� 1
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1;

        // ������ʾ displayDuration ��
        yield return new WaitForSeconds(displayDuration);

        // �����˳����� alpha 1 �� 0
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0;

        // ����������ر�Ԥ����
        instance.SetActive(false);
    }
}
