using UnityEditor;
using UnityEngine;
using System.Collections;

public class Enemy_Generator : EditorWindow
{
    private GameObject combattente;
    private GameObject tank;
    private GameObject assassino;

    public string nome;


    private bool addRigidbody = false;

    private MonoScript mad_Script;
    private MonoScript happy_Script;
    private MonoScript sad_Script;

    private MonoScript referenceScript;

    public int selectedIndex = 0;

    public int scriptIndex = 0;

    private string[] classi = { "Combattente", "Tank", "Assassino" };

    private string[] comportamenti = { "Cattivo", "Felice", "Triste" };


    [MenuItem("Tool/Enemy_Generator")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(Enemy_Generator));
    }

    void OnGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);

        nome = EditorGUILayout.TextField("Nome", nome);


        GUILayout.Space(10);

        combattente = (GameObject)EditorGUILayout.ObjectField("Combattente", combattente, typeof(GameObject), false);
        tank = (GameObject)EditorGUILayout.ObjectField("Tank", tank, typeof(GameObject), false);
        assassino = (GameObject)EditorGUILayout.ObjectField("Assassino", assassino, typeof(GameObject), false);


        selectedIndex = EditorGUILayout.Popup("Classi", selectedIndex, classi);



        GUILayout.Space(30);



        // Opzione per aggiungere un Rigidbody
        addRigidbody = EditorGUILayout.Toggle("Aggiungi Rigidbody", addRigidbody);

        GUILayout.Space(20);




        // Selezione di uno script personalizzato
        mad_Script = (MonoScript)EditorGUILayout.ObjectField("Script Cattivo", mad_Script, typeof(MonoScript), false);
        happy_Script = (MonoScript)EditorGUILayout.ObjectField("Script Felice", happy_Script, typeof(MonoScript), false);
        sad_Script = (MonoScript)EditorGUILayout.ObjectField("Script Triste", sad_Script, typeof(MonoScript), false);

        scriptIndex = EditorGUILayout.Popup("Script", scriptIndex, comportamenti);


        GUILayout.Space(10);




        if (GUILayout.Button("Stampa Selezione"))
        {
            CreaNemico();
        }

    }


    public void CreaNemico()
    {
        

        switch (selectedIndex)
        {
            case 0:
                GameObject newObj = Instantiate(combattente, new Vector3(0, 3, 0), Quaternion.identity);
                newObj.name = string.IsNullOrWhiteSpace(nome) ? newObj.name : nome;

                if (addRigidbody)
                {
                    if (!newObj.GetComponent<Rigidbody>()) // Evita duplicati
                        newObj.AddComponent<Rigidbody>();
                }

                // Aggiungi script personalizzato se selezionato
                if (mad_Script != null)
                {
                    System.Type scriptType = referenceScript.GetClass();
                    if (scriptType != null && newObj.GetComponent(scriptType) == null) // Evita duplicati
                    {
                        newObj.AddComponent(scriptType);
                    }
                }

                break;
            case 1:
                newObj = Instantiate(tank, new Vector3(0, 3, 0), Quaternion.identity);
                newObj.name = string.IsNullOrWhiteSpace(nome) ? newObj.name : nome;

                if (addRigidbody)
                {
                    if (!newObj.GetComponent<Rigidbody>()) // Evita duplicati
                        newObj.AddComponent<Rigidbody>();
                }

                // Aggiungi script personalizzato se selezionato
                if (mad_Script != null)
                {
                    System.Type scriptType = mad_Script.GetClass();
                    if (scriptType != null && newObj.GetComponent(scriptType) == null) // Evita duplicati
                    {
                        newObj.AddComponent(scriptType);
                    }
                }
                break;
            case 2:
                newObj = Instantiate(assassino, new Vector3(0, 3, 0), Quaternion.identity);
                newObj.name = string.IsNullOrWhiteSpace(nome) ? newObj.name : nome;

                if (addRigidbody)
                {
                    if (!newObj.GetComponent<Rigidbody>()) // Evita duplicati
                        newObj.AddComponent<Rigidbody>();
                }

                // Aggiungi script personalizzato se selezionato
                if (mad_Script != null)
                {
                    System.Type scriptType = mad_Script.GetClass();
                    if (scriptType != null && newObj.GetComponent(scriptType) == null) // Evita duplicati
                    {
                        newObj.AddComponent(scriptType);
                    }
                }
                break;

            

        }
        


    }



}
