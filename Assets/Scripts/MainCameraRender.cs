using UnityEngine;
using UnityEngine.Rendering;

public class MainCameraRender : MonoBehaviour
{
    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (!camera.CompareTag("MainCamera")) return;
        
        var portals = FindObjectsOfType<PortalScript>();
        foreach (var portal in portals)
        {
            portal.PrePortalRender();
        }
    }
}