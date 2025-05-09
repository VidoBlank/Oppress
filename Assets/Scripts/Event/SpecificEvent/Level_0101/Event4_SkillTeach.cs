using System.Collections;
using UnityEngine;

public class Event4_SkillTeach : BaseEvent
{
    public GameObject objectToEnable; // ���õĽ�ѧ����
    public PlayerController player; // ָ����ҿ��ƽű������ã�����Inspector��ָ����Ҷ���
    public TypewriterColorJitterEffect typewriterEffect; // ���ֻ�Ч�����
    public PolygonCollider2D newCameraBounds; // �µ�����߽�
    public CameraController cameraController;

  
  

    private void Start()
    {
        // ��ȡCameraController����
        cameraController = FindObjectOfType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("CameraController δ�ҵ�����ȷ���������������������");
        }
        typewriterEffect = FindObjectOfType<TypewriterColorJitterEffect>();
    }

    public override void EnableEvent()
    {
        Debug.Log("��ʼ���ܽ�ѧ�¼�");
        StartCoroutine(typing());
        // ����ָ��object�µ�Canvas���
        if (objectToEnable != null)
        {
            Canvas[] canvases = objectToEnable.GetComponentsInChildren<Canvas>(true);
            if (canvases.Length > 0)
            {
                foreach (var canvas in canvases)
                {
                    canvas.enabled = true; // ����Canvas���
                }
            }
            else
            {
                Debug.LogWarning("��ָ��������δ�ҵ�Canvas�����");
            }
        }
        else
        {
            Debug.LogWarning("δ����Ҫ���õ����壡");
        }

        // ��������߽磨�����¼�ʱ�����£�
        if (cameraController != null && newCameraBounds != null)
        {
            cameraController.SetCameraBounds(newCameraBounds);
            Debug.Log("����߽��Ѹ���-04��");
        }

        

        
    }



    private IEnumerator typing()
    {
        yield return DisplayText("�����ǹ�ָ�Ա��<color=#FFFF00>����</color>���������ĵ�ҩ�����ͷ�<color=#FFFF00>�������[��ݼ�Q]</color>��������Ϯ�ĵ��ˡ�"
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

        // �����¼�
        EndEvent();
    }
}
