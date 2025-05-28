using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level3Mechanic : MonoBehaviour
{
    [SerializeField]
    GameObject[] portals;

    [SerializeField]
    CardSlideDoor CardSlideDoor;

    void Start()
    {
        DisableLevel();
        CardSlideDoor = transform.Find("Level3End").GetComponentInChildren<CardSlideDoor>();
    }

    public void DisableLevel()
    {
        for (int i = 0; i < portals.Length; i++)
        {
            PortalScript portalScript = portals[i].transform.Find("Portal/PortalBothWayFix").GetComponent<PortalScript>();
            portalScript.enabled = false;
            portalScript.OtherPortal= null;
            GameObject screen = portals[i].transform.Find("Portal/PortalBothWayFix/PerfectSquarePortal/Screen").gameObject;
            screen.GetComponent<MeshCollider>().enabled = true;
        }
    }

}
