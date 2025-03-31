using UnityEngine;
using System.Collections.Generic;

public class PortalScript : MonoBehaviour
{
    [Header("Portal Pairing")]
    public PortalScript otherPortal;
    public GameObject screenObject;

    [Header("Rendering Settings")]
    public int recursionLimit = 2;
    public float nearClipOffset = 0.05f;
    public float nearClipLimit = 0.2f;
    public int minTextureSize = 256;
    [Range(0.1f, 1f)] public float textureScaleFactor = 0.7f;

    [Header("Traveller Settings")]
    public List<PortalTraveller> trackedTravellers = new List<PortalTraveller>();

    // Component references
    public Camera portalCamera;
    public MeshRenderer screen;
    private MeshFilter screenMeshFilter;
    private Camera playerCamera;

    // Runtime tracking
    private RenderTexture currentTexture;
    private Vector3 offsetFromPortalToCam;

    void Awake()
    {
        portalCamera = GetComponentInChildren<Camera>();
        portalCamera.enabled = false;
        screen = screenObject.GetComponent<MeshRenderer>();
        screenMeshFilter = screenObject.GetComponent<MeshFilter>();
        playerCamera = Camera.main;
    }

    void LateUpdate()
    {
        HandleTravellers();
    }

    void HandleTravellers()
    {
        for (int i = 0; i < trackedTravellers.Count; i++)
        {
            PortalTraveller traveller = trackedTravellers[i];
            Transform travellerT = traveller.transform;
            
            Vector3 offsetFromPortal = travellerT.position - transform.position;
            int portalSide = System.Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));
            int portalSideOld = System.Math.Sign(Vector3.Dot(traveller.previousOffsetFromPortal, transform.forward));

