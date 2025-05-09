using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    private TypewriterColorJitterEffect typewriterEffect;

    [SerializeField]
    [Header("碰撞盒")]
    public List<TextCollision> collisions = new List<TextCollision>();

    [System.Serializable]
    public class TextCollision
    {
        [Header("碰撞盒")]
        public GameObject collision;
        [Header("对应的文本")]
        public string text;

    }

    private void Awake()
    {
        typewriterEffect = GameObject.Find("管理器/GameManager").GetComponent<TypewriterColorJitterEffect>();
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
