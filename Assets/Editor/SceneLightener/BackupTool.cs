#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class BackupTool
{
    private const string BACKUP_PATH = "Assets/_Project/Scenes/Backups";
    private const int MAX_SELECTION = 10;

    [MenuItem("GameObject/Scene Tools/Backup Selected Objects", false, 20)]
    private static void BackupSelectedObjects()
    {
        var selection = Selection.gameObjects.Take(MAX_SELECTION).ToList();
        if (selection.Count == 0)
        {
            Debug.LogWarning("No objects selected for backup");
            return;
        }

        BackupObjects(selection);
    }

    [MenuItem("GameObject/Scene Tools/Backup Selected Objects", true, 20)]
    private static bool ValidateBackupSelectedObjects()
    {
        return Selection.gameObjects.Length > 0 && Selection.gameObjects.Length <= MAX_SELECTION;
    }

    public static void BackupObjects(List<GameObject> objects)
    {
        if (objects.Count == 0) return;
        if (objects.Count > MAX_SELECTION)
        {
            objects = objects.Take(MAX_SELECTION).ToList();
        }

        // Crea cartella se necessario
        if (!AssetDatabase.IsValidFolder(BACKUP_PATH))
        {
            ProjectSetupTool.CreateBackupFolder();
        }

        // Crea asset di archivio
        var archive = ScriptableObject.CreateInstance<SceneArchive>();
        archive.CaptureObjects(objects);

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string sceneName = Path.GetFileNameWithoutExtension(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);
        string assetName = $"ARCH_{sceneName}_{objects.Count}objs_{timestamp}.asset";
        string assetPath = Path.Combine(BACKUP_PATH, assetName);

        AssetDatabase.CreateAsset(archive, assetPath);
        AssetDatabase.SaveAssets();

        // Disabilita gli oggetti in scena
        foreach (var obj in objects)
        {
            obj.SetActive(false);
            EditorUtility.SetDirty(obj);
        }

        Debug.Log($"<color=green>✓</color> Backed up {objects.Count} objects to {assetPath}", archive);
    }

    public static void RestoreArchive(SceneArchive archive)
    {
        if (archive == null || archive.archivedObjects.Count == 0) return;

        foreach (var objData in archive.archivedObjects)
        {
            RestoreObject(objData);
        }

        Debug.Log($"<color=green>✓</color> Restored {archive.archivedObjects.Count} objects from archive");
    }

    private static void RestoreObject(SceneArchive.ObjectData data)
    {
        GameObject restoredObject = null;

        // Prova a ripristinare da prefab
        if (!string.IsNullOrEmpty(data.prefabPath))
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.prefabPath);
            if (prefab != null)
            {
                restoredObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            }
        }

        // Crea un nuovo oggetto se non c'è prefab
        if (restoredObject == null)
        {
            restoredObject = new GameObject(data.objectName);
        }

        // Applica trasformazione
        Transform t = restoredObject.transform;
        t.position = data.position;
        t.eulerAngles = data.rotation;
        t.localScale = data.scale;

        // Imposta stato attivo
        restoredObject.SetActive(data.wasActive);

        // Ripristina componenti
        RestoreMeshComponents(restoredObject, data);
        RestoreLightComponent(restoredObject, data);
        RestoreColliderComponent(restoredObject, data);
    }

    private static void RestoreMeshComponents(GameObject obj, SceneArchive.ObjectData data)
    {
        if (data.mesh != null)
        {
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if (mf == null) mf = obj.AddComponent<MeshFilter>();
            mf.sharedMesh = data.mesh;
        }

        if (data.materials != null && data.materials.Length > 0)
        {
            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            if (mr == null) mr = obj.AddComponent<MeshRenderer>();
            mr.sharedMaterials = data.materials;
        }
    }

    private static void RestoreLightComponent(GameObject obj, SceneArchive.ObjectData data)
    {
        if (data.lightIntensity > 0)
        {
            Light light = obj.GetComponent<Light>();
            if (light == null) light = obj.AddComponent<Light>();

            light.color = data.lightColor;
            light.intensity = data.lightIntensity;
            light.range = data.lightRange;
        }
    }

    private static void RestoreColliderComponent(GameObject obj, SceneArchive.ObjectData data)
    {
        if (data.colliderSize != Vector3.zero)
        {
            BoxCollider collider = obj.GetComponent<BoxCollider>();
            if (collider == null) collider = obj.AddComponent<BoxCollider>();

            collider.center = data.colliderCenter;
            collider.size = data.colliderSize;
        }
    }
}
#endif