using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class B_Happy : MonoBehaviour
{
    public float radius = 3f; // Raggio del cerchio
    public float speed = 2f; // Velocità di rotazione
    private float angle = 0f;

    private Renderer objectRenderer;
    private Vector3 startPosition;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        startPosition = transform.position;
    }

    void Update()
    {
        // Calcola la nuova posizione lungo un percorso circolare
        angle += speed * Time.deltaTime;
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        transform.position = new Vector3(startPosition.x + x, startPosition.y, startPosition.z + z);
    }
}
