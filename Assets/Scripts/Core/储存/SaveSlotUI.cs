using UnityEngine;

public class SaveSlotUI : MonoBehaviour
{
    public int slotIndex; // ��ǰ��λ����
    public GameObject deleteButton; // ɾ���浵��ť

    private void Start()
    {
        UpdateSlotDisplay(); // ��ʼ��ʱ������ʾ״̬
    }

    // ���´浵�۵���ʾ״̬
    public void UpdateSlotDisplay()
    {
        
    }

    // ɾ���浵��ť����¼�
    public void OnDeleteButtonClicked()
    {
        
        UpdateSlotDisplay(); // ɾ����ˢ�°�ť��ʾ״̬
    }

    // ���浵��ť����¼�
    public void OnCheckSaveButtonClicked()
    {
       
    }
}
