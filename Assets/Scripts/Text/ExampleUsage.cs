using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleUsage : MonoBehaviour
{
    public TypewriterColorJitterEffect typewriterEffect;

    void Start()
    {
        // ���ó�ʼ�ı����������ֻ�Ч��
        typewriterEffect.SetText("����������ʾ�ı����밴�¿ո����");
    }

    void Update()
    {
        // ����Ұ��¿ո��ʱ����̬�ı��ı�����
        if (Input.GetKeyDown(KeyCode.Space))
        {
            typewriterEffect.SetText("���¿ո����<color=#FFFF00><size=125%>�ı�</size></color>�Ѿ������仯��");
        }
    }
}
