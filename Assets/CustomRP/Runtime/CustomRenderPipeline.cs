using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    CameraRender camRender = new CameraRender();
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach(Camera cam in cameras)
        {
            camRender.Render(context, cam);
        }
    }
}
