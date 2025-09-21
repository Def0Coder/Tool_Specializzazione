// PrefabSplitterWindow.cs
using UnityEngine;
using UnityEditor;
using EzySlice;

namespace PrefabSplitterTool
{
    public class PrefabSplitterWindow : EditorWindow
    {
        private const string PLANE_NAME = "_SlicePlane_EDITOR";
        private const float DEFAULT_PLANE_SIZE = 1.5f;

        private GameObject targetObject;          // l'oggetto da tagliare
        private GameObject slicePlaneObject;      // la quad che rappresenta il piano di taglio
        private Material previewMaterial;         // materiale di anteprima (trasparente)
        private bool toolEnabled = true;
        private bool autoAttach = true;           // attacca piano automaticamente alla selezione
        private bool keepPlaneAfterSlice = false; // se true non distrugge il piano dopo il taglio
        private float planeSize = DEFAULT_PLANE_SIZE;

        [MenuItem("Tools/Prefab Splitter")]
        public static void ShowWindow()
        {
            GetWindow<PrefabSplitterWindow>("Prefab Splitter");
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            CreatePreviewMaterialIfNeeded();
            UpdateTargetFromSelection();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSelectionChange()
        {
            UpdateTargetFromSelection();
            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Prefab Splitter Tool", EditorStyles.boldLabel);
            toolEnabled = EditorGUILayout.Toggle("Tool Attivo", toolEnabled);

            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(!toolEnabled);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Target:", GUILayout.Width(50));
            EditorGUILayout.ObjectField(targetObject, typeof(GameObject), true);
            EditorGUILayout.EndHorizontal();

            autoAttach = EditorGUILayout.Toggle("Auto Attach alla selezione", autoAttach);
            planeSize = EditorGUILayout.Slider("Dimensione Piano", planeSize, 0.1f, 10f);
            keepPlaneAfterSlice = EditorGUILayout.Toggle("Mantieni piano dopo taglio", keepPlaneAfterSlice);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Crea/Attacca Piano"))
            {
                if (targetObject != null)
                {
                    CreateOrRepositionPlane();
                }
                else Debug.LogWarning("Seleziona un oggetto (con MeshFilter) prima di creare il piano.");
            }

            if (GUILayout.Button("Rimuovi Piano"))
            {
                RemoveSlicePlane();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (GUILayout.Button("Esegui Taglio"))
            {
                ExecuteSlice();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Seleziona un oggetto (CTRL+Click o dalla gerarchia). Usa gli strumenti di trasformazione (Move/Rotate/Scale) per posizionare il piano, oppure muovilo con gli handle verdi che appaiono in scena.",
                MessageType.Info);

            EditorGUI.EndDisabledGroup();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!toolEnabled) return;

            // scelta via CTRL+click (comoda)
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && e.control)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    var go = hit.collider.gameObject;
                    if (go != null)
                    {
                        // se abbiamo cliccato su un slice plane creato dal tool, agganciamo il parent come target
                        if (go.name == PLANE_NAME && go.transform.parent != null)
                        {
                            Selection.activeGameObject = go.transform.parent.gameObject;
                            targetObject = go.transform.parent.gameObject;
                        }
                        else
                        {
                            Selection.activeGameObject = go;
                            targetObject = go;
                        }

                        if (autoAttach) CreateOrRepositionPlane();
                        Repaint();
                        e.Use();
                    }
                }
            }

            // se il piano esiste, mostriamo gli handle (pos/rot/scale) anche se il piano non è selezionato
            if (slicePlaneObject != null)
            {
                // draw a faint outline (the quad renderer already shows it, but this helps)
                Handles.color = Color.green;
                var p = slicePlaneObject.transform;
                Vector3[] verts = new Vector3[4];
                float half = 0.5f;
                verts[0] = p.TransformPoint(new Vector3(-half, -half, 0) * p.localScale.x);
                verts[1] = p.TransformPoint(new Vector3(-half, half, 0) * p.localScale.y);
                verts[2] = p.TransformPoint(new Vector3(half, half, 0) * p.localScale.x);
                verts[3] = p.TransformPoint(new Vector3(half, -half, 0) * p.localScale.y);
                Handles.DrawSolidRectangleWithOutline(verts, new Color(0, 1f, 0, 0.08f), Color.green);

                EditorGUI.BeginChangeCheck();

                // Position handle
                Vector3 newPos = Handles.PositionHandle(slicePlaneObject.transform.position, slicePlaneObject.transform.rotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(slicePlaneObject.transform, "Move Slice Plane");
                    slicePlaneObject.transform.position = newPos;
                    EditorUtility.SetDirty(slicePlaneObject);
                }

                EditorGUI.BeginChangeCheck();
                Quaternion newRot = Handles.RotationHandle(slicePlaneObject.transform.rotation, slicePlaneObject.transform.position);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(slicePlaneObject.transform, "Rotate Slice Plane");
                    slicePlaneObject.transform.rotation = newRot;
                    EditorUtility.SetDirty(slicePlaneObject);
                }