            if (portalSide != portalSideOld)
            {
                var m = otherPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travellerT.localToWorldMatrix;
                traveller.Teleport(transform, otherPortal.transform, m.GetColumn(3), m.rotation);
                otherPortal.OnTravellerEnterPortal(traveller);
                trackedTravellers.RemoveAt(i);
                i--;
            }
            else
            {
                var m = otherPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travellerT.localToWorldMatrix;
                traveller.graphicsClone.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
                traveller.previousOffsetFromPortal = offsetFromPortal;
            }
        }
    }

    public void PrePortalRender()
    {
        foreach (var traveller in trackedTravellers)
        {
            UpdateSliceParams(traveller);
        }
    }

    public void PostPortalRender()
    {
        foreach (var traveller in trackedTravellers)
        {
            UpdateSliceParams(traveller);
        }
        ProtectScreenFromClipping(playerCamera.transform.position);
    }

    public Matrix4x4 CalculateCameraMatrix(Camera sourceCam, int recursionLevel)
    {
        var matrix = sourceCam.transform.localToWorldMatrix;
        for (int i = 0; i <= recursionLevel; i++)
        {
            matrix = transform.localToWorldMatrix * otherPortal.transform.worldToLocalMatrix * matrix;
        }
        return matrix;
    }

    public void SetNearClipPlane()
    {
        Transform clipPlane = transform;
        int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - portalCamera.transform.position));

        Vector3 camSpacePos = portalCamera.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        Vector3 camSpaceNormal = portalCamera.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
        float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset;

        if (Mathf.Abs(camSpaceDst) > nearClipLimit)
        {
            Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);
            portalCamera.projectionMatrix = playerCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
        }
        else
        {
            portalCamera.projectionMatrix = playerCamera.projectionMatrix;
        }
    }

    float ProtectScreenFromClipping(Vector3 viewPoint)
    {
        float halfHeight = playerCamera.nearClipPlane * Mathf.Tan(playerCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * playerCamera.aspect;
        float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, playerCamera.nearClipPlane).magnitude;
        float screenThickness = dstToNearClipPlaneCorner;

        Transform screenT = screen.transform;
        bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;
        screenT.localPosition = Vector3.forward * screenThickness * ((camFacingSameDirAsPortal) ? 0.5f : -0.5f);
        screenT.localScale = new Vector3(screenT.localScale.x, screenT.localScale.y, screenThickness);
        return screenThickness;
    }

    void UpdateSliceParams(PortalTraveller traveller)
    {
        int side = SideOfPortal(traveller.transform.position);
        Vector3 sliceNormal = transform.forward * -side;
        Vector3 cloneSliceNormal = otherPortal.transform.forward * side;
        Vector3 slicePos = transform.position;
        Vector3 cloneSlicePos = otherPortal.transform.position;

        float screenThickness = screen.transform.localScale.z;
        float sliceOffsetDst = 0;
        float cloneSliceOffsetDst = 0;

        bool playerSameSideAsTraveller = SameSideOfPortal(playerCamera.transform.position, traveller.transform.position);
        if (!playerSameSideAsTraveller) sliceOffsetDst = -screenThickness;
        bool playerSameSideAsClone = side != otherPortal.SideOfPortal(playerCamera.transform.position);
        if (!playerSameSideAsClone) cloneSliceOffsetDst = -screenThickness;

        for (int i = 0; i < traveller.originalMaterials.Length; i++)
        {
            traveller.originalMaterials[i].SetVector("sliceCentre", slicePos);
            traveller.originalMaterials[i].SetVector("sliceNormal", sliceNormal);
            traveller.originalMaterials[i].SetFloat("sliceOffsetDst", sliceOffsetDst);

            traveller.cloneMaterials[i].SetVector("sliceCentre", cloneSlicePos);
            traveller.cloneMaterials[i].SetVector("sliceNormal", cloneSliceNormal);
            traveller.cloneMaterials[i].SetFloat("sliceOffsetDst", cloneSliceOffsetDst);
        }
    }

    public bool isVisibleToPlayer => isVisibileFromPlayerCamera(otherPortal.screen, playerCamera);

    bool isVisibileFromPlayerCamera(Renderer renderer, Camera camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }

    void OnTravellerEnterPortal(PortalTraveller traveller)
    {
        if (!trackedTravellers.Contains(traveller))
        {
            traveller.EnterPortalThreshold();
            traveller.previousOffsetFromPortal = traveller.transform.position - transform.position;
            trackedTravellers.Add(traveller);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var traveller = other.GetComponent<PortalTraveller>();
        if (traveller) OnTravellerEnterPortal(traveller);
    }

    void OnTriggerExit(Collider other)
    {
        var traveller = other.GetComponent<PortalTraveller>();
        if (traveller && trackedTravellers.Contains(traveller))
        {
            traveller.ExitPortalThreshold();
            trackedTravellers.Remove(traveller);
        }
    }

    bool SameSideOfPortal(Vector3 posA, Vector3 posB) => SideOfPortal(posA) == SideOfPortal(posB);
    int SideOfPortal(Vector3 pos) => System.Math.Sign(Vector3.Dot(pos - transform.position, transform.forward));

    public List<PortalScript> FindVisiblePortals()
    {
        List<PortalScript> visiblePortals = new List<PortalScript>();
        PortalScript[] allPortals = FindObjectsOfType<PortalScript>();

        foreach (PortalScript portal in allPortals)
        {
            // Skip self and linked portal
            if (portal == this || portal == otherPortal) continue;

            // Check if portal is visible from current camera position
            if (IsPortalVisibleFromPosition(portal, portalCamera.transform.position))
            {
                visiblePortals.Add(portal);
            }
        }

        return visiblePortals;
    }

    private bool IsPortalVisibleFromPosition(PortalScript portal, Vector3 fromPosition)
    {
        // Basic frustum check
        if (!isVisibileFromPlayerCamera(portal.screen, portalCamera))
        {
            return false;
        }

        // Optional: Add raycast check for occlusion
        Vector3 direction = portal.screen.transform.position - fromPosition;
        float distance = direction.magnitude;
        direction.Normalize();

        // Check if there are obstacles between cameras
        if (Physics.Raycast(fromPosition, direction, out RaycastHit hit, distance))
        {
            if (hit.collider.gameObject != portal.screen.gameObject)
            {
                return false;
            }
        }

        return true;
    }
}