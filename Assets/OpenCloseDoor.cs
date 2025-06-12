using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCloseDoor : MonoBehaviour
{
    [SerializeField]
    private GameObject door1;
    [SerializeField]
    private GameObject door2;

    [SerializeField]
    string levelName;

    [SerializeField]
    private AudioClip doorSound;

    [SerializeField]
    public bool isDoorOpen = false;

    Vector3 door1StartPos;
    Vector3 door2StartPos;

    Vector3 door1EndPos;
    Vector3 door2EndPos;

    private AudioSource audioSource;

    void Start()
    {   

        if (transform != null)
        {
            door1 = transform.Find("Door_Big_1").gameObject;
            door2 = transform.Find("Door_Big_2").gameObject;
            door1StartPos = door1.transform.position;
            door2StartPos = door2.transform.position;
            door1EndPos = door1StartPos + transform.right * -1.5f; // Adjust the offset as needed
            door2EndPos = door2StartPos + transform.right * 1.5f;
            levelName = transform.parent.name.Replace("Hallway", "");
        }
        else
        {
            Debug.LogError("Door_Hallway not found in parent object.");
        }

        if (doorSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = doorSound;
            audioSource.loop = true;
            audioSource.volume = 0.2f; // Set volume to a reasonable level
            audioSource.playOnAwake = false;
        }
        if(isDoorOpen)
        {
            door1.transform.position = door1EndPos;
            door2.transform.position = door2EndPos;
        }
        else
        {
            door1.transform.position = door1StartPos;
            door2.transform.position = door2StartPos;
        }
        if (door1 == null || door2 == null)
        {
            Debug.LogError("Door objects are not assigned or not found in the hierarchy.");
            return;
        }
    }

    private Coroutine doorCoroutine;

    public void OpenDoor()
    {
        if(isDoorOpen)
        {
            return; // Doors are already open
        }

        if (doorCoroutine != null)
        {
            StopCoroutine(doorCoroutine);
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        doorCoroutine = StartCoroutine(OpenDoors());
        isDoorOpen = true;
    }

    private IEnumerator OpenDoors()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }

        float duration = 1.5f; // Time in seconds to fully open the doors
        float elapsedTime = 0f;

        Vector3 currentDoor1Pos = door1.transform.position;
        Vector3 currentDoor2Pos = door2.transform.position;

        float remainingDistance1 = Vector3.Distance(currentDoor1Pos, door1EndPos);
        float remainingDistance2 = Vector3.Distance(currentDoor2Pos, door2EndPos);
        float remainingDuration = duration * Mathf.Max(remainingDistance1, remainingDistance2) / Vector3.Distance(door1StartPos, door1EndPos);

        while (elapsedTime < remainingDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / remainingDuration);

            door1.transform.position = Vector3.Lerp(currentDoor1Pos, door1EndPos, t);
            door2.transform.position = Vector3.Lerp(currentDoor2Pos, door2EndPos, t);


            // if (doorSound != null)
            // {
            //     AudioSource.PlayClipAtPoint(doorSound, transform.position);
            // }

            yield return null;
        }

        door1.transform.position = door1EndPos;
        door2.transform.position = door2EndPos;

        if (audioSource != null)
        {
            audioSource.Stop();
        }
        doorCoroutine = null;
        
    }

    public void CloseDoor()
    {
        if(!isDoorOpen)
        {
            return; // Doors are already closed
        }

        if (doorCoroutine != null)
        {
            StopCoroutine(doorCoroutine);
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        doorCoroutine = StartCoroutine(CloseDoors());
        isDoorOpen = false;
    }

    private IEnumerator CloseDoors()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }

        float duration = 1.5f; // Time in seconds to fully close the doors
        float elapsedTime = 0f;

        Vector3 currentDoor1Pos = door1.transform.position;
        Vector3 currentDoor2Pos = door2.transform.position;

        float remainingDistance1 = Vector3.Distance(currentDoor1Pos, door1StartPos);
        float remainingDistance2 = Vector3.Distance(currentDoor2Pos, door2StartPos);
        float remainingDuration = duration * Mathf.Max(remainingDistance1, remainingDistance2) / Vector3.Distance(door1EndPos, door1StartPos);

        while (elapsedTime < remainingDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / remainingDuration);

            door1.transform.position = Vector3.Lerp(currentDoor1Pos, door1StartPos, t);
            door2.transform.position = Vector3.Lerp(currentDoor2Pos, door2StartPos, t);

            yield return null;
        }

        door1.transform.position = door1StartPos;
        door2.transform.position = door2StartPos;

        if (audioSource != null)
        {
            audioSource.Stop();
        }
        doorCoroutine = null;
    }

}
