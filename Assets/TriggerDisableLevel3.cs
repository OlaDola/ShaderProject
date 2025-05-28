using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDisableLevel3 : MonoBehaviour
{
    [SerializeField]
    private Level3Mechanic level3Mechanic;

    [SerializeField]
    PortalSwitch portalSwitch;

    [SerializeField]
    private GameObject crossHairUI;

    void Start()
    {
        level3Mechanic = transform.parent.GetComponent<Level3Mechanic>();
        if (level3Mechanic == null)
        {
            Debug.LogError("Level3Mechanic component not found in parent object.");
        }
        portalSwitch = Camera.main.transform.parent.GetComponent<PortalSwitch>();
        if(crossHairUI == null)
        {
            Debug.LogError("Crosshair UI not assigned.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            crossHairUI.SetActive(true); // Show the crosshair UI
            portalSwitch.enabled = true; // Enable the portal switch
            Debug.Log("Level 3 activated.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            level3Mechanic.DisableLevel();
            portalSwitch.enabled = false; // Disable the portal switch
            crossHairUI.SetActive(false); // Hide the crosshair UI
            Debug.Log("Level 3 deactivated.");
        }
    }
}
