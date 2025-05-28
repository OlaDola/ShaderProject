using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerLevelActive : MonoBehaviour
{
    [SerializeField]
    private string levelName;
    [SerializeField]
    private GameObject levelObjects;

    void Start()
    {
        levelObjects = transform.parent.gameObject;
        levelName = levelObjects.name;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject player = Camera.main.transform.parent.gameObject;
            if (player != null)
            {
                player.GetComponent<PortalRotation>().enabled = true;
            }
            Level1Mechanic levelMechanic = levelObjects.GetComponent<Level1Mechanic>();
            if (levelMechanic != null)
            {
                levelMechanic.ActivatePortals();
            }
            Debug.Log("Level activated: " + levelName);
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {   
            GameObject player = Camera.main.transform.parent.gameObject;
            if (player != null)
            {
                player.GetComponent<PortalRotation>().enabled = false;
            }
            Level1Mechanic levelMechanic = levelObjects.GetComponent<Level1Mechanic>();
            if (levelMechanic != null)
            {
                levelMechanic.DeactivatePortals();
            }
            Debug.Log("Level deactivated: " + levelName);
        }
    }
}
