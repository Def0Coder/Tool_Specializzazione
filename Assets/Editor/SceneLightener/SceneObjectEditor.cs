//using UnityEditor;
//using UnityEngine;
//using System.IO;

//public static class BackupTool
//{
//    public static void BackupObject(GameObject obj)
//    {
//        // Crea la cartella se non esiste
//        string backupPath = "Assets/_Project/Scenes/Backups";
//        if (!AssetDatabase.IsValidFolder(backupPath))
//        {
//            AssetDatabase.CreateFolder("Assets/_Project/Scenes", "Backups");
//        }

//        // Crea l'asset di backup
//        var archive = ScriptableObject.CreateInstance<SceneArchive>();
//        archive.CaptureFrom(obj);

//        string assetPath = $"{backupPath}/ARCH_{obj.name}_{System.DateTime.Now:yyyyMMddHHmmss}.asset";
//        AssetDatabase.CreateAsset(archive, assetPath);

//        // Disabilita l'oggetto nella scena
//        obj.SetActive(false);
//        Debug.Log($"Backup creato: {assetPath}");
//    }
//}