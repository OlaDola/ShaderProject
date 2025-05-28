using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalRotation : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public float activationDistance = 2.5f;
    [SerializeField] float distanceToPortal = 0f;
    [SerializeField] GameObject UI;

    [SerializeField] Transform player;

    private bool isRotating = false;
    private Transform currentTargetPortal;

    void Start()
    {
        if (player == null)
        {
            player = Camera.main.transform;
        }
    }

    void Update()
    {
        Transform portal = GetLookedAtPortal();
        if (portal != null)
        {
            currentTargetPortal = portal;
            if (UI != null && !UI.activeSelf)
            {
                UI.SetActive(true);
            }
            if (!isRotating)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    StartCoroutine(RotatePortal(currentTargetPortal, 180f)); // Rotate clockwise
                }
                else if (Input.GetKeyDown(KeyCode.Q))
                {
                    StartCoroutine(RotatePortal(currentTargetPortal, -180f)); // Rotate counterclockwise
                }
            }
        }
        else
        {
            currentTargetPortal = null;
            if (UI != null && UI.activeSelf)
            {
                UI.SetActive(false);
            }
        }
    }

    private Transform GetLookedAtPortal()
    {
        Ray ray = new Ray(player.position, player.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, activationDistance))
        {
            // You can use tag or component check depending on your portal setup
            // Example: if (hit.transform.CompareTag("Portal"))
            if (hit.transform.GetComponent<PortalScript>() != null)
            {
                distanceToPortal = Vector3.Distance(player.position, hit.transform.position);
                return hit.transform;
            }
        }
        return null;
    }

    private IEnumerator RotatePortal(Transform portal, float angle)
    {
        isRotating = true;
        Quaternion startRotation = portal.parent.rotation;
        Quaternion endRotation = startRotation * Quaternion.AngleAxis(angle, portal.forward);
        float elapsedTime = 0f;
        float duration = 0.5f;

        while (elapsedTime < duration)
        {
            portal.parent.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        portal.parent.rotation = endRotation;
        isRotating = false;
    }
}
