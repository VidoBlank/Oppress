using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleUsage2 : MonoBehaviour
{
    public TypewriterColorJitterEffect typewriterEffect;

    void Start()
    {
        // 设置初始文本并启动打字机效果
        typewriterEffect.SetText("按下键盘A键，瞄准音响并打爆它！");
    }

    void Update()
    {
       
    }
}
