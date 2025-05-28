using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDoor : MonoBehaviour
{
    [SerializeField]
    private OpenCloseDoor doorScript; // Reference to the OpenCloseDoor script

    void Start()
    {
        doorScript = transform.parent.Find("Door_Hallway").GetComponent<OpenCloseDoor>(); // Get the OpenCloseDoor script from the parent object
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            doorScript.OpenDoor(); // Call the OpenDoor method from OpenCloseDoor script
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            doorScript.CloseDoor(); // Call the CloseDoor method from OpenCloseDoor script
        }
    }
}
