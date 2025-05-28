using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPortal : MonoBehaviour
{
    [SerializeField]
    GameObject portal;

    void Start()
    {
        portal = transform.parent.Find("Portal").gameObject;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            portal.SetActive(true);
            // gameObject.SetActive(false);
        }
    }

}
