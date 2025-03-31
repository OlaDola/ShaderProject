using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PortalRenderManager : MonoBehaviour
{
    // Singleton instance with auto-creation
    private static PortalRenderManager _instance;
    public static PortalRenderManager Instance => _instance ??= CreateInstance();

    private static PortalRenderManager CreateInstance()
    {
        var go = new GameObject("PortalRenderManager");
        return go.AddComponent<PortalRenderManager>();
    }

    // Settings
    public int maxTotalRendersPerFrame = 20; // Absolute limit
    public float gpuTimeLimitMs = 5f; // Max milliseconds for portal rendering
    
    // Runtime tracking
    private int rendersThisFrame;
    private float lastRenderStartTime;
    private Stack<RenderTexture> texturePool = new Stack<RenderTexture>();
    private HashSet<PortalScript> renderedThisFrame = new HashSet<PortalScript>();

    public bool CanRenderMore()
    {
        // Check if we've exceeded our frame budget
        if (rendersThisFrame >= maxTotalRendersPerFrame) return false;
        
        // Check if we're taking too much GPU time
        if ((Time.realtimeSinceStartup - lastRenderStartTime) * 1000 > gpuTimeLimitMs) return false;
        
        return true;
    }

    public RenderTexture GetRenderTexture(int width, int height)
    {
        if (texturePool.Count > 0)
        {
            var tex = texturePool.Pop();
            if (tex.width != width || tex.height != height)
            {
                tex.Release();
                return new RenderTexture(width, height, 24);
            }
            return tex;
        }
        return new RenderTexture(width, height, 24);
    }

    public void BeginFrame()
    {
        rendersThisFrame = 0;
        renderedThisFrame.Clear();
        lastRenderStartTime = Time.realtimeSinceStartup;
    }

    public bool RegisterRender(PortalScript portal)
    {
        if (renderedThisFrame.Contains(portal)) return false;
        renderedThisFrame.Add(portal);
        rendersThisFrame++;
        return true;
    }
}