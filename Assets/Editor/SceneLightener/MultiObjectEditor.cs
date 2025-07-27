#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class MultiObjectEditor : Editor
{
    private bool showMultiTools = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var selectedObjects = new List<GameObject>();
        foreach (var obj in Selection.gameObjects)
        {
            if (obj != null) selectedObjects.Add(obj);
        }

        if (selectedObjects.Count == 0) return;

        GUILayout.Space(10);
        showMultiTools = EditorGUILayout.Foldout(showMultiTools,
            $"Scene Tools ({selectedObjects.Count} Selected)", true);

        if (showMultiTools)
        {
            EditorGUILayout.BeginVertical("box");

            // Backup button
            bool canBackup = selectedObjects.Count > 0 && selectedObjects.Count <= 10;
            EditorGUI.BeginDisabledGroup(!canBackup);
            if (GUILayout.Button($"Backup {selectedObjects.Count} Objects", GUILayout.Height(30)))
            {
                BackupTool.BackupObjects(selectedObjects);
            }
            EditorGUI.EndDisabledGroup();

            if (!canBackup && selectedObjects.Count > 10)
            {
                EditorGUILayout.HelpBox("Maximum 10 objects can be backed up at once", MessageType.Warning);
            }

            // Info panel
            EditorGUILayout.HelpBox(
                "Backup will disable selected objects and create an archive asset. " +
                "Supported components: Transform, Mesh, Materials, Lights, Colliders.",
                MessageType.Info
            );

            EditorGUILayout.EndVertical();
        }
    }
}
#endif