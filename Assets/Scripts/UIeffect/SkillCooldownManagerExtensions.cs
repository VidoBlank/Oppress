using UnityEngine;

// 扩展SkillCooldownManager类的辅助方法
public static class SkillCooldownManagerExtensions
{
    /// <summary>
    /// 检查充能系统是否已经初始化
    /// </summary>
    /// <param name="manager">SkillCooldownManager实例</param>
    /// <returns>是否已初始化</returns>
    public static bool IsChargeSystemInitialized(this SkillCooldownManager manager)
    {
        // 使用反射获取私有字段isChargeSystem的值
        System.Reflection.FieldInfo field = typeof(SkillCooldownManager).GetField("isChargeSystem",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            return (bool)field.GetValue(manager);
        }

        // 如果无法访问字段，则默认返回false
        return false;
    }

}