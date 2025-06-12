using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPosition : MonoBehaviour
{
    // Start is called before the first frame update
    private Vector3 initialPosition;
    private bool isPositionSet = false;

    public void SetPosition(Vector3 position)
    {
        initialPosition = position;
        isPositionSet = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // if (initialPosition == Vector3.zero)

        if (Input.GetKeyDown(KeyCode.R) && isPositionSet)
        {
            transform.GetComponent<FPSController>().isReseting = true;
            transform.parent.GetComponent<Rigidbody>().MovePosition(initialPosition);
            // transform.parent.position = initialPosition;
            transform.parent.localScale = Vector3.one; // Reset scale to default
            transform.parent.rotation = Quaternion.identity; // Reset rotation to default
            transform.GetComponent<FPSController>().isReseting = false;
        }
    }
}
