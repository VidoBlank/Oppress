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
        // 查找目标着色器
        Shader shaderMaster = Shader.Find("Shader Graphs/S_Master");
        Shader shaderSimpleLit = Shader.Find("Universal Render Pipeline/Simple Lit");
        Shader shaderProPixelizer = Shader.Find("ProPixelizer/SRP/PixelizedWithOutline");

        if (shaderProPixelizer == null)
        {
            Debug.LogError("ProPixelizer shader not found!");
            return;
        }

        int materialCount = 0;

        // 遍历场景中所有材质
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.sharedMaterials)
            {
                if (material == null) continue;

                bool isUpdated = false;

                // 检查是否使用 Shader Graphs/S_Master 或 Universal Render Pipeline/Simple Lit
                if (material.shader == shaderMaster || material.shader == shaderSimpleLit)
                {
                    // 保存当前材质属性
                    Texture baseMap = material.GetTexture("_BaseMap") ?? material.GetTexture("_AlbedoTexture");
                    Texture normalMap = material.GetTexture("_BumpMap") ?? material.GetTexture("_Normal_Map_Master");
                    Texture emissionMap = material.GetTexture("_EmissionMap") ?? material.GetTexture("_EmissiveTexture");

                    // **处理 Base Color**
                    Color baseColor;
                    if (material.shader == shaderMaster)
                    {
                        baseColor = Color.gray; // Shader Graphs/S_Master 的 Base Color 设置为灰色
                    }
                    else if (material.shader == shaderSimpleLit)
                    {
                        baseColor = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : Color.white; // Simple Lit 保留其原有 Base Color
                    }
                    else
                    {
                        baseColor = Color.white; // 默认值（作为兜底逻辑）
                    }

                    // **贴图名称转换逻辑**
                    baseMap = TryReplaceTextureName(baseMap, "_DIF", "_DISP");

                    // 替换为 ProPixelizer shader
                    material.shader = shaderProPixelizer;

                    // 恢复必要属性
                    if (baseMap != null) material.SetTexture("_Albedo", baseMap);
                    if (normalMap != null) material.SetTexture("_NormalMap", normalMap);
                    if (emissionMap != null) material.SetTexture("_Emission", emissionMap);

                    // 设置颜色
                    material.SetColor("_BaseColor", Color.white); 
                    material.SetColor("_EmissionColor", Color.black); // 设置发光颜色为黑色

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

        // 强制刷新场景
        EditorApplication.QueuePlayerLoopUpdate();
        SceneView.RepaintAll();
    }

    /// <summary>
    /// 尝试替换贴图名称
    /// </summary>
    /// <param name="originalTexture">原始贴图</param>
    /// <param name="fromSuffix">需要替换的后缀</param>
    /// <param name="toSuffix">目标后缀</param>
    /// <returns>替换后的贴图，如果找不到则返回原始贴图</returns>
    private static Texture TryReplaceTextureName(Texture originalTexture, string fromSuffix, string toSuffix)
    {
        if (originalTexture == null) return null;

        // 获取贴图路径
        string path = AssetDatabase.GetAssetPath(originalTexture);
        if (string.IsNullOrEmpty(path)) return originalTexture;

        // 替换文件名后缀
        string newPath = path.Replace(fromSuffix, toSuffix);

        // 加载新贴图
        Texture newTexture = AssetDatabase.LoadAssetAtPath<Texture>(newPath);
        return newTexture != null ? newTexture : originalTexture;
    }
}
