using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MainCameraRender : MonoBehaviour
{
    [SerializeField]
    private PortalScript[] portals;

    private void Awake()
    {
        portals = FindObjectsOfType<PortalScript>();
    }

    private void OnEnable()
    {
        // Subscribe to the URP render pipeline event
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        // RenderPipelineManager.beginFrameRendering += OnBeginFrameRendering;
        // RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    private void OnDisable()
    {
        // Unsubscribe from the URP render pipeline event
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        // RenderPipelineManager.beginFrameRendering -= OnBeginFrameRendering;
        // RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

   private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        // Only render portals for the main camera
        if (camera.CompareTag("MainCamera"))
        {
            for (int i = 0; i < portals.Length; i++)
            {
                portals[i].PrePortalRender();
            }
            for (int i = 0; i < portals.Length; i++)
            {
                portals[i].Render(context);
            }
            for (int i = 0; i < portals.Length; i++)
            {
                portals[i].PostPortalRender();
            }
        }
    }

    private void OnBeginFrameRendering(ScriptableRenderContext context, Camera[] camera){
        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].PrePortalRender();
        }
    }

    private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].PostPortalRender();
        }
    }
}