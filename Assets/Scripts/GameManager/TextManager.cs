using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    private TypewriterColorJitterEffect typewriterEffect;

    [SerializeField]
    [Header("��ײ��")]
    public List<TextCollision> collisions = new List<TextCollision>();

    [System.Serializable]
    public class TextCollision
    {
        [Header("��ײ��")]
        public GameObject collision;
        [Header("��Ӧ���ı�")]
        public string text;

    }

    private void Awake()
    {
        typewriterEffect = GameObject.Find("������/GameManager").GetComponent<TypewriterColorJitterEffect>();
        int index = 0;
        foreach (TextCollision collision in collisions)
        {
            collision.collision.AddComponent<TextChangeCheck>().index = index;
            index++;
        }
    }


    public void ShowDialog(int index)
    {
        typewriterEffect.SetText(collisions[index].text);
    }
}
