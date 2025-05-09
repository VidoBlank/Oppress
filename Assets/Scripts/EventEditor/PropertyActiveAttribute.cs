using UnityEngine;

namespace InspectorEx
{
    public enum CompareType
    {
        Equal,
        NonEqual,
        Less,
        LessEqual,
        More,
        MoreEqual,
    }

    /// <summary>
    /// 特性：条件判断Inspector窗口属性的显隐
    /// </summary>
    public class PropertyActiveAttribute : PropertyAttribute
    {
        public string field;
        public CompareType compareType;
        public object compareValue;
        public string chinese;

        public PropertyActiveAttribute(string fieldName, CompareType type, object value, string chineseName)
        {
            field = fieldName;
            compareType = type;
            compareValue = value;
            chinese = chineseName;
        }
    }
}

