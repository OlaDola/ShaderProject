using System;
using System.Collections;
using UnityEngine;

public class CardSlidePortalSwap: CardSlide
{
    [SerializeField]
    private PortalScript portal1;
    [SerializeField]
    private PortalScript portal1OtherSide;

    [SerializeField]
    private PortalScript portal2;
    [SerializeField]
    private PortalScript portal2OtherSide;

    [SerializeField]
    private Material defaultMaterial;


    protected override void Start()
    {
        base.Start();
        portal1 = transform.parent.parent.GetComponentInChildren<PortalScript>();
        if (portal1 == null)
        {
            Debug.LogError("PortalScript component not found in parent object.");
        }
        portal1OtherSide = portal1.OtherPortal;
        
        string cardName = gameObject.name.Replace("Panel_CardSlider_", "");
        portal2 = transform.parent.parent.parent.Find("Portal" + cardName).GetComponentInChildren<PortalScript>();
        if (portal2 == null)
        {
            Debug.LogError("PortalScript component not found in parent object.");
        }
        portal2OtherSide = portal2.OtherPortal;

    }

    protected override void ActivateCardSlideMechanic()
    {
        if (portal1 != null && portal2 != null)
        {

            StartCoroutine(SlideCardSwapPortals());
        }
        else
        {
            Debug.LogError("Portals components are not assigned or not found.");
        }
    }

    private IEnumerator SlideCardSwapPortals()
    {
        yield return base.SlideCardAnimation(); // Perform base animation

        portal1OtherSide = portal1.OtherPortal;
        portal2OtherSide = portal2.OtherPortal;

        portal1.SwitchPortal(portal2);
        if(portal1OtherSide != null && portal1OtherSide != portal2){
            portal1OtherSide.DeactivatePortal(defaultMaterial);
            portal1OtherSide.transform.Find("PerfectSquarePortal/Screen").GetComponent<MeshCollider>().enabled = true;
            // portal1OtherSide.transform.Find("PerfectSquarePortal/Screen").GetComponent<MeshRenderer>().material = defaultMaterial;
        }
            
        portal2.SwitchPortal(portal1);
        if(portal2OtherSide != null && portal2OtherSide != portal1){
            portal2OtherSide.DeactivatePortal(defaultMaterial);
            portal2OtherSide.transform.Find("PerfectSquarePortal/Screen").GetComponent<MeshCollider>().enabled = true;
            }

        // Ensure the forward of portal1 is always opposite of the forward of portal2
        if (Vector3.Dot(portal1.transform.forward, portal2.transform.forward) > 0)
        {
            portal1.transform.parent.Rotate(0, 180, 0);
        }

        portal1.enabled = true;
        portal1.transform.Find("PerfectSquarePortal/Screen").GetComponent<MeshCollider>().enabled = false;

        portal2.enabled = true;
        portal2.transform.Find("PerfectSquarePortal/Screen").GetComponent<MeshCollider>().enabled = false;

    }
}
