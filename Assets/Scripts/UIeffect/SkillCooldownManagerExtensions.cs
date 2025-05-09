using UnityEngine;

// ��չSkillCooldownManager��ĸ�������
public static class SkillCooldownManagerExtensions
{
    /// <summary>
    /// ������ϵͳ�Ƿ��Ѿ���ʼ��
    /// </summary>
    /// <param name="manager">SkillCooldownManagerʵ��</param>
    /// <returns>�Ƿ��ѳ�ʼ��</returns>
    public static bool IsChargeSystemInitialized(this SkillCooldownManager manager)
    {
        // ʹ�÷����ȡ˽���ֶ�isChargeSystem��ֵ
        System.Reflection.FieldInfo field = typeof(SkillCooldownManager).GetField("isChargeSystem",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            return (bool)field.GetValue(manager);
        }

        // ����޷������ֶΣ���Ĭ�Ϸ���false
        return false;
    }

}