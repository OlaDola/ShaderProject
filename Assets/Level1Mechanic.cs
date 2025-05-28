using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1Mechanic : MonoBehaviour
{
    [SerializeField]
    PortalScript[] portals;

    [SerializeField]
    GameObject[] portalObjects;

    [SerializeField]
    bool isActive = false;

    void Start()
    {
        portals = new PortalScript[portalObjects.Length];
        for (int i = 0; i < portalObjects.Length; i++)
        {
            portals[i] = portalObjects[i].GetComponentInChildren<PortalScript>();
            if (portals[i] == null)
            {
                Debug.LogError("PortalScript not found in " + portalObjects[i].name);
            }
            portals[i].enabled = false;
        }
    }

    public void ActivatePortals()
    {
        if (isActive) return; // Prevent reactivation if already active
        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].enabled = true;
        }
        isActive = true;
    }
    public void DeactivatePortals()
    {   
        if (!isActive) return; // Prevent deactivation if already inactive
        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].enabled = false;
        }
        isActive = false;
    }
}
