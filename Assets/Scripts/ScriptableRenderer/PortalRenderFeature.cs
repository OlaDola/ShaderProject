using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PortalRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Shader portalShader;
        public bool usePortalShader = true;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    }

    public Settings settings = new Settings();

    class PortalRenderPass : ScriptableRenderPass
    {
        private Material portalMaterial;
        private bool usePortalShader;

        public PortalRenderPass(Settings settings)
        {
            if (settings.usePortalShader && settings.portalShader != null)
            {
                portalMaterial = new Material(settings.portalShader);
                usePortalShader = true;
            }
            renderPassEvent = settings.renderPassEvent;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!renderingData.cameraData.camera.CompareTag("MainCamera")) return;

            var portals = Object.FindObjectsOfType<PortalScript>();
            if (portals == null || portals.Length == 0) return;

            CommandBuffer cmd = CommandBufferPool.Get("Portal Rendering");
            
            try
            {
                foreach (var portal in portals) portal.PrePortalRender();

                foreach (var portal in portals)
                {
                    if (portal.isVisibleToPlayer)
                    {
                        RenderPortalRecursive(context, cmd, portal, renderingData.cameraData.camera);
                    }
                }
            }
            finally
            {
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
                foreach (var portal in portals) portal.PostPortalRender();
            }
        }

        void RenderPortalRecursive(ScriptableRenderContext context, CommandBuffer cmd, 
                                PortalScript portal, Camera camera, int recursionLevel = 0)
        {
            if (recursionLevel > portal.recursionLimit) return;

            int texWidth = Mathf.Max(portal.minTextureSize, 
                (int)(Screen.width * Mathf.Pow(portal.textureScaleFactor, recursionLevel)));
            int texHeight = Mathf.Max(portal.minTextureSize, 
                (int)(Screen.height * Mathf.Pow(portal.textureScaleFactor, recursionLevel)));

            int tempRT = Shader.PropertyToID($"_PortalTempRT_{recursionLevel}");
            cmd.GetTemporaryRT(tempRT, texWidth, texHeight, 24, FilterMode.Bilinear);
            
            try
            {
                var matrix = portal.CalculateCameraMatrix(camera, recursionLevel);
                portal.portalCamera.transform.SetPositionAndRotation(matrix.GetColumn(3), matrix.rotation);
                portal.portalCamera.targetTexture = RenderTexture.GetTemporary(texWidth, texHeight, 24);

                portal.screen.shadowCastingMode = ShadowCastingMode.ShadowsOnly;

                if (recursionLevel < portal.recursionLimit)
                {
                    foreach (var nestedPortal in portal.FindVisiblePortals())
                    {
                        if (nestedPortal != portal && nestedPortal != portal.otherPortal)
                        {
                            RenderPortalRecursive(context, cmd, nestedPortal, portal.portalCamera, recursionLevel + 1);
                        }
                    }
                }

                portal.SetNearClipPlane();
                UniversalRenderPipeline.RenderSingleCamera(context, portal.portalCamera);

                if (recursionLevel == 0)
                {
                    Material screenMaterial = usePortalShader ? 
                        new Material(portalMaterial) : 
                        new Material(Shader.Find("Standard"));
                    
                    screenMaterial.mainTexture = portal.portalCamera.targetTexture;
                    screenMaterial.name = $"{portal.name} Portal Material";
                    portal.otherPortal.screen.sharedMaterial = screenMaterial;
                    portal.otherPortal.screen.sharedMaterial.SetInt("_DisplayMask", 1);
                }
            }
            finally
            {
                RenderTexture.ReleaseTemporary(portal.portalCamera.targetTexture);
                portal.screen.shadowCastingMode = ShadowCastingMode.On;
                cmd.ReleaseTemporaryRT(tempRT);
            }
        }
    }

    private PortalRenderPass m_PortalRenderPass;

    public override void Create()
    {
        m_PortalRenderPass = new PortalRenderPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_PortalRenderPass);
    }
}