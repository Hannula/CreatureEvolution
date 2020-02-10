using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Camera cam;
    public float panSpeed = 1f;
    public float mousePanSpeed = 1f;
    public float panLerpSpeed = 1f;
    private Vector3 targetPosition;
    public float zoomSpeed = 1f;
    public float zoomLerpSpeed = 1f;
    private float cameraZoomTarget = 5f;
    public Vector2 zoomRange;

    // Rotation
    public float rotationSpeed = 1f;

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        float zoomInput = Input.GetAxis("Mouse ScrollWheel");

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Pan
        targetPosition += (transform.right *  horizontalInput * Time.deltaTime * panSpeed * cameraZoomTarget)
            + (transform.up * verticalInput * Time.deltaTime * panSpeed * cameraZoomTarget);
        
        // Pan mouse
        if (Input.GetAxis("MousePan") > 0)
        {
            targetPosition -= (transform.right * mouseX * Time.deltaTime * mousePanSpeed * cameraZoomTarget)
            + (transform.up * mouseY* Time.deltaTime * mousePanSpeed * cameraZoomTarget);
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, panLerpSpeed * Time.deltaTime);

        // Zoom
        cameraZoomTarget = Mathf.Clamp(cameraZoomTarget - zoomInput * zoomSpeed * Time.deltaTime, zoomRange.x, zoomRange.y);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, cameraZoomTarget, zoomLerpSpeed * Time.deltaTime);

        // Rotation
        if (Input.GetAxis("MouseRotate") > 0)
        {
            transform.Rotate(Vector3.forward, mouseX * Time.deltaTime * rotationSpeed);
        }

    }
}
