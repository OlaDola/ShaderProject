using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingRoom : MonoBehaviour
{
    [SerializeField]
    GameObject player;

    [SerializeField]
    Transform startingPoint;


    [SerializeField]
    private AudioClip MenuMusic;

    private AudioSource audioSource;

    void Start()
    {
        InitializeAudioSource();
        if (MenuMusic != null)
        {
            audioSource.clip = MenuMusic;
            audioSource.loop = true;
            audioSource.volume = 0.05f; // Set volume to a reasonable level
            audioSource.Play();
        }
    }
    private void InitializeAudioSource()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = true;
    }
}
