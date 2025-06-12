using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScript : MonoBehaviour
{
    [SerializeField]
    private AudioClip MenuMusic;

    private AudioSource audioSource;

    [SerializeField]
    private GameObject MainMenuUI;

    void Awake()
    {
        InitializeAudioSource();
        if (MenuMusic != null)
        {
            audioSource.clip = MenuMusic;
            audioSource.loop = true;
            audioSource.volume = 0.2f; // Set volume to a reasonable level
            audioSource.Play();
        }
    }

    private void InitializeAudioSource()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = true;
    }

    public void StartGame()
    {
        Debug.Log("Starting game...");
        if(MainMenuUI != null)
        {
            MainMenuUI.SetActive(false);
        }
        StartCoroutine(FadeOutAndLoadScene());
    }

    private IEnumerator FadeOutAndLoadScene()
    {
        // Create a fullscreen black Image if not already present
        Canvas fadeCanvas = FindObjectOfType<Canvas>();
        if (fadeCanvas == null)
        {
            GameObject canvasObj = new GameObject("FadeCanvas");
            fadeCanvas = canvasObj.AddComponent<Canvas>();
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasGroup>();
        }

        // Create the fade Image
        UnityEngine.UI.Image fadeImage = fadeCanvas.GetComponentInChildren<UnityEngine.UI.Image>();
        if (fadeImage == null)
        {
            GameObject imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(fadeCanvas.transform, false);
            fadeImage = imageObj.AddComponent<UnityEngine.UI.Image>();
            fadeImage.rectTransform.anchorMin = Vector2.zero;
            fadeImage.rectTransform.anchorMax = Vector2.one;
            fadeImage.rectTransform.offsetMin = Vector2.zero;
            fadeImage.rectTransform.offsetMax = Vector2.zero;
            fadeImage.color = new Color(0, 0, 0, 0);
        }

        float fadeDuration = 2.0f;
        float elapsed = 0f;
        Color startColor = fadeImage.color;
        Color endColor = new Color(0, 0, 0, 1);

        float startVolume = audioSource.volume;

        // Fade out both music and screen
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = Color.Lerp(startColor, endColor, t);
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        fadeImage.color = endColor;
        audioSource.volume = 0f;

        // Optionally stop the music
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Load the next scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("LevelDesign");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

        // If running in the editor, stop playing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
