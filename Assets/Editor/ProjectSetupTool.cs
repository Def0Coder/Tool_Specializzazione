#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class ProjectSetupTool
{
    [MenuItem("Tools/Project Setup/Create Default Folders")]
    public static void CreateDefaultFolders()
    {
        CreateFolder("_Project");
        CreateFolder("_Project/Art");
        CreateFolder("_Project/Scripts");
        CreateFolder("_Project/Scenes");
        CreateFolder("_Project/Scenes/Backups");
        CreateFolder("_Project/Audio");
        CreateFolder("_Project/Prefabs");

        Debug.Log("<color=green>✓</color> Project structure created successfully!");
    }

    public static void CreateBackupFolder()
    {
        CreateFolder("_Project/Scenes/Backups");
    }

    private static void CreateFolder(string path)
    {
        string fullPath = Path.Combine("Assets", path);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
            AssetDatabase.Refresh();
        }
    }
}
#endif