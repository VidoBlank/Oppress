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
    /// ���ԣ������ж�Inspector�������Ե�����
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