                EditorGUI.BeginChangeCheck();
                Vector3 newScale = Handles.ScaleHandle(slicePlaneObject.transform.localScale, slicePlaneObject.transform.position, slicePlaneObject.transform.rotation, HandleUtility.GetHandleSize(slicePlaneObject.transform.position));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(slicePlaneObject.transform, "Scale Slice Plane");
                    slicePlaneObject.transform.localScale = newScale;
                    EditorUtility.SetDirty(slicePlaneObject);
                }
            }
        }

        private void UpdateTargetFromSelection()
        {
            var sel = Selection.activeGameObject;
            if (sel == null)
            {
                targetObject = null;
                slicePlaneObject = null;
                return;
            }

            // se l'oggetto selezionato è un slice-plane creato precedentemente, aggancia il parent come target
            if (sel.name == PLANE_NAME && sel.transform.parent != null)
            {
                targetObject = sel.transform.parent.gameObject;
                slicePlaneObject = sel;
                return;
            }

            // altrimenti, se l'oggetto selezionato ha MeshFilter, impostalo come target
            if (sel.GetComponent<MeshFilter>() != null)
            {
                targetObject = sel;
                // tenta di trovare un eventuale piano già presente
                var existingPlane = sel.transform.Find(PLANE_NAME);
                slicePlaneObject = existingPlane ? existingPlane.gameObject : null;

                if (autoAttach && slicePlaneObject == null)
                {
                    CreateOrRepositionPlane();
                }
            }
            else
            {
                // non valido come target -> non cambiare
                // mantiene target precedente
            }
        }

        private void CreatePreviewMaterialIfNeeded()
        {
            if (previewMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null)
                {
                    shader = Shader.Find("Hidden/Internal-Colored");
                }
                previewMaterial = new Material(shader);
                previewMaterial.hideFlags = HideFlags.HideAndDontSave;
                previewMaterial.SetColor("_Color", new Color(0f, 1f, 0f, 0.18f));
            }
        }

        private void CreateOrRepositionPlane()
        {
            if (targetObject == null)
            {
                Debug.LogWarning("Nessun oggetto target valido per attaccare il piano.");
                return;
            }

            // cerca piano esistente
            Transform existing = targetObject.transform.Find(PLANE_NAME);
            if (existing != null)
            {
                slicePlaneObject = existing.gameObject;
            }
            else
            {
                // crea una Quad come figlio (editor-only)
                slicePlaneObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                slicePlaneObject.name = PLANE_NAME;
                Undo.RegisterCreatedObjectUndo(slicePlaneObject, "Create Slice Plane");
                slicePlaneObject.transform.SetParent(targetObject.transform, false);
            }

            // rimuovi collider (non serve per preview)
            var col = slicePlaneObject.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            // assegna materiale preview
            CreatePreviewMaterialIfNeeded();
            var mr = slicePlaneObject.GetComponent<MeshRenderer>();
            if (mr == null) mr = slicePlaneObject.AddComponent<MeshRenderer>();
            mr.sharedMaterial = previewMaterial;

            // posizione centrale/iniziale: davanti al centro dell'oggetto
            Undo.RecordObject(slicePlaneObject.transform, "Position Slice Plane");
            slicePlaneObject.transform.localPosition = Vector3.zero;
            slicePlaneObject.transform.localRotation = Quaternion.identity;
            slicePlaneObject.transform.localScale = Vector3.one * planeSize;
            EditorUtility.SetDirty(slicePlaneObject);

            Selection.activeGameObject = targetObject; // manteniamo la selezione sul target per comodità
            Debug.Log($"Slice plane attaccato a '{targetObject.name}'. Modificalo con Gizmo o selezionando '{PLANE_NAME}' nella gerarchia.");
        }

        private void RemoveSlicePlane()
        {
            if (slicePlaneObject != null)
            {
                Undo.DestroyObjectImmediate(slicePlaneObject);
                slicePlaneObject = null;
            }
        }

        private void ExecuteSlice()
        {
            if (targetObject == null)
            {
                Debug.LogWarning("Nessun oggetto selezionato per il taglio.");
                return;
            }

            // se l'utente ha selezionato la slice plane come selection, recupera parent
            if (Selection.activeGameObject != null && Selection.activeGameObject.name == PLANE_NAME && Selection.activeGameObject.transform.parent != null)
            {
                targetObject = Selection.activeGameObject.transform.parent.gameObject;
            }

            // preferisci il piano creato; altrimenti usa un piano generato centrato sull'oggetto
            Transform planeTransform = slicePlaneObject != null ? slicePlaneObject.transform : null;

            if (planeTransform == null)
            {
                Debug.LogWarning("Nessun piano di taglio disponibile. Crea il piano prima di eseguire il taglio.");
                return;
            }

            // assicurati che il target abbia MeshFilter + MeshRenderer
            var mf = targetObject.GetComponent<MeshFilter>();
            var mr = targetObject.GetComponent<MeshRenderer>();
            if (mf == null || mf.sharedMesh == null || mr == null)
            {
                Debug.LogWarning("L'oggetto selezionato non ha MeshFilter/MeshRenderer con mesh valida.");
                return;
            }

            // punto e normale del piano (il Quad ha normale in forward)
            Vector3 planePoint = planeTransform.position;
            Vector3 planeNormal = planeTransform.forward;

            // usa primo materiale come cap
            Material capMat = mr.sharedMaterial != null ? mr.sharedMaterial : previewMaterial;

            SlicedHull hull = null;
            try
            {
                hull = targetObject.Slice(planePoint, planeNormal, capMat);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"EzySlice ha sollevato un'eccezione: {ex.Message}");
                hull = null;
            }

            if (hull != null)
            {
                GameObject upper = hull.CreateUpperHull(targetObject, capMat);
                GameObject lower = hull.CreateLowerHull(targetObject, capMat);

                if (upper != null) upper.name = targetObject.name + "_Upper";
                if (lower != null) lower.name = targetObject.name + "_Lower";

                // mantieni la trasformazione del target
                if (upper != null)
                {
                    Undo.RegisterCreatedObjectUndo(upper, "Create Upper Hull");
                    upper.transform.SetPositionAndRotation(targetObject.transform.position, targetObject.transform.rotation);
                    upper.transform.localScale = targetObject.transform.localScale;
                    upper.transform.SetParent(targetObject.transform.parent, true);
                    EnsureMeshCollider(upper);
                }
                if (lower != null)
                {
                    Undo.RegisterCreatedObjectUndo(lower, "Create Lower Hull");
                    lower.transform.SetPositionAndRotation(targetObject.transform.position, targetObject.transform.rotation);
                    lower.transform.localScale = targetObject.transform.localScale;
                    lower.transform.SetParent(targetObject.transform.parent, true);
                    EnsureMeshCollider(lower);
                }

                // distruggi l'originale
                Undo.DestroyObjectImmediate(targetObject);

                Debug.Log("Taglio completato.");
            }
            else
            {
                Debug.LogWarning("Il taglio è fallito (hull == null).");
            }

            if (!keepPlaneAfterSlice)
            {
                RemoveSlicePlane();
            }

            // reset stato interno
            targetObject = null;
            Selection.activeGameObject = null;
        }

        private void EnsureMeshCollider(GameObject obj)
        {
            if (obj == null) return;

            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                MeshCollider mc = obj.GetComponent<MeshCollider>();
                if (mc == null)
                {
                    mc = obj.AddComponent<MeshCollider>();
                }
                mc.sharedMesh = mf.sharedMesh;
                mc.convex = true;
            }
        }
    }
}