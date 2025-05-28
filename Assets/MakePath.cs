using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakePath : MonoBehaviour
{   
    [SerializeField]
    string connectionName;

    [SerializeField]
    Transform MainLevel;

    [SerializeField]
    Level2Mechanic level2Mechanic;

    [SerializeField]
    GameObject room;

    [SerializeField]
    GameObject pair;

    void Start()
    {
        connectionName = gameObject.name;
        MainLevel = GameObject.Find("Level2").transform;
        level2Mechanic = MainLevel.GetComponent<Level2Mechanic>();
        room = transform.parent.parent.gameObject;
    }

    void GoodConection()
    {
        if (level2Mechanic != null)
        {
            if(room.name == "StartRoom")
                level2Mechanic.setWay("Depth1", "forwards", true);
            if(room.name == "Room1")
            {
                pair = room.transform.parent.gameObject;
                int index = int.Parse(pair.name.Replace("Pair", ""));
                if(index<= 4)
                    level2Mechanic.setWay("Depth" + index, "backwards", true);
            }
            if(room.name == "Room2")
            {
                pair = room.transform.parent.gameObject;
                int index = int.Parse(pair.name.Replace("Pair", ""));
                if(index<= 4)
                    level2Mechanic.setWay("Depth" + (index + 1), "forwards", true);
            }
            if(room.name == "LastRoom")
                level2Mechanic.setWay("Depth5", "backwards", true);
        }
        else
        {
            Debug.LogWarning("Level2Mechanic is not assigned in the Inspector.");
        }
    }
    void WrongConection()
    {
        if(level2Mechanic != null)
        {
            if(room.name == "StartRoom")
                level2Mechanic.setWay("Depth1", "forwards", false);
            if(room.name == "Room1")
            {
                pair = room.transform.parent.gameObject;
                int index = int.Parse(pair.name.Replace("Pair", ""));
                if(index<= 4)
                    level2Mechanic.setWay("Depth" + index, "backwards", false);
            }
            if(room.name == "Room2")
            {
                pair = room.transform.parent.gameObject;
                int index = int.Parse(pair.name.Replace("Pair", ""));
                if(index<= 4)
                    level2Mechanic.setWay("Depth" + (index + 1), "forwards", false);
            }
            if(room.name == "LastRoom")
                level2Mechanic.setWay("Depth5", "backwards", false);
        }
        else
        {
            Debug.LogWarning("Level2Mechanic is not assigned in the Inspector.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player"))
        {   
            
            if(connectionName == "CorrectWay")
                GoodConection();
            else if(connectionName == "WrongWay")
                WrongConection();

        }
        else
        {
            Debug.LogWarning("One or both portals are not assigned.");
        }
    }
}

