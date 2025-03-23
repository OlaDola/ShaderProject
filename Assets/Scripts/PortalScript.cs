using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PortalScript : MonoBehaviour
{
    [SerializeField]
    PortalScript otherPortal;

    [SerializeField]
    Camera portalCamera;

    [SerializeField]
    bool withShader = true;

    [SerializeField]
    Camera playerCamera;

    public GameObject ScreenObject;

    public int recursionLimit = 5;

    MeshRenderer screen;

    [Header ("Advanced Settings")]
    public float nearClipOffset = 0.05f;
    public float nearClipLimit = 0.2f;

    [SerializeField]
    List<PortalTraveller> trackedTravellers;

    MeshFilter screenMeshFilter;
    [SerializeField]
    Vector3 offsetFromPortalToCam;


    void Awake()
    {
        // portalCamera.enabled = false;
        playerCamera = Camera.main;
        portalCamera = GetComponentInChildren<Camera>();
        portalCamera.enabled = false;
        screen = ScreenObject.GetComponent<MeshRenderer>();
        trackedTravellers = new List<PortalTraveller> ();
        screenMeshFilter = screen.GetComponent<MeshFilter> ();
    }

    // private void Update()
    // {
    //     var m = transform.localToWorldMatrix * otherPortal.transform.worldToLocalMatrix * playerCamera.transform.localToWorldMatrix;
    //     portalCamera.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
    // }

    void LateUpdate(){
        for (int i = 0; i < trackedTravellers.Count; i++){
            PortalTraveller traveller = trackedTravellers[i];
            Transform travellerT = traveller.transform;
            var m = otherPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travellerT.localToWorldMatrix;

            Vector3 offsetFromPortal = travellerT.position - transform.position;
            int portalSide = System.Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));
            int portalSideOld = System.Math.Sign(Vector3.Dot(traveller.previousOffsetFromPortal, transform.forward));

            if (portalSide != portalSideOld){
                var positionOld = travellerT.position;
                var rotOld = travellerT.rotation;

                traveller.Teleport(transform, otherPortal.transform, m.GetColumn(3), m.rotation);
                
                traveller.graphicsClone.transform.SetPositionAndRotation (positionOld, rotOld);
                otherPortal.OnTravellerEnterPortal(traveller);
                trackedTravellers.RemoveAt(i);
                i--;
            }
            else{
                traveller.graphicsClone.transform.SetPositionAndRotation (m.GetColumn (3), m.rotation);
                UpdateSliceParams (traveller);
                traveller.previousOffsetFromPortal = offsetFromPortal;
            }
        }
    }

    static bool isVisibileFromPlayerCamera(Renderer renderer, Camera camera){
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }

    // Called before any portal cameras are rendered for the current frame
    public void PrePortalRender () {
        foreach (var traveller in trackedTravellers) {
            UpdateSliceParams (traveller);
        }
        // print("PrePortalRender");
    }
     // Called once all portals have been rendered, but before the player camera renders
    public void PostPortalRender () {
        foreach (var traveller in trackedTravellers) {
            UpdateSliceParams (traveller);
        }
        // print("PostPortalRender");
        ProtectScreenFromClipping (playerCamera.transform.position);
    }

    public void Render (ScriptableRenderContext context) {

        // Skip rendering the view from this portal if player is not looking at the linked portal
        if (!CameraUtility.VisibleFromCamera (otherPortal.screen, playerCamera)) {
            return;
        }

        CreateTexture();

        var localToWorldMatrix = playerCamera.transform.localToWorldMatrix;
        var renderPositions = new Vector3[recursionLimit];
        var renderRotations = new Quaternion[recursionLimit];

        int startIndex = 0;
        portalCamera.projectionMatrix = playerCamera.projectionMatrix;
        for (int i = 0; i < recursionLimit; i++) {
            if (i > 0) {
                // No need for recursive rendering if linked portal is not visible through this portal
                if (!CameraUtility.BoundsOverlap (screenMeshFilter, otherPortal.screenMeshFilter, portalCamera)) {
                    break;
                }
            }
            localToWorldMatrix = transform.localToWorldMatrix * otherPortal.transform.worldToLocalMatrix * localToWorldMatrix;
            int renderOrderIndex = recursionLimit - i - 1;
            renderPositions[renderOrderIndex] = localToWorldMatrix.GetColumn (3);
            renderRotations[renderOrderIndex] = localToWorldMatrix.rotation;

            portalCamera.transform.SetPositionAndRotation (renderPositions[renderOrderIndex], renderRotations[renderOrderIndex]);
            startIndex = renderOrderIndex;
        }

        // Hide screen so that camera can see through portal
        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        // print("Hide screen");
        otherPortal.screen.material.SetInt("_DisplayMask", 0); // Hide

        for (int i = startIndex; i < recursionLimit; i++) {
            portalCamera.transform.SetPositionAndRotation (renderPositions[i], renderRotations[i]);
            SetNearClipPlane();
            HandleCliping();
            UniversalRenderPipeline.RenderSingleCamera(context, portalCamera);

            if (i == startIndex) {
                // print("Show screen");
                otherPortal.screen.material.SetInt("_DisplayMask", 1); // Show
            }
        }

        // Unhide objects hidden at start of render
        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }

    void HandleCliping(){
        // This function handles the clipping of the view that sees through the portal
        // It calculates the screen thickness and sets the slice offset of the portal travellers
        // The screen thickness is the distance between the near clip plane and the screen of the portal
        // The slice offset is the distance between the near clip plane and the object that is being viewed through the portal
        // The slice offset is negative if the object is in front of the near clip plane and positive if it is behind the near clip plane

        const float hideDst = -1000;
        const float showDst = 1000;
        float screenThickness = otherPortal.ProtectScreenFromClipping(portalCamera.transform.position);

        foreach(var traveller in trackedTravellers){
            if(SameSideOfPortal(traveller.transform.position, portalCamPos)){
                // Object is in front of the portal
                traveller.SetSliceOffsetDst(hideDst, false);
            } else {
                // Object is behind the portal
                traveller.SetSliceOffsetDst(showDst, false);
            }

            // Calculate slice offset
            int cloneSideOfLinkedPortal = -SideOfPortal(traveller.transform.position);
            bool camSameSideAsTraveller = otherPortal.SideOfPortal(portalCamPos) == cloneSideOfLinkedPortal;
            if(camSameSideAsTraveller){
                traveller.SetSliceOffsetDst(screenThickness, true);
            } else {
                traveller.SetSliceOffsetDst(-screenThickness, true);
            }
        }

        offsetFromPortalToCam = portalCamPos - transform.position;
        // print("offsetFromPortalToCam: "+ offsetFromPortalToCam);
        foreach(var linkedTraveller in otherPortal.trackedTravellers){
            var travellerPos = linkedTraveller.graphicsObject.transform.position;
            var clonePos = linkedTraveller.graphicsClone.transform.position;
            // Calculate slice offset

            bool cloneOnSameSideAsCam = otherPortal.SideOfPortal(travellerPos) != SideOfPortal(portalCamPos);
            if(cloneOnSameSideAsCam){
                // Object is on same side as camera
                linkedTraveller.SetSliceOffsetDst(hideDst, true);
            }
            else{
                // Object is on opposite side of camera
                linkedTraveller.SetSliceOffsetDst(showDst, true);
            }

            bool camSameSideAsTraveller = otherPortal.SameSideOfPortal(linkedTraveller.transform.position, portalCamPos);
            if(camSameSideAsTraveller){
                linkedTraveller.SetSliceOffsetDst(screenThickness, false);
            } else {
                linkedTraveller.SetSliceOffsetDst(-screenThickness, false);
            }
        }
    }

    float ProtectScreenFromClipping(Vector3 viewPoint){
        float halfHeight = playerCamera.nearClipPlane * Mathf.Tan(playerCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * playerCamera.aspect;
        float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, playerCamera.nearClipPlane).magnitude;
        float screenThickness = dstToNearClipPlaneCorner;

        Transform screenT = screen.transform;
        bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;
        screenT.localScale = new Vector3(screenT.localScale.x, screenT.localScale.y, screenThickness);
        screenT.localPosition = Vector3.forward * screenThickness * ((camFacingSameDirAsPortal) ? 0.5f : -0.5f);
        return screenThickness;
    }

    void UpdateSliceParams(PortalTraveller portalTraveller){
        //Calculate slice normal
        int side = SideOfPortal(portalTraveller.transform.position);
        Vector3 sliceNormal = transform.forward * -side;
        Vector3 cloneSliceNormal = otherPortal.transform.forward * side;

        // Calculate slice centre
        Vector3 slicePos = transform.position;
        Vector3 cloneSlicePos = otherPortal.transform.position;

        // Adjust slice offset so that when player standing on the other side of the portal, the slice doesn't clip through the portal
        float sliceOffsetDst = 0;
        float cloneSliceOffsetDst = 0;
        float screenThickness = screen.transform.localScale.z;

        bool playerSameSideAsTraveller = SameSideOfPortal(playerCamera.transform.position, portalTraveller.transform.position);
        if (!playerSameSideAsTraveller){
            sliceOffsetDst = -screenThickness;
        }
        bool playerSameSideAsClone = side != otherPortal.SideOfPortal(playerCamera.transform.position);
        if (!playerSameSideAsClone){
            cloneSliceOffsetDst = -screenThickness;
        }

        // Apply parameters
        for (int i = 0; i < portalTraveller.originalMaterials.Length; i++){
            portalTraveller.originalMaterials[i].SetVector("sliceCentre", slicePos);
            portalTraveller.originalMaterials[i].SetVector("sliceNormal", sliceNormal);
            portalTraveller.originalMaterials[i].SetFloat("sliceOffsetDst", sliceOffsetDst);

            portalTraveller.cloneMaterials[i].SetVector("sliceCentre", cloneSlicePos);
            portalTraveller.cloneMaterials[i].SetVector("sliceNormal", cloneSliceNormal);
            portalTraveller.cloneMaterials[i].SetFloat("sliceOffsetDst", cloneSliceOffsetDst);
        }

    }

    // Use custom projection matrix to align portal camera's near clip plane with the surface of the portal
    // Note that this affects precision of the depth buffer, which can cause issues with effects like screenspace AO    
    void SetNearClipPlane(){
        // Learning resource:
        // http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
        Transform clipPlane = transform;
        int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - portalCamera.transform.position));

        Vector3 camSpacePos = portalCamera.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        Vector3 camSpaceNormal = portalCamera.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
        float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset;

        // Don't use oblique clip plane if very close to portal as it can cause issues with precision
        if (Mathf.Abs(camSpaceDst) > nearClipLimit) {
            // print("Setting near clip plane:"+ this.name);
            Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);
            portalCamera.projectionMatrix = playerCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
        } else {
            // print("Not setting near clip plane:" + this.name);
            portalCamera.projectionMatrix = playerCamera.projectionMatrix;
        }
    }

    void CreateTexture()
    {
        // print("Creating texture");

        // Check for null references
        if (portalCamera == null || otherPortal == null || otherPortal.screen == null)
        {
            Debug.LogError("PortalCamera, OtherPortal, or Screen is not assigned!");
            return;
        }

        // Release existing render texture
        if (portalCamera.targetTexture != null)
        {
            portalCamera.targetTexture.Release();
        }

        // Create new render texture
        RenderTexture rt = new(Screen.width, Screen.height, 24) // Use 24-bit depth buffer
        {
            name = gameObject.name + " RenderTexture"
        };
        portalCamera.targetTexture = rt;

        // Create material
        Material mat;
        if (withShader)
        {
            Shader portalShader = Shader.Find("Unlit/PortalShaderCut");
            if (portalShader == null)
            {
                Debug.LogError("Shader 'Unlit/PortalShaderCut' not found!");
                return;
            }
            mat = new Material(portalShader);
        }
        else
        {
            mat = new Material(Shader.Find("Standard"));
        }

        // Assign render texture to material
        mat.mainTexture=rt;

        // Set additional material properties (if needed)
        // mat.SetColor("_InactiveColour", Color.white); // Example
        // mat.SetInt("_DisplayMask", 1); // Example

        // Assign material to portal screen
        mat.name = gameObject.name + " Material";
        otherPortal.screen.material = mat;
    }


    public PortalScript GetOtherPortal(){
        return otherPortal;
    }
    
    

    void OnTravellerEnterPortal (PortalTraveller traveller) {
        if (!trackedTravellers.Contains (traveller)) {
            traveller.EnterPortalThreshold ();
            traveller.previousOffsetFromPortal = traveller.transform.position - transform.position;
            trackedTravellers.Add (traveller);
        }
    }

    void OnTriggerEnter (Collider other) {
        var traveller = other.GetComponent<PortalTraveller> ();
        if (traveller) {
            OnTravellerEnterPortal (traveller);
        }
    }

    void OnTriggerExit (Collider other) {
        var traveller = other.GetComponent<PortalTraveller> ();
        if (traveller && trackedTravellers.Contains (traveller)) {
            traveller.ExitPortalThreshold ();
            trackedTravellers.Remove (traveller);
        }
    }

    Vector3 portalCamPos {
        get {
            return portalCamera.transform.position;
        }
    }
    void OnValidate () {
        if (otherPortal != null) {
            otherPortal.otherPortal = this;
        }
    }

    bool SameSideOfPortal (Vector3 posA, Vector3 posB) {
        return SideOfPortal (posA) == SideOfPortal (posB);
    }

    int SideOfPortal (Vector3 pos) {
        return System.Math.Sign (Vector3.Dot (pos - transform.position, transform.forward));
    }
}
