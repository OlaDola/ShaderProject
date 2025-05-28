using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSlideCheckList : CardSlide
{
    [SerializeField]
    private GameObject[] portals;
    
    [SerializeField]
    bool isKeyCardTriggered = false;

    [SerializeField]
    GameObject LevelEnd;

    [SerializeField]
    CardSlideDoor CardSlideDoor;

    protected override void Start()
    {
        base.Start();
        isKeyCardTriggered = false;
        if (LevelEnd == null)
        {
            Debug.LogError("LevelEnd not found in child object.");
        }
        CardSlideDoor = LevelEnd.GetComponentInChildren<CardSlideDoor>();
        if (CardSlideDoor == null)
        {
            Debug.LogError("CardSlideDoor component not found in child object.");
        }
    }

    protected override void ActivateCardSlideMechanic()
    {
        if (!isKeyCardTriggered)
        {
            StartCoroutine(SlideCardCheckList());
        }
        else
        {
            Debug.Log("Key card already triggered.");
        }
    }

    private IEnumerator SlideCardCheckList()
    {
        yield return base.SlideCardAnimation(); // Perform base animation

        if (this.TryGetComponent<MeshRenderer>(out var meshRenderer))
        {
            Material[] materials = meshRenderer.materials;
            materials[1] = panelColorCorrect;
            Debug.Log("Card is correct, door will open.");  
            meshRenderer.materials = materials;
        }

        // Trigger the checklist mechanic here
        isKeyCardTriggered = true;
        CheckKeyCard();
        Debug.Log("Key card triggered, checklist activated.");
    }

    public void CheckKeyCard()
    {
        bool isKeyCardTriggered = false;
        for (int i = 0; i < portals.Length; i++)
        {
            CardSlideCheckList cardSlideCheckList = portals[i].GetComponentInChildren<CardSlideCheckList>();
            if (cardSlideCheckList != null)
            {
                if(cardSlideCheckList.IsKeyCardTriggered())
                {
                    isKeyCardTriggered = true;
                }
                else
                {
                    isKeyCardTriggered = false;
                    break;
                }
            }
            else
            {
                Debug.LogError("CardSlideCheckList component not found in child object of portal " + (i + 1));
            }
        }
        CardSlideDoor.isLevelComplete = isKeyCardTriggered;
    }
    
    public bool IsKeyCardTriggered()
    {
        return isKeyCardTriggered;
    }
}
