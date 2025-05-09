using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelGenerator generator = (LevelGenerator)target;

        if (GUILayout.Button("生成关卡"))
        {
            generator.ClearLevel(); // 可同步清除
            EditorApplication.delayCall += () =>
            {
                if (generator != null)
                {
                    generator.GenerateLevel(); // 异步延迟执行，防止卡死
                }
            };
        }

        if (GUILayout.Button("清除关卡"))
        {
            generator.ClearLevel();
        }
    }
}
