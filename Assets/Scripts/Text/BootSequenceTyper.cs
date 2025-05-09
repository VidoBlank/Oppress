using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class BootSequenceTyper : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public float typingSpeed = 0.02f;
    public float lineDelay = 0.25f;

    [Header("打字音效")]
    public AudioClip typingSound;
    private AudioSource audioSource;

    [TextArea(3, 10)]
    public string[] bootLines;
    [Header("是否在描述前加分割线")]
    public bool showDividerLine = true;

    private string cachedDescription = "";
    Coroutine typingCoroutine;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void ShowBootAndDescription(string descriptionText)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        cachedDescription = descriptionText;
        typingCoroutine = StartCoroutine(TypeBootThenDesc(descriptionText));
    }

    public void UpdateCachedDescription(string newDescription)
    {
        cachedDescription = newDescription;
    }

    IEnumerator TypeBootThenDesc(string descriptionText)
    {
        textComponent.text = "";

        foreach (string line in bootLines)
        {
            foreach (char c in line)
            {
                textComponent.text += c;
                PlayCharSound(c);
                yield return new WaitForSeconds(typingSpeed);
            }
            textComponent.text += "\n";
            yield return new WaitForSeconds(lineDelay);
        }

        if (showDividerLine)
        {
            textComponent.text += "\n------------------------------\n";
        }

        foreach (char c in descriptionText)
        {
            textComponent.text += c;
            PlayCharSound(c);
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    void PlayCharSound(char c)
    {
        if (!char.IsWhiteSpace(c) && typingSound != null)
        {
            audioSource.PlayOneShot(typingSound, 0.3f);
        }
    }

    public void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;

            string fullContent = "";
            foreach (string line in bootLines)
            {
                fullContent += line + "\n";
            }

            if (showDividerLine)
            {
                fullContent += "\n------------------------------\n";
            }

            fullContent += cachedDescription;
            textComponent.text = fullContent;
        }
    }

    /// <summary>
/// 立即显示文本内容（不播放打字机效果）
/// </summary>
public void ShowInstant(string descriptionText)
{
    if (typingCoroutine != null)
    {
        StopCoroutine(typingCoroutine);
        typingCoroutine = null;
    }

    cachedDescription = descriptionText;

    string fullContent = "";

    foreach (string line in bootLines)
    {
        fullContent += line + "\n";
    }

    if (showDividerLine)
    {
        fullContent += "\n------------------------------\n";
    }

    fullContent += descriptionText;
    textComponent.text = fullContent;
}


}
