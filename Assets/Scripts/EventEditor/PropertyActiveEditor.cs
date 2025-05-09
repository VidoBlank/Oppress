#if UNITY_EDITOR
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace InspectorEx
{
    [CustomPropertyDrawer(typeof(PropertyActiveAttribute), false)]
    public class PropertyActiveEditor : PropertyDrawer
    {
        private bool isShow = false;
        string chineseName;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var attr = attribute as PropertyActiveAttribute;
            var field = attr.field;
            var compareType = attr.compareType;
            var compareValue = attr.compareValue;
            chineseName = attr.chinese;

            var parent = property.GetActualObjectParent();
            var fieldInfo = parent.GetType().GetField(field);
            var fieldValue = fieldInfo?.GetValue(parent);

            isShow = IsMeetCondition(fieldValue, compareType, compareValue);
            if (!isShow) return 0;
            return base.GetPropertyHeight(property, label);
        }

        private bool IsMeetCondition(object fieldValue, CompareType compareType, object compareValue)
        {
            if (compareType == CompareType.Equal)
            {
                return fieldValue.Equals(compareValue);
            }
            else if (compareType == CompareType.NonEqual)
            {
                return !fieldValue.Equals(compareValue);
            }
            else if (IsValueType(fieldValue.GetType()) && IsValueType(compareValue.GetType()))
            {
                switch (compareType)
                {
                    case CompareType.Less:
                        return Comparer.DefaultInvariant.Compare(fieldValue, compareValue) < 0;
                    case CompareType.LessEqual:
                        return Comparer.DefaultInvariant.Compare(fieldValue, compareValue) <= 0;
                    case CompareType.More:
                        return Comparer.DefaultInvariant.Compare(fieldValue, compareValue) > 0;
                    case CompareType.MoreEqual:
                        return Comparer.DefaultInvariant.Compare(fieldValue, compareValue) >= 0;
                }
            }
            return false;
        }

        private bool IsValueType(Type type)
        {
            return (type.IsPrimitive && type.IsValueType && type != typeof(char));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label.text = chineseName;
            if (isShow) EditorGUI.PropertyField(position, property, label, true);
        }
    }
}
#endif
