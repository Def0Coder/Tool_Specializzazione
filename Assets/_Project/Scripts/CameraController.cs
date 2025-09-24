using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public Transform player; 
    public Transform cam;    

    [Header("Impostazioni generali")]
    public float sensitivity = 3f;
    public float minY = -40f;
    public float maxY = 70f;

    [Header("3° Persona")]
    public float thirdPersonDistance = 3f;
    public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, 0);

    [Header("1° Persona")]
    public Vector3 firstPersonOffset = new Vector3(0, 1.7f, 0);

    private float rotX;
    private float rotY;
    private bool isFirstPerson = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Input mouse
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        rotX += mouseX;
        rotY -= mouseY;
        rotY = Mathf.Clamp(rotY, minY, maxY);

        // Cambio modalità con il tasto V
        if (Input.GetKeyDown(KeyCode.V))
            isFirstPerson = !isFirstPerson;
    }

    void LateUpdate()
    {

        player.rotation = Quaternion.Euler(0, rotX, 0);

        if (isFirstPerson)
        {

            cam.position = player.position + player.TransformDirection(firstPersonOffset);
            cam.rotation = Quaternion.Euler(rotY, rotX, 0);
        }
        else
        {
           
            Vector3 targetPos = player.position + player.TransformDirection(thirdPersonOffset);
            Vector3 camOffset = Quaternion.Euler(rotY, rotX, 0) * new Vector3(0, 0, -thirdPersonDistance);
            cam.position = targetPos + camOffset;
            cam.LookAt(targetPos);
        }
    }
}
