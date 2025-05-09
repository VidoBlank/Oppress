using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Event_Fail : BaseEvent
{
    public CanvasGroup uiCanvasGroup; // ��Ϸʧ����ʾ UI �� CanvasGroup
    public float fadeDuration = 2f; // UI ���Ե�ʱ��
    public string failSceneName = "LoadScene_Fail"; // ��Ϸʧ�ܺ���صĳ�������

    private List<PlayerController> playersInScene = new List<PlayerController>(); // �����ڵ�����б�
    private bool isGameOver = false; // �Ƿ��Ѿ�������Ϸ����

    private void Start()
    {
        // ��ȡ�����ڵ��������
        playersInScene.AddRange(FindObjectsOfType<PlayerController>());

        if (playersInScene.Count == 0)
        {
            Debug.LogError("������û���ҵ���ң�");
        }

        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 0f;
            uiCanvasGroup.gameObject.SetActive(false); // ��ʼ���� UI
        }
    }

    private void Update()
    {
        if (isGameOver) return; // �����Ϸ�ѽ������������

        // �����������Ƿ񶼴��ڵ���״̬
        bool allPlayersDown = true;
        foreach (var player in playersInScene)
        {
            if (!player.isKnockedDown)
            {
                allPlayersDown = false;
                break;
            }
        }

        if (allPlayersDown)
        {
            StartCoroutine(HandleGameOver());
        }
    }

    private IEnumerator HandleGameOver()
    {
        isGameOver = true;

        // ��ͣ��Ϸ
        Time.timeScale = 0f;
        Debug.Log("������Ҷ����أ���Ϸʧ�ܡ�");

        // ��ʾʧ�� UI
        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.gameObject.SetActive(true);

            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime; // ʹ�� unscaledDeltaTime������ Time.timeScale Ӱ��
                uiCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }

            uiCanvasGroup.alpha = 1f;
        }

        // �ȴ�һ��ʱ��
        yield return new WaitForSecondsRealtime(3f);

        // �ָ���Ϸ������ʧ�ܳ���
        Time.timeScale = 1f;
        SceneManager.LoadScene(failSceneName);
    }
}
