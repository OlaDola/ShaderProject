using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MainCameraRender : MonoBehaviour
{
   [SerializeField]
    PortalScript[] portals;

    void Awake () {
        portals = FindObjectsOfType<PortalScript>();
    }

    

    // Unity calls the methods in this delegate's invocation list before rendering any camera
    void OnPreCull()
    {
        for (int i = 0; i < portals.Length; i++){
            portals[i].Render();
        }
    }

}
