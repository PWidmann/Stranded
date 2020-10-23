
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    CinemachineVirtualCamera cMvirtualCamera;
    public float cameraZoomRate = 3f;
    Vector3 target;
    NavMeshAgent agent;
    RaycastHit hitInfo;
    Ray ray;
    Camera cam;

    void Start()
    {
        cam = Camera.main;
        agent = GetComponent<NavMeshAgent>();
        cMvirtualCamera = cam.GetComponent<CinemachineVirtualCamera>();
        cMvirtualCamera.Follow = gameObject.transform;
        cMvirtualCamera.LookAt = gameObject.transform;
    }


    void Update()
    {
        Movement();
        CameraZoom();
    }

    void Movement()
    {
        ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButton(1))
        {
            if (Physics.Raycast(ray, out hitInfo, maxDistance: 300f))
            {
                if (hitInfo.collider.CompareTag("Terrain"))
                {
                    target = hitInfo.point;
                    agent.SetDestination(target);
                    //Debug.DrawRay(Camera.main.transform.position, hitInfo.point - Camera.main.transform.position, Color.green);
                }
            }
        }
    }

    void CameraZoom()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            if (cMvirtualCamera.m_Lens.OrthographicSize <= 5.6f && cMvirtualCamera.m_Lens.OrthographicSize >= 2.4f)
            {
                cMvirtualCamera.m_Lens.OrthographicSize += Input.GetAxis("Mouse ScrollWheel") * cameraZoomRate * -1;
            } 
        }

        // Clamp camera distance
        if (cMvirtualCamera.m_Lens.OrthographicSize > 5.6f)
            cMvirtualCamera.m_Lens.OrthographicSize = 5.6f;
        if (cMvirtualCamera.m_Lens.OrthographicSize < 2.4f)
            cMvirtualCamera.m_Lens.OrthographicSize = 2.4f;
    }
}

