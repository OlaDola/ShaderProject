using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDisableLevel2 : MonoBehaviour
{
    [SerializeField]

    private Level2Mechanic level2Mechanic;

    void Start()
    {
        level2Mechanic = transform.parent.GetComponent<Level2Mechanic>();
        if (level2Mechanic == null)
        {
            Debug.LogError("Level2Mechanic component not found in parent object.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            level2Mechanic.DisableLevel();
            Debug.Log("Level 2 deactivated.");
        }
    }
}
