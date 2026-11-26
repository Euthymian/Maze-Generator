using UnityEngine.Rendering.Universal;

public sealed class MazeFogRendererFeature : FullScreenPassRendererFeature
{
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.camera.TryGetComponent(out MazeFog fog) &&
            fog.isActiveAndEnabled && fog.FogEnabled)
        {
            base.AddRenderPasses(renderer, ref renderingData);
        }
    }
}
