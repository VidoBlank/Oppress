using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Event11_Success : BaseEvent
{
    [Header("UI����")]
    public CanvasGroup uiCanvasGroup; // ָ����UI�����ϵ�CanvasGroup
    public float fadeDuration = 2f; // UI��0��1�Ĺ���ʱ��

    public override void EnableEvent()
    {
        StartCoroutine(HandleEventSuccess());
    }

    private IEnumerator HandleEventSuccess()
    {
        if (uiCanvasGroup != null)
        {
            
            // ��CanvasGroup��alpha��0���ɵ�1
            float elapsedTime = 0f;
            uiCanvasGroup.alpha = 0f;
            uiCanvasGroup.gameObject.SetActive(true);

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                uiCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }

            uiCanvasGroup.alpha = 1f;
        }
        else
        {
            Debug.LogWarning("δָ��CanvasGroup�����");
        }

        // ��ͣ��Ϸ
        Time.timeScale = 0f;
        Debug.Log("��Ϸ����ͣ��");

        // �ȴ��û�ȷ�ϻ��Զ���������
        yield return new WaitForSecondsRealtime(3f); // ʹ��ʵʱ�ȴ�������Time.timeScaleӰ��
        Time.timeScale = 1f;
        // �������������������¼�
        SceneManager.LoadScene("LoadScene_out"); // �滻Ϊ��Ľ�����������
        EndEvent();
    }
}
