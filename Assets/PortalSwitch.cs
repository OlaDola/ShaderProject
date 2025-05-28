using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalSwitch : MonoBehaviour
{
    public float activationDistance = 10f;

    public Material defaultMaterial;

    [SerializeField]
    PortalScript portal1;
    [SerializeField]
    PortalScript portal1OtherSide;

    [SerializeField]
    bool portal1Set = false;

    [SerializeField]
    PortalScript portal2;
    
    [SerializeField]
    PortalScript portal2OtherSide;

    [SerializeField]
    bool portal2Set = false;

    [SerializeField]
    Transform player;

    int layerMask;


    void Start()
    {
        if(player == null)
        {
            player = Camera.main.transform;
        }
        layerMask = LayerMask.GetMask("Portal"); // Set the layer mask to include only the Portal layer
    }

    void Update()
    {
        (bool isLookingAtPortal, PortalScript portal) = IsPlayerLookingAtPortal();
        if(isLookingAtPortal)
        {
            if(Input.GetMouseButtonDown(0) && portal!= portal2){
                portal1 = portal;
                portal1OtherSide = portal.OtherPortal;
                portal1Set = true;
                ActivatePortal();
            }
            if(Input.GetMouseButtonDown(1) && portal!= portal1){
                portal2 = portal;
                portal2OtherSide = portal.OtherPortal;
                portal2Set = true;
                ActivatePortal();
            }
        }
    }

    private void ActivatePortal()
    {
        if(portal1Set && portal2Set)
        {
            portal1.SwitchPortal(portal2);
            if(portal1OtherSide != null && portal1OtherSide != portal2){
                portal1OtherSide.DeactivatePortal();
                portal1OtherSide.transform.Find("PerfectSquarePortal/Screen").GetComponent<MeshCollider>().enabled = true;
                portal1OtherSide.transform.Find("PerfectSquarePortal/Screen").GetComponent<MeshRenderer>().material = defaultMaterial;
            }
                
            portal2.SwitchPortal(portal1);
            if(portal2OtherSide != null && portal2OtherSide != portal1){
                portal2OtherSide.DeactivatePortal();
                portal2OtherSide.transform.Find("PerfectSquarePortal/Screen").GetComponent<MeshCollider>().enabled = true;
                portal2OtherSide.transform.Find("PerfectSquarePortal/Screen").GetComponent<MeshRenderer>().material = defaultMaterial;
            }

            if (Vector3.Dot(portal1.transform.forward, portal2.transform.position - portal1.transform.position) > 0 &&
                Vector3.Dot(portal2.transform.forward, portal1.transform.position - portal2.transform.position) > 0)
            {
                portal1.transform.parent.Rotate(0, 180, 0);
            }
            else if (Vector3.Dot(portal1.transform.forward, portal2.transform.position - portal1.transform.position) < 0 &&
                     Vector3.Dot(portal2.transform.forward, portal1.transform.position - portal2.transform.position) < 0)
            {
                portal2.transform.parent.Rotate(0, 180, 0);
            }

            portal1.enabled = true;
            portal1.transform.Find("PerfectSquarePortal/Screen").GetComponent<MeshCollider>().enabled = false;

            portal2.enabled = true;
            portal2.transform.Find("PerfectSquarePortal/Screen").GetComponent<MeshCollider>().enabled = false;

            portal1Set = false;
            portal2Set = false;
            portal1 = null;
            portal2 = null;
        }
    }

    private (bool, PortalScript) IsPlayerLookingAtPortal()
    {
        // Vector3 directionToPortal = (transform.position - player.position).normalized;

        Ray ray = new Ray(player.position, player.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, activationDistance))
        {
            if (hit.transform.GetComponent<PortalScript>() == null)
            {
                Debug.DrawLine(player.position, hit.point, Color.red);
                return (false, null); // Ray hit something else
            }
            Debug.DrawLine(player.position, hit.point, Color.green);
            return (true, hit.transform.GetComponent<PortalScript>()); // Ray hit the portal
        }

        return (false, null); // Ray did not hit the portal
    }
}
