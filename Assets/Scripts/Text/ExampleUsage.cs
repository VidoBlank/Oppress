using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleUsage : MonoBehaviour
{
    public TypewriterColorJitterEffect typewriterEffect;

    void Start()
    {
        // 设置初始文本并启动打字机效果
        typewriterEffect.SetText("我这里是演示文本，请按下空格键。");
    }

    void Update()
    {
        // 当玩家按下空格键时，动态改变文本内容
        if (Input.GetKeyDown(KeyCode.Space))
        {
            typewriterEffect.SetText("按下空格键后，<color=#FFFF00><size=125%>文本</size></color>已经发生变化！");
        }
    }
}
