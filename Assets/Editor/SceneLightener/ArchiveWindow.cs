#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;

public class ArchiveWindow : EditorWindow
{
    [SerializeField] string nameArchive;

    private Vector2 scrollPos;
    private string searchFilter = "";
    private Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

    [MenuItem("Tools/Scene Archive Manager")]
    public static void ShowWindow() => GetWindow<ArchiveWindow>("Scene Archive");

    void OnGUI()
    {
        GUILayout.Label(" Scene Archive Manager", EditorStyles.boldLabel);

        // Name bar
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Name:", GUILayout.Width(100));
        nameArchive = EditorGUILayout.TextField(nameArchive);
        EditorGUILayout.EndHorizontal();

        // Search bar
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Search:", GUILayout.Width(50));
        searchFilter = EditorGUILayout.TextField(searchFilter);
        EditorGUILayout.EndHorizontal();

        // Global buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh Archives", GUILayout.Width(120)))
        {
            // Force refresh
        }
        EditorGUILayout.EndHorizontal();

        // Archive list
        var archives = LoadArchives();
        if (archives.Count == 0)
        {
            EditorGUILayout.HelpBox("No archives found. Select objects and use 'Backup Selected Objects'.", MessageType.Info);
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (var archive in archives)
        {
            if (!string.IsNullOrEmpty(searchFilter) &&
                !archive.name.Contains(searchFilter) &&
                !archive.sceneName.Contains(searchFilter))
            {
                continue;
            }

            string key = archive.name;
            if (!foldoutStates.ContainsKey(key)) foldoutStates[key] = true;

            // Archive header
            EditorGUILayout.BeginVertical("box");
            foldoutStates[key] = EditorGUILayout.Foldout(foldoutStates[key],
                $"{archive.name} ({archive.archivedObjects.Count} objects)");

            if (foldoutStates[key])
            {
                // Archive info
                EditorGUILayout.LabelField($"Scene: {archive.sceneName}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Created: {archive.creationTime:g}", EditorStyles.miniLabel);

                // Object list
                EditorGUI.indentLevel++;
                foreach (var objData in archive.archivedObjects)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(GetObjectIcon(objData), GUILayout.Width(20));
                    EditorGUILayout.LabelField(objData.objectName);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;

                // Action buttons
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Restore All"))
                {
                    BackupTool.RestoreArchive(archive);
                }

                if (GUILayout.Button("Select Archive"))
                {
                    Selection.activeObject = archive;
                }

                if (GUILayout.Button("Delete", GUILayout.Width(80)))
                {
                    if (EditorUtility.DisplayDialog("Delete Archive",
                        $"Delete archive '{archive.name}' with {archive.archivedObjects.Count} objects?",
                        "Delete", "Cancel"))
                    {
                        DeleteArchive(archive);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    private List<SceneArchive> LoadArchives()
    {
        return AssetDatabase.FindAssets("t:SceneArchive")
            .Select(guid => AssetDatabase.LoadAssetAtPath<SceneArchive>(
                AssetDatabase.GUIDToAssetPath(guid)))
            .Where(a => a != null)
            .OrderByDescending(a => a.creationTime)
            .ToList();
    }

    private Texture GetObjectIcon(SceneArchive.ObjectData data)
    {
        if (data.mesh != null) return EditorGUIUtility.IconContent("MeshFilter Icon").image;
        if (data.lightIntensity > 0) return EditorGUIUtility.IconContent("Light Icon").image;
        return EditorGUIUtility.IconContent("GameObject Icon").image;
    }

    private void DeleteArchive(SceneArchive archive)
    {
        string path = AssetDatabase.GetAssetPath(archive);
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.Refresh();
    }
}
#endif