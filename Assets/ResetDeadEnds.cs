using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetDeadEnds : MonoBehaviour
{
    [SerializeField]
    GameObject portal;

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            portal.SetActive(false);
        }
    }
}
