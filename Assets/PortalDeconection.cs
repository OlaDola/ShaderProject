using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalDeconection : MonoBehaviour
{
    [SerializeField]
    GameObject player;
    [SerializeField]
    GameObject StartRoom;
    [SerializeField]
    GameObject LastRoom;
    [SerializeField]
    GameObject[] pairsToActivate;
    [SerializeField]
    GameObject[] pairsToDeactivate;

    void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player"))
        {
            if(transform.parent.name == "Pair1"){
                StartRoom.GetComponentInChildren<PortalScript>().enabled = true;
                LastRoom.GetComponentInChildren<PortalScript>().DeactivatePortal();
            }
            if(transform.parent.name == "Pair2" || transform.parent.name == "Pair3"){
                StartRoom.GetComponentInChildren<PortalScript>().DeactivatePortal();
                LastRoom.GetComponentInChildren<PortalScript>().DeactivatePortal();
            }

            if(transform.parent.name == "Pair4"){
                StartRoom.GetComponentInChildren<PortalScript>().DeactivatePortal();
                LastRoom.GetComponentInChildren<PortalScript>().enabled = true;
            }
            for(int i = 0; i < pairsToActivate.Length; i++)
            {
                pairsToActivate[i].transform.Find("Room1/Portal").GetComponentInChildren<PortalScript>().enabled = true;
                pairsToActivate[i].transform.Find("Room2/Portal").GetComponentInChildren<PortalScript>().enabled = true;
            }
            for(int i = 0; i < pairsToDeactivate.Length; i++)
            {
                pairsToDeactivate[i].transform.Find("Room1/Portal").GetComponentInChildren<PortalScript>().DeactivatePortal();
                pairsToDeactivate[i].transform.Find("Room2/Portal").GetComponentInChildren<PortalScript>().DeactivatePortal();
            }
            
        }
    }

    // void OnTriggerExit(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         if(transform.parent.name == "Pair2" || transform.parent.name == "Pair3" || transform.parent.name == "Pair4"){
    //             StartRoom.GetComponentInChildren<PortalScript>().enabled = false;
    //         }
    //         if(transform.parent.name == "Pair1" || transform.parent.name == "Pair2" || transform.parent.name == "Pair3"){
    //             LastRoom.GetComponentInChildren<PortalScript>().enabled = false;
    //         }
                
    //         for(int i = 0; i < pairsToDeactivate.Length; i++)
    //         {
    //             pairsToDeactivate[i].transform.Find("Room1/Portal").GetComponentInChildren<PortalScript>().enabled = false;
    //             pairsToDeactivate[i].transform.Find("Room2/Portal").GetComponentInChildren<PortalScript>().enabled = false;
    //         }
    //     }
    // }
}
