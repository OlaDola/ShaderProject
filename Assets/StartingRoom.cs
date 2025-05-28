using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingRoom : MonoBehaviour
{
    [SerializeField]
    GameObject player;

    [SerializeField]
    Transform startingPoint;


    void Start()
    {
        
        if (player != null && startingPoint != null)
        {
            player.transform.position = startingPoint.position;
        }
    }
}
