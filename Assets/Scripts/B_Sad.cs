using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class B_Sad : MonoBehaviour
{
    private Renderer objectRenderer;
    private Rigidbody rb;
    public float bounceForce = 1f;

    void Start()
    {
        // Prende il Renderer per cambiare colore
        objectRenderer = GetComponent<Renderer>();

        // Prende il Rigidbody per far rimbalzare l'oggetto
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning("Nessun Rigidbody trovato! Aggiungendone uno automaticamente.");
            rb = gameObject.AddComponent<Rigidbody>();
        }
    }


    private void Update()
    {
        
       // ChangeColor();
    }


    private void OnCollisionEnter(Collision collision)
    {
        // Cambia colore quando tocca qualcosa
        //ChangeColor();

        // Aggiunge una forza verso l'alto per rimbalzare
        rb.velocity = new Vector3(rb.velocity.x, bounceForce, rb.velocity.z);
    }

    void ChangeColor()
    {
        if (objectRenderer != null)
        {
            objectRenderer.material.color = new Color(Random.value, Random.value, Random.value);
        }
    }
}
