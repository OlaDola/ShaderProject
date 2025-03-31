using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PortalLaser : MonoBehaviour
{
    int maxLaserTeleport = 2;

    [SerializeField]
    float laserRange = 100f;

    [SerializeField]
    LineRenderer[] laserLine;

    [SerializeField]
    List<Vector3> laserPoints;

    void Awake()
    {
        laserLine = new LineRenderer[maxLaserTeleport];
        for (int i = 0; i < maxLaserTeleport; i++)
        {
            GameObject laser = new GameObject("Laser" + i);
            
            laser.transform.SetParent(this.transform);
            laserLine[i] = laser.AddComponent<LineRenderer>();
            laserLine[i].material = new Material(Shader.Find("Sprites/Default"));
            laserLine[i].startWidth = 0.1f;
            laserLine[i].endWidth = 0.1f;
            laserLine[i].positionCount = 0;
            laserLine[i].enabled = false;
        }
    }

    public void PreShootLaser(Vector3 laserStartPoint, Vector3 direction, int laserIndex=0){
        GameObject portalScreen = gameObject.transform.parent.GetComponentInChildren<BoxCollider>().gameObject;

        float distanceToPlane = SignedDistancePlanePoint(portalScreen.transform.forward, portalScreen.transform.position, laserStartPoint);
        
        // If the laser start point is behind the portal screen, we need to move it to the front of the screen
        if (distanceToPlane < 0)
        {
            laserStartPoint += portalScreen.transform.forward * Mathf.Abs(distanceToPlane);
        }
        else
        {
            laserStartPoint -= portalScreen.transform.forward * distanceToPlane;
        }
        
    
        laserPoints = new List<Vector3>
        {
            laserStartPoint
        };

        PreCheckLaser(laserStartPoint, direction, laserIndex);
    }

    void PreCheckLaser(Vector3 startPoint, Vector3 direction, int laserIndex)
    {   
        // if (laserIndex > maxLaserTeleport - 1)
        //     return;
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
                
        //         pointOfInterest.x += hit.transform.localScale.x;
        //         Vector3 otherPortalStartPoint = portalTransform.TransformPoint(pointOfInterest);
        //         Vector3 otherPortalDirection = portalTransform.TransformDirection(portalDirection);
                
        //         portalLaser.PreShootLaser(otherPortalStartPoint, otherPortalDirection, laserIndex + 1);
        //     }
        // }
        // else
        // {
        //     laserPoints.Add(startPoint + direction * laserRange);
        // }
        // laserLine[laserIndex].enabled = true;
        // laserLine[laserIndex].positionCount = laserPoints.Count;
        // laserLine[laserIndex].SetPositions(laserPoints.ToArray());
    }

    //Get the shortest distance between a point and a plane. The output is signed so it holds information
    //as to which side of the plane normal the point is.
    public static float SignedDistancePlanePoint(Vector3 planeNormal, Vector3 planePoint, Vector3 point){

        return Vector3.Dot(planeNormal, (point - planePoint));
    }

}
