using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGrounded : MonoBehaviour
{   
    

    public bool isGrounded = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        isGrounded = Physics.CheckSphere(transform.position, 0.06f, LayerMask.GetMask("Default"));
    }
}
