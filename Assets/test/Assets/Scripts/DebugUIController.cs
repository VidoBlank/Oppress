using UnityEngine;

public class DebugUIController : MonoBehaviour
{
    public void OnClickResetSaveDataCompletely()
    {
        // 这里调用你的清空存档方法
        GameData.Instance.ResetSaveDataCompletely();

        // （可选）可以加一个输出提示
        Debug.Log("存档已彻底清空！");
    }
}
