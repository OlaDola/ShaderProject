using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalConnection : MonoBehaviour
{   
    [SerializeField]
    string connectionName;

    [SerializeField]
    Transform MainLevel;

    [SerializeField]
    Level2Mechanic level2Mechanic;

    [SerializeField]
    PortalScript portalA;
    
    [SerializeField]
    PortalScript portalB;

    [SerializeField]
    GameObject room;

    [SerializeField]
    GameObject pair;

    void Start()
    {
        connectionName = gameObject.name;
        MainLevel = GameObject.Find("Level2").transform;
        level2Mechanic = MainLevel.GetComponent<Level2Mechanic>();
        portalA = transform.parent.Find("Portal").GetComponentInChildren<PortalScript>();
        room = transform.parent.gameObject;
        MakeConection();
    }

    void MakeConection()
    {
        // ResetConnection();
        if (room.name == "StartRoom")
        {
            portalB = room.transform.parent.Find("UndergroundRooms/Pair1/Room1").GetComponentInChildren<PortalScript>();
            return;
        }
        if (room.name == "LastRoom")
        {
            portalB = room.transform.parent.Find("UndergroundRooms/Pair4/Room2").GetComponentInChildren<PortalScript>();
            return;
        }
        if (room.name == "Room1")
        {
            pair = room.transform.parent.gameObject;
            int index = int.Parse(pair.name.Replace("Pair", ""));
            if(index == 1){
                portalB = MainLevel.Find("StartRoom").GetComponentInChildren<PortalScript>();
                return;
            }
            portalB = MainLevel.Find("UndergroundRooms/Pair" + (index - 1) + "/Room2").GetComponentInChildren<PortalScript>();
            return;
        }
        if (room.name == "Room2")
        {
            pair = room.transform.parent.gameObject;
            int index = int.Parse(pair.name.Replace("Pair", ""));
            if(index == 4){
                portalB = MainLevel.Find("LastRoom").GetComponentInChildren<PortalScript>();
                return;
            }
            portalB = MainLevel.Find("UndergroundRooms/Pair" + (index + 1) + "/Room1").GetComponentInChildren<PortalScript>();
            return;
        }
    }

   

    void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player"))
        {   
            if (portalA != null && portalB != null)
            {
                portalA.SwitchPortal(portalB);
                portalB.SwitchPortal(portalA);
                portalA.enabled = true;
                portalB.enabled = true;
            }
            else
            {
                Debug.LogWarning("One or both portals are not assigned.");
            }
        }
    }
}
