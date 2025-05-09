using TMPro;
using UnityEngine;
using System.Collections;

public class TypewriterColorJitterEffect : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public float typingSpeed = 0.05f;
    public float jitterAmount = 1.0f;
    public float jitterSpeed = 5.0f;
    public float jitterDuration = 2.0f;

    private string fullText;
    private TMP_TextInfo textInfo;
    public bool isTyping = false;

    void Start()
    {
        textMeshPro.text = "";
    }

    public void SetText(string newText)
    {
        StopAllCoroutines();
        fullText = newText;
        textMeshPro.text = "";
        isTyping = true;
        StartCoroutine(TypewriterEffect());
    }

    IEnumerator TypewriterEffect()
    {
        textMeshPro.ForceMeshUpdate();
        textInfo = textMeshPro.textInfo;

        int richTextOffset = 0; // 用于修正富文本标签偏移
        string visibleText = ""; // 当前可见文本

        for (int i = 0; i < fullText.Length; i++)
        {
            // 如果遇到富文本标签 '<'
            if (fullText[i] == '<')
            {
                int tagStart = i;
                while (i < fullText.Length && fullText[i] != '>')
                {
                    i++;
                }
                // 完整的富文本标签加入到可见文本中
                visibleText += fullText.Substring(tagStart, i - tagStart + 1);
                continue;
            }

            // 添加当前字符到可见文本
            visibleText += fullText[i];
            textMeshPro.text = visibleText;
            textMeshPro.ForceMeshUpdate();
            textInfo = textMeshPro.textInfo;

            // 启动抖动协程
            int charIndex = i - richTextOffset; // 实际可见字符索引
            if (charIndex >= 0 && charIndex < textInfo.characterCount)
            {
                StartCoroutine(ApplyJitter(charIndex));
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    IEnumerator ApplyJitter(int index)
    {
        float elapsedTime = 0f;

        while (elapsedTime < jitterDuration && isTyping)
        {
            elapsedTime += Time.deltaTime;

            textMeshPro.ForceMeshUpdate();
            textInfo = textMeshPro.textInfo;

            if (index >= textInfo.characterInfo.Length || !textInfo.characterInfo[index].isVisible)
                yield break;

            int materialIndex = textInfo.characterInfo[index].materialReferenceIndex;
            int vertexIndex = textInfo.characterInfo[index].vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            Vector3 jitterOffset = new Vector3(Random.Range(-jitterAmount, jitterAmount), Random.Range(-jitterAmount, jitterAmount), 0);
            for (int j = 0; j < 4; j++)
                vertices[vertexIndex + j] += jitterOffset;

            textMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);

            yield return new WaitForSeconds(1.0f / jitterSpeed);
        }

        ResetCharacterVertices(index);
    }

    void ResetCharacterVertices(int index)
    {
        textMeshPro.ForceMeshUpdate();
        textInfo = textMeshPro.textInfo;

        if (index < 0 || index >= textInfo.characterInfo.Length || !textInfo.characterInfo[index].isVisible)
            return;

        int materialIndex = textInfo.characterInfo[index].materialReferenceIndex;
        int vertexIndex = textInfo.characterInfo[index].vertexIndex;
        Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

        Vector3 bottomLeft = textInfo.characterInfo[index].bottomLeft;
        Vector3 topRight = textInfo.characterInfo[index].topRight;

        vertices[vertexIndex + 0] = bottomLeft;
        vertices[vertexIndex + 1] = new Vector3(bottomLeft.x, topRight.y, 0);
        vertices[vertexIndex + 2] = topRight;
        vertices[vertexIndex + 3] = new Vector3(topRight.x, bottomLeft.y, 0);

        textMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
}
