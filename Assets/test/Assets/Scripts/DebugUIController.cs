using UnityEngine;

public class DebugUIController : MonoBehaviour
{
    public void OnClickResetSaveDataCompletely()
    {
        // ������������մ浵����
        GameData.Instance.ResetSaveDataCompletely();

        // ����ѡ�����Լ�һ�������ʾ
        Debug.Log("�浵�ѳ�����գ�");
    }
}
