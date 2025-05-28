using System.Collections;
using UnityEngine;

public class CardSlideDoor : CardSlide
{
    [SerializeField]
    private OpenCloseDoor doorMechanic;

    [SerializeField]
    private bool needsActivation = false;

    [SerializeField]
    public bool isLevelComplete = false;

    private PortalScript levelEndPortalScript;
    private PortalScript levelSelectorPortalScript;

    protected override void Start()
    {
        base.Start();

        doorMechanic = transform.parent.GetComponentInChildren<OpenCloseDoor>();
        if (doorMechanic == null)
        {
            Debug.LogError("OpenCloseDoor component not found in parent object.");
        }

        levelEndPortalScript = transform.parent.Find("PortalLevelEnd").GetComponentInChildren<PortalScript>();
        if (levelEndPortalScript == null)
        {
            Debug.LogError("PortalLevelEnd component not found in child object.");
        }

        levelSelectorPortalScript = GameObject.Find("LevelSelector/PortalLevelSelector").GetComponentInChildren<PortalScript>();
        if (levelSelectorPortalScript == null)
        {
            Debug.LogError("PortalLevelSelector component not found in LevelSelector.");
        }
    }

    protected override void ActivateCardSlideMechanic()
    {
        if (doorMechanic != null)
        {
            if (!doorMechanic.isDoorOpen) // Check if the door is already open
            {
                StartCoroutine(SlideCardAndHandleDoor(needsActivation));
            }
            else
            {
                Debug.Log("Door is already open.");
            }
        }
        else
        {
            Debug.LogError("OpenCloseDoor component is not assigned or not found.");
        }
    }

    private IEnumerator SlideCardAndHandleDoor(bool needsActivation)
    {
        yield return base.SlideCardAnimation(); // Perform base animation

        if (this.TryGetComponent<MeshRenderer>(out var meshRenderer))
        {
            Material[] materials = meshRenderer.materials;
            if (needsActivation)
            {
                if (isLevelComplete)
                {
                    materials[1] = panelColorCorrect;
                    Debug.Log("Card is correct, door will open.");
                }
                else
                {
                    materials[1] = panelColorWrong;
                    Debug.Log("Card is not correct, please try again.");
                }
            }
            else
            {
                materials[1] = panelColorCorrect;
                Debug.Log("Card is correct, door will open.");
                isLevelComplete = true; // Set the level as complete
            }
            meshRenderer.materials = materials;
        }

        if (isLevelComplete)
        {
            PortalScript otherConnectedPortal = levelSelectorPortalScript.OtherPortal;
            if (otherConnectedPortal != null)
            {
                otherConnectedPortal.DeactivatePortal(); // Disable the other portal
                otherConnectedPortal.OtherPortal = null;
            }
            levelEndPortalScript.DeactivatePortal(); // Disable the level end portal
            levelSelectorPortalScript.DeactivatePortal(); // Disable the level selector portal

            levelEndPortalScript.OtherPortal = levelSelectorPortalScript;
            levelSelectorPortalScript.OtherPortal = levelEndPortalScript;

            levelEndPortalScript.enabled = true;
            levelSelectorPortalScript.enabled = true;

            doorMechanic.OpenDoor();
        }
        yield return new WaitForSeconds(0.5f); // Wait to reset color
        if (this.TryGetComponent<MeshRenderer>(out var resetMeshRenderer))
        {
            Material[] resetMaterials = resetMeshRenderer.materials;
            resetMaterials[1] = panelColorDefault;
            resetMeshRenderer.materials = resetMaterials;
        }
    }
}
