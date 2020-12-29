using UnityEngine;
using UnityEngine.Rendering;

namespace MySRP
{
    public partial class CameraRender
    {
        private static ShaderTagId 
        s_UnlitShaderTagId = new ShaderTagId("SRPDefaultUnlit"),
        s_LitShaderTagId = new ShaderTagId("CustomLit");

        private ScriptableRenderContext m_context;
        private Camera m_camera;

        private const string c_BufferName = "Render Camera";
        private CommandBuffer m_buffer = new CommandBuffer { name = c_BufferName };

        private CullingResults m_cullingResults;

        private E_BatchingMode m_batchMode;

        private Lighting m_lighting = new Lighting();

        public CameraRender(E_BatchingMode batchMode)
        {
            m_batchMode = batchMode;
        }


        public void Render(ScriptableRenderContext context, Camera camera, ShadowSettings shadowsSettings)
        {
            m_context = context;
            m_camera = camera;

            PrepareBuffer();
            PrepareSceneMesh();

            if (!Cull(shadowsSettings.m_maxDistance))
            {
                return;
            }

            m_buffer.BeginSample(c_BufferName);
            ExecuteBuffer();
            m_lighting.Setup(context, m_cullingResults, shadowsSettings);
            m_buffer.EndSample(c_BufferName);
            Setup();
            DrawVisibleGeometry();
            DrawUnsupportedShaders();
            DrawGizmos();
            m_lighting.CleanUp();
            Submit();
        }

        private void Setup()
        {
            m_context.SetupCameraProperties(m_camera);
            m_buffer.ClearRenderTarget( m_camera.clearFlags <= CameraClearFlags.Depth, m_camera.clearFlags == CameraClearFlags.Color, m_camera.clearFlags == CameraClearFlags.Color ? m_camera.backgroundColor.linear : Color.clear);
            m_buffer.BeginSample(c_BufferName);
            ExecuteBuffer();
        }

        private void DrawVisibleGeometry()
        {
            var filterSettings = new FilteringSettings(RenderQueueRange.opaque);
            var sortingSettings = new SortingSettings(m_camera) { criteria = SortingCriteria.CommonOpaque };
            var drawSettings = new DrawingSettings(s_UnlitShaderTagId, sortingSettings) 
            { 
                enableDynamicBatching = m_batchMode == E_BatchingMode.DynamicBatching, 
                enableInstancing = m_batchMode == E_BatchingMode.GPUInstancing
            };
            drawSettings.SetShaderPassName(1, s_LitShaderTagId);

            m_context.DrawRenderers(m_cullingResults, ref drawSettings, ref filterSettings);

            m_context.DrawSkybox(m_camera);

            filterSettings.renderQueueRange = RenderQueueRange.transparent;
            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawSettings.sortingSettings = sortingSettings;
            m_context.DrawRenderers(m_cullingResults, ref drawSettings, ref filterSettings);
        }

        private void Submit()
        {
            m_buffer.EndSample(c_BufferName);
            ExecuteBuffer();
            m_context.Submit();
        }

        private void ExecuteBuffer()
        {
            m_context.ExecuteCommandBuffer(m_buffer);
            m_buffer.Clear();
        }

        bool Cull(float maxShadowDistance)
        {
            if (m_camera.TryGetCullingParameters(out ScriptableCullingParameters p))
            {
                p.shadowDistance = Mathf.Min(maxShadowDistance, m_camera.farClipPlane);
                m_cullingResults = m_context.Cull(ref p);
                return true;
            }
            return false;
        }

    }

}
