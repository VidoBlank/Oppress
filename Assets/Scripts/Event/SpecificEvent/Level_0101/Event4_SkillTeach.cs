using System.Collections;
using UnityEngine;

public class Event4_SkillTeach : BaseEvent
{
    public GameObject objectToEnable; // 启用的教学物体
    public PlayerController player; // 指向玩家控制脚本的引用（需在Inspector中指定玩家对象）
    public TypewriterColorJitterEffect typewriterEffect; // 打字机效果组件
    public PolygonCollider2D newCameraBounds; // 新的相机边界
    public CameraController cameraController;

  
  

    private void Start()
    {
        // 获取CameraController引用
        cameraController = FindObjectOfType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("CameraController 未找到，请确保场景中有相机控制器。");
        }
        typewriterEffect = FindObjectOfType<TypewriterColorJitterEffect>();
    }

    public override void EnableEvent()
    {
        Debug.Log("开始技能教学事件");
        StartCoroutine(typing());
        // 启用指定object下的Canvas组件
        if (objectToEnable != null)
        {
            Canvas[] canvases = objectToEnable.GetComponentsInChildren<Canvas>(true);
            if (canvases.Length > 0)
            {
                foreach (var canvas in canvases)
                {
                    canvas.enabled = true; // 启用Canvas组件
                }
            }
            else
            {
                Debug.LogWarning("在指定对象中未找到Canvas组件！");
            }
        }
        else
        {
            Debug.LogWarning("未分配要启用的物体！");
        }

        // 更新相机边界（进入事件时即更新）
        if (cameraController != null && newCameraBounds != null)
        {
            cameraController.SetCameraBounds(newCameraBounds);
            Debug.Log("相机边界已更新-04。");
        }

        

        
    }



    private IEnumerator typing()
    {
        yield return DisplayText("点击步枪手干员的<color=#FFFF00>技能</color>按键，消耗弹药储备释放<color=#FFFF00>快速射击[快捷键Q]</color>以消灭来袭的敌人。"
);
    }







private IEnumerator DisplayText(string text)
    {
        if (typewriterEffect != null)
        {
            typewriterEffect.SetText(text);
            
        }
        while (typewriterEffect.isTyping)
        {
            yield return null;
        }
        typewriterEffect.textMeshPro.ForceMeshUpdate();

        // 结束事件
        EndEvent();
    }
}
