using UnityEditor;
using UnityEngine;

/// <summary>
/// Handles accessing the Atlas
/// </summary>
public class PsdAtlasManager : EditorWindow
{
    #region Constants

    /// <summary>
    /// Shader
    /// </summary>
    private const string STR_UnlitTransparentColored = "Unlit/Transparent Colored";

    // Constants
    #endregion

    #region CheckSourceFolder

    /// <summary>
    /// creates the source folder
    /// </summary>
    /// <param name="psdAssetFolderPath"></param>
    public static void CheckSourceFolder(string psdAssetFolderPath)
    {
        if (string.IsNullOrEmpty(PsdImporter.SourceFolder))
        {
            AssetDatabase.CreateFolder(psdAssetFolderPath, "Source");
            PsdImporter.SourceFolder = string.Format("{0}/Source/", psdAssetFolderPath);
        }
    }

    // CheckSourceFolder
    #endregion

    #region CreateNewAtlas

    /// <summary>
    /// Create a new atlas
    /// </summary>
    /// <param name="psdAssetFolderPath"></param>
    /// <returns></returns>
    public static UIAtlas CreateNewAtlas(string psdAssetFolderPath)
    {
        CheckSourceFolder(psdAssetFolderPath);

        string prefabPath = string.Empty, matPath = string.Empty;
        string AtlasName = PsdImporter.NSettingsAtlasName;

        // If we have an atlas to work with, see if we can figure out the path for it and its material
        if (NGUISettings.atlas != null && NGUISettings.atlas.name == NGUISettings.GetString(AtlasName, string.Empty))
        {
            prefabPath = AssetDatabase.GetAssetPath(NGUISettings.atlas.gameObject.GetInstanceID());
            if (NGUISettings.atlas.spriteMaterial != null)
                matPath = AssetDatabase.GetAssetPath(NGUISettings.atlas.spriteMaterial.GetInstanceID());
        }

        // Assume default values if needed
        NGUISettings.SetString(AtlasName, PsdImporter.ObjPSDFolderToLoad.name);
        if (string.IsNullOrEmpty(prefabPath))
            prefabPath = string.Format("{0}{1}.prefab", PsdImporter.SourceFolder, AtlasName);
        if (string.IsNullOrEmpty(matPath))
            matPath = string.Format("{0}{1}.mat", PsdImporter.SourceFolder, AtlasName);

        // Try to load the prefab
        GameObject go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
        if (NGUISettings.atlas == null && go != null)
            NGUISettings.atlas = go.GetComponent<UIAtlas>();

        // Try to load the material
        Material mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;

        // If the material doesn't exist, create it
        if (mat == null)
        {
            mat = new Material(Shader.Find(STR_UnlitTransparentColored));

            // Save the material
            AssetDatabase.CreateAsset(mat, matPath);
            AssetDatabase.Refresh();

            // Load the material so it's usable
            mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;
        }

        // create atlas if one is not already loaded
        if (NGUISettings.atlas == null || NGUISettings.atlas.name != AtlasName)
        {
            // Create a new prefab for the atlas
            Object prefab = go ?? PrefabUtility.CreateEmptyPrefab(prefabPath);

            // Create a new game object for the atlas
            go = new GameObject(AtlasName);
            go.AddComponent<UIAtlas>().spriteMaterial = mat;

            // Update the prefab
            PrefabUtility.ReplacePrefab(go, prefab);

            DestroyImmediate(go);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select the atlas
            go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
            NGUISettings.atlas = go.GetComponent<UIAtlas>();
        }

        return go.GetComponent<UIAtlas>();
    }

    // CreateNewAtlas
    #endregion
}
