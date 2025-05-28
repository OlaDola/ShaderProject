using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    [SerializeField]
    private OpenCloseDoor doorScript;

    void Start()
    {
        doorScript = GetComponentInParent<OpenCloseDoor>();
        if (doorScript == null)
        {
            Debug.LogError("OpenCloseDoor script not found in parent object.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (doorScript != null && !doorScript.isDoorOpen)
            {
                doorScript.OpenDoor(); // Open the door
                Debug.Log("Door is opened.");
            }
        }
    }
}
