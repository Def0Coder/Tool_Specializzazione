using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
[CreateAssetMenu(fileName = "SceneArchive", menuName = "Scene Tools/Scene Archive")]
#endif
public class SceneArchive : ScriptableObject
{



    [Serializable]
    public class ObjectData
    {
        public string objectName;
        public int instanceID;
        public bool wasActive;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public string prefabPath;

        // Hierarchy
        public string parentName;


        // Dati mesh
        public Mesh mesh;
        public Material[] materials;

        // Dati luce
        public Color lightColor;
        public float lightIntensity;
        public float lightRange;

        // Dati collider
        public Vector3 colliderCenter;
        public Vector3 colliderSize;

        // Rigidbody
        public bool useGravity;
        public bool isKinematic;
        public float mass;

        // Animator
        public RuntimeAnimatorController animatorController;
        public Avatar animatorAvatar;

        // Particle System
        public bool psLooping;
        public float psStartLifetime;
        public float psStartSpeed;
        public Color psStartColor;

    }

    public List<ObjectData> archivedObjects = new List<ObjectData>();
    public string sceneName;
    public DateTime creationTime;

    public void CaptureObjects(List<GameObject> objects)
    {
        archivedObjects.Clear();
        sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        creationTime = DateTime.Now;

        foreach (var obj in objects)
        {
            if (obj == null) continue;

            var data = new ObjectData
            {
                objectName = obj.name,
                instanceID = obj.GetInstanceID(),
                wasActive = obj.activeSelf,
                position = obj.transform.position,
                rotation = obj.transform.eulerAngles,
                scale = obj.transform.localScale
            };

#if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(obj))
            {
                data.prefabPath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
            }
#endif

            // Salva componenti mesh
            var meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter != null) data.mesh = meshFilter.sharedMesh;

            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null) data.materials = renderer.sharedMaterials;

            // Salva dati luce
            var light = obj.GetComponent<Light>();
            if (light != null)
            {
                data.lightColor = light.color;
                data.lightIntensity = light.intensity;
                data.lightRange = light.range;
            }

            // Salva dati collider
            var boxCollider = obj.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                data.colliderCenter = boxCollider.center;
                data.colliderSize = boxCollider.size;
            }

            // Hierarchy
            if (obj.transform.parent != null)
                data.parentName = obj.transform.parent.name;

            // Rigidbody
            var rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                data.useGravity = rb.useGravity;
                data.isKinematic = rb.isKinematic;
                data.mass = rb.mass;
            }

            // Animator
            var animator = obj.GetComponent<Animator>();
            if (animator != null)
            {
                data.animatorController = animator.runtimeAnimatorController;
                data.animatorAvatar = animator.avatar;
            }

            // ParticleSystem
            var ps = obj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                data.psLooping = main.loop;
                data.psStartLifetime = main.startLifetime.constant;
                data.psStartSpeed = main.startSpeed.constant;
                data.psStartColor = main.startColor.color;
            }

            archivedObjects.Add(data);
        }
    }
}