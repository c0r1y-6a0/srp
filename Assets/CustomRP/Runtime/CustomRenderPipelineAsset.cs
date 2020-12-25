using UnityEngine;
using UnityEngine.Rendering;

namespace MySRP
{
    public enum E_BatchingMode
    {
        None,
        SRPBatcher,
        GPUInstancing,
        DynamicBatching,
    }

    [CreateAssetMenu(menuName = "Rendering/My Render Pipeline")]
    public class CustomRenderPipelineAsset : RenderPipelineAsset
    {

        [SerializeField]
        public E_BatchingMode BatchMode;

        protected override RenderPipeline CreatePipeline()
        {
            return new CustomRenderPipeline(BatchMode);
        }
    }

}
