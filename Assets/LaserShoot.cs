using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserShoot : MonoBehaviour
{

    Camera _mainCamera;

    [SerializeField]
    Transform LaserSpawnPoint;
    
    [SerializeField]
    float laserRange = 100f;

    [SerializeField]
    List<Vector3> laserPoints;

    
    LineRenderer laserLine;

    [SerializeField]
    bool isShooting = false;

    private void Awake() {
        if (_mainCamera == null)
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        laserLine = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            isShooting = !isShooting;
        }

        if(isShooting)
            PreShootLaser();
        else
        {
            laserLine.enabled = false;
            laserLine.positionCount = 1;
        }
    }

    void PreShootLaser(){
        laserPoints = new List<Vector3>
        {
            LaserSpawnPoint.position
        };
        PreCheckLaser(LaserSpawnPoint.position, _mainCamera.transform.forward);
        laserLine.enabled = true;
        laserLine.positionCount = laserPoints.Count;
        laserLine.SetPositions(laserPoints.ToArray());
        laserLine.SetPosition(0, LaserSpawnPoint.position);
    }

    void PreCheckLaser(Vector3 startPoint, Vector3 direction)
    {
        // Ray ray = new(startPoint, direction);
        // if (Physics.Raycast(ray, out RaycastHit hit, laserRange, ~LayerMask.GetMask("PortalBody")))
        // {
        //     laserPoints.Add(hit.point);
        //     if (hit.collider.CompareTag("PortalScreen") && hit.collider!=this.GetComponent<Collider>())
        //     {
        //         Vector3 pointOfInterest=hit.transform.InverseTransformPoint(hit.point);
        //         GameObject portal = hit.transform.gameObject;
        //         PortalScript portalScript = portal.GetComponent<PortalScript>();
                
        //         Transform portalTransform = portalScript.GetOtherPortal().transform;
        //         GameObject otherPortal = portalScript.GetOtherPortal().gameObject;
        //         PortalLaser portalLaser = otherPortal.GetComponentInChildren<PortalLaser>();
                
        //         Vector3 portalDirection = hit.transform.InverseTransformDirection(direction);

                
        //         Vector3 otherPortalStartPoint = portalTransform.TransformPoint(pointOfInterest);
        //         Vector3 otherPortalDirection = portalTransform.TransformDirection(portalDirection);
                
        //         portalLaser.PreShootLaser(otherPortalStartPoint, otherPortalDirection);

        //     }
        // }
        // else
        // {
        //     laserPoints.Add(startPoint + direction * laserRange);
        // }
    }
}
