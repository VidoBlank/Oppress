using UnityEngine;
using TMPro;  // ����TextMeshPro�����ռ�
using System.Collections;

public class AmmoBoxUI : MonoBehaviour
{
    [Header("UI ���")]
    public CanvasGroup canvasGroup; // CanvasGroup ���� UI ��ʾ/����
    public TextMeshProUGUI ammoText; // ������ʾ��ǰ��ҩ���� TextMeshProUGUI

    [Header("UI ����")]
    public float fadeDuration = 0.5f; // UI ����/������ʱ��
    public float displayDuration = 3f; // UI �����ɼ�ʱ��

    private AmmoBox ammoBox;
    private Coroutine fadeCoroutine;

    void Start()
    {
        ammoBox = GetComponent<AmmoBox>();

        if (ammoBox == null)
        {
            Debug.LogError("AmmoBoxUI: δ�ҵ� AmmoBox �����");
            return;
        }

        if (canvasGroup == null || ammoText == null)
        {
            Debug.LogError("AmmoBoxUI: UI ���δ�󶨣�");
            return;
        }

        canvasGroup.alpha = 0; // ��ʼʱ���� UI
        UpdateAmmoUI(); // ��ʼ����ҩ����ʾ
    }

    public void ShowAmmoUI()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeUI(1, fadeDuration)); // ���� UI
        UpdateAmmoUI(); // ���µ�ҩ�� UI
        StartCoroutine(HideAfterDelay());
    }

    private void UpdateAmmoUI()
    {
        if (ammoBox != null)
        {
            ammoText.text = $"ʣ�൯ҩ: {ammoBox.currentEnergy} "; // �����ı�����
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration); // �ȴ� X ��
        fadeCoroutine = StartCoroutine(FadeUI(0, fadeDuration)); // ���� UI
    }

    private IEnumerator FadeUI(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}
