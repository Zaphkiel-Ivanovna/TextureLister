using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class TextureLister : EditorWindow
{
    private GameObject selectedGameObject;
    private List<Texture> textures = new List<Texture>();
    private string targetFolderPath = "Assets/Textures/";
    private Vector2 scrollPosition;
    private List<string> blacklistedFolders = new List<string>() { "_PoiyomiShaders" };

    [MenuItem("Zaphkiel/Texture Lister")]
    public static void ShowWindow()
    {
        GetWindow<TextureLister>("Texture Lister");
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("Select a GameObject", EditorStyles.boldLabel);
        selectedGameObject = (GameObject)EditorGUILayout.ObjectField(selectedGameObject, typeof(GameObject), true);

        if (GUILayout.Button("List Textures"))
        {
            ListTextures();
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Textures:", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
        foreach (Texture texture in textures)
        {
            EditorGUILayout.ObjectField(texture, typeof(Texture), false);
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Target Folder Path:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(targetFolderPath, GUILayout.MinWidth(100));
        if (GUILayout.Button("Select", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Target Folder", Application.dataPath, "");
            if (path.StartsWith(Application.dataPath))
            {
                targetFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Blacklisted Folders:", EditorStyles.boldLabel);
        for (int i = 0; i < blacklistedFolders.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            blacklistedFolders[i] = EditorGUILayout.TextField(blacklistedFolders[i]);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                blacklistedFolders.RemoveAt(i);
            }
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("Add Blacklisted Folder"))
        {
            blacklistedFolders.Add("");
        }

        if (GUILayout.Button("Move Textures"))
        {
            MoveTextures();
        }
        if (GUILayout.Button("Copy Textures"))
        {
            CopyTextures();
        }

        EditorGUILayout.EndVertical();
    }

    private bool IsTextureInBlacklistedFolder(Texture texture)
    {
        string assetPath = AssetDatabase.GetAssetPath(texture);
        foreach (string folder in blacklistedFolders)
        {
            if (assetPath.Contains("/" + folder + "/") || assetPath.StartsWith(folder + "/"))
            {
                return true;
            }
        }
        return false;
    }

    private void ListTextures()
    {
        textures.Clear();
        if (selectedGameObject == null) return;

        Renderer[] renderers = selectedGameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            foreach (Material material in materials)
            {
                if (material == null) continue;

                Shader shader = material.shader;
                int propertyCount = ShaderUtil.GetPropertyCount(shader);
                for (int i = 0; i < propertyCount; i++)
                {
                    if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        string propertyName = ShaderUtil.GetPropertyName(shader, i);
                        Texture texture = material.GetTexture(propertyName);
                        if (texture != null && !textures.Contains(texture) && !IsTextureInBlacklistedFolder(texture))
                        {
                            textures.Add(texture);
                        }
                    }
                }
            }
        }
    }

    private void MoveTextures()
    {
        if (textures.Count == 0) return;

        string folderPath = Application.dataPath + "/" + targetFolderPath.Substring(7);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        foreach (Texture texture in textures)
        {
            string sourcePath = AssetDatabase.GetAssetPath(texture);
            string fileName = Path.GetFileName(sourcePath);
            string targetPath = Path.Combine(targetFolderPath, fileName);
            if (sourcePath != targetPath)
            {
                AssetDatabase.MoveAsset(sourcePath, targetPath);
            }
        }

        AssetDatabase.Refresh();
    }

    private void CopyTextures()
    {
        if (textures.Count == 0) return;
    
        string folderPath = Application.dataPath + "/" + targetFolderPath.Substring(7);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    
        foreach (Texture texture in textures)
        {
            string sourcePath = AssetDatabase.GetAssetPath(texture);
            string fileName = Path.GetFileName(sourcePath);
            string targetPath = Path.Combine(targetFolderPath, fileName);
            if (sourcePath != targetPath)
            {
                AssetDatabase.CopyAsset(sourcePath, targetPath);
            }
        }
    
        AssetDatabase.Refresh();
    }

}
