#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[InitializeOnLoad]
public static class SceneArchiveTool
{
    private const float BUTTON_SIZE = 40f;
    private const float BUTTON_MARGIN = 10f;
    private static Texture2D archiveIcon;
    private static GUIStyle buttonStyle;
    private static bool isToolEnabled = true;
    private static bool isInitialized = false;

    static SceneArchiveTool()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void InitializeIfNeeded()
    {
        if (isInitialized) return;

        // Carica l'icona senza usare GUI
        archiveIcon = EditorGUIUtility.IconContent("d_SaveAs").image as Texture2D;

        // Crea lo stile del pulsante in modo sicuro
        buttonStyle = new GUIStyle(EditorStyles.miniButton)
        {
            fixedWidth = BUTTON_SIZE,
            fixedHeight = BUTTON_SIZE,
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(10, 10, 10, 10)
        };

        isInitialized = true;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        isToolEnabled = state != PlayModeStateChange.EnteredPlayMode;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!isToolEnabled) return;

        InitializeIfNeeded();

        Handles.BeginGUI();

        try
        {
            // Calcola la posizione del pulsante
            float xPos = sceneView.position.width - BUTTON_SIZE - BUTTON_MARGIN;
            float yPos = BUTTON_MARGIN;

            // Pulsante di archiviazione
            if (GUI.Button(new Rect(xPos, yPos, BUTTON_SIZE, BUTTON_SIZE),
                new GUIContent(archiveIcon, "Archive Selected Objects"), buttonStyle))
            {
                ArchiveSelectedObjects();
            }

            // Pulsante gestione archivi
            yPos += BUTTON_SIZE + BUTTON_MARGIN;
            if (GUI.Button(new Rect(xPos, yPos, BUTTON_SIZE, BUTTON_SIZE),
                new GUIContent(EditorGUIUtility.IconContent("d_SceneViewVisibility").image, "Open Archive Manager"), buttonStyle))
            {
                ArchiveWindow.ShowWindow();
            }
        }
        finally
        {
            Handles.EndGUI();
        }

        // Elementi 3D disegnati fuori dal blocco GUI
        DrawSelectionHandles();
    }

    private static void ArchiveSelectedObjects()
    {
        var selectedObjects = new List<GameObject>();
        foreach (var obj in Selection.gameObjects)
        {
            if (obj != null && obj.scene.IsValid())
            {
                selectedObjects.Add(obj);
            }
        }

        if (selectedObjects.Count == 0)
        {
            Debug.LogWarning("No valid objects selected for archiving");
            return;
        }

        if (selectedObjects.Count > 10)
        {
            selectedObjects = selectedObjects.GetRange(0, 10);
            Debug.LogWarning("Archiving only first 10 selected objects");
        }

        BackupTool.BackupObjects(selectedObjects);
    }

    private static void DrawSelectionHandles()
    {
        if (Selection.gameObjects.Length == 0) return;

        Handles.color = new Color(0.2f, 0.8f, 0.3f, 0.3f);

        foreach (GameObject obj in Selection.gameObjects)
        {
            if (obj == null) continue;

            Bounds bounds = GetRendererBounds(obj);
            Handles.DrawWireCube(bounds.center, bounds.size);

            Handles.Label(
                obj.transform.position + Vector3.up * (bounds.size.y + 0.2f),
                obj.name,
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = Color.green },
                    fontSize = 10,
                    fontStyle = FontStyle.Bold
                }
            );
        }
    }

    private static Bounds GetRendererBounds(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null) return renderer.bounds;

        return new Bounds(obj.transform.position, Vector3.one * 0.5f);
    }

    [MenuItem("Tools/Scene Tools/Toggle Archive Tool &a")]
    private static void ToggleTool()
    {
        isToolEnabled = !isInitialized || !isToolEnabled;
        Debug.Log($"Scene Archive Tool {(isToolEnabled ? "Enabled" : "Disabled")}");
        SceneView.RepaintAll();
    }
}
#endif