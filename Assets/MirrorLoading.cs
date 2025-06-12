using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorLoading : MonoBehaviour
{   
    [SerializeField]
    Camera mirrorCamera;

    [SerializeField]
    MeshRenderer mirrorRenderer;

    [SerializeField]
    RenderTexture mirrorTexture;

    public float increaseDuration = 2f;
    private Coroutine increaseCoroutine;
    private bool isIncreasing = false;

    private int startRes = 2;
    private int endRes = 1024;
    private float elapsed = 0f; // Persist elapsed time

    private GameObject hatObject;
    [SerializeField] GameObject graduationObject;

    void Start()
    {
        if (mirrorTexture != null)
        {
            mirrorCamera.targetTexture = mirrorTexture;
            mirrorRenderer.material.mainTexture = mirrorTexture;
        }
        else
        {
            // Initialize with startRes if not set
            mirrorTexture = RenderTexture.GetTemporary(startRes, startRes, 16);
            mirrorCamera.targetTexture = mirrorTexture;
            mirrorRenderer.material.mainTexture = mirrorTexture;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && mirrorTexture != null)
        {
            int currentRes = mirrorTexture.width;
            if (currentRes >= endRes)
                return; // Already at max, do nothing

            if (!isIncreasing)
            {
                // Start coroutine to handle look-at and resolution increase
                increaseCoroutine = StartCoroutine(PlayerLookAtAndIncrease(other.gameObject));
                hatObject = other.transform.Find("Hat")?.gameObject;
                if (hatObject != null)
                {
                    hatObject.SetActive(true);
                }
            }
        }
    }

    private IEnumerator PlayerLookAtAndIncrease(GameObject player)
    {
        // Try to get the player's movement and camera scripts
        var characterController = player.GetComponent<FPSController>();

        if (characterController != null)
            characterController.enabled = false;

        // Smoothly rotate player to look at the portal (mirror)
        Vector3 lookTarget = mirrorRenderer.transform.position;
        float duration = 0.5f;
        float timer = 0f;
        Quaternion startRot = player.transform.rotation;
        Vector3 direction = (lookTarget - player.transform.position).normalized;
        direction.y = 0; // Only rotate on Y axis
        Quaternion targetRot = Quaternion.LookRotation(direction);

        while (timer < duration)
        {
            player.transform.rotation = Quaternion.Slerp(startRot, targetRot, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        player.transform.rotation = targetRot;

        timer = 0f;
        Transform cameraTransform = Camera.main.transform;
        Quaternion cameraStartRot = cameraTransform.rotation;
        // Optionally, also rotate the camera if needed
        if (cameraTransform != null)
        {
            while (timer < duration)
            {
                Vector3 camDir = (lookTarget - cameraTransform.position).normalized;
                Quaternion camTargetRot = Quaternion.LookRotation(camDir);
                cameraTransform.rotation = Quaternion.Slerp(cameraStartRot, camTargetRot, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }
        }

        // Start increasing resolution
        yield return StartCoroutine(IncreaseResolutionCoroutine());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && mirrorTexture != null)
        {
            if (increaseCoroutine != null)
            {
                StopCoroutine(increaseCoroutine);
                increaseCoroutine = null;
                isIncreasing = false;
                // Do not reset elapsed, so it resumes where it left off
            }
        }
    }

    private IEnumerator IncreaseResolutionCoroutine()
    {
        isIncreasing = true;

        while (elapsed < increaseDuration)
        {
            float t = Mathf.Clamp01(elapsed / increaseDuration);
            int currentRes = Mathf.RoundToInt(Mathf.Lerp(startRes, endRes, t));

            if (mirrorTexture == null || mirrorTexture.width != currentRes)
            {
                if (mirrorTexture != null)
                {
                    RenderTexture.ReleaseTemporary(mirrorTexture);
                }
                mirrorTexture = RenderTexture.GetTemporary(currentRes, currentRes, 16);

                mirrorCamera.targetTexture = mirrorTexture;
                mirrorRenderer.material.mainTexture = mirrorTexture;
            }

            yield return null;
            elapsed += Time.deltaTime;
        }

        // Ensure it's set to max at the end
        if (mirrorTexture == null || mirrorTexture.width != endRes)
        {
            if (mirrorTexture != null)
            {
                RenderTexture.ReleaseTemporary(mirrorTexture);
            }
            mirrorTexture = RenderTexture.GetTemporary(endRes, endRes, 16);

            mirrorCamera.targetTexture = mirrorTexture;
            mirrorRenderer.material.mainTexture = mirrorTexture;
        }

        isIncreasing = false;
        elapsed = increaseDuration; // Clamp to max

        if(graduationObject != null)
        {
            graduationObject.SetActive(true);
        }
    }

    // private void OnDestroy()
    // {
    //     if (mirrorTexture != null)
    //     {
    //         RenderTexture.ReleaseTemporary(mirrorTexture);
    //         mirrorTexture = null;
    //     }
    // }
}
