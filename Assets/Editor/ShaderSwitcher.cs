using UnityEditor;
using UnityEngine;

public class ShaderSwitcher : EditorWindow
{
    [MenuItem("Tools/Switch Shader to ProPixelizer")]
    public static void ShowWindow()
    {
        GetWindow<ShaderSwitcher>("Shader Switcher");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Switch Shader to ProPixelizer"))
        {
            SwitchShaders();
        }
    }

    private static void SwitchShaders()
    {
        // ����Ŀ����ɫ��
        Shader shaderMaster = Shader.Find("Shader Graphs/S_Master");
        Shader shaderSimpleLit = Shader.Find("Universal Render Pipeline/Simple Lit");
        Shader shaderProPixelizer = Shader.Find("ProPixelizer/SRP/PixelizedWithOutline");

        if (shaderProPixelizer == null)
        {
            Debug.LogError("ProPixelizer shader not found!");
            return;
        }

        int materialCount = 0;

        // �������������в���
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.sharedMaterials)
            {
                if (material == null) continue;

                bool isUpdated = false;

                // ����Ƿ�ʹ�� Shader Graphs/S_Master �� Universal Render Pipeline/Simple Lit
                if (material.shader == shaderMaster || material.shader == shaderSimpleLit)
                {
                    // ���浱ǰ��������
                    Texture baseMap = material.GetTexture("_BaseMap") ?? material.GetTexture("_AlbedoTexture");
                    Texture normalMap = material.GetTexture("_BumpMap") ?? material.GetTexture("_Normal_Map_Master");
                    Texture emissionMap = material.GetTexture("_EmissionMap") ?? material.GetTexture("_EmissiveTexture");

                    // **���� Base Color**
                    Color baseColor;
                    if (material.shader == shaderMaster)
                    {
                        baseColor = Color.gray; // Shader Graphs/S_Master �� Base Color ����Ϊ��ɫ
                    }
                    else if (material.shader == shaderSimpleLit)
                    {
                        baseColor = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : Color.white; // Simple Lit ������ԭ�� Base Color
                    }
                    else
                    {
                        baseColor = Color.white; // Ĭ��ֵ����Ϊ�����߼���
                    }

                    // **��ͼ����ת���߼�**
                    baseMap = TryReplaceTextureName(baseMap, "_DIF", "_DISP");

                    // �滻Ϊ ProPixelizer shader
                    material.shader = shaderProPixelizer;

                    // �ָ���Ҫ����
                    if (baseMap != null) material.SetTexture("_Albedo", baseMap);
                    if (normalMap != null) material.SetTexture("_NormalMap", normalMap);
                    if (emissionMap != null) material.SetTexture("_Emission", emissionMap);

                    // ������ɫ
                    material.SetColor("_BaseColor", Color.white); 
                    material.SetColor("_EmissionColor", Color.black); // ���÷�����ɫΪ��ɫ

                    isUpdated = true;
                    materialCount++;
                }

                if (isUpdated)
                {
                    Debug.Log($"Material on {renderer.name} updated.");
                }
            }
        }

        Debug.Log(materialCount > 0
            ? $"Shader switching completed. {materialCount} materials updated."
            : "No materials using the specified shaders were found.");

        // ǿ��ˢ�³���
        EditorApplication.QueuePlayerLoopUpdate();
        SceneView.RepaintAll();
    }

    /// <summary>
    /// �����滻��ͼ����
    /// </summary>
    /// <param name="originalTexture">ԭʼ��ͼ</param>
    /// <param name="fromSuffix">��Ҫ�滻�ĺ�׺</param>
    /// <param name="toSuffix">Ŀ���׺</param>
    /// <returns>�滻�����ͼ������Ҳ����򷵻�ԭʼ��ͼ</returns>
    private static Texture TryReplaceTextureName(Texture originalTexture, string fromSuffix, string toSuffix)
    {
        if (originalTexture == null) return null;

        // ��ȡ��ͼ·��
        string path = AssetDatabase.GetAssetPath(originalTexture);
        if (string.IsNullOrEmpty(path)) return originalTexture;

        // �滻�ļ�����׺
        string newPath = path.Replace(fromSuffix, toSuffix);

        // ��������ͼ
        Texture newTexture = AssetDatabase.LoadAssetAtPath<Texture>(newPath);
        return newTexture != null ? newTexture : originalTexture;
    }
}
