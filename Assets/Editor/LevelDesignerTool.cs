using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class LevelDesignerTool : EditorWindow
{
    GameObject prefabToSpawn;
    int numberOfObjects = 8;
    float radius = 5f;
    Vector3 previewCenter = Vector3.zero;
    bool hasValidSurface = false;

    // Tool states
    bool spawnCircleEnabled = false;
    bool spawnCubeEnabled = false;

    // Cube tool params
    Vector3 cubeSize = new Vector3(5, 5, 5);

    [MenuItem("Tool/Level Designer Tool")]
    public static void ShowWindow()
    {
        GetWindow<LevelDesignerTool>("Level Designer Tool");
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnGUI()
    {
        GUILayout.Label("Strumenti di Posizionamento", EditorStyles.boldLabel);

        // Gestione toggle mutualmente esclusivi
        bool newCircle = EditorGUILayout.Toggle("Tool spawn a cerchio", spawnCircleEnabled);
        if (newCircle != spawnCircleEnabled && newCircle)
        {
            spawnCircleEnabled = true;
            spawnCubeEnabled = false;
        }
        else if (!newCircle)
        {
            spawnCircleEnabled = false;
        }

        bool newCube = EditorGUILayout.Toggle("Tool spawn random in cubo", spawnCubeEnabled);
        if (newCube != spawnCubeEnabled && newCube)
        {
            spawnCubeEnabled = true;
            spawnCircleEnabled = false;
        }
        else if (!newCube)
        {
            spawnCubeEnabled = false;
        }

        EditorGUILayout.Space(10);

        prefabToSpawn = (GameObject)EditorGUILayout.ObjectField("Prefab da spawnare", prefabToSpawn, typeof(GameObject), false);
        numberOfObjects = EditorGUILayout.IntField("Numero oggetti", numberOfObjects);

        if (spawnCircleEnabled)
        {

            if (numberOfObjects >= 50)
            {
                EditorGUILayout.HelpBox("Stai per spawnare più di 50 oggetti nel cubo! Attenzione a prestazioni.", MessageType.Warning);
            }

            radius = EditorGUILayout.FloatField("Raggio (cerchio)", radius);
            EditorGUILayout.HelpBox("Posiziona il cursore nella Scene View.\nScrolla per regolare il raggio.\nClic sinistro per posizionare i prefab.", MessageType.Info);
        }

        if (spawnCubeEnabled)
        {
            cubeSize = EditorGUILayout.Vector3Field("Dimensione Cubo", cubeSize);

            if (numberOfObjects >= 50)
            {
                EditorGUILayout.HelpBox("Stai per spawnare più di 50 oggetti nel cubo! Attenzione a prestazioni.", MessageType.Warning);
            }

            EditorGUILayout.HelpBox("Posiziona il cursore nella Scene View.\nClic sinistro per spawnare prefab nel cubo.", MessageType.Info);
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Vector3 targetPoint = Vector3.zero;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            targetPoint = hit.point;
            hasValidSurface = true;
        }
        else
        {
            hasValidSurface = false;
        }

        previewCenter = Vector3.Lerp(previewCenter, targetPoint, 0.1f);

        if (spawnCircleEnabled)
        {
            if (e.type == EventType.ScrollWheel)
            {
                float scrollDelta = -e.delta.y * 0.2f;
                radius = Mathf.Max(0.1f, radius + scrollDelta);
                e.Use();
                SceneView.RepaintAll();
            }

            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                PlaceObjectsInCircle(previewCenter);
                e.Use();
            }

            if (hasValidSurface)
            {
                DrawCirclePreview(previewCenter);
            }
        }
        else if (spawnCubeEnabled)
        {
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                SpawnRandomInCube(previewCenter);
                e.Use();
            }

            if (hasValidSurface)
            {
                DrawCubePreview(previewCenter);
            }
        }
    }

    void DrawCirclePreview(Vector3 center)
    {
        if (prefabToSpawn == null || numberOfObjects <= 0) return;

        Handles.color = Color.cyan;
        Handles.DrawWireDisc(center, Vector3.up, radius);

        float angleStep = 360f / numberOfObjects;

        for (int i = 0; i < numberOfObjects; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(
                center.x + Mathf.Cos(angle) * radius,
                center.y,
                center.z + Mathf.Sin(angle) * radius
            );
            Handles.SphereHandleCap(0, pos, Quaternion.identity, 0.2f, EventType.Repaint);
        }
    }

    void PlaceObjectsInCircle(Vector3 center)
    {
        if (prefabToSpawn == null) return;

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Place Circle Objects");

        float angleStep = 360f / numberOfObjects;

        for (int i = 0; i < numberOfObjects; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;

            Vector3 position = new Vector3(
                center.x + Mathf.Cos(angle) * radius,
                center.y,
                center.z + Mathf.Sin(angle) * radius
            );

            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
            Undo.RegisterCreatedObjectUndo(obj, "Spawn Object");
            obj.transform.position = position;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    void SpawnRandomInCube(Vector3 center)
    {
        if (prefabToSpawn == null) return;

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Spawn Cube Objects");

        for (int i = 0; i < numberOfObjects; i++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-cubeSize.x / 2f, cubeSize.x / 2f),
                Random.Range(-cubeSize.y / 2f, cubeSize.y / 2f),
                Random.Range(-cubeSize.z / 2f, cubeSize.z / 2f)
            );

            Vector3 spawnPos = center + randomOffset;

            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
            Undo.RegisterCreatedObjectUndo(obj, "Spawn Cube Object");
            obj.transform.position = spawnPos;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    void DrawCubePreview(Vector3 center)
    {
        Handles.color = Color.green;
        Handles.DrawWireCube(center, cubeSize);
    }
}