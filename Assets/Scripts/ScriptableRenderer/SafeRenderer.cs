using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SafeRenderer : ScriptableRenderer
{
    public SafeRenderer(ScriptableRendererData data) : base(data) { }

    // Modern URP doesn't expose ExecuteBlock - we override this instead
    public override void SetupRenderPasses(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        base.SetupRenderPasses(context, ref renderingData);
        
        // Validate all passes before execution
        for (int i = 0; i < m_ActiveRenderPassQueue.Count; i++)
        {
            if (m_ActiveRenderPassQueue[i] == null)
            {
                Debug.LogError($"Null render pass at index {i}");
                m_ActiveRenderPassQueue.RemoveAt(i);
                i--;
            }
        }
    }

    // Main execution point in modern URP
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        try
        {
            base.Execute(context, ref renderingData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Render execution failed: {e.Message}");
            context.Submit();
        }
    }
}