using UnityEngine;

public class SaveSlotUI : MonoBehaviour
{
    public int slotIndex; // 当前槽位索引
    public GameObject deleteButton; // 删除存档按钮

    private void Start()
    {
        UpdateSlotDisplay(); // 初始化时更新显示状态
    }

    // 更新存档槽的显示状态
    public void UpdateSlotDisplay()
    {
        
    }

    // 删除存档按钮点击事件
    public void OnDeleteButtonClicked()
    {
        
        UpdateSlotDisplay(); // 删除后刷新按钮显示状态
    }

    // 检测存档按钮点击事件
    public void OnCheckSaveButtonClicked()
    {
       
    }
}
