using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [SerializeField]
    private AudioClip hoverSound;

    [SerializeField]
    private AudioClip clickSound;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0.5f; // Set a default volume
        audioSource.playOnAwake = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}