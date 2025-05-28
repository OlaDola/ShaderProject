using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalFail : MonoBehaviour
{
    [SerializeField]
    GameObject portal;
    [SerializeField]
    GameObject portalDeadEndA;
    [SerializeField]
    GameObject portalDeadEndB;



    void Start()
    {
        portal = transform.parent.Find("Portal").gameObject;
        portalDeadEndA = transform.parent.Find("DeadEndPortal").gameObject;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            portal.SetActive(false);
            portalDeadEndB.SetActive(true);
            

            portalDeadEndA.GetComponentInChildren<PortalScript>().OtherPortal = portalDeadEndB.GetComponentInChildren<PortalScript>();
            portalDeadEndB.GetComponentInChildren<PortalScript>().OtherPortal = portalDeadEndA.GetComponentInChildren<PortalScript>();

            portalDeadEndA.GetComponentInChildren<PortalScript>().enabled = true;
            portalDeadEndB.GetComponentInChildren<PortalScript>().enabled = true;
        }
    }

}
