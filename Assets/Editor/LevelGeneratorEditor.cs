using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelGenerator generator = (LevelGenerator)target;

        if (GUILayout.Button("���ɹؿ�"))
        {
            generator.ClearLevel(); // ��ͬ�����
            EditorApplication.delayCall += () =>
            {
                if (generator != null)
                {
                    generator.GenerateLevel(); // �첽�ӳ�ִ�У���ֹ����
                }
            };
        }

        if (GUILayout.Button("����ؿ�"))
        {
            generator.ClearLevel();
        }
    }
}
