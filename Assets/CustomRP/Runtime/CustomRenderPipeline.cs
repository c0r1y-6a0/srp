using UnityEngine;
using UnityEngine.Rendering;

namespace MySRP
{
    public class CustomRenderPipeline : RenderPipeline
    {
        CameraRender camRender;
        public CustomRenderPipeline(E_BatchingMode mode)
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = mode == E_BatchingMode.SRPBatcher;
            camRender = new CameraRender(mode);
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (Camera cam in cameras)
            {
                camRender.Render(context, cam);
            }
        }
    }

}
