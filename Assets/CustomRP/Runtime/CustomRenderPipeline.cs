using UnityEngine;
using UnityEngine.Rendering;

namespace MySRP
{
    public class CustomRenderPipeline : RenderPipeline
    {
        CameraRender camRender;
        ShadowSettings shadowsSettings;
        public CustomRenderPipeline(E_BatchingMode mode, ShadowSettings _shadowSettings)
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = mode == E_BatchingMode.SRPBatcher;
            GraphicsSettings.lightsUseLinearIntensity = true;
            camRender = new CameraRender(mode);
            shadowsSettings = _shadowSettings;
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (Camera cam in cameras)
            {
                camRender.Render(context, cam, shadowsSettings);
            }
        }
    }

}
