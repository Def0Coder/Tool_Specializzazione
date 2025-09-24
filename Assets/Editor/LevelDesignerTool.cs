using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class LevelDesignerTool : EditorWindow
{
    // === Prefab Management ===
    List<GameObject> prefabVariants = new List<GameObject>();
    string parentGroupName = "SpawnedGroup";

    // === Randomization ===
    Vector2 randomRotationY = new Vector2(0, 0);
    Vector2 randomScaleY = new Vector2(1, 1);

    // === Tools State ===
    bool spawnCircleEnabled = false;
    bool spawnCubeEnabled = false;
    bool pathToolEnabled = false;
    bool brushToolEnabled = false;
    bool randomCircleAreaToolEnabled = false;
    bool replaceToolEnabled = false;

    // === Tool Parameters ===
    int numberOfObjects = 8;
    float radius = 5f;
    Vector3 cubeSize = new Vector3(5, 5, 5);
    float pathSpacing = 2f;
    float brushSpacing = 1f;

    // === Replace Tool ===
    GameObject replacePrefab;

    // === Path Tool ===
    List<Vector3> pathPoints = new List<Vector3>();

    // === Brush Tool state ===
    bool isBrushing = false;
    Vector3 lastBrushPos;

    // === Scene Preview ===
    Vector3 previewCenter = Vector3.zero;
    bool hasValidSurface = false;

    [MenuItem("Tools/Level Designer Tool")]
    public static void ShowWindow() => GetWindow<LevelDesignerTool>("Level Designer Tool");

    void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
    void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

    void OnGUI()
    {
        GUILayout.Label("Level Designer Tool", EditorStyles.boldLabel);

        // === Tool Toggles ===
        DrawToolToggle("Circle Tool", ref spawnCircleEnabled);
        DrawToolToggle("Cube Tool", ref spawnCubeEnabled);
        DrawToolToggle("Path Tool", ref pathToolEnabled);
        DrawToolToggle("Brush Tool", ref brushToolEnabled);
        DrawToolToggle("Random Circle Area Tool", ref randomCircleAreaToolEnabled);

        replaceToolEnabled = EditorGUILayout.Toggle("Replace Tool", replaceToolEnabled);
        if (replaceToolEnabled)
        {
            replacePrefab = (GameObject)EditorGUILayout.ObjectField("Prefab di rimpiazzo", replacePrefab, typeof(GameObject), false);
            EditorGUILayout.HelpBox("Seleziona oggetti in scena e premi R per sostituirli.", MessageType.Info);
        }

        EditorGUILayout.Space(10);

        // === Prefab Variants ===
        if (spawnCircleEnabled || spawnCubeEnabled || pathToolEnabled || brushToolEnabled || randomCircleAreaToolEnabled)
        {
            GUILayout.Label("Prefab Varianti", EditorStyles.boldLabel);

            int toRemove = -1;
            for (int i = 0; i < prefabVariants.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                prefabVariants[i] = (GameObject)EditorGUILayout.ObjectField($"Prefab {i + 1}", prefabVariants[i], typeof(GameObject), false);
                if (GUILayout.Button("Rimuovi", GUILayout.Width(60))) toRemove = i;
                EditorGUILayout.EndHorizontal();
            }
            if (toRemove >= 0) prefabVariants.RemoveAt(toRemove);
            if (GUILayout.Button("➕ Aggiungi Prefab")) prefabVariants.Add(null);

            randomRotationY = EditorGUILayout.Vector2Field("Rotazione Y random", randomRotationY);
            randomScaleY = EditorGUILayout.Vector2Field("Scala Y random", randomScaleY);
            parentGroupName = EditorGUILayout.TextField("Nome gruppo parent", parentGroupName);
        }

        // === Tool-specific fields and instructions ===
        if (spawnCircleEnabled)
        {
            numberOfObjects = EditorGUILayout.IntField("Numero oggetti", numberOfObjects);
            radius = EditorGUILayout.FloatField("Raggio cerchio", radius);
            EditorGUILayout.HelpBox("Scrolla con la rotella per modificare il raggio.\nClick sinistro = piazza oggetti lungo il cerchio.", MessageType.Info);
        }

        if (spawnCubeEnabled)
        {
            numberOfObjects = EditorGUILayout.IntField("Numero oggetti", numberOfObjects);
            cubeSize = EditorGUILayout.Vector3Field("Dimensione Cubo", cubeSize);
            EditorGUILayout.HelpBox("Click sinistro = piazza oggetti random nel cubo.", MessageType.Info);
        }

        if (pathToolEnabled)
        {
            pathSpacing = EditorGUILayout.FloatField("Distanza tra oggetti", pathSpacing);
            if (GUILayout.Button("Conferma Path")) PlacePathObjects();
            if (GUILayout.Button("Cancella Path")) pathPoints.Clear();
            EditorGUILayout.HelpBox("Click sinistro = aggiungi punti.\nInvio o 'Conferma Path' = piazza oggetti.\n'Cancella Path' = reset path.", MessageType.Info);
        }

        if (brushToolEnabled)
        {
            brushSpacing = EditorGUILayout.FloatField("Spaziatura Brush", brushSpacing);
            EditorGUILayout.HelpBox("Click sinistro = piazza prefab singolo.\nAlt + trascina = dipinge prefab lungo il movimento.", MessageType.Info);
        }

        if (randomCircleAreaToolEnabled)
        {
            numberOfObjects = EditorGUILayout.IntField("Numero oggetti", numberOfObjects);
            radius = EditorGUILayout.FloatField("Raggio cerchio", radius);
            EditorGUILayout.HelpBox("Scrolla con la rotella per modificare il raggio.\nClick sinistro = piazza N prefab random dentro il cerchio.", MessageType.Info);
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            previewCenter = hit.point;
            hasValidSurface = true;
        }
        else hasValidSurface = false;

        // Scroll raggio per Circle Tool
        if (spawnCircleEnabled && e.type == EventType.ScrollWheel)
        {
            radius = Mathf.Max(0.1f, radius - e.delta.y * 0.2f);
            e.Use(); SceneView.RepaintAll();
        }

        // Scroll raggio per Random Circle Area Tool
        if (randomCircleAreaToolEnabled && e.type == EventType.ScrollWheel)
        {
            radius = Mathf.Max(0.1f, radius - e.delta.y * 0.2f);
            e.Use(); SceneView.RepaintAll();
        }

        // Tools handling
        if (spawnCircleEnabled) HandleCircleTool(e);
        if (spawnCubeEnabled) HandleCubeTool(e);
        if (pathToolEnabled) HandlePathTool(e);
        if (brushToolEnabled) HandleBrushTool(e);
        if (randomCircleAreaToolEnabled) HandleRandomCircleTool(e);

        // Replace Tool
        if (replaceToolEnabled && replacePrefab != null && e.type == EventType.KeyDown && e.keyCode == KeyCode.R)
        {
            foreach (GameObject selected in Selection.gameObjects)
            {
                GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(replacePrefab);
                Undo.RegisterCreatedObjectUndo(newObj, "Replace Object");
                newObj.transform.position = selected.transform.position;
                newObj.transform.rotation = selected.transform.rotation;
                newObj.transform.localScale = selected.transform.localScale;
                Undo.DestroyObjectImmediate(selected);
            }
            e.Use();
        }
    }

    // ================= Tool Implementations =================

    void DrawToolToggle(string label, ref bool state)
    {
        bool newState = EditorGUILayout.Toggle(label, state);
        if (newState != state && newState)
        {
            spawnCircleEnabled = spawnCubeEnabled = pathToolEnabled = brushToolEnabled = randomCircleAreaToolEnabled = false;
            state = true;
        }
        else if (!newState) state = false;
    }

    GameObject SpawnPrefab(Vector3 position)
    {
        if (prefabVariants.Count == 0) return null;
        GameObject prefab = prefabVariants[Random.Range(0, prefabVariants.Count)];
        if (prefab == null) return null;

        GameObject obj;
        if (PrefabUtility.IsPartOfPrefabAsset(prefab)) obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        else obj = Instantiate(prefab);

        Undo.RegisterCreatedObjectUndo(obj, "Spawn Object");
        obj.transform.position = position;

        float rotY = Random.Range(randomRotationY.x, randomRotationY.y);
        obj.transform.rotation = Quaternion.Euler(0, rotY, 0);

        float scaleY = Random.Range(randomScaleY.x, randomScaleY.y);
        obj.transform.localScale = new Vector3(1, scaleY, 1);

        return obj;
    }

    void HandleCircleTool(Event e)
    {
        if (!hasValidSurface) return;
        Handles.color = Color.cyan;
        Handles.DrawWireDisc(previewCenter, Vector3.up, radius);

        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            GameObject parent = new GameObject(parentGroupName);
            float angleStep = 360f / numberOfObjects;

            for (int i = 0; i < numberOfObjects; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 pos = previewCenter + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                GameObject obj = SpawnPrefab(pos);
                if (obj != null) obj.transform.SetParent(parent.transform);
            }
            e.Use();
        }
    }

    void HandleCubeTool(Event e)
    {
        if (!hasValidSurface) return;
        Handles.color = Color.green;
        Handles.DrawWireCube(previewCenter, cubeSize);

        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            GameObject parent = new GameObject(parentGroupName);

            for (int i = 0; i < numberOfObjects; i++)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-cubeSize.x / 2f, cubeSize.x / 2f),
                    Random.Range(-cubeSize.y / 2f, cubeSize.y / 2f),
                    Random.Range(-cubeSize.z / 2f, cubeSize.z / 2f)
                );
                GameObject obj = SpawnPrefab(previewCenter + offset);
                if (obj != null) obj.transform.SetParent(parent.transform);
            }
            e.Use();
        }
    }

    void HandlePathTool(Event e)
    {
        Handles.color = Color.red;
        for (int i = 0; i < pathPoints.Count - 1; i++) Handles.DrawLine(pathPoints[i], pathPoints[i + 1]);

        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt && hasValidSurface)
        {
            pathPoints.Add(previewCenter);
            e.Use();
        }
    }

    void PlacePathObjects()
    {
        if (pathPoints.Count < 2 || prefabVariants.Count == 0) return;

        GameObject parent = new GameObject(parentGroupName);
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            Vector3 start = pathPoints[i];
            Vector3 end = pathPoints[i + 1];
            float dist = Vector3.Distance(start, end);
            int steps = Mathf.CeilToInt(dist / pathSpacing);

            for (int s = 0; s <= steps; s++)
            {
                Vector3 pos = Vector3.Lerp(start, end, s / (float)steps);
                GameObject obj = SpawnPrefab(pos);
                if (obj != null) obj.transform.SetParent(parent.transform);
            }
        }
        pathPoints.Clear();
    }

    void HandleBrushTool(Event e)
    {
        if (!hasValidSurface) return;
        Handles.color = Color.magenta;
        Handles.SphereHandleCap(0, previewCenter, Quaternion.identity, 0.3f, EventType.Repaint);

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            isBrushing = true;
            lastBrushPos = previewCenter;
            PlaceBrushObject();
            e.Use();
        }
        else if (e.type == EventType.MouseUp && e.button == 0) isBrushing = false;
        else if (isBrushing && e.type == EventType.MouseDrag && e.button == 0 && e.alt)
        {
            if (Vector3.Distance(lastBrushPos, previewCenter) >= brushSpacing)
            {
                PlaceBrushObject();
                lastBrushPos = previewCenter;
                e.Use();
            }
        }
    }

    void PlaceBrushObject()
    {
        GameObject parent = GameObject.Find(parentGroupName) ?? new GameObject(parentGroupName);
        GameObject obj = SpawnPrefab(previewCenter);
        if (obj != null) obj.transform.SetParent(parent.transform);
    }

    void HandleRandomCircleTool(Event e)
    {
        if (!hasValidSurface) return;
        Handles.color = Color.red;
        Handles.DrawWireDisc(previewCenter, Vector3.up, radius);

        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            GameObject parent = new GameObject(parentGroupName);

            for (int i = 0; i < numberOfObjects; i++)
            {
                Vector2 rnd = Random.insideUnitCircle * radius;
                Vector3 pos = previewCenter + new Vector3(rnd.x, 0, rnd.y);
                GameObject obj = SpawnPrefab(pos);
                if (obj != null) obj.transform.SetParent(parent.transform);
            }
            e.Use();
        }
    }
}