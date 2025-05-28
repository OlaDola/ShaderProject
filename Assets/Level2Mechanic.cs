using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Level2Mechanic : MonoBehaviour
{   
    [SerializeField]
    GameObject player;
    [SerializeField]
    GameObject StartRoom;
    [SerializeField]
    GameObject LastRoom;
    [SerializeField]
    GameObject[] rooms;
    [SerializeField]
    GameObject[] deadEndRooms;

    [SerializeField]
    SerializedDictionary<string, SerializedDictionary<string,bool>> correctWay;

    [SerializeField]
    bool isCorrectWay = false;

    [SerializeField]
    CardSlideDoor CardSlideDoor;
    
    void Start()
    {   
        DisableLevel();
    }
    
    public void DisableLevel()
    {
        correctWay = new SerializedDictionary<string, SerializedDictionary<string, bool>>()
        {
            { "Depth1", new SerializedDictionary<string, bool> { { "forwards", false }, { "backwards", false } } },
            { "Depth2", new SerializedDictionary<string, bool> { { "forwards", false }, { "backwards", false } } },
            { "Depth3", new SerializedDictionary<string, bool> { { "forwards", false }, { "backwards", false } } },
            { "Depth4", new SerializedDictionary<string, bool> { { "forwards", false }, { "backwards", false } } },
            { "Depth5", new SerializedDictionary<string, bool> { { "forwards", false }, { "backwards", false } } }
        };
        StartRoom.GetComponentInChildren<PortalScript>().DeactivatePortal();
        LastRoom.GetComponentInChildren<PortalScript>().DeactivatePortal();
        for (int i = 0; i < rooms.Length; i++)
        {
            rooms[i].GetComponentsInChildren<PortalScript>()[0].DeactivatePortal();
            rooms[i].GetComponentsInChildren<PortalScript>()[1].DeactivatePortal();
        }
        for (int i = 0; i < deadEndRooms.Length; i++)
        {
            deadEndRooms[i].GetComponentsInChildren<PortalScript>()[0].DeactivatePortal();
            deadEndRooms[i].GetComponentsInChildren<PortalScript>()[1].DeactivatePortal();
        }
    }


    public void setWay(string depth, string direction, bool value)
    {
        if (correctWay.ContainsKey(depth))
        {
            correctWay[depth][direction] = value;
        }

        foreach (var pair in correctWay)
        {
            if (pair.Value["forwards"] && pair.Value["backwards"])
            {
                isCorrectWay = true;
            }
            else
            {
                isCorrectWay = false;
                break;
            }
        }
        CardSlideDoor.isLevelComplete = isCorrectWay;
    }
}
