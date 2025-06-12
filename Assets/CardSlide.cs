using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSlide : MonoBehaviour
{
    public float activationDistance = 2.5f;
    [SerializeField]
    protected Transform player;
    [SerializeField]
    protected GameObject card;
    [SerializeField]
    protected Material panelColorDefault;
    [SerializeField]
    protected Material panelColorCorrect;
    [SerializeField]
    protected Material panelColorWrong;

    protected Vector3 initialCardPosition;

    protected virtual void Start()
    {
        if (player == null)
        {
            player = Camera.main.transform; // Default to main camera if no player is assigned
        }
        card = transform.Find("SecurityCard").gameObject;
        if (card == null)
        {
            Debug.LogError("SecurityCard not found in child object.");
        }
        initialCardPosition = card.transform.localPosition;
        card.SetActive(false);

        if (this.TryGetComponent<MeshRenderer>(out var meshRenderer))
        {
            Material[] materials = meshRenderer.materials;
            materials[1] = panelColorDefault;
            meshRenderer.materials = materials;
        }
    }

    private void Update()
    {
        if (IsPlayerCloseEnough() && IsPlayerLookingAtPanel())
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ActivateCardSlideMechanic();
            }
        }
    }

    private bool IsPlayerLookingAtPanel()
    {
        Ray ray = new Ray(player.position, player.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, activationDistance))
        {
            Debug.DrawLine(player.position, hit.point, Color.red, 0.1f);
            if (hit.transform == transform)
            {
                return true; // Ray hit the panel
            }
        }

        return false; // Ray did not hit the panel
    }

    private bool IsPlayerCloseEnough()
    {
        float distanceToPanel = Vector3.Distance(player.position, transform.position);
        return distanceToPanel <= activationDistance;
    }

    protected virtual void ActivateCardSlideMechanic()
    {
        StartCoroutine(SlideCardAnimation());
    }

    protected IEnumerator SlideCardAnimation()
    {
        card.transform.localPosition = initialCardPosition; // Reset card position
        Vector3 targetPosition = initialCardPosition - new Vector3(0, 0.6f, 0);
        float duration = 1.0f; // Duration of the slide in seconds
        float elapsedTime = 0f;

        card.SetActive(true);

        while (elapsedTime < duration)
        {
            card.transform.localPosition = Vector3.Lerp(initialCardPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        card.transform.localPosition = targetPosition; // Ensure it reaches the exact target position

        // Wait for a short delay before resetting the card
        yield return new WaitForSeconds(0.5f);

        // Reset card position and visibility
        card.transform.localPosition = initialCardPosition;
        card.SetActive(false);

        // Reset the panel material to default
        if (this.TryGetComponent<MeshRenderer>(out var resetMeshRenderer))
        {
            Material[] materials = resetMeshRenderer.materials;
            materials[1] = panelColorDefault;
            resetMeshRenderer.materials = materials;
        }
    }
}