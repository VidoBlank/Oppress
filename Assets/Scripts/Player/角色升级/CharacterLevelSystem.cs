using UnityEngine;

public abstract class CharacterLevelSystem : MonoBehaviour
{
    protected PlayerController player;

    protected virtual void Awake()
    {
        player = GetComponent<PlayerController>();
        if (player == null)
        {
            Debug.LogError($"{GetType().Name} 未找到 PlayerController 组件！");
        }
    }

    /// <summary>
    /// 根据角色等级设置属性（需要子类实现）
    /// </summary>
    /// <param name="level">当前等级</param>
    public abstract void SetAttributesByLevel(int level);


}
