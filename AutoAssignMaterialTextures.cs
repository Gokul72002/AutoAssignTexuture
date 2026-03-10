using System;
using UnityEngine;
using UnityEditor;

public class AutoAssignMaterialTextures : EditorWindow
{
    private string materialsFolder = "Assets/Materials";
    private string texturesFolder = "Assets/Textures";

    [MenuItem("Tools/Auto Assign Material Textures")]
    public static void ShowWindow()
    {
        GetWindow(typeof(AutoAssignMaterialTextures));
    }

    void OnGUI()
    {
        GUILayout.Label("Material Texture Auto Assigner", EditorStyles.boldLabel);
        materialsFolder = EditorGUILayout.TextField("Materials Folder:", materialsFolder);
        texturesFolder = EditorGUILayout.TextField("Textures Folder:", texturesFolder);

        if (GUILayout.Button("Assign Textures"))
        {
            AssignTextures();
        }
    }

    void AssignTextures()
    {
        string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { materialsFolder });
        string[] texGuids = AssetDatabase.FindAssets("t:Texture", new[] { texturesFolder });

        foreach (string matGuid in matGuids)
        {
            string matPath = AssetDatabase.GUIDToAssetPath(matGuid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null) continue;

            string matCore = mat.name; // e.g. "Block Assy Stage_03"

            foreach (string texGuid in texGuids)
            {
                string texPath = AssetDatabase.GUIDToAssetPath(texGuid);
                Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(texPath);

                if (tex == null) continue;

                string texName = tex.name;

                // Case-insensitive contains check for material core name
                if (texName.IndexOf(matCore, StringComparison.OrdinalIgnoreCase) < 0) continue;

                // Assign based on suffix (case-insensitive)
                if (texName.IndexOf("AO", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    mat.SetTexture("_OcclusionMap", tex);
                    Debug.Log($"AO → {mat.name} : {tex.name}");
                }
                else if (texName.IndexOf("AlbedoTransparency", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         texName.IndexOf("Albedo", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    mat.SetTexture("_BaseMap", tex);
                    Debug.Log($"BaseMap → {mat.name} : {tex.name}");
                }
                else if (texName.IndexOf("Normal", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         texName.IndexOf("NormalMap", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    mat.SetTexture("_BumpMap", tex);
                    mat.EnableKeyword("_NORMALMAP");
                    Debug.Log($"Normal → {mat.name} : {tex.name}");
                }
                else if (texName.IndexOf("SpecularSmoothness", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         texName.IndexOf("Specular", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    mat.SetTexture("_SpecGlossMap", tex);
                    mat.EnableKeyword("_SPECGLOSSMAP");
                    Debug.Log($"Specular → {mat.name} : {tex.name}");
                }
                else if (texName.IndexOf("MetallicSmoothness", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         texName.IndexOf("Metallic", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Standard/Legacy: metallic+smoothness map property name
                    mat.SetTexture("_MetallicGlossMap", tex);
                    // enable metallic keyword used by Standard shader
                    mat.EnableKeyword("_METALLICGLOSSMAP");

                    // If your shader uses a different property name (URP/HDRP), you may also want to set:
                    // mat.SetTexture("_MetallicMap", tex);
                    // mat.SetTexture("_Metallic", tex); // usually not a texture property though
                    Debug.Log($"Metallic → {mat.name} : {tex.name}");
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ All materials processed!");
    }
}
