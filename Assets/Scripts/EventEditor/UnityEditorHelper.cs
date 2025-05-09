#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

public static class UnityEditorHelper
{
    public static T GetActualObject<T>(this SerializedProperty property)
    {
        try
        {
            if (property == null)
                return default(T);
            var serializedObject = property.serializedObject;
            if (serializedObject == null)
            {
                return default(T);
            }

            var targetObject = serializedObject.targetObject;

            var slicedName = property.propertyPath.Split('.').ToList();
            List<int> arrayCounts = new List<int>();
            for (int index = 0; index < slicedName.Count; index++)
            {
                arrayCounts.Add(-1);
                var currName = slicedName[index];
                if (currName.EndsWith("]"))
                {
                    var arraySlice = currName.Split('[', ']');
                    if (arraySlice.Length >= 2)
                    {
                        arrayCounts[index - 2] = Convert.ToInt32(arraySlice[1]);
                        slicedName[index] = string.Empty;
                        slicedName[index - 1] = string.Empty;
                    }
                }
            }

            while (string.IsNullOrEmpty(slicedName.Last()))
            {
                int i = slicedName.Count - 1;
                slicedName.RemoveAt(i);
                arrayCounts.RemoveAt(i);
            }

            return DescendHierarchy<T>(targetObject, slicedName, arrayCounts, 0);
        }
        catch
        {
            return default(T);
        }
    }

    public static object GetActualObjectParent(this SerializedProperty property)
    {
        try
        {
            if (property == null)
                return default;
            //��ȡ��ǰ���л���Object
            var serializedObject = property.serializedObject;
            if (serializedObject == null)
            {
                return default;
            }
            //��ȡtargetObject�������targetObject����
            //�Ҳ�������ֱ�Ӿٸ����ӣ�a.b.c.d.e.f,����serializedObject����f����ôtargetObject����a
            var targetObject = serializedObject.targetObject;
            //�������������propertyPath��ʵ����a.b.c.d.e.f
            //�����������ĳһ����Array�Ļ�������b��ô�ͻ���a.b.Array.data[x].c.d.e.f
            //����xΪindex
            var slicedName = property.propertyPath.Split('.').ToList();
            List<int> arrayCounts = new List<int>();
            //����"."�ֺú���Ҫ��ȡ���е����鼰��index������һ������
            for (int index = 0; index < slicedName.Count; index++)
            {
                arrayCounts.Add(-1);
                var currName = slicedName[index];
                if (currName.EndsWith("]"))
                {
                    var arraySlice = currName.Split('[', ']');
                    if (arraySlice.Length >= 2)
                    {
                        arrayCounts[index - 2] = Convert.ToInt32(arraySlice[1]);
                        slicedName[index] = string.Empty;
                        slicedName[index - 1] = string.Empty;
                    }
                }
            }
            //������鵼�µĿ�
            while (string.IsNullOrEmpty(slicedName.Last()))
            {
                int i = slicedName.Count - 1;
                slicedName.RemoveAt(i);
                arrayCounts.RemoveAt(i);
            }
            //���������������ͬ�����
            if (slicedName.Last().Equals(property.name))
            {
                int i = slicedName.Count - 1;
                slicedName.RemoveAt(i);
                arrayCounts.RemoveAt(i);
            }
            //���������ô����targetObjectΪ��ǰ�ĸ�����
            if (slicedName.Count == 0) return targetObject;
            //����������飬��ֹ������Ҳ������
            while (string.IsNullOrEmpty(slicedName.Last()))
            {
                int i = slicedName.Count - 1;
                slicedName.RemoveAt(i);
                arrayCounts.RemoveAt(i);
            }
            //���������ô����targetObjectΪ��ǰ�ĸ�����
            if (slicedName.Count == 0) return targetObject;
            //��ȡ������
            return DescendHierarchy<object>(targetObject, slicedName, arrayCounts, 0);
        }
        catch
        {
            return default;
        }
    }
    //�Լ���
    static T DescendHierarchy<T>(object targetObject, List<string> splitName, List<int> splitCounts, int depth)
    {
        if (depth >= splitName.Count)
            return default(T);

        var currName = splitName[depth];

        if (string.IsNullOrEmpty(currName))
            return DescendHierarchy<T>(targetObject, splitName, splitCounts, depth + 1);

        int arrayIndex = splitCounts[depth];

        var newField = targetObject.GetType().GetField(currName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (newField == null)
        {
            Type baseType = targetObject.GetType().BaseType;
            while (baseType != null && newField == null)
            {
                newField = baseType.GetField(currName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                baseType = baseType.BaseType;
            }
        }

        var newObj = newField.GetValue(targetObject);
        if (depth == splitName.Count - 1)
        {
            T actualObject = default(T);
            if (arrayIndex >= 0)
            {
                if (newObj.GetType().IsArray && ((System.Array)newObj).Length > arrayIndex)
                    actualObject = (T)((System.Array)newObj).GetValue(arrayIndex);

                var newObjList = newObj as IList;
                if (newObjList != null && newObjList.Count > arrayIndex)
                {
                    actualObject = (T)newObjList[arrayIndex];
                }
            }
            else
            {
                actualObject = (T)newObj;
            }

            return actualObject;
        }
        else if (arrayIndex >= 0)
        {
            if (newObj is IList)
            {
                IList list = (IList)newObj;
                newObj = list[arrayIndex];
            }
            else if (newObj is System.Array)
            {
                System.Array a = (System.Array)newObj;
                newObj = a.GetValue(arrayIndex);
            }
        }

        return DescendHierarchy<T>(newObj, splitName, splitCounts, depth + 1);
    }
}
#endif

