using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float normalSpeed;
    public float fastSpeed;
    public float currentFloorHeight = 1f;
    public float movementSpeed;
    public float movementTime;
    //public float rotationAmount;

    public GameObject player;

    // Zoom
    public Vector3 zoomAmount;
    public float minCameraDistance;
    public float maxCameraDistance;
    private Vector3 newZoom;
    private float cameraDistance = -6f;

    private Camera cam;

    bool IsMouseOverGameWindow { get { return !(0 > Input.mousePosition.x || 0 > Input.mousePosition.y || Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y); } }

    // Drag
    private Vector3 newPosition;
    private Vector3 dragStartPosition;
    private Vector3 dragCurrentPosition;

    //Rotation
    private Quaternion newRotation;
    private Vector3 rotateStartPosition;
    private Vector3 rotateCurrentPosition;

    void Start()
    {
        newPosition = transform.position;
        newRotation = transform.rotation;
        cam = GetComponentInChildren<Camera>();
        
        newZoom = cam.transform.localPosition;
    }

    private void Update()
    {
        HandleMouseInput();
        HandleMovementInput();

    }

    void HandleMovementInput()
    {
        // Fast camera movement
        if (Input.GetKey(KeyCode.LeftShift))
            movementSpeed = fastSpeed;
        else
            movementSpeed = normalSpeed;

        // Movement
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            newPosition += (transform.forward * movementSpeed);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            newPosition += (transform.forward * -movementSpeed);
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            newPosition += (transform.right * movementSpeed);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            newPosition += (transform.right * -movementSpeed);
        }

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, zoomAmount * cameraDistance, Time.deltaTime * movementTime);
    }

    void HandleMouseInput()
    {


        // Zoom (Mouse scroll wheel)
        if (Input.GetAxis("Mouse ScrollWheel") > 0f && IsMouseOverGameWindow) // forward
        {
            cameraDistance = Mathf.Clamp(cameraDistance + 2f, maxCameraDistance, minCameraDistance);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f && IsMouseOverGameWindow) // backwards
        {
            cameraDistance = Mathf.Clamp(cameraDistance - 2f, maxCameraDistance, minCameraDistance);
        }

        // Drag Camera (Right mouse button)
        if (Input.GetMouseButtonDown(1))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
                dragStartPosition = ray.GetPoint(entry);
        }
        if (Input.GetMouseButton(1))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
                dragCurrentPosition = ray.GetPoint(entry);

            newPosition = transform.position + dragStartPosition - dragCurrentPosition;
        }

        // Rotate Camera (Middle mouse button)
        if (Input.GetMouseButtonDown(2))
        {
            rotateStartPosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(2))
        {
            rotateCurrentPosition = Input.mousePosition;

            Vector3 difference = rotateStartPosition - rotateCurrentPosition;

            rotateStartPosition = rotateCurrentPosition;

            newRotation *= Quaternion.Euler(Vector3.up * (difference.x / 5f));
        }
    }
}
