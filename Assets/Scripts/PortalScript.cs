using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PortalScript : MonoBehaviour
{

    [SerializeField]
    PortalScript otherPortal;
    public PortalScript OtherPortal
    {
        get => otherPortal;
        set => otherPortal = value;
    }

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

    [SerializeField]
    LayerMask layerScreen;

    [SerializeField]
    bool materialSetup = false;

    [SerializeField]
    bool isVisible = false;

    [SerializeField]
    bool showDebugLines = false;

    [SerializeField]
    Vector3 scaleRatio;

    void Awake()
    {
        // portalCamera.enabled = false;
        playerCamera = Camera.main;
        portalCamera = GetComponentInChildren<Camera>();
        portalCamera.enabled = false;
        screen = ScreenObject.GetComponent<MeshRenderer>();
        trackedTravellers = new List<PortalTraveller> ();
        screenMeshFilter = screen.GetComponent<MeshFilter> ();

        if (otherPortal == null && this.isActiveAndEnabled) {
            Debug.LogError("Other portal is not assigned!");
        }
        if (playerCamera == null) {
            Debug.LogError("Player camera is not assigned!");
        }

        if (portalCamera == null) {
            Debug.LogError("Portal camera is not assigned!");
        }

        if (screen == null) {
            Debug.LogError("Screen is not assigned!");
        }
        if (screenMeshFilter == null) {
            Debug.LogError("Screen mesh filter is not assigned!");
        }
        
    }

    void Start(){
        // layerScreen = screen.gameObject.layer;
        print("Layer: " + layerScreen);
        ConfigureScaleRatio();

    }

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

                traveller.Teleport(transform, otherPortal.transform, m.GetColumn(3), m.rotation, scaleRatio);
                
                traveller.graphicsClone.transform.SetPositionAndRotation (positionOld, rotOld);
                otherPortal.OnTravellerEnterPortal(traveller, scaleRatio);
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

    public void ConfigureScaleRatio(){
        scaleRatio = new Vector3(
            otherPortal.transform.parent.localScale.x / transform.parent.localScale.x,
            otherPortal.transform.parent.localScale.y / transform.parent.localScale.y,
            otherPortal.transform.parent.localScale.z / transform.parent.localScale.z
        );
    }

    static bool isVisibileFromPlayerCamera(Renderer renderer, Camera camera){
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }

    bool CanSeePortal(Camera from, PortalScript other)
    {
        // Is the portal in the camera's view frustum?
        if (!GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(from), other.screen.bounds))
            return false;

        // Debug.DrawRay(from.transform.position, (other.transform.position - from.transform.position).normalized, Color.blue, 0.1f);
        if (!Physics.Raycast(from.transform.position, (other.transform.position - from.transform.position).normalized, out var hit, Mathf.Infinity, ~layerScreen))
            return false;

        if(showDebugLines)
            Debug.DrawLine(from.transform.position, hit.point, Color.red, 0.1f);
        return Vector3.Distance(hit.transform.position, other.transform.position) < 0.001f;

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

    public void Render(ScriptableRenderContext context)
    {

        if (!CameraUtility.VisibleFromCamera(otherPortal.screen, playerCamera, transform.parent.parent.name))
        {
            return;
        }

        CreateTexture();

        var localToWorldMatrix = playerCamera.transform.localToWorldMatrix;
        var renderPositions = new Vector3[recursionLimit];
        var renderRotations = new Quaternion[recursionLimit];

        int startIndex = 0;
        portalCamera.projectionMatrix = playerCamera.projectionMatrix;
        for (int i = 0; i < recursionLimit; i++)
        {
            if (i > 0)
            {
                if (!CameraUtility.BoundsOverlap(screenMeshFilter, otherPortal.screenMeshFilter, portalCamera))
                {
                    break;
                }
            }
            localToWorldMatrix = transform.localToWorldMatrix * otherPortal.transform.worldToLocalMatrix * localToWorldMatrix;
            int renderOrderIndex = recursionLimit - i - 1;
            renderPositions[renderOrderIndex] = localToWorldMatrix.GetColumn(3);
            renderRotations[renderOrderIndex] = localToWorldMatrix.rotation;

            portalCamera.transform.SetPositionAndRotation(renderPositions[renderOrderIndex], renderRotations[renderOrderIndex]);
            startIndex = renderOrderIndex;
        }

        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        otherPortal.screen.material.SetInt("_DisplayMask", 0);

        for (int i = startIndex; i < recursionLimit; i++)
        {
            portalCamera.transform.SetPositionAndRotation(renderPositions[i], renderRotations[i]);
            SetNearClipPlane();
            HandleCliping();
            try
            {
                UniversalRenderPipeline.RenderSingleCamera(context, portalCamera);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error rendering portal camera on {gameObject.name}:  + {e.Message}");
                return;
            }
            // UniversalRenderPipeline.RenderSingleCamera(context, portalCamera);

            if (i == startIndex)
            {
                otherPortal.screen.material.SetInt("_DisplayMask", 1);
            }
        }

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
    void SetNearClipPlane() {
        Transform clipPlane = transform;
        int dot = (int)Mathf.Sign(Vector3.Dot(clipPlane.forward, transform.position - portalCamera.transform.position));

        Vector3 camSpacePos = portalCamera.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        Vector3 camSpaceNormal = portalCamera.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
        float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset;

        if (Mathf.Abs(camSpaceDst) > nearClipLimit) {
            Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);
            
            // Get the oblique matrix
            Matrix4x4 obliqueMatrix = playerCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
            
            // Validate the matrix before applying
            if (IsValidProjectionMatrix(obliqueMatrix)) {
                portalCamera.projectionMatrix = obliqueMatrix;
                return;
            }
        }
    
        // Fallback to standard projection if oblique would be invalid
        portalCamera.projectionMatrix = playerCamera.projectionMatrix;
    }

    bool IsValidProjectionMatrix(Matrix4x4 matrix) {
        // Check for NaN values in critical matrix components
        for (int i = 0; i < 16; i++) {
            if (float.IsNaN(matrix[i])) return false;
        }
        
        // Additional checks can be added here if needed
        return true;
    }

    void CreateTexture()
    {
        // print("Creating texture");

        if (materialSetup) return; // Avoid setting up the material multiple times

        // Check for null references
        if (portalCamera == null || otherPortal == null || otherPortal.screen == null || otherPortal.screen.material == null)
        {
            Debug.LogError("PortalCamera, OtherPortal, or Screen is not assigned!");
            return;
        }

        // Release existing render texture
        if (portalCamera.targetTexture != null)
        {
            RenderTexture.ReleaseTemporary(portalCamera.targetTexture);
        }

        // Create new render texture
        RenderTexture rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 24); // Use 24-bit depth buffer
        rt.name = gameObject.name + " RenderTexture";
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

        materialSetup = true; // Mark material as set up
        // Debug.Log("Material setup complete: " + transform.parent.parent.parent.name + "-" + transform.parent.parent.name);
    }

    public void DeactivatePortal(Material material = null){
        // if(this.enabled == false){
        //     return;
        // }
        materialSetup = false;
        if (portalCamera.targetTexture != null)
        {
            RenderTexture.ReleaseTemporary(portalCamera.targetTexture);
            portalCamera.targetTexture = null;
            screen.material = material;
        }
        this.enabled = false;
    }

    public void SwitchPortal(PortalScript newPortal){
        if (newPortal == null || newPortal == this) return;

        otherPortal = newPortal;
        materialSetup = false;
        if (portalCamera.targetTexture != null)
        {
            RenderTexture.ReleaseTemporary(portalCamera.targetTexture);
            portalCamera.targetTexture = null;
            screen.material = null;
        }
        ConfigureScaleRatio();
        // otherPortal.enabled = true;
        // otherPortal.CreateTexture();
        // otherPortal.SetNearClipPlane();
        // otherPortal.HandleCliping();
    }

    public PortalScript GetOtherPortal(){
        return otherPortal;
    }
    
    

    void OnTravellerEnterPortal (PortalTraveller traveller, Vector3 scale) {
        if (!trackedTravellers.Contains (traveller)) {
            traveller.EnterPortalThreshold (scale);
            traveller.previousOffsetFromPortal = traveller.transform.position - transform.position;
            trackedTravellers.Add (traveller);
        }
    }

    void OnTriggerEnter (Collider other) {
        var traveller = other.GetComponent<PortalTraveller> ();
        if (traveller) {
            OnTravellerEnterPortal (traveller, scaleRatio);
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
    // void OnValidate () {
    //     if (otherPortal != null) {
    //         otherPortal.otherPortal = this;
    //     }
    // }

    bool SameSideOfPortal (Vector3 posA, Vector3 posB) {
        return SideOfPortal (posA) == SideOfPortal (posB);
    }

    int SideOfPortal (Vector3 pos) {
        return System.Math.Sign (Vector3.Dot (pos - transform.position, transform.forward));
    }
}
