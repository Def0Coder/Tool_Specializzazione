using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class KeepPlaymodeChangesHandler
{
    // Flag per evitare ripetizioni multiple
    private static bool _changesSaved = false;

    // Inizializza al caricamento dello script (anche quando ricompili in editor)
    static KeepPlaymodeChangesHandler()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Quando stai per uscire dal Play Mode, copia le modifiche (questo esempio assume l'esistenza del tuo LevelDesignerTool in scena)
        if (state == PlayModeStateChange.ExitingPlayMode && !_changesSaved)
        {
            LevelDesignerTool tool = Object.FindObjectOfType<LevelDesignerTool>();
            if (tool != null)
            {
                // Registra le modifiche per il sistema di Undo e segnala a Unity che l'oggetto è cambiato
                Undo.RecordObject(tool, "Keep Playmode Changes");
                EditorUtility.SetDirty(tool);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

                Debug.Log("Le modifiche apportate in Play Mode sono state salvate in Edit Mode.");
                _changesSaved = true;
            }
        }
        // Reset del flag quando si rientra in Edit Mode
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            _changesSaved = false;
        }
    }
}