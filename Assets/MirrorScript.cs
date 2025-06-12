using UnityEngine;

public class MirrorScript : MonoBehaviour
{
    public Camera mainCamera;
    public Transform mirrorPlane;
    
    [Header ("Advanced Settings")]
    public float nearClipOffset = 0.05f;
    public float nearClipLimit = 0.2f;

    void LateUpdate()
    {
        // Calculate mirrored position
        Vector3 mirroredPosition = MirrorPosition(mainCamera.transform.position, mirrorPlane);
        transform.position = mirroredPosition;

        // Calculate mirrored rotation
        Vector3 lookAtMirror = MirrorPosition(mainCamera.transform.position + mainCamera.transform.forward * 10, mirrorPlane);
        transform.LookAt(lookAtMirror);
        SetNearClipPlane();
    }

    Vector3 MirrorPosition(Vector3 original, Transform mirror)
    {
        // Mirror the position across the mirror plane
        Vector3 localPos = mirror.InverseTransformPoint(original);
        localPos.z *= -1; // Flip the X or Z depending on your mirror orientation
        return mirror.TransformPoint(localPos);
    }

    void SetNearClipPlane()
    {
        Transform clipPlane = mirrorPlane;
        int dot = (int)Mathf.Sign(Vector3.Dot(clipPlane.forward, transform.parent.position - transform.position));

        Vector3 camSpacePos = GetComponent<Camera>().worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        Vector3 camSpaceNormal = GetComponent<Camera>().worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
        float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset;

        if (Mathf.Abs(camSpaceDst) > nearClipLimit)
        {
            Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

            // Get the oblique matrix
            Matrix4x4 obliqueMatrix = mainCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);

            // Validate the matrix before applying
            if (IsValidProjectionMatrix(obliqueMatrix))
            {
                GetComponent<Camera>().projectionMatrix = obliqueMatrix;
                return;
            }
        }

        // Fallback to standard projection if oblique would be invalid
        GetComponent<Camera>().projectionMatrix = mainCamera.projectionMatrix;
    }


    bool IsValidProjectionMatrix(Matrix4x4 matrix)
    {
        // Check for NaN values in critical matrix components
        for (int i = 0; i < 16; i++) {
            if (float.IsNaN(matrix[i])) return false;
        }
        
        // Additional checks can be added here if needed
        return true;
    }
}