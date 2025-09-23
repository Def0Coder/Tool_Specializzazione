#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class ProjectSetupTool
{
    private static readonly string[] defaultFolders =
    {
        "_Project",
        "_Project/Art",
        "_Project/Materials",
        "_Project/Scripts",
        "_Project/Scenes",
        "_Project/Scenes/Backups",
        "_Project/Audio",
        "_Project/Prefabs"
    };

    [MenuItem("Tools/Project Setup/Create Default Structure")]
    public static void CreateDefaultStructure()
    {
        CreateFolders(defaultFolders);
        CreateDefaultAssets();
        AssetDatabase.Refresh();
        Debug.Log("<color=green>✓ Project structure and templates created successfully!</color>");
    }

    private static void CreateFolders(string[] paths)
    {
        foreach (var path in paths)
        {
            string fullPath = Path.Combine("Assets", path);
            if (Directory.Exists(fullPath)) continue;

            Directory.CreateDirectory(fullPath);
            Debug.Log($"<color=cyan>Created:</color> {fullPath}");
        }
    }

    private static void CreateDefaultAssets()
    {
        // Crea script base
        string scriptPath = "Assets/_Project/Scripts/PlayerController.cs";
        if (!File.Exists(scriptPath))
        {
            File.WriteAllText(scriptPath, GetPlayerControllerTemplate());
            Debug.Log("<color=magenta>Created default script:</color> PlayerController.cs");
        }

        // Crea materiale base
        string materialPath = "Assets/_Project/Materials/DefaultMaterial.mat";
        if (!File.Exists(materialPath))
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = Color.white;
            AssetDatabase.CreateAsset(mat, materialPath);
            Debug.Log("<color=magenta>Created default material:</color> DefaultMaterial.mat");
        }

        // Crea prefab base
        string prefabPath = "Assets/_Project/Prefabs/DefaultCapsule.prefab";
        if (!File.Exists(prefabPath))
        {
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.AddComponent<Rigidbody>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(capsule, prefabPath);
            Object.DestroyImmediate(capsule);
            Debug.Log("<color=magenta>Created default prefab:</color> DefaultCapsule.prefab");
        }
    }

    private static string GetPlayerControllerTemplate()
    {
        return
@"using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float moveX = Input.GetAxis(""Horizontal"");
        float moveZ = Input.GetAxis(""Vertical"");
        Vector3 move = new Vector3(moveX, 0, moveZ) * moveSpeed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }
}";
    }
}
#endif