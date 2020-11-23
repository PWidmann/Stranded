using Cinemachine;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    //Movement
    [Header("Movement")]
    Vector3 velocity;
    public float runSpeed = 1.7f;
    private float gravity = -10;
    private float speedSmoothTime = 0.5f;
    float speedSmoothVelocity;
    float currentSpeed;
    float velocityY;

    public float turnspeed = 300f;
    
    //References
    Animator animator;
    CharacterController controller;
    float animationSpeedPercent;

    Vector3 target;
    Vector3 targetDirection;
    Ray ray;
    RaycastHit hitInfo;
    Camera cam;
    float distanceToTarget = 0f;


    Vector2 mouseDown = Vector2.zero;
    Vector2 mouseUp = Vector2.zero;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        cam = Camera.main;

        target = transform.position;
        GameManager.playerPosition = transform.position;

    }

    void Update()
    {
        ClickToMove();
        Movement();
    }

    void Movement()
    {

        Move();

        // animator
        animationSpeedPercent = currentSpeed / runSpeed;
        animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
        if (transform.position.y < 0.3f)
            animator.SetBool("isSwimming", true);
        else
            animator.SetBool("isSwimming", false);
    }

    void ClickToMove()
    {
        ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(1))
        {
            mouseDown = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }

        if (Input.GetMouseButtonUp(1))
        { 
            mouseUp = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            if (mouseDown != Vector2.zero && mouseDown == mouseUp)
            {
                if (Physics.Raycast(ray, out hitInfo, maxDistance: 300f))
                {
                    if (hitInfo.collider.CompareTag("Terrain") || hitInfo.collider.CompareTag("Water"))
                    {
                        Vector3 hit = new Vector3(Mathf.Floor(hitInfo.point.x) + 0.5f, Mathf.Floor(hitInfo.point.y), Mathf.Floor(hitInfo.point.z) + 0.5f);
                        target = hit;

                        Debug.DrawRay(Camera.main.transform.position, hitInfo.point - Camera.main.transform.position, Color.green);
                    }
                }
            }
        }

        

    }

    void Move()
    {
        distanceToTarget = Vector3.Distance(new Vector3(target.x, 0, target.z), new Vector3(transform.position.x, 0, transform.position.z));

        if (distanceToTarget < 0.1f)
        {
            target = Vector3.zero;
            targetDirection = Vector3.zero;
        }

        // Smooth runspeed 
        float targetSpeed = runSpeed * targetDirection.magnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

        //Gravity
        velocityY += Time.deltaTime * gravity;

        if (target != Vector3.zero)
        {
            targetDirection = target - transform.position;
            targetDirection.y = 0;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(targetDirection), Time.deltaTime * turnspeed);

            velocity = transform.forward * runSpeed + Vector3.up * velocityY;
            controller.Move(velocity * Time.deltaTime);
        }

        //Grounded
        if (controller.isGrounded)
        {
            velocityY = 0; // Deactivate gravity
        }
    }
}
