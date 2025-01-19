using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Update()
    {
        var m = transform.localToWorldMatrix * otherPortal.transform.worldToLocalMatrix * playerCamera.transform.localToWorldMatrix;
        portalCamera.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
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

                traveller.Teleport (transform, otherPortal.transform, m.GetColumn (3), m.rotation);
                otherPortal.OnTravellerEnterPortal(traveller);
                trackedTravellers.RemoveAt(i);
                i--;
            }
            else{
                traveller.previousOffsetFromPortal = offsetFromPortal;
            }
        }
    }

    static bool isVisibileFromPlayerCamera(Renderer renderer, Camera camera){
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }

    public void Render () {

        // Skip rendering the view from this portal if player is not looking at the linked portal
        if (!CameraUtility.VisibleFromCamera (otherPortal.screen, playerCamera)) {
            return;
        }

        CreateTexture ();

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
        otherPortal.screen.material.SetInt ("displayMask", 0);

        for (int i = startIndex; i < recursionLimit; i++) {
            portalCamera.transform.SetPositionAndRotation (renderPositions[i], renderRotations[i]);
            portalCamera.Render ();

            if (i == startIndex) {
                otherPortal.screen.material.SetInt ("displayMask", 1);
            }
        }

        // Unhide objects hidden at start of render
        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }

    public void CreateTexture(){
        RenderTexture rt = new(Screen.width, Screen.height, 0)
        {
            name = gameObject.name + " RenderTexture"
        };
        portalCamera.targetTexture = rt;
        Material mat;
        if(withShader)
            mat = new(Shader.Find("Unlit/PortalShaderCut"));
        else
            mat = new(Shader.Find("Standard"));
        mat.SetTexture("_MainTex", rt);
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
