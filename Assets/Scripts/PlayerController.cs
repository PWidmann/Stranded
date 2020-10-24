using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    //Movement
    [Header("Movement")]
    Vector2 inputDir;
    Vector3 velocity;
    public float runSpeed = 1;
    public float gravity = -10;
    public float turnSmoothTime = 0.05f;
    float turnSmoothVelocity;
    public float speedSmoothTime = 0.1f;
    float speedSmoothVelocity;
    float currentSpeed;
    float velocityY;
    private float targetRotation;
    public float cameraZoomRate = 2f;
    //References
    Animator animator;
    Transform cameraT;
    CinemachineVirtualCamera cMVirtualCamera;
    RaycastHit hitInfo;
    CharacterController controller;
    float angle;
    float animationSpeedPercent;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        cameraT = Camera.main.transform;
        cMVirtualCamera = cameraT.GetComponent<CinemachineVirtualCamera>();

        cMVirtualCamera.Follow = gameObject.transform;
        cMVirtualCamera.LookAt = gameObject.transform;
    }

    void Update()
    {
        Movement();
        CameraZoom();
    }

    private void LateUpdate()
    {
        
    }
    void Movement()
    {
        //Movement
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        inputDir = input.normalized;
        Move(inputDir);

        // animator
        animationSpeedPercent = currentSpeed / runSpeed;
        animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
    }

    void Move(Vector2 inputDir)
    {

        //Gravity
        velocityY += Time.deltaTime * gravity;

        //Player Rotation
        if (inputDir != Vector2.zero)
        {
            // Turn the player in movement direction when not aiming
            targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
        }

        // Smooth runspeed 
        float targetSpeed = runSpeed * inputDir.magnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

        velocity = transform.forward * currentSpeed + Vector3.up * velocityY;
        controller.Move(velocity * Time.deltaTime);

        //Grounded
        if (controller.isGrounded)
        {
            velocityY = 0; // Deactivate gravity
        }
    }

    void CameraZoom()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            if (cMVirtualCamera.m_Lens.OrthographicSize <= 5.6f && cMVirtualCamera.m_Lens.OrthographicSize >= 2.4f)
            {
                cMVirtualCamera.m_Lens.OrthographicSize += Input.GetAxis("Mouse ScrollWheel") * cameraZoomRate * -1;
            }
        }

        // Clamp camera distance
        if (cMVirtualCamera.m_Lens.OrthographicSize > 5.6f)
            cMVirtualCamera.m_Lens.OrthographicSize = 5.6f;
        if (cMVirtualCamera.m_Lens.OrthographicSize < 2.4f)
            cMVirtualCamera.m_Lens.OrthographicSize = 2.4f;
    }
}
