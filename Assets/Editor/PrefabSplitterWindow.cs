using UnityEngine;
using UnityEditor;
using EzySlice;

namespace PrefabSplitterTool
{
    public class PrefabSplitterWindow : EditorWindow
    {
        private Vector3 pointA, pointB;
        private bool hasFirstPoint = false;
        private bool pointsReady = false;

        private GameObject selectedObject;
        private UnityEngine.Plane slicePlane;

        private bool toolEnabled = true;

        [MenuItem("Tools/Prefab Splitter")]
        public static void ShowWindow()
        {
            GetWindow<PrefabSplitterWindow>("Prefab Splitter");
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Prefab Splitter Tool", EditorStyles.boldLabel);

            toolEnabled = EditorGUILayout.Toggle("Tool Attivo", toolEnabled);
            EditorGUILayout.Space();

            if (selectedObject != null)
            {
                EditorGUILayout.LabelField("Oggetto selezionato:", selectedObject.name);
            }
            else
            {
                EditorGUILayout.HelpBox("CTRL + Click sinistro per selezionare un oggetto in scena.", MessageType.Info);
            }

            if (hasFirstPoint)
            {
                EditorGUILayout.LabelField("Punto A:", pointA.ToString("F2"));
            }

            if (pointsReady)
            {
                EditorGUILayout.LabelField("Punto B:", pointB.ToString("F2"));
                if (GUILayout.Button("Esegui Taglio"))
                {
                    ExecuteSlice();
                }
            }

            EditorGUILayout.HelpBox("SHIFT + Click due punti per definire il piano di taglio.", MessageType.None);
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!toolEnabled) return;

            Event e = Event.current;
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            // CTRL + Click sinistro: seleziona oggetto
            if (e.type == EventType.MouseDown && e.button == 0 && e.control)
            {
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    selectedObject = hit.collider.gameObject;
                    EnsureMeshCollider(selectedObject);
                    hasFirstPoint = false;
                    pointsReady = false;
                    pointA = pointB = Vector3.zero;

                    Debug.Log($"Oggetto selezionato: {selectedObject.name}");
                    e.Use();
                }
            }

            // SHIFT + Click sinistro: imposta punti di taglio
            if (e.type == EventType.MouseDown && e.button == 0 && e.shift && selectedObject != null)
            {
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (!hasFirstPoint)
                    {
                        pointA = hit.point;
                        hasFirstPoint = true;
                        pointsReady = false;
                    }
                    else
                    {
                        pointB = hit.point;

                        Vector3 direction = pointB - pointA;
                        Vector3 normal = Vector3.Cross(direction, Vector3.up).normalized;
                        slicePlane = new UnityEngine.Plane(normal, (pointA + pointB) * 0.5f);

                        pointsReady = true;
                    }

                    e.Use();
                }
            }

            // Debug visivo
            if (hasFirstPoint)
            {
                Handles.color = Color.red;
                Handles.DrawWireDisc(pointA, Vector3.up, 0.1f);
            }

            if (pointsReady)
            {
                Handles.color = Color.green;
                Handles.DrawLine(pointA, pointB);
            }
        }

        private void ExecuteSlice()
        {
            if (selectedObject == null || !pointsReady)
            {
                Debug.LogWarning("Taglio non pronto: assicurati di selezionare un oggetto e due punti.");
                return;
            }

            Renderer renderer = selectedObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                Debug.LogWarning("L'oggetto selezionato non ha un Renderer valido.");
                return;
            }

            Material mat = renderer.sharedMaterial;
            Vector3 planePoint = slicePlane.normal * -slicePlane.distance;

            SlicedHull hull = selectedObject.Slice(planePoint, slicePlane.normal, mat);

            if (hull != null)
            {
                GameObject upper = hull.CreateUpperHull(selectedObject, mat);
                GameObject lower = hull.CreateLowerHull(selectedObject, mat);

                upper.name = selectedObject.name + "_Upper";
                lower.name = selectedObject.name + "_Lower";

                upper.transform.SetPositionAndRotation(selectedObject.transform.position, selectedObject.transform.rotation);
                lower.transform.SetPositionAndRotation(selectedObject.transform.position, selectedObject.transform.rotation);

                upper.transform.localScale = selectedObject.transform.localScale;
                lower.transform.localScale = selectedObject.transform.localScale;

                EnsureMeshCollider(upper);
                EnsureMeshCollider(lower);

                Object.DestroyImmediate(selectedObject);

                Debug.Log("Taglio completato.");
            }
            else
            {
                Debug.LogWarning("Il taglio è fallito.");
            }

            // Reset
            hasFirstPoint = false;
            pointsReady = false;
            selectedObject = null;
            pointA = pointB = Vector3.zero;
        }

        private void EnsureMeshCollider(GameObject obj)
        {
            if (!obj.TryGetComponent<Collider>(out _))
            {
                MeshFilter mf = obj.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    MeshCollider mc = obj.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                    mc.convex = true;
                }
            }
        }
    }
}
